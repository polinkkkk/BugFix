using BugFix.Api.Views.Projects;

namespace BugFix.Api.Services;

public interface IProjectService
{
    Task<CreateProjectResult> CreateProjectAsync(Guid userId, CreateProjectRequest request);
}