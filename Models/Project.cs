namespace BugFix.Api.Models;

public sealed class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string RepositoryUrl { get; set; } = "";
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
}