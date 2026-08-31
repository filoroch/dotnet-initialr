using NHibernate;
using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Interfaces;

namespace Filoroch.Template.Infra.Persistence;

public sealed class NHibernateUnitOfWork(ISession session) : INHibernateUnitOfWork, IAsyncDisposable
{
    private ITransaction? _transaction;

    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= session.BeginTransaction();
        return Task.CompletedTask;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;
        await session.FlushAsync(cancellationToken);
        await _transaction.CommitAsync(cancellationToken);
        _transaction.Dispose();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync(cancellationToken);
        _transaction.Dispose();
        _transaction = null;
    }

    public ValueTask DisposeAsync()
    {
        _transaction?.Dispose();
        session.Dispose();
        return ValueTask.CompletedTask;
    }
}
