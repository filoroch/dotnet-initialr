using System.Data.Common;
using Dapper;
using Filoroch.Template.CrossCutting.Persistence.Pagination;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Filters;
using Filoroch.Template.Domain.Usuarios.Queries;
using Filoroch.Template.Domain.Usuarios.Repositories;
using Filoroch.Template.Infra.Persistence;

namespace Filoroch.Template.Infra.Usuarios.Repositories;

public sealed class UsuarioDapperRepository(DapperConnection holder) : IUsuarioDapperRepository
{
    private readonly DbConnection connection = holder.Connection;
    private const string Columns = "Id, Username, Email, Ativo, SenhaHash, Perfil, CriadoEm, AtualizadoEm";
    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await connection.QuerySingleOrDefaultAsync<Usuario>(new CommandDefinition($"SELECT {Columns} FROM Usuarios WHERE Id = @id", new { id }, cancellationToken: cancellationToken));
    public async Task AddAsync(Usuario entity, CancellationToken cancellationToken = default)
        => await connection.ExecuteAsync(new CommandDefinition("INSERT INTO Usuarios (Id, Username, Email, Ativo, SenhaHash, Perfil, CriadoEm, AtualizadoEm) VALUES (@Id, @Username, @Email, @Ativo, @SenhaHash, @Perfil, @CriadoEm, @AtualizadoEm)", entity, cancellationToken: cancellationToken));
    public async Task UpdateAsync(Usuario entity, CancellationToken cancellationToken = default)
        => await connection.ExecuteAsync(new CommandDefinition("UPDATE Usuarios SET Username = @Username, Email = @Email, Ativo = @Ativo, SenhaHash = @SenhaHash, Perfil = @Perfil, AtualizadoEm = @AtualizadoEm WHERE Id = @Id", entity, cancellationToken: cancellationToken));
    public void Remove(Usuario entity) => connection.Execute("DELETE FROM Usuarios WHERE Id = @Id", new { entity.Id });
    public Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default)
        => connection.ExecuteScalarAsync<bool>(new CommandDefinition("SELECT CASE WHEN EXISTS (SELECT 1 FROM Usuarios WHERE Email = @email) THEN 1 ELSE 0 END", new { email = email.Trim().ToLower() }, cancellationToken: cancellationToken));
    public Task<Usuario?> BuscarPorEmailParaAutenticacaoAsync(string email, CancellationToken cancellationToken = default)
        => connection.QuerySingleOrDefaultAsync<Usuario>(new CommandDefinition($"SELECT {Columns} FROM Usuarios WHERE Email = @email", new { email = email.Trim().ToLower() }, cancellationToken: cancellationToken));
    public ListarUsuariosQuery Filtrar(ListarUsuariosFilter filter) => new() { Username = filter.Username, Email = filter.Email, Ativo = filter.Ativo };
    public async Task<PaginatedResult<UsuarioQuery>> ListarAsync(ListarUsuariosQuery query, int? quantity, int? page, string? orderBy, OrderType? orderType, CancellationToken cancellationToken = default)
    {
        int take = quantity is > 0 and <= 100 ? quantity.Value : 20;
        int skip = ((page is > 0 ? page.Value : 1) - 1) * take;
        const string filter = "FROM Usuarios WHERE (@Username IS NULL OR Username LIKE @UsernameFilter) AND (@Email IS NULL OR Email = @Email) AND (@Ativo IS NULL OR Ativo = @Ativo)";
        var parameters = new { Username = string.IsNullOrWhiteSpace(query.Username) ? null : query.Username, UsernameFilter = $"%{query.Username}%", Email = string.IsNullOrWhiteSpace(query.Email) ? null : query.Email.Trim().ToLower(), Ativo = query.Ativo, skip, take };
        string pagination = connection.GetType().Name.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) || connection.GetType().Name.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)
            ? "LIMIT @take OFFSET @skip"
            : "OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
        var grid = await connection.QueryMultipleAsync(new CommandDefinition($"SELECT COUNT(1) {filter}; SELECT {Columns} {filter} ORDER BY Username {(orderType == OrderType.Descending ? "DESC" : "ASC")} {pagination}", parameters, cancellationToken: cancellationToken));
        int total = await grid.ReadSingleAsync<int>();
        var items = (await grid.ReadAsync<UsuarioQuery>()).ToList();
        return new PaginatedResult<UsuarioQuery>(items, total);
    }
}
