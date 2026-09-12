using HelpDeskAI.Application.Common;
using HelpDeskAI.Application.Contracts;

namespace HelpDeskAI.Infrastructure.AI;

/// <summary>Selects the registered <see cref="IAiTicketService"/> that matches the requested provider.</summary>
public sealed class AiTicketServiceResolver(IEnumerable<IAiTicketService> services) : IAiTicketServiceResolver
{
    public IAiTicketService Resolve(AiProvider provider) =>
        services.FirstOrDefault(service => service.Provider == provider)
        ?? throw new ExternalServiceException($"AI provider '{provider}' is not supported.");
}
