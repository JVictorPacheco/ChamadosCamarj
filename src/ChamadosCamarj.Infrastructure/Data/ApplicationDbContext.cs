using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Chamado> Chamados => Set<Chamado>();
    public DbSet<Comentario> Comentarios => Set<Comentario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Anexo> Anexos => Set<Anexo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
