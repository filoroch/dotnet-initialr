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

    public async Task DesativarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginAsync(cancellationToken);

            await _service.DesativarAsync(id, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            _logger.LogError("Erro ao desativar usuário {UsuarioId}", id);
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<int> DesativarInativosAsync(DesativarUsuariosInativosRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DiasInatividade <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.DiasInatividade), "Dias de inatividade deve ser maior que zero.");

        DateTime limite = DateTime.UtcNow.AddDays(-request.DiasInatividade);
        int desativados = 0;

        try
        {
            await _unitOfWork.BeginAsync(cancellationToken);

            const int quantidade = 100;
            int pagina = 1;

            while (true)
            {
                ListarUsuariosFilter filter = new() { Ativo = true };
                ListarUsuariosQuery query = _repository.Filtrar(filter);

                PaginatedResult<UsuarioQuery> result = await _repository.ListarAsync(
                    query, quantidade, pagina, null, null, cancellationToken);

                if (result.Items.Count == 0)
                    break;

                foreach (UsuarioQuery item in result.Items)
                {
                    Usuario? entidade = await _repository.GetByIdAsync(item.Id, cancellationToken);

                    if (entidade is null || !entidade.Ativo || entidade.AtualizadoEm > limite)
                        continue;

                    await _service.DesativarAsync(entidade.Id, cancellationToken);
                    desativados++;
                }

                if (result.Items.Count < quantidade)
                    break;

                pagina++;
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Desativação de inativos concluída. Dias: {Dias}, Desativados: {Desativados}",
                request.DiasInatividade, desativados);

            return desativados;
        }
        catch
        {
            _logger.LogError("Erro ao desativar usuários inativos. Dias: {Dias}", request.DiasInatividade);
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
