using HelpDeskAI.Application.Common;
using HelpDeskAI.Domain.Entities;

namespace HelpDeskAI.Application.Contracts;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<User>> GetAgentsAsync(CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
}

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, bool includeComments, CancellationToken cancellationToken);
    Task<PagedResult<Ticket>> GetPageAsync(TicketFilter filter, CancellationToken cancellationToken);
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);
    void Remove(Ticket ticket);
    Task AddCommentAsync(Comment comment, CancellationToken cancellationToken);
    Task<DashboardData> GetDashboardAsync(CancellationToken cancellationToken);
}

public sealed record DashboardData(int Total, int Open, int Closed, int Unassigned, IReadOnlyDictionary<string, int> ByCategory, IReadOnlyDictionary<string, int> ByStatus);

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
