using Filoroch.Template.CrossCutting.Persistence.Pagination;

namespace Filoroch.Template.CrossCutting.Persistence.Repositories.Interfaces;

public interface IQueryRepository<in TQuery, TResult>
    where TQuery : class
    where TResult : class
{
    Task<PaginatedResult<TResult>> ListarAsync(
        TQuery query,
        int? quantity,
        int? page,
        string? orderBy,
        OrderType? orderType,
        CancellationToken cancellationToken = default);
}
