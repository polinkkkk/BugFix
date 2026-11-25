using System.ComponentModel.DataAnnotations;

namespace BugFix.Api.Views.Projects;

public sealed class CreateProjectRequest
{
    [Required(ErrorMessage = "Укажите название ПО.")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символо.")]
    public string Name { get; init; } = "";

    [Required(ErrorMessage = "Укажите ссылку на репозиторий.")]
    [StringLength(2048, ErrorMessage = "Ссылка слишком длинная.")]
    [Url(ErrorMessage = "Укажите корректную ссылку на репозиторий.")]
    public string RepositoryUrl { get; init; } = "";
}