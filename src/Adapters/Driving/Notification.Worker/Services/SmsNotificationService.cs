using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using final_challenge_grupo_118_notification.Models;
using final_challenge_grupo_118_notification.Services.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace final_challenge_grupo_118_notification.Services;

[ExcludeFromCodeCoverage]
public class SmsNotificationService : INotificationService, ISmsService
{
    private readonly ILogger<SmsNotificationService> _logger;
    private readonly SmsSettings _settings;

    public SmsNotificationService(IOptions<SmsSettings> settings, ILogger<SmsNotificationService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        // Inicializa o cliente globalmente ou por requisição
        TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
    }

    public async Task SendAsync(NotificationMessage message)
    {
        _logger.LogInformation($"Sending SMS to {message.UserId}: {message.Message}");
        // Implementação real de envio de SMS aqui
        await MessageResource.CreateAsync(
            body: message.Message,
            from: new PhoneNumber(_settings.FromPhoneNumber),
            to: new PhoneNumber("")
        );
    }

    public Task<Task> SendAsync(ContentMessage message)
    {
        throw new NotImplementedException();
    }
}
