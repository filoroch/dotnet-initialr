namespace Filoroch.Template.Domain.Usuarios.Commands;

public sealed record AtualizarUsuarioCommand(Guid Id, string Username, string Email, string? Senha = null);
