using Filoroch.Template.IoC.Configurations;
using Filoroch.Template.IoC.ExceptionHandling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Filoroch.Template.IoC;

public static class NativeInjectorBootstrapper
{
    public static IServiceCollection AddProjectDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        IServiceCollection configuredServices = services
            .AddDomainDependencies()
            .AddApplicationDependencies()
            .AddInfrastructureDependencies(configuration)
            .AddProjectSwagger(configuration)
            .AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                    context.ProblemDetails.Extensions["traceId"] =
                        context.HttpContext.TraceIdentifier;
            })
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProjectOpenTelemetry(configuration);

        return configuredServices;
    }

    public static IServiceCollection AddProjectApiDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddProjectDependencies(configuration)
            .AddProjectAuthentication(configuration)
            .AddProjectCors(configuration)
            .AddHealthChecks();

        return services;
    }
}
