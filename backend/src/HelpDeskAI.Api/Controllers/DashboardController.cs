using HelpDeskAI.Application.Common;
using HelpDeskAI.Application.DTOs;
using HelpDeskAI.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskAI.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Agent")]
[Route("api/dashboard")]
public sealed class DashboardController(TicketService tickets) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetStats(CancellationToken cancellationToken) => Ok(ApiResponse<DashboardDto>.Ok(await tickets.GetDashboardAsync(cancellationToken)));
}
