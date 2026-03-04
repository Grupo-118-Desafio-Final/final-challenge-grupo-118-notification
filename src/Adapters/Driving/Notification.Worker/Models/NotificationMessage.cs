using System;

namespace final_challenge_grupo_118_notification.Models;

public class NotificationMessage
{
    public bool IsSuccess { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
