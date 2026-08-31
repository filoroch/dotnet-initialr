using System.Data.Common;
using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Interfaces;

namespace Filoroch.Template.Infra.Persistence;

public sealed class DapperUnitOfWork(DapperConnection holder) : IDapperUnitOfWork, IAsyncDisposable
{
    private readonly DbConnection connection = holder.Connection;
    private DbTransaction? _transaction;

    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null) return;
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);
        _transaction = await connection.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;
        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public ValueTask DisposeAsync() => connection.DisposeAsync();
}
