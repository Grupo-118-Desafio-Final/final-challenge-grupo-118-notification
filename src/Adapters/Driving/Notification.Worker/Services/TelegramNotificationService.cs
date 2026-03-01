using final_challenge_grupo_118_notification.Models;
using Microsoft.Extensions.Logging;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;

namespace final_challenge_grupo_118_notification.Services;

public class TelegramNotificationService : INotificationService
{
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(ILogger<TelegramNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task<Task> SendAsync(ContentMessage message)
    {
        _logger.LogInformation($"Sending Telegram message to {message.Recipient}: {message.Content}");
        // Implementação real de envio de Telegram aqui        // Exemplo de integração com a API do Telegram
         var botClient = new TelegramBotClient("YOUR_BOT_TOKEN");
         await botClient.SendMessageAsync(message.Recipient, message.Content);

        return Task.CompletedTask;
    }
}
