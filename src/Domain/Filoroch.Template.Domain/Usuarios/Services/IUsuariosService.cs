using Filoroch.Template.Domain.Usuarios.Commands;
using Filoroch.Template.Domain.Usuarios.Entities;

namespace Filoroch.Template.Domain.Usuarios.Services;

public interface IUsuariosService
{
    Task<Usuario> CriarAsync(CriarUsuarioCommand command, CancellationToken cancellationToken = default);
    Task AtualizarAsync(AtualizarUsuarioCommand command, CancellationToken cancellationToken = default);
}
