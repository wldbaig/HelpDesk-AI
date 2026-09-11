using FluentValidation;
using HelpDeskAI.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDeskAI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, string? autoMapperLicenseKey = null)
    {
        services.AddAutoMapper(config =>
        {
            if (!string.IsNullOrWhiteSpace(autoMapperLicenseKey)) config.LicenseKey = autoMapperLicenseKey;
        }, typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<AuthService>();
        services.AddScoped<TicketService>();
        return services;
    }
}
