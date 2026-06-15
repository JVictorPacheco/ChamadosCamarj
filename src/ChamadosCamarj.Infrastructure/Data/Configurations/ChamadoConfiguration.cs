using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class ChamadoConfiguration : IEntityTypeConfiguration<Chamado>
{
    public void Configure(EntityTypeBuilder<Chamado> builder)
    {
        builder.ToTable("Chamados");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Titulo)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Descricao)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.Prioridade)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.SolicitanteNome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.SolicitanteEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Origem)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.DataCriacao)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Índices
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.Prioridade);
        builder.HasIndex(c => c.SolicitanteEmail);
        builder.HasIndex(c => c.ResponsavelId);
        builder.HasIndex(c => c.DataLimite);

        // Relacionamentos
        builder.HasOne(c => c.Categoria)
            .WithMany(cat => cat.Chamados)
            .HasForeignKey(c => c.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Comentarios)
            .WithOne(com => com.Chamado)
            .HasForeignKey(com => com.ChamadoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Anexos)
            .WithOne(a => a.Chamado)
            .HasForeignKey(a => a.ChamadoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
