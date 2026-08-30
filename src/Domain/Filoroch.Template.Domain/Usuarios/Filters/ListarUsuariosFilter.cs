namespace Filoroch.Template.Domain.Usuarios.Filters;

public sealed class ListarUsuariosFilter
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public bool? Ativo { get; set; }
}