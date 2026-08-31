namespace Filoroch.Template.Domain.Usuarios.Queries;

public sealed class ListarUsuariosQuery
{
    public string? Username { get; init; }
    public string? Email { get; init; }
    public bool? Ativo { get; init; }
}
