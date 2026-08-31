using Filoroch.Template.CrossCutting.Persistence.Repositories.Interfaces;
using Filoroch.Template.Domain.Usuarios.Entities;

namespace Filoroch.Template.Domain.Usuarios.Repositories;

public interface IUsuarioDapperRepository : IUsuarioRepository, IDapperRepository<Usuario, Guid>;
