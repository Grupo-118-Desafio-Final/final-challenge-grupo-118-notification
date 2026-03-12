using System.Diagnostics.CodeAnalysis;

namespace final_challenge_grupo_118_notification.Models;

[ExcludeFromCodeCoverage]
public class RabbitMQSettings
{
    public string? ConnectionString { get; set; }

    public string? QueueName { get; set; }
}