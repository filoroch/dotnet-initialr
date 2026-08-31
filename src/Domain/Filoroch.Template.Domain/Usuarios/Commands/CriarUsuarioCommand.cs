namespace Filoroch.Template.Domain.Usuarios.Commands;

public sealed record CriarUsuarioCommand(string Username, string Email, string Senha);
