using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Descricao)
            .HasMaxLength(300);

        builder.HasIndex(c => c.Nome)
            .IsUnique();

        // Seed das categorias padrão
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new Categoria("Autorização", "Pedidos de autorização")
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567891"),
                DataCriacao = seedDate
            },
            new Categoria("Atendimento", "Atendimento geral")
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567892"),
                DataCriacao = seedDate
            },
            new Categoria("Super e Tendência", "Assuntos de supervisão e tendências")
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567893"),
                DataCriacao = seedDate
            },
            new Categoria("Reembolso", "Solicitações de reembolso")
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567894"),
                DataCriacao = seedDate
            },
            new Categoria("Financeiro", "Assuntos financeiros")
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567895"),
                DataCriacao = seedDate
            }
        );
    }
}
