namespace Filoroch.Template.Consumers.Usuarios;

/// <summary>
/// Evento de exemplo para o pipeline de consumers. Em produção seria publicado
/// pela Application (ex. após CriarAsync) via broker (MassTransit/RabbitMQ);
/// aqui trafega em Channel in-memory para manter o template sem infra externa.
/// </summary>
public sealed record UsuarioCriadoEvent(Guid UsuarioId, string Email, DateTime OcorridoEm);
