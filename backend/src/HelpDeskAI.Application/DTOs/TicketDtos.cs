namespace HelpDeskAI.Application.DTOs;

public sealed record CreateTicketRequest(string Title, string Description, string Priority);
public sealed record UpdateTicketRequest(string Title, string Description, string Category, string Priority, string Status, string? SuggestedReply);
public sealed record AssignTicketRequest(Guid AgentId);
public sealed record AddCommentRequest(string Body);
public sealed record CommentDto(Guid Id, string Body, DateTime CreatedAt, UserDto Author);
public sealed record TicketDto(
    Guid Id,
    string Title,
    string Description,
    string Category,
    string Priority,
    string Status,
    string? Sentiment,
    string? SuggestedReply,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    UserDto? AssignedAgent,
    IReadOnlyList<CommentDto> Comments);
public sealed record AiAnalysisResult(string Category, string Sentiment, string SuggestedReply);
public sealed record DashboardDto(int Total, int Open, int Closed, int Unassigned, IReadOnlyDictionary<string, int> ByCategory, IReadOnlyDictionary<string, int> ByStatus);
