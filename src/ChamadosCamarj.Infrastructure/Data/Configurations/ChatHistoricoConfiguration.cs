using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class ChatHistoricoConfiguration : IEntityTypeConfiguration<ChatHistorico>
{
    public void Configure(EntityTypeBuilder<ChatHistorico> builder)
    {
        builder.ToTable("ChatHistoricos");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        builder.Property(h => h.UsuarioId)
            .IsRequired();

        builder.Property(h => h.UsuarioNome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(h => h.Acao)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(h => h.Detalhe)
            .IsRequired(false)
            .HasColumnType("text");

        builder.Property(h => h.ConversaId)
            .IsRequired(false);

        builder.Property(h => h.MensagemId)
            .IsRequired(false);

        builder.HasIndex(h => h.ConversaId);
        builder.HasIndex(h => h.DataCriacao);
    }
}
