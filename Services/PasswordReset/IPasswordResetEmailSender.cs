namespace BugFix.Api.Services.PasswordReset;

public interface IPasswordResetEmailSender
{
    Task SendAsync(string email, string resetLink);
}