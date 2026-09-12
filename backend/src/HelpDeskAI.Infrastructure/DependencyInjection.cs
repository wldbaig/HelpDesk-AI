using HelpDeskAI.Application.Contracts;
using HelpDeskAI.Infrastructure.AI;
using HelpDeskAI.Infrastructure.Identity;
using HelpDeskAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HelpDeskAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.Configure<GrokOptions>(configuration.GetSection(GrokOptions.SectionName));
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        services.Configure<ClaudeOptions>(configuration.GetSection(ClaudeOptions.SectionName));
        services.AddDbContext<HelpDeskDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<HelpDeskDbContext>());
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<DataSeeder>();

        // Register one typed HttpClient per LLM provider, each named after its concrete type so it
        // keeps its own BaseAddress. (Registering them all as IAiTicketService would share a single
        // named client and the last BaseAddress would win for every provider.)
        services.AddHttpClient<OpenAiTicketService>((provider, client) =>
            ConfigureAiClient(client, provider.GetRequiredService<IOptions<OpenAiOptions>>().Value.BaseUrl));
        services.AddHttpClient<GrokTicketService>((provider, client) =>
            ConfigureAiClient(client, provider.GetRequiredService<IOptions<GrokOptions>>().Value.BaseUrl));
        services.AddHttpClient<GeminiTicketService>((provider, client) =>
            ConfigureAiClient(client, provider.GetRequiredService<IOptions<GeminiOptions>>().Value.BaseUrl));
        services.AddHttpClient<ClaudeTicketService>((provider, client) =>
            ConfigureAiClient(client, provider.GetRequiredService<IOptions<ClaudeOptions>>().Value.BaseUrl));

        // Expose every provider as IAiTicketService so the resolver can pick one by provider name.
        services.AddTransient<IAiTicketService>(provider => provider.GetRequiredService<OpenAiTicketService>());
        services.AddTransient<IAiTicketService>(provider => provider.GetRequiredService<GrokTicketService>());
        services.AddTransient<IAiTicketService>(provider => provider.GetRequiredService<GeminiTicketService>());
        services.AddTransient<IAiTicketService>(provider => provider.GetRequiredService<ClaudeTicketService>());
        services.AddScoped<IAiTicketServiceResolver, AiTicketServiceResolver>();
        return services;
    }

    private static void ConfigureAiClient(HttpClient client, string baseUrl)
    {
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(45);
    }
}
