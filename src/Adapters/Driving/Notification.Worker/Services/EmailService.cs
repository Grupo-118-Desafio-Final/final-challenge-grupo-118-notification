using System;
using System.Threading.Tasks;
using final_challenge_grupo_118_notification.Models;
using final_challenge_grupo_118_notification.Services.Interface;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace final_challenge_grupo_118_notification.Services;

public class EmailService : INotificationService, IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _settings;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }
    
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        message.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            // Conecta usando STARTTLS (Porta 587)
            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    public Task<Task> SendAsync(ContentMessage message)
    {
        throw new NotImplementedException();
    }
}
