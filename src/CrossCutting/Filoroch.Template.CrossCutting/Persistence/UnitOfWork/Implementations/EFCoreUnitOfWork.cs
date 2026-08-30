using Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Filoroch.Template.CrossCutting.Persistence.UnitOfWork.Implementations;

public sealed class EFCoreUnitOfWork(DbContext context) : IUnitOfWork, IAsyncDisposable
{
    private readonly DbContext _context = context;
    private IDbContextTransaction? _transaction;

    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);

        if (_transaction is not null)
            await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            await _transaction.RollbackAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            await _transaction.DisposeAsync();
    }
}
