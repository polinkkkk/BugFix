namespace BugFix.Api.Views.Projects.Dtos;

public sealed record AvailableProjectUserDto(
    Guid Id,
    string UserName,
    bool IsTeacher
);