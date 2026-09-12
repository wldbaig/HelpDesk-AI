using System.Security.Claims;
using HelpDeskAI.Application.Common;
using HelpDeskAI.Application.Contracts;
using HelpDeskAI.Application.DTOs;
using HelpDeskAI.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDeskAI.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Agent")]
[Route("api/tickets")]
public sealed class TicketsController(TicketService tickets) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TicketDto>>>> List([FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? category, [FromQuery] string? priority, [FromQuery] Guid? assignedAgentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
        Ok(ApiResponse<PagedResult<TicketDto>>.Ok(await tickets.GetPageAsync(new TicketFilter(search, status, category, priority, assignedAgentId, page, pageSize), cancellationToken)));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> Get(Guid id, CancellationToken cancellationToken) => Ok(ApiResponse<TicketDto>.Ok(await tickets.GetAsync(id, cancellationToken)));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = await tickets.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = ticket.Id }, ApiResponse<TicketDto>.Ok(ticket, "Ticket created."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> Update(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<TicketDto>.Ok(await tickets.UpdateAsync(id, request, cancellationToken), "Ticket updated."));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await tickets.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> Assign(Guid id, AssignTicketRequest request, CancellationToken cancellationToken) => Ok(ApiResponse<TicketDto>.Ok(await tickets.AssignAsync(id, request.AgentId, cancellationToken), "Agent assigned."));

    [HttpPost("{id:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid id, AddCommentRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var comment = await tickets.AddCommentAsync(id, userId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CommentDto>.Ok(comment, "Comment added."));
    }

    [HttpPost("{id:guid}/analyze")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> Analyze(Guid id, [FromQuery] AiProvider provider = AiProvider.OpenAi, CancellationToken cancellationToken = default) => Ok(ApiResponse<TicketDto>.Ok(await tickets.AnalyzeAsync(id, provider, cancellationToken), "AI analysis completed."));

    private Guid GetUserId()
    {
        var value = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedException("Invalid user identity.");
    }
}
