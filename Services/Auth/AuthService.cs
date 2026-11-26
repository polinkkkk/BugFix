using System.Security.Claims;
using BugFix.Api.Models;
using BugFix.Api.Views.Auth.Dtos;
using BugFix.Api.Views.Auth;
using BugFix.Api.Services.PasswordReset;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;
using BugFix.Api.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace BugFix.Api.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IPasswordResetEmailSender _passwordResetEmailSender;
    private readonly PasswordResetOptions _passwordResetOptions;

    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IPasswordResetEmailSender passwordResetEmailSender,
        IOptions<PasswordResetOptions> passwordResetOptions
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _passwordResetEmailSender = passwordResetEmailSender;
        _passwordResetOptions = passwordResetOptions.Value;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        var timeZoneId = request.TimeZoneId.Trim();

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _))
        {
            return new RegisterResult
            {
                FieldErrors = new Dictionary<string, List<string>>
                {
                    ["timeZoneId"] = new()
                    {
                        "Необходимо указать существующий часовой пояс, например Asia/Tomsk."
                    }
                }
            };
        }

        var user = new AppUser
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            TimeZoneId = timeZoneId
        };

        IdentityResult result;

        try
        {
            result = await _userManager.CreateAsync(user, request.Password);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "EmailIndex" or "UserNameIndex"
            })
        {
            return new RegisterResult { IsConflict = true };
        }

        if (result.Errors.Any(error => error.Code is "DuplicateUserName" or "DuplicateEmail"))
        {
            return new RegisterResult { IsConflict = true };
        }

        if (!result.Succeeded)
        {
            var fieldErrors = new Dictionary<string, List<string>>();

            foreach (var error in result.Errors)
            {
                var field = error.Code switch
                {
                    "InvalidUserName" => "userName",
                    "InvalidEmail" => "email",
                    _ when error.Code.StartsWith("Password", StringComparison.Ordinal) => "password",
                    _ => "registration"
                };

                if (!fieldErrors.TryGetValue(field, out var list))
                {
                    list = new List<string>();
                    fieldErrors[field] = list;
                }

                list.Add(error.Description);
            }

            return new RegisterResult { FieldErrors = fieldErrors };
        }

        return new RegisterResult
        {
            Succeeded = true,
            User = ToDto(user)
        };
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null)
        {
            return new LoginResult { UserNotFound = true };
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: true
            );

        if (!result.Succeeded)
        {
            return new LoginResult();
        }

        return new LoginResult
        {
            Succeeded = true,
            User = ToDto(user)
        };
    }

    public async Task<UserDto?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var user = await _userManager.GetUserAsync(principal);
        return user is null ? null : ToDto(user);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    private static UserDto ToDto(AppUser user) => new(
        user.Id,
        user.UserName!,
        user.Email!,
        user.TimeZoneId,
        user.IsTeacher
    );

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null) { return; }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var resetLink = QueryHelpers.AddQueryString(
            _passwordResetOptions.ResetPageUrl,
            new Dictionary<string, string?>
            {
                ["userId"] = user.Id.ToString(),
                ["token"] = encodedToken
            });

        await _passwordResetEmailSender.SendAsync(user.Email!, resetLink);
    }

    public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        string token;

        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch (FormatException)
        {
            return IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            return IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        return await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
    }
}