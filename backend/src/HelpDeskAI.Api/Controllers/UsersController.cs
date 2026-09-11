using HelpDeskAI.Application.Common;
using HelpDeskAI.Application.DTOs;
using HelpDeskAI.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskAI.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Agent")]
[Route("api/users")]
public sealed class UsersController(TicketService tickets) : ControllerBase
{
    [HttpGet("agents")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> Agents(CancellationToken cancellationToken) => Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(await tickets.GetAgentsAsync(cancellationToken)));
}
