using Filoroch.Template.IoC;
using Filoroch.Template.IoC.Configurations;
using Filoroch.Template.Jobs;
using Filoroch.Template.Jobs.Usuarios;
using Quartz;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddProjectAppSettings(builder.Environment);
builder.Services.AddProjectSerilog(builder.Configuration);
builder.Services.AddProjectDependencies(builder.Configuration);
builder.Services.Configure<JobsSettings>(builder.Configuration.GetSection(JobsSettings.SectionName));

JobsSettings jobsSettings = builder.Configuration.GetSection(JobsSettings.SectionName).Get<JobsSettings>() ?? new JobsSettings();
JobKey jobKey = new("desativar-usuarios-inativos");

builder.Services.AddQuartz(q =>
{
    q.AddJob<DesativarUsuariosInativosJob>(options => options.WithIdentity(jobKey));
    q.AddTrigger(options => options
        .ForJob(jobKey)
        .WithIdentity("desativar-usuarios-inativos-trigger")
        .WithCronSchedule(jobsSettings.Cron));
});
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

IHost host = builder.Build();
InfrastructureConfiguration.InitializeDatabase(host.Services);
host.Run();
