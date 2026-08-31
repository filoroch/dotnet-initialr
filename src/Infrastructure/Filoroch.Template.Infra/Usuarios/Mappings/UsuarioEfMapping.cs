using Filoroch.Template.Domain.Usuarios.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Filoroch.Template.Infra.Usuarios.Mappings;

public sealed class UsuarioEfMapping : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Username).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Ativo).IsRequired();
        builder.Property(x => x.SenhaHash).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Perfil).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.CriadoEm).IsRequired();
        builder.Property(x => x.AtualizadoEm).IsRequired();
    }
}
