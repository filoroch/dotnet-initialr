namespace Filoroch.Template.Domain.Usuarios.Commands;

public sealed record AtualizarUsuarioCommand(Guid Id, string Nome, string Email);
