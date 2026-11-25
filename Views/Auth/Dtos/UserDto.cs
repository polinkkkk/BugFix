namespace BugFix.Api.Views.Auth.Dtos;

public sealed record UserDto(
    Guid Id, 
    string UserName, 
    string Email, 
    string TimeZoneId,
    bool IsTeacher
);