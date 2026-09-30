using Filoroch.Template.IoC;
using Filoroch.Template.IoC.Configurations;
using Filoroch.Template.Workers;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddProjectAppSettings(builder.Environment);
builder.Services.AddProjectSerilog(builder.Configuration);
builder.Services.AddProjectDependencies(builder.Configuration);
builder.Services.Configure<WorkersSettings>(builder.Configuration.GetSection(WorkersSettings.SectionName));
builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
InfrastructureConfiguration.InitializeDatabase(host.Services);
host.Run();
