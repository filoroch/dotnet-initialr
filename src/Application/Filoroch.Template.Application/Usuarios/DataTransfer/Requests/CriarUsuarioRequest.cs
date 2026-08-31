namespace Filoroch.Template.Application.Usuarios.DataTransfer.Requests;

public sealed record CriarUsuarioRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
