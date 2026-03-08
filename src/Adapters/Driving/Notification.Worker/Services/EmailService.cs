using System;
using System.Threading.Tasks;
using final_challenge_grupo_118_notification.Models;
using final_challenge_grupo_118_notification.Services.Interface;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace final_challenge_grupo_118_notification.Services;

public class EmailService : INotificationService, IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _settings;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }
    
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            _logger.Log(LogLevel.Information, "Sending email");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Ignora validação de certificado se necessário (comum em servidores SMTP de dev/testes)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // Conecta usando STARTTLS (Porta 587)
                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        catch(Exception ex)
        {
         _logger.Log(LogLevel.Error, $"Error sending email.: {ex.Message}");   
        }
    }

    public async Task<Task> SendAsync(ContentMessage contentMessage)
    {
        try
        {
            _logger.Log(LogLevel.Information, "Sending email");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress("Tech Challenge", contentMessage.Recipient));
            message.Subject = contentMessage.Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = contentMessage.Content };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Ignora validação de certificado se necessário
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // Conecta usando STARTTLS (Porta 587)
                _logger.LogInformation("Connecting to SMTP server {SmtpServer} on port {Port}", _settings.SmtpServer, _settings.Port);
                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        catch(Exception ex)
        {
            _logger.Log(LogLevel.Error, $"Error sending email.: {ex.Message}");   
        }

        return Task.CompletedTask;
    }
}
