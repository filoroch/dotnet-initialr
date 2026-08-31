namespace Filoroch.Template.Domain.Usuarios.Filters;

public sealed class ListarUsuariosFilter
{
    public string? Username { get; set; }
    public string Email { get; set; }
    public bool? Ativo { get; set; }
}
