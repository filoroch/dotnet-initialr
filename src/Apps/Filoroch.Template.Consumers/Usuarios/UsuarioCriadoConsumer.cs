using System.Diagnostics;
using System.Threading.Channels;

namespace Filoroch.Template.Consumers.Usuarios;

public sealed class UsuarioCriadoConsumer(
    Channel<UsuarioCriadoEvent> channel,
    ILogger<UsuarioCriadoConsumer> logger) : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("Filoroch.Template.Consumers");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Consumer de usuários criado aguardando eventos");

        await foreach (UsuarioCriadoEvent evento in channel.Reader.ReadAllAsync(stoppingToken))
        {
            using Activity? activity = ActivitySource.StartActivity("consumers.usuario_criado");
            activity?.SetTag("usuario.id", evento.UsuarioId);

            try
            {
                logger.LogInformation(
                    "Boas-vindas ao usuário {UsuarioId} ({Email})",
                    evento.UsuarioId, evento.Email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar evento de usuário criado {UsuarioId}", evento.UsuarioId);
            }
        }
    }
}
