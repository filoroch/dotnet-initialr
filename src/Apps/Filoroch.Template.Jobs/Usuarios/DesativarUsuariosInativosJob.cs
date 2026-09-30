using System.Diagnostics;
using Filoroch.Template.Application.Usuarios.DataTransfer.Requests;
using Filoroch.Template.Application.Usuarios.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace Filoroch.Template.Jobs.Usuarios;

[DisallowConcurrentExecution]
public sealed class DesativarUsuariosInativosJob(
    IServiceScopeFactory scopeFactory,
    IOptions<JobsSettings> options,
    ILogger<DesativarUsuariosInativosJob> logger) : IJob
{
    private static readonly ActivitySource ActivitySource = new("Filoroch.Template.Jobs");

    public async Task Execute(IJobExecutionContext context)
    {
        JobsSettings settings = options.Value;
        int dias = settings.DiasInatividade <= 0 ? 90 : settings.DiasInatividade;
        CancellationToken cancellationToken = context.CancellationToken;

        using Activity? activity = ActivitySource.StartActivity("jobs.desativar_inativos");
        activity?.SetTag("dias_inatividade", dias);

        using IServiceScope scope = scopeFactory.CreateScope();
        IUsuarioAppService appService = scope.ServiceProvider.GetRequiredService<IUsuarioAppService>();

        int desativados = await appService.DesativarInativosAsync(
            new DesativarUsuariosInativosRequest { DiasInatividade = dias },
            cancellationToken);

        activity?.SetTag("usuarios_desativados", desativados);
        logger.LogInformation(
            "Job de desativação concluído. Dias: {Dias}, Desativados: {Desativados}",
            dias, desativados);
    }
}
