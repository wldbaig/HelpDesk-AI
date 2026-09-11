using HelpDeskAI.Application.Contracts;
using HelpDeskAI.Domain.Entities;
using HelpDeskAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskAI.Infrastructure.Persistence;

public sealed class DataSeeder(HelpDeskDbContext db, IPasswordHasher passwords)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);
        if (await db.Users.AnyAsync(cancellationToken)) return;

        var admin = new User { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Amina Khan", Email = "admin@helpdesk.local", PasswordHash = passwords.Hash("Admin123!"), Role = UserRole.Admin };
        var agent = new User { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "Daniel Reed", Email = "agent@helpdesk.local", PasswordHash = passwords.Hash("Agent123!"), Role = UserRole.Agent };
        db.Users.AddRange(admin, agent);

        var tickets = new[]
        {
            new Ticket { Title = "Charged twice for annual plan", Description = "Our card shows two identical renewals from yesterday. Please reverse the duplicate charge.", Category = TicketCategory.Billing, Priority = TicketPriority.High, Status = TicketStatus.Open, Sentiment = TicketSentiment.Negative, AssignedAgent = agent, CreatedAt = DateTime.UtcNow.AddHours(-3) },
            new Ticket { Title = "Cannot reset my password", Description = "The reset link says it has expired even when I open it immediately.", Category = TicketCategory.Account, Priority = TicketPriority.High, Status = TicketStatus.InProgress, Sentiment = TicketSentiment.Negative, AssignedAgent = agent, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Ticket { Title = "Export stalls at 80 percent", Description = "CSV export for our quarterly report never completes for larger date ranges.", Category = TicketCategory.Technical, Priority = TicketPriority.Medium, Status = TicketStatus.Open, Sentiment = TicketSentiment.Neutral, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new Ticket { Title = "Thanks for the quick onboarding help", Description = "Everything is working now. Your setup guide was very clear.", Category = TicketCategory.Other, Priority = TicketPriority.Low, Status = TicketStatus.Closed, Sentiment = TicketSentiment.Positive, AssignedAgent = admin, CreatedAt = DateTime.UtcNow.AddDays(-4) },
            new Ticket { Title = "Update billing contact", Description = "Please change the invoice recipient from finance-old to ap at our company domain.", Category = TicketCategory.Billing, Priority = TicketPriority.Low, Status = TicketStatus.Resolved, Sentiment = TicketSentiment.Neutral, AssignedAgent = agent, CreatedAt = DateTime.UtcNow.AddDays(-6) },
            new Ticket { Title = "Mobile app keeps signing me out", Description = "Since the latest update I have to log in every time I reopen the app.", Category = TicketCategory.Technical, Priority = TicketPriority.Urgent, Status = TicketStatus.Open, Sentiment = TicketSentiment.Negative, CreatedAt = DateTime.UtcNow.AddHours(-8) }
        };
        db.Tickets.AddRange(tickets);
        await db.SaveChangesAsync(cancellationToken);
    }
}
