using System.ComponentModel.DataAnnotations;

namespace BugFix.Api.Views.Auth.Dtos;

public sealed class ResetPasswordRequest
{
    public Guid UserId { get; init; }

    [Required]
    [StringLength(4096)]
    public string Token { get; init; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string NewPassword { get; init; } = string.Empty;
}