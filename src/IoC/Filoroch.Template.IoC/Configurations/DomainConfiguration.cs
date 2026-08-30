using Filoroch.Template.Domain.Usuarios.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Filoroch.Template.IoC.Configurations;

public static class DomainConfiguration
{
    public static IServiceCollection AddDomainDependencies(this IServiceCollection services)
    {
        services.AddScoped<IUsuariosService, UsuariosService>();
        return services;
    }
}
