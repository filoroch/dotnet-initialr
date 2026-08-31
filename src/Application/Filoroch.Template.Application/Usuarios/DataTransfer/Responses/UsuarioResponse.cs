namespace Filoroch.Template.Application.Usuarios.DataTransfer.Responses;

public sealed class UsuarioResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public DateTime CriadoEm { get; init; }
    public DateTime AtualizadoEm { get; init; }
}
