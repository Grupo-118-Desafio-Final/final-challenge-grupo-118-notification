using System.Diagnostics.CodeAnalysis;

namespace final_challenge_grupo_118_notification.Models;

[ExcludeFromCodeCoverage]
public class ContentMessage
{
    public string Recipient { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
}