using Filoroch.Template.Domain.Usuarios.Services;
using Filoroch.Template.IoC.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Filoroch.Template.IoC.Configurations;

public static class SeedConfiguration
{
    /// <summary>
    /// Cria o administrador inicial quando nenhum usuário existe e as credenciais de seed
    /// estão configuradas (Seed:AdminEmail / Seed:AdminPassword). Idempotente e nunca derruba a inicialização.
    /// </summary>
    public static void SeedAdminUser(IServiceProvider services)
    {
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(SeedConfiguration));
        SeedSettings settings = services.GetRequiredService<IConfiguration>()
            .GetSection(SeedSettings.SectionName).Get<SeedSettings>() ?? new SeedSettings();

        if (string.IsNullOrWhiteSpace(settings.AdminEmail) || string.IsNullOrWhiteSpace(settings.AdminPassword))
        {
            logger.LogInformation("Seed do administrador ignorado: Seed:AdminEmail/Seed:AdminPassword não configurados.");
            return;
        }

        try
        {
            IUsuariosService usuariosService = services.GetRequiredService<IUsuariosService>();
            bool criado = usuariosService
                .CriarAdministradorInicialAsync(settings.AdminUsername, settings.AdminEmail, settings.AdminPassword)
                .GetAwaiter().GetResult();

            if (criado)
                logger.LogInformation("Administrador inicial criado com sucesso.");
            else
                logger.LogInformation("Seed do administrador ignorado: já existem usuários cadastrados.");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Não foi possível criar o administrador inicial. O servidor continuará em execução.");
        }
    }
}
