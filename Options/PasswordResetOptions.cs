namespace BugFix.Api.Options;

public sealed class PasswordResetOptions
{
    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; }
    public string From { get; set; } = "";
    public string ResetPageUrl { get; set; } = "";
}