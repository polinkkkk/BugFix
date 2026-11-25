using BugFix.Api.Views.Auth.Dtos;

namespace BugFix.Api.Views.Auth;

public sealed class LoginResult
{
    public bool Succeeded { get; init; }
    public bool UserNotFound { get; init; }
    public UserDto? User { get; init; }
}