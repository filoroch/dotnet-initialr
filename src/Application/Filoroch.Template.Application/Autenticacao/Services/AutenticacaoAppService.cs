using Filoroch.Template.Application.Autenticacao.DataTransfer.Requests;
using Filoroch.Template.Application.Autenticacao.DataTransfer.Responses;
using Filoroch.Template.CrossCutting.Exceptions;
using Filoroch.Template.Domain.Usuarios.Repositories;
using Filoroch.Template.Domain.Usuarios.Services;
using Microsoft.Extensions.Logging;

namespace Filoroch.Template.Application.Autenticacao.Services;

public sealed class AutenticacaoAppService(
    IUsuarioRepository usuarioRepository,
    IPasswordService passwordService,
    IJwtTokenService jwtTokenService,
    ILogger<AutenticacaoAppService> logger) : IAutenticacaoAppService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            throw new AutenticacaoException();

        string email = request.Email.Trim().ToLowerInvariant();
        var usuario = await usuarioRepository.BuscarPorEmailParaAutenticacaoAsync(email, cancellationToken);

        if (usuario is null || !usuario.Ativo || string.IsNullOrWhiteSpace(usuario.SenhaHash) ||
            !passwordService.Verify(request.Senha, usuario.SenhaHash))
        {
            logger.LogWarning("Falha de autenticação para usuário com e-mail informado");
            throw new AutenticacaoException();
        }

        (string token, DateTime expiresAt) = jwtTokenService.CreateToken(
            usuario.Id, usuario.Username, usuario.Email, usuario.Perfil);

        logger.LogInformation("Autenticação realizada com sucesso para o usuário {UsuarioId}", usuario.Id);

        return new LoginResponse(token, "Bearer", expiresAt, usuario.Id, usuario.Username, usuario.Email, usuario.Perfil);
    }
}
