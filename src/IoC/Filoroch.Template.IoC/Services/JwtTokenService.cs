using Filoroch.Template.Application.Autenticacao.Services;
using Filoroch.Template.IoC.Settings;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Filoroch.Template.Domain.Usuarios.Enums;

namespace Filoroch.Template.IoC.Services;

public sealed class JwtTokenService(JwtSettings settings) : IJwtTokenService
{
    public (string Token, DateTime ExpiresAt) CreateToken(Guid usuarioId, string username, string email, PerfilUsuario perfil)
    {
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(settings.AccessTokenMinutes);
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(settings.SigningKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, perfil.ToString())
        ];

        JwtSecurityToken token = new(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
