using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BugFix.Api.Services.Projects;
using BugFix.Api.Views.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugFix.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}")]
public sealed class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberService _projectMemberService;

    public ProjectMembersController(IProjectMemberService projectMemberService)
    {
        _projectMemberService = projectMemberService;
    }

    [HttpGet("available-users")]
    public async Task<IActionResult> SearchUsers(Guid projectId, [FromQuery, Required, StringLength(64, MinimumLength = 2)] string search)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var actorId))
        {
            return Unauthorized();
        }

        var result = await _projectMemberService.SearchUsersAsync(projectId, actorId, search);

        return ToResponse(result, StatusCodes.Status200OK);
    }

    [HttpPost("members")]
    [Consumes("application/json")]
    public async Task<IActionResult> AddMembers(Guid projectId, [FromBody] AddProjectMembersRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var actorId))
        {
            return Unauthorized();
        }

        var result = await _projectMemberService.AddMembersAsync(projectId, actorId, request);

        return ToResponse(result, StatusCodes.Status201Created);
    }

    private IActionResult ToResponse<T>(ProjectMembersResult<T> result, int successStatusCode)
    {
        switch (result.Status)
        {
            case ProjectMembersStatus.Success:
                return StatusCode(successStatusCode, result.Value);

            case ProjectMembersStatus.Unauthorized:
                return Unauthorized();

            case ProjectMembersStatus.ProjectNotFound:
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "ПО не найдено."
                );

            case ProjectMembersStatus.Forbidden:
                return Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Действие доступно только тимлиду этого ПО."
                );

            case ProjectMembersStatus.InvalidRequest:
                ModelState.AddModelError(
                    result.Field ?? "members",
                    result.Message ?? "Некорректные данные."
                );

                return ValidationProblem(ModelState);

            case ProjectMembersStatus.Conflict:
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: result.Message ?? "Конфликт при добавлении участников."
                );

            default:
                throw new InvalidOperationException("Неизвестный результат операции.");
        }
    }
}