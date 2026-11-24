using System.Net.Mail;
using System.Text;
using BugFix.Api.Options;
using Microsoft.Extensions.Options;

namespace BugFix.Api.Services;

public sealed class SmtpPasswordResetEmailSender : IPasswordResetEmailSender
{
    private readonly PasswordResetOptions _options;

    public SmtpPasswordResetEmailSender(IOptions<PasswordResetOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(string email, string resetLink)
    {
        using var message = new MailMessage(_options.From, email)
        {
            Subject = "Восстановление пароля BugFix",
            Body = $"Для установки нового пароля перейдите по ссылке:\n\n{resetLink}",
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };

        using var client = new SmtpClient(
            _options.SmtpHost,
            _options.SmtpPort
        );

        await client.SendMailAsync(message);
    }
}