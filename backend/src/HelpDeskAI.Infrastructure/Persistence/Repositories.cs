using HelpDeskAI.Application.Common;
using HelpDeskAI.Application.Contracts;
using HelpDeskAI.Domain.Entities;
using HelpDeskAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskAI.Infrastructure.Persistence;

public sealed class UserRepository(HelpDeskDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) => db.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => db.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    public async Task<IReadOnlyList<User>> GetAgentsAsync(CancellationToken cancellationToken) => await db.Users.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
    public Task AddAsync(User user, CancellationToken cancellationToken) => db.Users.AddAsync(user, cancellationToken).AsTask();
}

public sealed class TicketRepository(HelpDeskDbContext db) : ITicketRepository
{
    public Task<Ticket?> GetByIdAsync(Guid id, bool includeComments, CancellationToken cancellationToken)
    {
        IQueryable<Ticket> query = db.Tickets.Include(x => x.AssignedAgent);
        if (includeComments) query = query.Include(x => x.Comments.OrderBy(c => c.CreatedAt)).ThenInclude(x => x.Author);
        return query.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Ticket>> GetPageAsync(TicketFilter filter, CancellationToken cancellationToken)
    {
        var query = db.Tickets.AsNoTracking().Include(x => x.AssignedAgent).AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(x => x.Title.ToLower().Contains(term) || x.Description.ToLower().Contains(term));
        }
        if (Enum.TryParse<TicketStatus>(filter.Status, true, out var status)) query = query.Where(x => x.Status == status);
        if (Enum.TryParse<TicketCategory>(filter.Category, true, out var category)) query = query.Where(x => x.Category == category);
        if (Enum.TryParse<TicketPriority>(filter.Priority, true, out var priority)) query = query.Where(x => x.Priority == priority);
        if (filter.AssignedAgentId.HasValue) query = query.Where(x => x.AssignedAgentId == filter.AssignedAgentId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(cancellationToken);
        return new PagedResult<Ticket>(items, filter.Page, filter.PageSize, total);
    }

    public Task AddAsync(Ticket ticket, CancellationToken cancellationToken) => db.Tickets.AddAsync(ticket, cancellationToken).AsTask();
    public void Remove(Ticket ticket) => db.Tickets.Remove(ticket);
    public Task AddCommentAsync(Comment comment, CancellationToken cancellationToken) => db.Comments.AddAsync(comment, cancellationToken).AsTask();

    public async Task<DashboardData> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var statuses = await db.Tickets.AsNoTracking().GroupBy(x => x.Status).Select(g => new { Key = g.Key.ToString(), Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var categories = await db.Tickets.AsNoTracking().GroupBy(x => x.Category).Select(g => new { Key = g.Key.ToString(), Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        var total = statuses.Values.Sum();
        var open = statuses.GetValueOrDefault(nameof(TicketStatus.Open)) + statuses.GetValueOrDefault(nameof(TicketStatus.InProgress));
        var closed = statuses.GetValueOrDefault(nameof(TicketStatus.Resolved)) + statuses.GetValueOrDefault(nameof(TicketStatus.Closed));
        var unassigned = await db.Tickets.AsNoTracking().CountAsync(x => x.AssignedAgentId == null, cancellationToken);
        return new DashboardData(total, open, closed, unassigned, categories, statuses);
    }
}
