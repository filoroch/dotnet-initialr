using FluentNHibernate.Mapping;
using Filoroch.Template.Domain.Usuarios.Entities;

namespace Filoroch.Template.Infra.Usuarios.Mappings;

public sealed class UsuarioNHibernateMapping : ClassMap<Usuario>
{
    public UsuarioNHibernateMapping()
    {
        Table("Usuarios");
        Id(usuario => usuario.Id).GeneratedBy.GuidComb();
    }
}
