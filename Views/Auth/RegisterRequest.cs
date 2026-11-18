using System.ComponentModel.DataAnnotations;

namespace BugFix.Api.Views.Auth;

public sealed class RegisterRequest
{
    [Required]
    [StringLength(64)]
    public string UserName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string TimeZoneId { get; init; } = string.Empty;
}