namespace Filoroch.Template.CrossCutting.Persistence.Repositories.Interfaces;

public interface IRepository<TEntity, in TId>
    where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Remove(TEntity entity);
}
