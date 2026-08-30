using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Implementations;
using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Interfaces;
using Filoroch.Template.Domain.Usuarios.Repositories;
using Filoroch.Template.Infra.Persistence;
using Filoroch.Template.Infra.Usuarios.Repositories;
using Filoroch.Template.IoC.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Filoroch.Template.IoC.Configurations;

public static class InfrastructureConfiguration
{
    /// <summary>
    /// Inicializa o banco configurado para o template. Para EF Core, o template
    /// usa EnsureCreated no bootstrap de desenvolvimento.
    /// Migrações de produção devem ser executadas pelo pipeline de deploy.
    /// </summary>
    public static void InitializeDatabase(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        scope.ServiceProvider.GetRequiredService<TemplateDbContext>()
            .Database.EnsureCreated();
    }

    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        DatabaseSettings database = configuration.GetSection(DatabaseSettings.SectionName)
            .Get<DatabaseSettings>() ?? new DatabaseSettings();

        services.AddDbContext<TemplateDbContext>(options =>
            options.UseSqlite(database.ConnectionString));

        services.AddScoped<DbContext>(provider =>
            provider.GetRequiredService<TemplateDbContext>());

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUnitOfWork, EFCoreUnitOfWork>();

        return services;
    }
}
