using HelpDeskAI.Application.DTOs;

namespace HelpDeskAI.Application.Contracts;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ITokenService
{
    string CreateToken(Guid userId, string email, string name, string role);
}

public interface IAiTicketService
{
    Task<AiAnalysisResult> AnalyzeAsync(string title, string description, CancellationToken cancellationToken);
}
