namespace Filoroch.Template.Application.Usuarios.DataTransfer.Requests;

public sealed class AtualizarUsuarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
