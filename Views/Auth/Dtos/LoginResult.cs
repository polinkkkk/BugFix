namespace BugFix.Api.Views.Auth.Dtos;

public sealed class LoginResult
{
    public bool Succeeded { get; init; }
    public bool UserNotFound { get; init; }
    public UserDto? User { get; init; }
}