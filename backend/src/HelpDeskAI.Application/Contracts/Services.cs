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

public enum AiProvider
{
    OpenAi,
    Grok,
    Gemini,
    Claude
}

public interface IAiTicketService
{
    AiProvider Provider { get; }
    Task<AiAnalysisResult> AnalyzeAsync(string title, string description, IReadOnlyList<string> comments, CancellationToken cancellationToken);
}

public interface IAiTicketServiceResolver
{
    IAiTicketService Resolve(AiProvider provider);
}
