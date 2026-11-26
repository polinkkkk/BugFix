namespace BugFix.Api.Views.Projects.Dtos;

public sealed record ProjectDetailsDto(
    Guid Id,
    string Name,
    string RepositoryUrl,
    string MyRole,
    IReadOnlyList<ProjectMemberDto> Members
);