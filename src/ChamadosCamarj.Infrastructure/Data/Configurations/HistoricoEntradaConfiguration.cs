using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class HistoricoEntradaConfiguration : IEntityTypeConfiguration<HistoricoEntrada>
{
    public void Configure(EntityTypeBuilder<HistoricoEntrada> builder)
    {
        builder.ToTable("HistoricoEntradas");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.ChamadoId)
            .IsRequired();

        builder.Property(h => h.UsuarioNome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(h => h.UsuarioId)
            .IsRequired(false);

        builder.Property(h => h.Acao)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(h => h.DetalheAnterior)
            .IsRequired(false)
            .HasColumnType("text");

        builder.Property(h => h.DetalheNovo)
            .IsRequired(false)
            .HasColumnType("text");

        builder.Property(h => h.DataHora)
            .IsRequired();

        builder.Property(h => h.Origem)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        // Índices para performance
        builder.HasIndex(h => h.ChamadoId)
            .HasName("IX_HistoricoEntradas_ChamadoId");

        builder.HasIndex(h => h.DataHora)
            .HasName("IX_HistoricoEntradas_DataHora");
    }
}
