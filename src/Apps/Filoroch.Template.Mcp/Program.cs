using Filoroch.Template.IoC;
using Filoroch.Template.IoC.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddProjectAppSettings(builder.Environment);

// O stdout pertence exclusivamente ao protocolo MCP; os logs devem ir para stderr.
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddProjectDependencies(builder.Configuration)
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

IHost host = builder.Build();
InfrastructureConfiguration.InitializeDatabase(host.Services);
await host.RunAsync();
