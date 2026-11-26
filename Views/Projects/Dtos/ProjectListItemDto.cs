namespace BugFix.Api.Views.Projects.Dtos;

public sealed record ProjectListItemDto(
    Guid Id,
    string Name,
    string RepositoryUrl,
    string MyRole
);