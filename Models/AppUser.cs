using Microsoft.AspNetCore.Identity;

namespace BugFix.Api.Models;

public class AppUser : IdentityUser<Guid>
{
    public string TimeZoneId { get; set; } = "Etc/UTC";
    public bool IsTeacher { get; set; } = false;
}