using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Interfaces;
using Filoroch.Template.Domain.Usuarios.Repositories;
using Filoroch.Template.Infra.Persistence;
using Filoroch.Template.Infra.Usuarios.Repositories;
using Filoroch.Template.IoC.Settings;

namespace Filoroch.Template.IoC.Configurations.Persistence;

public static partial class PersistenceModules
{
    static partial void AddDapper(IServiceCollection services, IConfiguration configuration, PersistenceSettings settings)
    {
        services.AddScoped<DapperConnection>(_ => new DapperConnection(DbConnectionFactory.Create(settings.GetDriver(PersistenceProviders.Dapper, settings.Providers.Dapper.Driver), settings.Providers.Dapper.ConnectionString)));
        services.AddScoped<IUsuarioDapperRepository, UsuarioDapperRepository>();
        services.AddScoped<DapperUnitOfWork>();
        services.AddScoped<IDapperUnitOfWork>(provider => provider.GetRequiredService<DapperUnitOfWork>());
        if (settings.DefaultProvider.Equals(PersistenceProviders.Dapper, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IUsuarioRepository>(provider => provider.GetRequiredService<IUsuarioDapperRepository>());
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IDapperUnitOfWork>());
        }
    }
}
