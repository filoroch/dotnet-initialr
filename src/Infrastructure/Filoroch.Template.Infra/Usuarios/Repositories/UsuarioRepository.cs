using Filoroch.Template.CrossCutting.Persistence.Repositories.Implementations.EFCore;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Filters;
using Filoroch.Template.Domain.Usuarios.Queries;
using Filoroch.Template.Domain.Usuarios.Repositories;
using Microsoft.EntityFrameworkCore;
using Filoroch.Template.Infra.Persistence;

namespace Filoroch.Template.Infra.Usuarios.Repositories;

public sealed class UsuarioRepository(TemplateDbContext dbContext) : EFCoreRepository<Usuario, Guid>(dbContext), IUsuarioRepository
{
    private readonly TemplateDbContext _dbContext = dbContext;

    public Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default)
        => _dbContext.Usuarios.AnyAsync(x => x.Email == email.Trim().ToLower(), cancellationToken);

    public IQueryable<UsuarioQuery> Filtrar(ListarUsuariosFilter filter)
    {
        IQueryable<Usuario> query = _dbContext.Usuarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Nome))
            query = query.Where(x => EF.Functions.Like(x.Nome, $"%{filter.Nome}%"));

        if (!string.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(x => x.Email == filter.Email.Trim().ToLower());

        if (filter.Ativo.HasValue)
            query = query.Where(x => x.Ativo == filter.Ativo.Value);

        return query.Select(x => new UsuarioQuery
        {
            Id = x.Id,
            Nome = x.Nome,
            Email = x.Email,
            Ativo = x.Ativo
        });
    }

}
