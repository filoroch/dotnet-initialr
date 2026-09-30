using System.Threading.Channels;
using Filoroch.Template.Consumers.Usuarios;
using Filoroch.Template.IoC;
using Filoroch.Template.IoC.Configurations;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddProjectAppSettings(builder.Environment);
builder.Services.AddProjectSerilog(builder.Configuration);
builder.Services.AddProjectDependencies(builder.Configuration);
builder.Services.AddSingleton(Channel.CreateUnbounded<UsuarioCriadoEvent>());
builder.Services.AddHostedService<UsuarioCriadoConsumer>();

IHost host = builder.Build();
InfrastructureConfiguration.InitializeDatabase(host.Services);
host.Run();
