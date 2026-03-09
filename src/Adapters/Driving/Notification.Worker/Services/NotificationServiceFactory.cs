using System;
using System.Diagnostics.CodeAnalysis;
using final_challenge_grupo_118_notification.Models;
using Microsoft.Extensions.DependencyInjection;

namespace final_challenge_grupo_118_notification.Services;

[ExcludeFromCodeCoverage]
public class NotificationServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public INotificationService GetService(NotificationTypeEnum type)
    {
        return type switch
        {
            NotificationTypeEnum.Email => _serviceProvider.GetRequiredService<EmailService>(),
            NotificationTypeEnum.Sms => _serviceProvider.GetRequiredService<SmsNotificationService>(),
            NotificationTypeEnum.Telegram => _serviceProvider.GetRequiredService<TelegramNotificationService>(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
