namespace Filoroch.Template.Application.Usuarios.DataTransfer.Requests;

public sealed class DesativarUsuariosInativosRequest
{
    public int DiasInatividade { get; set; } = 90;
}
