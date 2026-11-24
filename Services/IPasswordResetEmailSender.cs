namespace BugFix.Api.Services;

public interface IPasswordResetEmailSender
{
    Task SendAsync(string email, string resetLink);
}