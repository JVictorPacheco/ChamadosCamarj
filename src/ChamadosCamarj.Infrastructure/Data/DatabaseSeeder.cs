using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Infrastructure.Data;

namespace ChamadosCamarj.Infrastructure.Data;

public static class DatabaseSeeder
{
    // IDs fixos para as categorias padrão — use-os no POST
    public static readonly Guid CategoriaAutorizacao = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567891");
    public static readonly Guid CategoriaAtendimento = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567892");
    public static readonly Guid CategoriaSuperTendencia = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567893");
    public static readonly Guid CategoriaReembolso = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567894");
    public static readonly Guid CategoriaFinanceiro = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567895");

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (!await context.Categorias.AnyAsync())
        {
            var categorias = new List<Categoria>
            {
                new("Autorização", "Pedidos de autorização") { Id = CategoriaAutorizacao },
                new("Atendimento", "Atendimento geral") { Id = CategoriaAtendimento },
                new("Super e Tendência", "Assuntos de supervisão e tendências") { Id = CategoriaSuperTendencia },
                new("Reembolso", "Solicitações de reembolso") { Id = CategoriaReembolso },
                new("Financeiro", "Assuntos financeiros") { Id = CategoriaFinanceiro },
            };

            context.Categorias.AddRange(categorias);
            await context.SaveChangesAsync();
        }
    }
}
