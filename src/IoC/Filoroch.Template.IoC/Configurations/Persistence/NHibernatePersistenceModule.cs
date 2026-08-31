using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Interfaces;
using Filoroch.Template.Domain.Usuarios.Repositories;
using Filoroch.Template.Infra.Persistence;
using Filoroch.Template.Infra.Usuarios.Mappings;
using Filoroch.Template.Infra.Usuarios.Repositories;
using Filoroch.Template.IoC.Settings;

namespace Filoroch.Template.IoC.Configurations.Persistence;

public static partial class PersistenceModules
{
    static partial void AddNHibernate(IServiceCollection services, IConfiguration configuration, PersistenceSettings settings)
    {
        services.AddSingleton<ISessionFactory>(_ =>
        {
            FluentConfiguration builder = Fluently.Configure();
            builder = settings.GetDriver(PersistenceProviders.NHibernate, settings.Providers.NHibernate.Driver).ToLowerInvariant() switch
            {
                "sqlite" => builder.Database(SQLiteConfiguration.Standard.ConnectionString(settings.Providers.NHibernate.ConnectionString)),
                "sqlserver" => builder.Database(MsSqlConfiguration.MsSql2012.ConnectionString(settings.Providers.NHibernate.ConnectionString)),
                "postgresql" => builder.Database(PostgreSQLConfiguration.Standard.ConnectionString(settings.Providers.NHibernate.ConnectionString)),
                _ => throw new InvalidOperationException($"Driver NHibernate não suportado: {settings.GetDriver(PersistenceProviders.NHibernate, settings.Providers.NHibernate.Driver)}.")
            };
            return builder.Mappings(mapping => mapping.FluentMappings.AddFromAssemblyOf<UsuarioNHibernateMapping>()).BuildSessionFactory();
        });
        services.AddScoped(provider => provider.GetRequiredService<ISessionFactory>().OpenSession());
        services.AddScoped<IUsuarioNHibernateRepository, UsuarioNHibernateRepository>();
        services.AddScoped<NHibernateUnitOfWork>();
        services.AddScoped<INHibernateUnitOfWork>(provider => provider.GetRequiredService<NHibernateUnitOfWork>());
        if (settings.DefaultProvider.Equals(PersistenceProviders.NHibernate, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IUsuarioRepository>(provider => provider.GetRequiredService<IUsuarioNHibernateRepository>());
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<INHibernateUnitOfWork>());
        }
    }
}
