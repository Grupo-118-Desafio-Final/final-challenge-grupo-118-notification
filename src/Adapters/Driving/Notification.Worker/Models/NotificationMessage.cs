using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace final_challenge_grupo_118_notification.Models;

[ExcludeFromCodeCoverage]
public class NotificationMessage
{
    [JsonPropertyName("isSuccess")] public bool IsSuccess { get; set; }

    [JsonPropertyName("userId")] public int UserId { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonPropertyName("exceptionMessage")] public string ExceptionMessage { get; set; }

    [JsonPropertyName("createdAt")] public DateTimeOffset CreatedAt { get; set; }
}