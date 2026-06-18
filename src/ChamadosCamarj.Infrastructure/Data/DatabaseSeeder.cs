using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (db.Categorias.Any())
            return;

        var categorias = new List<Categoria>
        {
            new Categoria("Autorização", "Pedidos de autorização")           { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567891") },
            new Categoria("Atendimento", "Atendimento geral")                { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567892") },
            new Categoria("Super e Tendência", "Assuntos de supervisão e tendências") { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567893") },
            new Categoria("Reembolso", "Solicitações de reembolso")          { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567894") },
            new Categoria("Financeiro", "Assuntos financeiros")              { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567895") }
        };

        db.Categorias.AddRange(categorias);
        await db.SaveChangesAsync();
    }
}
