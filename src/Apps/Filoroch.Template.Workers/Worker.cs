using System.Diagnostics;
using Filoroch.Template.Application.Usuarios.DataTransfer.Requests;
using Filoroch.Template.Application.Usuarios.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Filoroch.Template.Workers;

public sealed class Worker(
    IServiceScopeFactory scopeFactory,
    IOptions<WorkersSettings> options,
    ILogger<Worker> logger) : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("Filoroch.Template.Workers");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        WorkersSettings settings = options.Value;
        int intervalo = settings.IntervaloSegundos <= 0 ? 300 : settings.IntervaloSegundos;
        int dias = settings.DiasInatividade <= 0 ? 90 : settings.DiasInatividade;

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(intervalo));

        logger.LogInformation(
            "Worker de desativação iniciado. Intervalo: {Intervalo}s, DiasInatividade: {Dias}",
            intervalo, dias);

        do
        {
            try
            {
                using Activity? activity = ActivitySource.StartActivity("workers.desativar_inativos");
                activity?.SetTag("dias_inatividade", dias);

                using IServiceScope scope = scopeFactory.CreateScope();
                IUsuarioAppService appService = scope.ServiceProvider.GetRequiredService<IUsuarioAppService>();

                int desativados = await appService.DesativarInativosAsync(
                    new DesativarUsuariosInativosRequest { DiasInatividade = dias },
                    stoppingToken);

                activity?.SetTag("usuarios_desativados", desativados);
                logger.LogInformation("Ciclo concluído. Usuários desativados: {Desativados}", desativados);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro no ciclo de desativação de usuários inativos");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
