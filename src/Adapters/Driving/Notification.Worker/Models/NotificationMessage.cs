using System;
using System.Diagnostics.CodeAnalysis;

namespace final_challenge_grupo_118_notification.Models;

[ExcludeFromCodeCoverage]
public class NotificationMessage
{
    public bool IsSuccess { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; }
    public string ExceptionMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
