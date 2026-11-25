using System.ComponentModel.DataAnnotations;

namespace BugFix.Api.Views.Auth;

public sealed class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = "";
}