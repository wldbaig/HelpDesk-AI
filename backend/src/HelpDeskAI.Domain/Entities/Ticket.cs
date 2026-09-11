using HelpDeskAI.Domain.Enums;

namespace HelpDeskAI.Domain.Entities;

public sealed class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Description { get; set; }
    public TicketCategory Category { get; set; } = TicketCategory.Other;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketSentiment? Sentiment { get; set; }
    public string? SuggestedReply { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedAgentId { get; set; }
    public User? AssignedAgent { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
