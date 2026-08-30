using Filoroch.Template.Domain.Usuarios.Entities;
using Microsoft.EntityFrameworkCore;

namespace Filoroch.Template.Infra.Persistence;

public sealed class TemplateDbContext(DbContextOptions<TemplateDbContext> options) : DbContext(options)
{

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TemplateDbContext).Assembly);
    }
}
