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
            new Categoria("Autorização", "Pedidos de autorização"),
            new Categoria("Atendimento", "Atendimento geral"),
            new Categoria("Super e Tendência", "Assuntos de supervisão e tendências"),
            new Categoria("Reembolso", "Solicitações de reembolso"),
            new Categoria("Financeiro", "Assuntos financeiros")
        };

        db.Categorias.AddRange(categorias);
        await db.SaveChangesAsync();
    }
}
