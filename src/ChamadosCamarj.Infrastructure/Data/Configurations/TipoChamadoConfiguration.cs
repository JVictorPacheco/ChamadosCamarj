using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class TipoChamadoConfiguration : IEntityTypeConfiguration<TipoChamado>
{
    public void Configure(EntityTypeBuilder<TipoChamado> builder)
    {
        builder.ToTable("TiposChamado");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Descricao)
            .HasMaxLength(300);

        builder.HasIndex(c => c.Nome)
            .IsUnique();
    }
}
