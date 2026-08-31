using Filoroch.Template.CrossCutting.Persistence.Repositories.Interfaces;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Filters;
using Filoroch.Template.Domain.Usuarios.Queries;

namespace Filoroch.Template.Domain.Usuarios.Repositories;

public interface IUsuarioRepository : IRepository<Usuario, Guid>, IQueryRepository<ListarUsuariosQuery, UsuarioQuery>
{
    Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Usuario?> BuscarPorEmailParaAutenticacaoAsync(string email, CancellationToken cancellationToken = default);

    ListarUsuariosQuery Filtrar(ListarUsuariosFilter filter);

}
