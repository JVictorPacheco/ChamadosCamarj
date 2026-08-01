using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class GrupoConfiguration : IEntityTypeConfiguration<Grupo>
{
    public void Configure(EntityTypeBuilder<Grupo> builder)
    {
        builder.ToTable("Grupos");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .ValueGeneratedNever();

        builder.Property(g => g.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(g => g.Descricao)
            .HasMaxLength(300);

        builder.HasIndex(g => g.Nome)
            .IsUnique();
    }
}
