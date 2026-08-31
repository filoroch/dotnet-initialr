using Filoroch.Template.IoC;
using Filoroch.Template.IoC.Configurations;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddProjectAppSettings(builder.Environment);
builder.WebHost.UseProjectApiUrls(builder.Configuration);
builder.Host.UseProjectSerilog(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProjectApiDependencies(builder.Configuration);

WebApplication app = builder.Build();

InfrastructureConfiguration.InitializeDatabase(app.Services);

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseProjectSwagger(builder.Configuration);

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();
