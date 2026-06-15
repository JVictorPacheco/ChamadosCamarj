using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class AnexoConfiguration : IEntityTypeConfiguration<Anexo>
{
    public void Configure(EntityTypeBuilder<Anexo> builder)
    {
        builder.ToTable("Anexos");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.NomeArquivo)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.CaminhoStorage)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.TipoArquivo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.TamanhoBytes)
            .IsRequired();
    }
}
