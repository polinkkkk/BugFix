namespace BugFix.Api.Views.Projects.Dtos;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string RepositoryUrl
);