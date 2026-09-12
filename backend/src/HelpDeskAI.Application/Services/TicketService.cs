using AutoMapper;
using HelpDeskAI.Application.Common;
using HelpDeskAI.Application.Contracts;
using HelpDeskAI.Application.DTOs;
using HelpDeskAI.Domain.Entities;
using HelpDeskAI.Domain.Enums;

namespace HelpDeskAI.Application.Services;

public sealed class TicketService(ITicketRepository tickets, IUserRepository users, IUnitOfWork unitOfWork, IAiTicketServiceResolver aiResolver, IMapper mapper)
{
    public async Task<PagedResult<TicketDto>> GetPageAsync(TicketFilter filter, CancellationToken cancellationToken)
    {
        var safe = filter with { Page = Math.Max(1, filter.Page), PageSize = Math.Clamp(filter.PageSize, 1, 100) };
        var page = await tickets.GetPageAsync(safe, cancellationToken);
        return new PagedResult<TicketDto>(mapper.Map<IReadOnlyList<TicketDto>>(page.Items), page.Page, page.PageSize, page.TotalCount);
    }

    public async Task<TicketDto> GetAsync(Guid id, CancellationToken cancellationToken) =>
        mapper.Map<TicketDto>(await FindAsync(id, true, cancellationToken));

    public async Task<TicketDto> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = new Ticket { Title = request.Title.Trim(), Description = request.Description.Trim(), Priority = Enum.Parse<TicketPriority>(request.Priority, true) };
        await tickets.AddAsync(ticket, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<TicketDto>(ticket);
    }

    public async Task<TicketDto> UpdateAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = await FindAsync(id, false, cancellationToken);
        ticket.Title = request.Title.Trim();
        ticket.Description = request.Description.Trim();
        ticket.Category = Enum.Parse<TicketCategory>(request.Category, true);
        ticket.Priority = Enum.Parse<TicketPriority>(request.Priority, true);
        ticket.Status = Enum.Parse<TicketStatus>(request.Status, true);
        ticket.SuggestedReply = request.SuggestedReply?.Trim();
        ticket.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<TicketDto>(ticket);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await FindAsync(id, false, cancellationToken);
        tickets.Remove(ticket);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<TicketDto> AssignAsync(Guid id, Guid agentId, CancellationToken cancellationToken)
    {
        var ticket = await FindAsync(id, false, cancellationToken);
        var agent = await users.GetByIdAsync(agentId, cancellationToken) ?? throw new NotFoundException("Agent not found.");
        ticket.AssignedAgentId = agent.Id;
        ticket.AssignedAgent = agent;
        ticket.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<TicketDto>(ticket);
    }

    public async Task<CommentDto> AddCommentAsync(Guid ticketId, Guid authorId, AddCommentRequest request, CancellationToken cancellationToken)
    {
        _ = await FindAsync(ticketId, false, cancellationToken);
        var author = await users.GetByIdAsync(authorId, cancellationToken) ?? throw new NotFoundException("User not found.");
        var comment = new Comment { TicketId = ticketId, AuthorId = authorId, Author = author, Body = request.Body.Trim() };
        await tickets.AddCommentAsync(comment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<CommentDto>(comment);
    }

    public async Task<TicketDto> AnalyzeAsync(Guid id, AiProvider provider, CancellationToken cancellationToken)
    {
        var ticket = await FindAsync(id, true, cancellationToken);
        var comments = ticket.Comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => $"{c.Author?.Name ?? "User"}: {c.Body}")
            .ToList();
        var ai = aiResolver.Resolve(provider);
        var result = await ai.AnalyzeAsync(ticket.Title, ticket.Description, comments, cancellationToken);
        ticket.Category = Enum.Parse<TicketCategory>(result.Category, true);
        ticket.Sentiment = Enum.Parse<TicketSentiment>(result.Sentiment, true);
        ticket.SuggestedReply = result.SuggestedReply.Trim();
        ticket.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<TicketDto>(ticket);
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken) =>
        mapper.Map<DashboardDto>(await tickets.GetDashboardAsync(cancellationToken));

    public async Task<IReadOnlyList<UserDto>> GetAgentsAsync(CancellationToken cancellationToken) =>
        mapper.Map<IReadOnlyList<UserDto>>(await users.GetAgentsAsync(cancellationToken));

    private async Task<Ticket> FindAsync(Guid id, bool comments, CancellationToken cancellationToken) =>
        await tickets.GetByIdAsync(id, comments, cancellationToken) ?? throw new NotFoundException("Ticket not found.");
}
