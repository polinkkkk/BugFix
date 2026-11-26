using BugFix.Api.Views.Projects;
using BugFix.Api.Views.Projects.Dtos;

namespace BugFix.Api.Services.Projects;

public interface IProjectService
{
    Task<CreateProjectResult> CreateProjectAsync(Guid userId, CreateProjectRequest request);

    Task<List<ProjectListItemDto>> GetMyProjectsAsync(Guid userId);

    Task<ProjectDetailsDto?> GetProjectByIdAsync(Guid projectId, Guid userId);
}