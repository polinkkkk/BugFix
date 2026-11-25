using BugFix.Api.Views.Auth.Dtos;

namespace BugFix.Api.Views.Auth;

public sealed class RegisterResult
{
    public bool Succeeded { get; init; }
    public bool IsConflict { get; init; }
    public UserDto? User { get; init; }

    public Dictionary<string, List<string>> FieldErrors { get; init; } = new();
}