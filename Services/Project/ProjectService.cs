using BugFix.Api.Data;
using BugFix.Api.Models;
using BugFix.Api.Views.Projects;
using BugFix.Api.Views.Projects.Dtos;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BugFix.Api.Services.Projects;

public sealed class ProjectService : IProjectService
{
    private readonly ContextDB _db;

    public ProjectService(ContextDB db)
    {
        _db = db;
    }

    public async Task<CreateProjectResult> CreateProjectAsync(Guid userId, CreateProjectRequest request)
    {
        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId);

        if (user is null)
        {
            return new CreateProjectResult
            {
                UserNotFound = true
            };
        }

        if (user.IsTeacher)
        {
            return new CreateProjectResult
            {
                IsForbidden = true
            };
        }

        var name = request.Name.Trim();
        var repositoryUrl = request.RepositoryUrl.Trim();

        var nameExists = await _db.Projects
            .AnyAsync(project => project.Name == name);

        if (nameExists)
        {
            return new CreateProjectResult
            {
                IsConflict = true
            };
        }

        var project = new Project
        {
            Name = name,
            RepositoryUrl = repositoryUrl
        };

        project.Members.Add(new TeamLead
        {
            ProjectId = project.Id,
            UserId = user.Id
        });

        _db.Projects.Add(project);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_Projects_Name"
            })
        {
            return new CreateProjectResult
            {
                IsConflict = true
            };
        }

        return new CreateProjectResult
        {
            Project = new ProjectDto(
                project.Id,
                project.Name,
                project.RepositoryUrl
            )
        };
    }

    public async Task<List<ProjectListItemDto>> GetMyProjectsAsync(Guid userId)
    {
        return await _db.ProjectMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .OrderBy(member => member.Project.Name)
            .ThenBy(member => member.ProjectId)
            .Select(member => new ProjectListItemDto(
                member.ProjectId,
                member.Project.Name,
                member.Project.RepositoryUrl,
                EF.Property<string>(member, "Role")
            ))
            .ToListAsync();
    }

    public async Task<ProjectDetailsDto?> GetProjectByIdAsync(Guid projectId,Guid userId)
    {
        return await _db.ProjectMembers
            .AsNoTracking()
            .Where(member =>
                member.ProjectId == projectId &&
                member.UserId == userId)
            .Select(member => new ProjectDetailsDto(
                member.ProjectId,
                member.Project.Name,
                member.Project.RepositoryUrl,
                EF.Property<string>(member, "Role"),
                member.Project.Members
                    .OrderBy(projectMember => projectMember.User.UserName)
                    .ThenBy(projectMember => projectMember.Id)
                    .Select(projectMember => new ProjectMemberDto(
                        projectMember.Id,
                        projectMember.UserId,
                        projectMember.User.UserName!,
                        EF.Property<string>(projectMember, "Role")))
                    .ToList()))
            .SingleOrDefaultAsync();
    }
}