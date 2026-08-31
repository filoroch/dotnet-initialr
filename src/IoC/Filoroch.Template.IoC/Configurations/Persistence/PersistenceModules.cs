using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Filoroch.Template.IoC.Settings;

namespace Filoroch.Template.IoC.Configurations.Persistence;

public static partial class PersistenceModules
{
    public static IServiceCollection AddConfiguredPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        PersistenceSettings settings)
    {
        AddEfCore(services, configuration, settings);
        AddNHibernate(services, configuration, settings);
        AddDapper(services, configuration, settings);
        AddMongo(services, configuration, settings);
        return services;
    }

    static partial void AddEfCore(IServiceCollection services, IConfiguration configuration, PersistenceSettings settings);
    static partial void AddNHibernate(IServiceCollection services, IConfiguration configuration, PersistenceSettings settings);
    static partial void AddDapper(IServiceCollection services, IConfiguration configuration, PersistenceSettings settings);
    static partial void AddMongo(IServiceCollection services, IConfiguration configuration, PersistenceSettings settings);
}
