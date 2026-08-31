using Filoroch.Template.CrossCutting.Persistence.Pagination;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Filters;
using Filoroch.Template.Domain.Usuarios.Queries;
using Filoroch.Template.Domain.Usuarios.Repositories;
using MongoDB.Driver;

namespace Filoroch.Template.Infra.Usuarios.Repositories;

public sealed class UsuarioMongoRepository(IMongoCollection<Usuario> collection, IClientSessionHandle? session = null) : IUsuarioMongoRepository
{
    private readonly IClientSessionHandle? _session = session;
    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await ( _session is null ? collection.Find(x => x.Id == id) : collection.Find(_session, x => x.Id == id)).FirstOrDefaultAsync(cancellationToken);
    public Task AddAsync(Usuario entity, CancellationToken cancellationToken = default)
        => _session is null ? collection.InsertOneAsync(entity, cancellationToken: cancellationToken) : collection.InsertOneAsync(_session, entity, cancellationToken: cancellationToken);
    public Task UpdateAsync(Usuario entity, CancellationToken cancellationToken = default)
        => (_session is null ? collection.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: cancellationToken) : collection.ReplaceOneAsync(_session, x => x.Id == entity.Id, entity, cancellationToken: cancellationToken)).ContinueWith(_ => { }, cancellationToken);
    public void Remove(Usuario entity)
    {
        if (_session is null) collection.DeleteOne(x => x.Id == entity.Id);
        else collection.DeleteOne(_session, x => x.Id == entity.Id);
    }
    public Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default) => (_session is null ? collection.Find(x => x.Email == email.Trim().ToLower()) : collection.Find(_session, x => x.Email == email.Trim().ToLower())).AnyAsync(cancellationToken);
    public async Task<Usuario?> BuscarPorEmailParaAutenticacaoAsync(string email, CancellationToken cancellationToken = default) => await (_session is null ? collection.Find(x => x.Email == email.Trim().ToLower()) : collection.Find(_session, x => x.Email == email.Trim().ToLower())).FirstOrDefaultAsync(cancellationToken);
    public ListarUsuariosQuery Filtrar(ListarUsuariosFilter filter) => new() { Username = filter.Username, Email = filter.Email, Ativo = filter.Ativo };
    public async Task<PaginatedResult<UsuarioQuery>> ListarAsync(ListarUsuariosQuery query, int? quantity, int? page, string? orderBy, OrderType? orderType, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Usuario>.Filter.Empty;
        if (!string.IsNullOrWhiteSpace(query.Username)) filter &= Builders<Usuario>.Filter.Regex(x => x.Username, new MongoDB.Bson.BsonRegularExpression(query.Username, "i"));
        if (!string.IsNullOrWhiteSpace(query.Email)) filter &= Builders<Usuario>.Filter.Eq(x => x.Email, query.Email.Trim().ToLower());
        if (query.Ativo.HasValue) filter &= Builders<Usuario>.Filter.Eq(x => x.Ativo, query.Ativo.Value);
        int take = quantity is > 0 and <= 100 ? quantity.Value : 20;
        int skip = ((page is > 0 ? page.Value : 1) - 1) * take;
        var finder = _session is null ? collection.Find(filter) : collection.Find(_session, filter);
        long total = await finder.CountDocumentsAsync(cancellationToken);
        var items = await finder.Sort(orderType == OrderType.Descending ? Builders<Usuario>.Sort.Descending(x => x.Username) : Builders<Usuario>.Sort.Ascending(x => x.Username)).Skip(skip).Limit(take).Project(x => new UsuarioQuery { Id = x.Id, Username = x.Username, Email = x.Email, Ativo = x.Ativo }).ToListAsync(cancellationToken);
        return new PaginatedResult<UsuarioQuery>(items, checked((int)total));
    }
}
