namespace Filoroch.Template.CrossCutting.Persistence.Repositories.Interfaces;

public interface IMongoRepository<TEntity, in TId> : IRepository<TEntity, TId>
    where TEntity : class
{
}
