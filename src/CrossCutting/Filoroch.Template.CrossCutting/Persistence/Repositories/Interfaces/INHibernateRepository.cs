namespace Filoroch.Template.CrossCutting.Persistence.Repositories.Interfaces;

public interface INHibernateRepository<TEntity, in TId> : IRepository<TEntity, TId>
    where TEntity : class
{
}
