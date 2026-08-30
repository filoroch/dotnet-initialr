using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Filoroch.Template.IoC.Configurations;

public static class SerilogConfiguration
{
    public static IHostBuilder UseProjectSerilog(
        this IHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        return hostBuilder.UseSerilog((_, logger) => logger
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext());
    }
}
