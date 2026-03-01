namespace final_challenge_grupo_118_notification.Services.Interface;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}