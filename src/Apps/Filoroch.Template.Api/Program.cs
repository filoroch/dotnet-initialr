using Filoroch.Template.IoC;
using Filoroch.Template.IoC.Configurations;
using Filoroch.Template.Domain.Usuarios;
using Filoroch.Template.Domain.Usuarios.Services;
using Filoroch.Template.Infra.Persistence;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Enums;
using Microsoft.EntityFrameworkCore;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddProjectAppSettings(builder.Environment);
builder.WebHost.UseProjectApiUrls(builder.Configuration);
builder.Host.UseProjectSerilog(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProjectApiDependencies(builder.Configuration);

WebApplication app = builder.Build();

// Seed admin user on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<TemplateDbContext>();
        var passwordService = services.GetRequiredService<IPasswordService>();

        var adminEmail = "admin@template.local";
        if (!context.Usuarios.Any(u => u.Email == adminEmail))
        {
            var adminUsername = "admin";
            var adminPassword = "Admin@123";
            var passwordHash = passwordService.Hash(adminPassword);

            var adminUser = new Usuario(adminUsername, adminEmail, passwordHash, PerfilUsuario.Admin);

            context.Usuarios.Add(adminUser);
            context.SaveChanges();

            Log.Information("Admin user seeded with email: {Email}", adminEmail);
        }
        else
        {
            Log.Information("Admin user already exists with email: {Email}", adminEmail);
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Erro ao fazer seed do usuário administrador");
    }
}

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
