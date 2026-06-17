using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (!await context.Categorias.AnyAsync())
        {
            var categorias = new List<Categoria>
            {
                new("Autorização", "Pedidos de autorização"),
                new("Atendimento", "Atendimento geral"),
                new("Super e Tendência", "Assuntos de supervisão e tendências"),
                new("Reembolso", "Solicitações de reembolso"),
                new("Financeiro", "Assuntos financeiros"),
            };

            context.Categorias.AddRange(categorias);
            await context.SaveChangesAsync();
        }
    }
}
