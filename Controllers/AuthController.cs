using BugFix.Api.Views.Auth;
using BugFix.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BugFix.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var timeZoneId = request.TimeZoneId.Trim();

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _))
        {
            ModelState.AddModelError(
                "timeZoneId",
                "Необходимо указать существующий часовой пояс, например Asia/Tomsk."
                );

            return ValidationProblem(ModelState);
        }

        var user = new AppUser
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            TimeZoneId = timeZoneId
        };

        IdentityResult result;

        try
        {
            result = await _userManager.CreateAsync(user, request.Password);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "EmailIndex" or "UserNameIndex"
            })
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Имя пользователя или email уже заняты."
                );
        }

        if (result.Errors.Any(error =>
            error.Code is "DuplicateUserName" or "DuplicateEmail"))
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Имя пользователя или email уже заняты."
                );
        }

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                var field = error.Code switch
                {
                    "InvalidUserName" => "userName",
                    "InvalidEmail" => "email",
                    _ when error.Code.StartsWith("Password", StringComparison.Ordinal) => "password",
                    _ => "registration"
                };

                ModelState.AddModelError(field, error.Description);
            }

            return ValidationProblem(ModelState);
        }

        var response = new 
        {
            user.Id,
            user.UserName,
            user.Email,
            user.TimeZoneId
        };

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes("application/json")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Не удалось войти. Проверьте email и пароль."
                );
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: true
            );

        if (!result.Succeeded)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Не удалось войти. Проверьте данные или повторите попытку позже."
                );
        }

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.TimeZoneId
        });
    }

    [Authorize]
    [HttpGet("me")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.TimeZoneId
        });
    }
}