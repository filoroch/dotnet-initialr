using Filoroch.Template.IoC.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Filoroch.Template.IoC.Configurations;

public static class OpenTelemetryConfiguration
{
    public static IServiceCollection AddProjectOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        OpenTelemetrySettings settings = configuration.GetSection(OpenTelemetrySettings.SectionName)
            .Get<OpenTelemetrySettings>() ?? new OpenTelemetrySettings();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(settings.ServiceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = new Uri(settings.Endpoint)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = new Uri(settings.Endpoint)));

        return services;
    }
}
