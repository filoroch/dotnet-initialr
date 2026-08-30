namespace Filoroch.Template.CrossCutting.Persistence.Repositories.Interfaces;

public interface IEFCoreRepository<TEntity, in TId> : IRepository<TEntity, TId>
    where TEntity : class
{
}
