using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class ComentarioConfiguration : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> builder)
    {
        builder.ToTable("Comentarios");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Autor)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Conteudo)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(c => c.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}
