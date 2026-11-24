using BugFix.Api.Services;
using BugFix.Api.Views.Auth;
using BugFix.Api.Views.Auth.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugFix.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result.IsConflict)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Имя пользователя или email уже заняты.");
        }

        if (result.FieldErrors.Count > 0)
        {
            foreach (var (field, errors) in result.FieldErrors)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(field, error);
                }
            }

            return ValidationProblem(ModelState);
        }

        return StatusCode(StatusCodes.Status201Created, result.User);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes("application/json")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.UserNotFound)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Не удалось войти. Проверьте email и пароль.");
        }

        if (!result.Succeeded)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Не удалось войти. Проверьте данные или повторите попытку позже.");
        }

        return Ok(result.User);
    }

    [Authorize]
    [HttpGet("me")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult> GetCurrentUser()
    {
        var user = await _authService.GetCurrentUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [Consumes("application/json")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.RequestPasswordResetAsync(request);

        return Ok(new
        {
            message = "Письмо со ссылкой отправлено на email."
        });
    }
}