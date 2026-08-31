using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Filoroch.Template.IoC.Configurations.Persistence;
using Filoroch.Template.IoC.Settings;

namespace Filoroch.Template.IoC.Configurations;

public static class InfrastructureConfiguration
{
    /// <summary>
    /// O template não cria schema, executa migrations ou cria índices automaticamente.
    /// </summary>
    public static void InitializeDatabase(IServiceProvider services)
    {
        _ = services.GetRequiredService<PersistenceSettings>();
    }

    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        PersistenceSettings persistence = configuration.GetSection(PersistenceSettings.SectionName)
            .Get<PersistenceSettings>() ?? new PersistenceSettings();

        PersistenceSettingsValidator.Validate(persistence);
        services.AddSingleton(persistence);
        services.AddConfiguredPersistence(configuration, persistence);
        return services;
    }
}
