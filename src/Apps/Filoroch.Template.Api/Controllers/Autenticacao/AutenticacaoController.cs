using Filoroch.Template.Application.Autenticacao.DataTransfer.Requests;
using Filoroch.Template.Application.Autenticacao.DataTransfer.Responses;
using Filoroch.Template.Application.Autenticacao.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Filoroch.Template.Api.Controllers.Autenticacao;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AutenticacaoController(IAutenticacaoAppService autenticacaoAppService) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<LoginResponse> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
        => autenticacaoAppService.LoginAsync(request, cancellationToken);
}
