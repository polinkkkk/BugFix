namespace BugFix.Api.Views.Projects;

public enum ProjectMembersStatus
{
    Success,
    Unauthorized,
    ProjectNotFound,
    Forbidden,
    InvalidRequest,
    Conflict
}

public sealed record ProjectMembersResult<T>(
    ProjectMembersStatus Status,
    T? Value = default,
    string? Message = null,
    string? Field = null
);