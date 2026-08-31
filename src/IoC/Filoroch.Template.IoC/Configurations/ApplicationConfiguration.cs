using Filoroch.Template.Application.Usuarios.Services;
using Filoroch.Template.Application.Autenticacao.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Filoroch.Template.IoC.Configurations;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioAppService, UsuarioAppService>();
        services.AddScoped<IAutenticacaoAppService, AutenticacaoAppService>();
        return services;
    }
}
