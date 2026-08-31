using Filoroch.Template.CrossCutting.Persistence.Pagination;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Filters;
using Filoroch.Template.Domain.Usuarios.Queries;
using Filoroch.Template.Domain.Usuarios.Repositories;
using NHibernate;
using NHibernate.Linq;

namespace Filoroch.Template.Infra.Usuarios.Repositories;

public sealed class UsuarioNHibernateRepository(ISession session) : IUsuarioNHibernateRepository
{
    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await session.GetAsync<Usuario>(id, cancellationToken);
    public Task AddAsync(Usuario entity, CancellationToken cancellationToken = default)
        => session.SaveAsync(entity, cancellationToken);
    public Task UpdateAsync(Usuario entity, CancellationToken cancellationToken = default)
        => session.UpdateAsync(entity, cancellationToken);
    public void Remove(Usuario entity) => session.Delete(entity);
    public Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default)
        => session.Query<Usuario>().AnyAsync(x => x.Email == email.Trim().ToLower(), cancellationToken);
    public async Task<Usuario?> BuscarPorEmailParaAutenticacaoAsync(string email, CancellationToken cancellationToken = default)
        => await session.Query<Usuario>().SingleOrDefaultAsync(x => x.Email == email.Trim().ToLower(), cancellationToken);
    public ListarUsuariosQuery Filtrar(ListarUsuariosFilter filter) => new() { Username = filter.Username, Email = filter.Email, Ativo = filter.Ativo };

    public async Task<PaginatedResult<UsuarioQuery>> ListarAsync(ListarUsuariosQuery query, int? quantity, int? page, string? orderBy, OrderType? orderType, CancellationToken cancellationToken = default)
    {
        IQueryable<Usuario> source = session.Query<Usuario>();
        if (!string.IsNullOrWhiteSpace(query.Username)) source = source.Where(x => x.Username.Contains(query.Username));
        if (!string.IsNullOrWhiteSpace(query.Email)) source = source.Where(x => x.Email == query.Email.Trim().ToLower());
        if (query.Ativo.HasValue) source = source.Where(x => x.Ativo == query.Ativo.Value);
        int take = quantity is > 0 and <= 100 ? quantity.Value : 20;
        int skip = ((page is > 0 ? page.Value : 1) - 1) * take;
        int total = await source.CountAsync(cancellationToken);
        source = orderType == OrderType.Descending ? source.OrderByDescending(x => x.Username) : source.OrderBy(x => x.Username);
        var items = await source.Skip(skip).Take(take).Select(x => new UsuarioQuery { Id = x.Id, Username = x.Username, Email = x.Email, Ativo = x.Ativo }).ToListAsync(cancellationToken);
        return new PaginatedResult<UsuarioQuery>(items, total);
    }
}
