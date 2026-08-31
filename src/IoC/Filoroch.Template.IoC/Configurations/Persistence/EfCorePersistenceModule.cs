using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Implementations;
using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Interfaces;
using Filoroch.Template.Domain.Usuarios.Repositories;
using Filoroch.Template.Infra.Persistence;
using Filoroch.Template.Infra.Usuarios.Repositories;
using Filoroch.Template.IoC.Settings;

namespace Filoroch.Template.IoC.Configurations.Persistence;

public static partial class PersistenceModules
{
    static partial void AddEfCore(IServiceCollection services, IConfiguration configuration, PersistenceSettings settings)
    {
        services.AddDbContext<TemplateDbContext>(options =>
        {
            switch (settings.GetDriver(PersistenceProviders.EfCore, settings.Providers.EfCore.Driver).ToLowerInvariant())
            {
                case "sqlite": options.UseSqlite(settings.Providers.EfCore.ConnectionString); break;
                case "sqlserver": options.UseSqlServer(settings.Providers.EfCore.ConnectionString); break;
                case "postgresql": options.UseNpgsql(settings.Providers.EfCore.ConnectionString); break;
                default: throw new InvalidOperationException($"Driver EF Core não suportado: {settings.GetDriver(PersistenceProviders.EfCore, settings.Providers.EfCore.Driver)}.");
            }
        });
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<TemplateDbContext>());
        services.AddScoped<IUsuarioEfRepository, UsuarioRepository>();
        services.AddScoped<EFCoreUnitOfWork>();
        services.AddScoped<IEFCoreUnitOfWork>(provider => provider.GetRequiredService<EFCoreUnitOfWork>());
        if (settings.DefaultProvider.Equals(PersistenceProviders.EfCore, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IUsuarioRepository>(provider => provider.GetRequiredService<IUsuarioEfRepository>());
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IEFCoreUnitOfWork>());
        }
    }
}
