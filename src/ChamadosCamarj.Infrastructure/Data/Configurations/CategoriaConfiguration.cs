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
        builder.HasData(
            new { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567891"), Nome = "Autorização", Descricao = "Pedidos de autorização", Ativa = true, DataCriacao = DateTime.UtcNow, DataAtualizacao = (DateTime?)null },
            new { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567892"), Nome = "Atendimento", Descricao = "Atendimento geral", Ativa = true, DataCriacao = DateTime.UtcNow, DataAtualizacao = (DateTime?)null },
            new { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567893"), Nome = "Super e Tendência", Descricao = "Assuntos de supervisão e tendências", Ativa = true, DataCriacao = DateTime.UtcNow, DataAtualizacao = (DateTime?)null },
            new { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567894"), Nome = "Reembolso", Descricao = "Solicitações de reembolso", Ativa = true, DataCriacao = DateTime.UtcNow, DataAtualizacao = (DateTime?)null },
            new { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567895"), Nome = "Financeiro", Descricao = "Assuntos financeiros", Ativa = true, DataCriacao = DateTime.UtcNow, DataAtualizacao = (DateTime?)null }
        );
    }
}
