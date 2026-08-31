using Filoroch.Template.Application.Usuarios.DataTransfer.Requests;
using Filoroch.Template.Application.Usuarios.DataTransfer.Responses;
using Filoroch.Template.CrossCutting.Persistence.Pagination;
using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Interfaces;
using Filoroch.Template.Domain.Usuarios.Commands;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Filters;
using Filoroch.Template.Domain.Usuarios.Queries;
using Filoroch.Template.Domain.Usuarios.Repositories;
using Filoroch.Template.Domain.Usuarios.Services;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Filoroch.Template.Application.Usuarios.Services;

public sealed class UsuarioAppService(
    IUsuariosService _service,
    ILogger<UsuarioAppService> _logger,
    IUsuarioRepository _repository,
    IUnitOfWork _unitOfWork) : IUsuarioAppService
{
    public async Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken = default)
    {
    
        try
        {
            CriarUsuarioCommand command = request.Adapt<CriarUsuarioCommand>();

            await _unitOfWork.BeginAsync(cancellationToken);

            Usuario usuario = await _service.CriarAsync(
                command, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            UsuarioResponse response = usuario.Adapt<UsuarioResponse>();

            return response;
        }
        catch
        {
            _logger.LogError("Erro ao criar usuário, request: {request}", request);
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<PaginatedResult<UsuarioQueryResponse>> ListarAsync(ListarUsuariosRequest request, CancellationToken cancellationToken = default)
    {
        ListarUsuariosFilter filter = request.Adapt<ListarUsuariosFilter>();
        ListarUsuariosQuery query = _repository.Filtrar(filter);

        PaginatedResult<UsuarioQuery> result = await _repository.ListarAsync(
            query, request.Quantity, request.Page,
            request.OrderBy, request.OrderType, cancellationToken);

        return new PaginatedResult<UsuarioQueryResponse>(
            result.Items.Adapt<IReadOnlyList<UsuarioQueryResponse>>(), result.TotalItems);
    }
}
