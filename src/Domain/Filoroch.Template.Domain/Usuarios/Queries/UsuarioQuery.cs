namespace Filoroch.Template.Domain.Usuarios.Queries;

public sealed class UsuarioQuery
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool Ativo { get; init; }
}
