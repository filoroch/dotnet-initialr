using MongoDB.Driver;
using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Interfaces;

namespace Filoroch.Template.Infra.Persistence;

public sealed class MongoUnitOfWork(IClientSessionHandle session) : IMongoUnitOfWork, IAsyncDisposable
{
    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        if (!session.IsInTransaction) session.StartTransaction();
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => session.CommitTransactionAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default)
        => session.AbortTransactionAsync(cancellationToken);

    public ValueTask DisposeAsync()
    {
        session.Dispose();
        return ValueTask.CompletedTask;
    }
}
