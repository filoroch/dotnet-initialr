using Filoroch.Template.Application.Usuarios.DataTransfer.Requests;
using Filoroch.Template.Application.Usuarios.DataTransfer.Responses;
using Filoroch.Template.Application.Usuarios.Services;
using Filoroch.Template.CrossCutting.Persistence.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Filoroch.Template.Api.Controllers.Usuarios;

/// <summary>
/// Gerenciamento de usuarios
/// </summary>
/// <param name="_service"></param>
[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController(IUsuarioAppService _service) : ControllerBase
{
    /// <summary>
    /// Cria um novo usuario
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<UsuarioResponse>> Criar(
        CriarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        UsuarioResponse response = await _service.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Criar), new { id = response.Id }, response);
    }

    /// <summary>
    /// Lista todos os usuarios da plataforma
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<UsuarioQueryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<UsuarioQueryResponse>>> Listar(
        [FromQuery] ListarUsuariosRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.ListarAsync(request, cancellationToken));
}
