using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.User.Dtos;
using External.User.API.User;
using final_challenge_grupo_118_notification.Models;
using final_challenge_grupo_118_notification.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace final_challenge_grupo_118_notification;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly NotificationServiceFactory _notificationServiceFactory;
    private IConnection _connection;
    private IModel _channel;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, NotificationServiceFactory notificationServiceFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _notificationServiceFactory = notificationServiceFactory;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker Notification: Waiting messages...");
        var factory = new ConnectionFactory()
        {
            Uri = new Uri(_configuration.GetConnectionString("RabbitMq"))
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "notification",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        _logger.LogInformation("Waiting for messages.");

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received {Message}", message);

            try
            {
                var decodedMessage = Encoding.UTF8.GetString(Convert.FromBase64String(message));
                var notificationMessage = JsonSerializer.Deserialize<NotificationMessage>(decodedMessage);
                if (notificationMessage != null)
                {
                    var service = _notificationServiceFactory.GetService(NotificationTypeEnum.Email);
                    
                    if (service == null)
                        throw new Exception("Service not found");

                    var userAPI = new UsersManager(new HttpClient(), _configuration);
                    var user = await userAPI.GetByIdAsync(notificationMessage.UserId, stoppingToken);
                        
                    if (user == null)
                        throw new Exception("User not found");
                    
                    var contentMessage = new ContentMessage()
                    {
                        Recipient = user.Email,
                        Subject = "Notification",
                        Content = $"{user.Name}, Message: {notificationMessage.Message} {(!notificationMessage.IsSuccess ? $" Exception: {notificationMessage.ExceptionMessage}" : "" )} CreatedAt: {notificationMessage.CreatedAt}" 
                    };
                    await service.SendAsync(contentMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        };

        _channel.BasicConsume(queue: "notification",
                             autoAck: true,
                             consumer: consumer);

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Close();
        _connection.Close();
        return base.StopAsync(cancellationToken);
    }
}
