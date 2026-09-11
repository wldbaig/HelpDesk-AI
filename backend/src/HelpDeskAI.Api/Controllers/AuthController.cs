using HelpDeskAI.Application.Common;
using HelpDeskAI.Application.DTOs;
using HelpDeskAI.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskAI.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await auth.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<AuthResponse>.Ok(result, "Account created."));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<AuthResponse>.Ok(await auth.LoginAsync(request, cancellationToken)));
}
