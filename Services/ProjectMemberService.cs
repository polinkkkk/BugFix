using BugFix.Api.Data;
using BugFix.Api.Models;
using BugFix.Api.Views.Projects;
using BugFix.Api.Views.Projects.Dtos;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BugFix.Api.Services;

public sealed class ProjectMemberService : IProjectMemberService
{
    private readonly ContextDB _db;

    public ProjectMemberService(ContextDB db)
    {
        _db = db;
    }

    public async Task<ProjectMembersResult<IReadOnlyList<AvailableProjectUserDto>>> SearchUsersAsync(
        Guid projectId,
        Guid actorId,
        string search
    )
    {
        var access = await CheckAccessAsync(projectId, actorId);

        if (access != ProjectMembersStatus.Success)
        {
            return new(access);
        }

        var normalizedSearch = search
            .Trim()
            .Normalize()
            .ToUpperInvariant();

        if (normalizedSearch.Length < 2)
        {
            return new(
                ProjectMembersStatus.InvalidRequest,
                Message: "Введите хотя бы два символа для поиска.",
                Field: "search"
            );
        }

        var users = await _db.Users
            .AsNoTracking()
            .Where(user =>
                user.NormalizedUserName != null &&
                user.NormalizedUserName.Contains(normalizedSearch) &&
                !_db.ProjectMembers.Any(member =>
                    member.ProjectId == projectId &&
                    member.UserId == user.Id))
            .OrderBy(user => user.NormalizedUserName)
            .ThenBy(user => user.Id)
            .Take(20)
            .Select(user => new AvailableProjectUserDto(
                user.Id,
                user.UserName!,
                user.IsTeacher
            ))
            .ToListAsync();

        return new(ProjectMembersStatus.Success, users);
    }

    public async Task<ProjectMembersResult<IReadOnlyList<ProjectMemberDto>>> AddMembersAsync(
        Guid projectId,
        Guid actorId,
        AddProjectMembersRequest request
    )
    {
        var access = await CheckAccessAsync(projectId, actorId);

        if (access != ProjectMembersStatus.Success)
        {
            return new(access);
        }

        if (request.Members is null ||
            request.Members.Count is < 1 or > 50 ||
            request.Members.Any(member => member is null || member.UserId == Guid.Empty)
        )
        {
            return new(
                ProjectMembersStatus.InvalidRequest,
                Message: "Укажите от 1 до 50 участников с корректными идентификаторами.",
                Field: "members"
            );
        }

        var userIds = request.Members
            .Select(member => member.UserId)
            .ToArray();

        if (userIds.Distinct().Count() != userIds.Length)
        {
            return new(
                ProjectMembersStatus.InvalidRequest,
                Message: "Один пользователь указан несколько раз.",
                Field: "members"
            );
        }

        var alreadyExists = await _db.ProjectMembers
            .AnyAsync(member => member.ProjectId == projectId && userIds.Contains(member.UserId));

        if (alreadyExists)
        {
            return new(
                ProjectMembersStatus.Conflict,
                Message: "Один из выбранных пользователей уже участвует в ПО."
            );
        }

        var users = await _db.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id);

        var members = new List<ProjectMember>();
        var addedMembers = new List<ProjectMemberDto>();

        for (var index = 0; index < request.Members.Count; index++)
        {
            var item = request.Members[index];

            if (!users.TryGetValue(item.UserId, out var user))
            {
                return new(
                    ProjectMembersStatus.InvalidRequest,
                    Message: "Пользователь не найден.",
                    Field: $"members[{index}].userId"
                );
            }

            if (item.Role is not ("Tester" or "Developer" or "Teacher"))
            {
                return new(
                    ProjectMembersStatus.InvalidRequest,
                    Message: "Допустимые роли: Tester, Developer, Teacher.",
                    Field: $"members[{index}].role"
                );
            }

            if (user.IsTeacher && item.Role != "Teacher")
            {
                return new(
                    ProjectMembersStatus.InvalidRequest,
                    Message: "Преподавателю доступна только роль Teacher.",
                    Field: $"members[{index}].role"
                );
            }

            if (!user.IsTeacher && item.Role == "Teacher")
            {
                return new(
                    ProjectMembersStatus.InvalidRequest,
                    Message: "Роль Teacher доступна только аккаунту преподавателя.",
                    Field: $"members[{index}].role"
                );
            }

            ProjectMember member = item.Role switch
            {
                "Tester" => new Tester(),
                "Developer" => new Developer(),
                "Teacher" => new Teacher(),
                _ => throw new InvalidOperationException("Неизвестная роль.")
            };

            member.ProjectId = projectId;
            member.UserId = user.Id;

            members.Add(member);

            addedMembers.Add(new ProjectMemberDto(
                member.Id,
                user.Id,
                user.UserName!,
                item.Role
            ));
        }

        _db.ProjectMembers.AddRange(members);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_ProjectMembers_ProjectId_UserId"
            })
        {
            return new(
                ProjectMembersStatus.Conflict,
                Message: "Один из пользователей уже добавлен в ПО. Обновите данные.");
        }

        return new(ProjectMembersStatus.Success, addedMembers);
    }

    private async Task<ProjectMembersStatus> CheckAccessAsync(Guid projectId, Guid actorId)
    {
        var actor = await _db.Users
            .AsNoTracking()
            .Where(user => user.Id == actorId)
            .Select(user => new { user.IsTeacher })
            .SingleOrDefaultAsync();

        if (actor is null)
        {
            return ProjectMembersStatus.Unauthorized;
        }

        var projectExists = await _db.Projects
            .AnyAsync(project => project.Id == projectId);

        if (!projectExists)
        {
            return ProjectMembersStatus.ProjectNotFound;
        }

        if (actor.IsTeacher)
        {
            return ProjectMembersStatus.Forbidden;
        }

        var isTeamLead = await _db.ProjectMembers
            .OfType<TeamLead>()
            .AnyAsync(member => member.ProjectId == projectId && member.UserId == actorId);

        return isTeamLead
            ? ProjectMembersStatus.Success
            : ProjectMembersStatus.Forbidden;
    }
}