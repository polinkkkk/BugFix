using System.Security.Claims;
using BugFix.Api.Views.Auth;
using BugFix.Api.Views.Auth.Dtos;
using Microsoft.AspNetCore.Identity;

namespace BugFix.Api.Services.Auth;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
    Task<LoginResult> LoginAsync(LoginRequest request);
    Task<UserDto?> GetCurrentUserAsync(ClaimsPrincipal principal);
    Task LogoutAsync();
    Task RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequest request);
}