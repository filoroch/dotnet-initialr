using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Filoroch.Template.IoC.Configurations;

public static class AppSettingsConfiguration
{
    public static IConfigurationBuilder AddProjectAppSettings(
        this IConfigurationBuilder configuration,
        IHostEnvironment environment)
    {
        PhysicalFileProvider fileProvider = new(AppContext.BaseDirectory);

        configuration
            .AddJsonFile(
                fileProvider,
                "Settings/appsettings.json",
                optional: false,
                reloadOnChange: true)
            .AddJsonFile(
                fileProvider,
                $"Settings/appsettings.{environment.EnvironmentName}.json",
                optional: true,
                reloadOnChange: true);

        if (environment.IsDevelopment())
            configuration.AddUserSecrets(Assembly.GetEntryAssembly()!, optional: true);

        configuration.AddEnvironmentVariables();

        return configuration;
    }
}
