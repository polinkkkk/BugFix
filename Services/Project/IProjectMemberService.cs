using BugFix.Api.Views.Projects;
using BugFix.Api.Views.Projects.Dtos;

namespace BugFix.Api.Services.Projects;

public interface IProjectMemberService
{
    Task<ProjectMembersResult<IReadOnlyList<AvailableProjectUserDto>>>
        SearchUsersAsync(
            Guid projectId,
            Guid actorId,
            string search
        );

    Task<ProjectMembersResult<IReadOnlyList<ProjectMemberDto>>>
        AddMembersAsync(
            Guid projectId,
            Guid actorId,
            AddProjectMembersRequest request
        );
}