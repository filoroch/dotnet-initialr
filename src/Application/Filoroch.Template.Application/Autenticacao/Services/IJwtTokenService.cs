namespace Filoroch.Template.Application.Autenticacao.Services;

using Filoroch.Template.Domain.Usuarios.Enums;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(Guid usuarioId, string username, string email, PerfilUsuario perfil);
}
