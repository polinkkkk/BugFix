using BugFix.Api.Views.Projects.Dtos;

namespace BugFix.Api.Views.Projects;

public sealed class CreateProjectResult
{
    public bool UserNotFound { get; init; }
    public bool IsForbidden { get; init; }
    public bool IsConflict { get; init; }
    public ProjectDto? Project { get; init; }
}