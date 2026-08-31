namespace Filoroch.Template.Application.Autenticacao.DataTransfer.Responses;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAt,
    Guid UsuarioId,
    string Username,
    string Email,
    Filoroch.Template.Domain.Usuarios.Enums.PerfilUsuario Perfil);
