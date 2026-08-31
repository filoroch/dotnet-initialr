using Filoroch.Template.CrossCutting.Persistence.Repositories.Implementations.EFCore;
using Filoroch.Template.CrossCutting.Persistence.Pagination;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Filters;
using Filoroch.Template.Domain.Usuarios.Queries;
using Filoroch.Template.Domain.Usuarios.Repositories;
using Microsoft.EntityFrameworkCore;
using Filoroch.Template.Infra.Persistence;

namespace Filoroch.Template.Infra.Usuarios.Repositories;

public sealed class UsuarioRepository(TemplateDbContext dbContext) : EFCoreRepository<Usuario, Guid>(dbContext), IUsuarioEfRepository
{
    private readonly TemplateDbContext _dbContext = dbContext;

    public Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default)
        => _dbContext.Usuarios.AnyAsync(x => x.Email == email.Trim().ToLower(), cancellationToken);

    public Task<Usuario?> BuscarPorEmailParaAutenticacaoAsync(string email, CancellationToken cancellationToken = default)
        => _dbContext.Usuarios.SingleOrDefaultAsync(x => x.Email == email.Trim().ToLower(), cancellationToken);

    public ListarUsuariosQuery Filtrar(ListarUsuariosFilter filter)
        => new()
        {
            Username = filter.Username,
            Email = filter.Email,
            Ativo = filter.Ativo
        };

    public async Task<PaginatedResult<UsuarioQuery>> ListarAsync(
        ListarUsuariosQuery query,
        int? quantity,
        int? page,
        string? orderBy,
        OrderType? orderType,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Usuario> usuarios = _dbContext.Usuarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Username))
            usuarios = usuarios.Where(x => EF.Functions.Like(x.Username, $"%{query.Username}%"));

        if (!string.IsNullOrWhiteSpace(query.Email))
            usuarios = usuarios.Where(x => x.Email == query.Email.Trim().ToLower());

        if (query.Ativo.HasValue)
            usuarios = usuarios.Where(x => x.Ativo == query.Ativo.Value);

        var finalQuantity = quantity is > 0 and <= 100 ? quantity.Value : 20;
        var finalPage = page is > 0 ? page.Value : 1;
        var totalItems = await usuarios.CountAsync(cancellationToken);

        IQueryable<Usuario> ordered = orderType == OrderType.Descending
            ? usuarios.OrderByDescending(x => x.Username)
            : usuarios.OrderBy(x => x.Username);

        var items = await ordered
            .Skip((finalPage - 1) * finalQuantity)
            .Take(finalQuantity)
            .Select(x => new UsuarioQuery
            {
                Id = x.Id,
                Username = x.Username,
                Email = x.Email,
                Ativo = x.Ativo
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<UsuarioQuery>(items, totalItems);
    }

}
