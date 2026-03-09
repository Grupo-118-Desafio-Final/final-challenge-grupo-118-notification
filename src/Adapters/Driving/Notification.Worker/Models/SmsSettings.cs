using System.Diagnostics.CodeAnalysis;

namespace final_challenge_grupo_118_notification.Models;

[ExcludeFromCodeCoverage]
public class SmsSettings
{
    public string AccountSid { get; set; }
    public string AuthToken { get; set; }
    public string FromPhoneNumber { get; set; }
}