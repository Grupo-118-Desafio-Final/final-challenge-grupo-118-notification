using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using final_challenge_grupo_118_notification.Models;
using Microsoft.Extensions.Logging;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;

namespace final_challenge_grupo_118_notification.Services;

[ExcludeFromCodeCoverage]
public class TelegramNotificationService(ILogger<TelegramNotificationService> logger) : INotificationService
{
    public async Task<Task> SendAsync(ContentMessage message)
    {
        logger.LogInformation($"Sending Telegram message to {message.Recipient}: {message.Content}");
         var botClient = new TelegramBotClient("YOUR_BOT_TOKEN");
         if (message.Recipient != null && message.Content != null)
             await botClient.SendMessageAsync(message.Recipient, message.Content);
         else
             return Task.FromException<Task>(new Exception("Recipient or content is null"));

         return Task.CompletedTask;
    }
}
