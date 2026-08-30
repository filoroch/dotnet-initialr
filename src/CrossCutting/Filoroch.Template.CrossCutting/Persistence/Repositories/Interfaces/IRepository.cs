using Filoroch.Template.CrossCutting.Persistence.Pagination;

namespace Filoroch.Template.CrossCutting.Persistence.Repositories.Interfaces;

public interface IRepository<TEntity, in TId>
    where TEntity : class
{
    Task<PaginatedResult<TQuery>> ListarAsync<TQuery>(
        IQueryable<TQuery> query,
        int? quantity,
        int? page,
        string? orderBy,
        OrderType? orderType,
        CancellationToken cancellationToken = default)
        where TQuery : class;

    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Remove(TEntity entity);
}
