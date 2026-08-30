using Filoroch.Template.CrossCutting.Persistence.Pagination;
using Filoroch.Template.CrossCutting.Persistence.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Filoroch.Template.CrossCutting.Persistence.Repositories.Implementations.EFCore;

public class EFCoreRepository<TEntity, TId>(DbContext context) : IEFCoreRepository<TEntity, TId>
    where TEntity : class
{
    protected readonly DbContext Context = context;

    public async Task<PaginatedResult<TQuery>> ListarAsync<TQuery>(
        IQueryable<TQuery> query,
        int? quantity,
        int? page,
        string? orderBy,
        OrderType? orderType,
        CancellationToken cancellationToken = default)
        where TQuery : class
    {
        var finalQuantity = quantity is > 0 and <= 100 ? quantity.Value : 20;
        var finalPage = page is > 0 ? page.Value : 1;

        query = AplicarOrdenacao(query, orderBy, orderType);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((finalPage - 1) * finalQuantity)
            .Take(finalQuantity)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TQuery>(items, totalItems);
    }

    private static IQueryable<TQuery> AplicarOrdenacao<TQuery>(
        IQueryable<TQuery> query,
        string? orderBy,
        OrderType? orderType)
        where TQuery : class
    {
        var propertyName = string.IsNullOrWhiteSpace(orderBy) ? "Id" : orderBy;

        return orderType == OrderType.Descending
            ? query.OrderByDescending(item => EF.Property<object>(item, propertyName))
            : query.OrderBy(item => EF.Property<object>(item, propertyName));
    }

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await Context.Set<TEntity>().FindAsync([id], cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await Context.Set<TEntity>().AddAsync(entity, cancellationToken);

    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }

    public void Remove(TEntity entity) => Context.Set<TEntity>().Remove(entity);
}
