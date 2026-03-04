using System.Threading.Tasks;
using final_challenge_grupo_118_notification.Models;

namespace final_challenge_grupo_118_notification.Services;

public interface INotificationService
{
    Task<Task> SendAsync(ContentMessage message);
}
