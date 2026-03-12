using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using final_challenge_grupo_118_notification.Models;

namespace final_challenge_grupo_118_notification.Services.Interface;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}