using Filoroch.Template.IoC.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Filoroch.Template.IoC.Configurations;

public static class EntrypointsConfiguration
{
    public static IWebHostBuilder UseProjectApiUrls(
        this IWebHostBuilder webHost,
        IConfiguration configuration)
    {
        EntrypointsSettings settings = configuration.GetSection(EntrypointsSettings.SectionName)
            .Get<EntrypointsSettings>() ?? new EntrypointsSettings();

        if (settings.Api.Urls.Length > 0)
            webHost.UseUrls(settings.Api.Urls);

        return webHost;
    }
}
