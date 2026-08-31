using FluentNHibernate.Mapping;
using Filoroch.Template.Domain.Usuarios.Entities;

namespace Filoroch.Template.Infra.Usuarios.Mappings;

public sealed class UsuarioNHibernateMapping : ClassMap<Usuario>
{
    public UsuarioNHibernateMapping()
    {
        Table("Usuarios");
        Id(x => x.Id).GeneratedBy.GuidComb();
        Map(x => x.Username).Length(150).Not.Nullable();
        Map(x => x.Email).Length(200).Not.Nullable();
        Map(x => x.Ativo).Not.Nullable();
        Map(x => x.SenhaHash).Length(200).Not.Nullable();
        Map(x => x.Perfil).Not.Nullable();
        Map(x => x.CriadoEm).Not.Nullable();
        Map(x => x.AtualizadoEm).Not.Nullable();
    }
}
