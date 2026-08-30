using Filoroch.Template.IoC.Configurations;
using Filoroch.Template.Workers;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddProjectAppSettings(builder.Environment);
builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
host.Run();
