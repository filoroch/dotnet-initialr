using Filoroch.Template.IoC.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Filoroch.Template.IoC.Configurations;

public static class CorsConfiguration
{
    public static IServiceCollection AddProjectCors(this IServiceCollection services, IConfiguration configuration)
    {
        CorsSettings settings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new();
        services.AddSingleton(settings);
        services.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            if (settings.AllowedOrigins.Length == 0)
                return;

            policy.WithOrigins(settings.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }));
        return services;
    }
}
