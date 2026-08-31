using System.Data.Common;

namespace Filoroch.Template.Infra.Persistence;

public sealed class DapperConnection(DbConnection connection) : IAsyncDisposable
{
    public DbConnection Connection { get; } = connection;
    public ValueTask DisposeAsync() => Connection.DisposeAsync();
}
