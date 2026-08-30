using Filoroch.Template.IoC.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Filoroch.Template.IoC.Configurations;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddProjectSwagger(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        SwaggerSettings settings = configuration.GetSection(SwaggerSettings.SectionName)
            .Get<SwaggerSettings>() ?? new SwaggerSettings();

        if (!settings.Enabled)
            return services;

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => options.SwaggerDoc(
            settings.Version,
            new OpenApiInfo
            {
                Title = settings.Title,
                Description = settings.Description,
                Version = settings.Version
            }));

        return services;
    }

    public static WebApplication UseProjectSwagger(
        this WebApplication app,
        IConfiguration configuration)
    {
        SwaggerSettings settings = configuration.GetSection(SwaggerSettings.SectionName)
            .Get<SwaggerSettings>() ?? new SwaggerSettings();

        if (!settings.Enabled)
            return app;

        app.UseSwagger(options => options.RouteTemplate = settings.JsonRoute);
        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = settings.RoutePrefix;
            string documentUrl = settings.JsonRoute
                .Replace("{documentName}", settings.Version, StringComparison.Ordinal);

            options.SwaggerEndpoint($"/{documentUrl}", settings.Title);
            options.DocumentTitle = settings.Title;
        });

        return app;
    }
}
