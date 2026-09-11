namespace HelpDeskAI.Application.Common;

public sealed record ApiResponse<T>(bool Success, T? Data, string? Message = null, IReadOnlyDictionary<string, string[]>? Errors = null)
{
    public static ApiResponse<T> Ok(T data, string? message = null) => new(true, data, message);
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed record TicketFilter(
    string? Search,
    string? Status,
    string? Category,
    string? Priority,
    Guid? AssignedAgentId,
    int Page = 1,
    int PageSize = 10);
