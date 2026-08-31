using Filoroch.Template.IoC.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
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

        string serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME")
            ?? settings.ServiceName;

        string endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
            ?? settings.Endpoint;

        string tracesEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_TRACES_ENDPOINT")
            ?? endpoint;

        string metricsEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_METRICS_ENDPOINT")
            ?? endpoint;

        string protocol = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL")
            ?? "grpc";

        string? headers = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS");

        Dictionary<string, object> resourceAttributes = GetResourceAttributes();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes(resourceAttributes))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddSqlClientInstrumentation()
                .AddOtlpExporter(options => ConfigureExporter(options, tracesEndpoint, protocol, headers, "traces")))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options => ConfigureExporter(options, metricsEndpoint, protocol, headers, "metrics")));

        return services;
    }

    private static void ConfigureExporter(
        OtlpExporterOptions options,
        string endpoint,
        string protocol,
        string? headers,
        string signal)
    {
        bool useHttpProtobuf = protocol.Equals("http/protobuf", StringComparison.OrdinalIgnoreCase);

        options.Endpoint = new Uri(useHttpProtobuf
            ? $"{endpoint.TrimEnd('/')}/v1/{signal}"
            : endpoint);
        options.Protocol = useHttpProtobuf ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;

        if (!string.IsNullOrWhiteSpace(headers))
            options.Headers = headers;
    }

    private static Dictionary<string, object> GetResourceAttributes()
    {
        string? attributes = Environment.GetEnvironmentVariable("OTEL_RESOURCE_ATTRIBUTES");

        if (string.IsNullOrWhiteSpace(attributes))
            return [];

        return attributes
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(attribute => attribute.Split('=', 2, StringSplitOptions.TrimEntries))
            .Where(attribute => attribute.Length == 2 && !string.IsNullOrWhiteSpace(attribute[0]))
            .ToDictionary(attribute => attribute[0], attribute => (object)attribute[1]);
    }
}
