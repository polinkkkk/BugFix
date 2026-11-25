using System.ComponentModel.DataAnnotations;

namespace BugFix.Api.Views.Projects;

public sealed class AddProjectMembersRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Выберите хотя бы одного участника.")]
    [MaxLength(50, ErrorMessage = "За один запрос можно добавить до 50 участников.")]
    public List<AddProjectMemberRequest> Members { get; init; } = new();
}

public sealed class AddProjectMemberRequest
{
    public Guid UserId { get; init; }

    [Required(ErrorMessage = "Укажите роль участника.")]
    [StringLength(20)]
    public string Role { get; init; } = "";
}