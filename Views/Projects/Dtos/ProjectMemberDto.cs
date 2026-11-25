namespace BugFix.Api.Views.Projects.Dtos;

public sealed record ProjectMemberDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string Role
);