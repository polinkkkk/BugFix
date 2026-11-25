using System.Security.Claims;
using BugFix.Api.Services;
using BugFix.Api.Views.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugFix.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _projectService.CreateProjectAsync(userId, request);

        if (result.UserNotFound)
        {
            return Unauthorized();
        }

        if (result.IsForbidden)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Преподаватель не может создавать ПО."
            );
        }

        if (result.IsConflict)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "ПО с таким названием уже существует."
            );
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result.Project
        );
    }
}