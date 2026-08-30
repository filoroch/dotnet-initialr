using Filoroch.Template.Application.Usuarios.DataTransfer.Requests;
using Filoroch.Template.Application.Usuarios.DataTransfer.Responses;
using Filoroch.Template.CrossCutting.Persistence.Pagination;

namespace Filoroch.Template.Application.Usuarios.Services;

public interface IUsuarioAppService
{
    Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken = default);
    Task<PaginatedResult<UsuarioQueryResponse>> ListarAsync(ListarUsuariosRequest request, CancellationToken cancellationToken = default);
}
