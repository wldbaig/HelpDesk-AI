using HelpDeskAI.Application.Contracts;
using HelpDeskAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskAI.Infrastructure.Persistence;

public sealed class HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HelpDeskDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
