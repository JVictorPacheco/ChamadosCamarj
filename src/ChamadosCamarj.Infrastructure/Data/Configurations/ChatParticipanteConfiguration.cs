using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class ChatParticipanteConfiguration : IEntityTypeConfiguration<ChatParticipante>
{
    public void Configure(EntityTypeBuilder<ChatParticipante> builder)
    {
        builder.ToTable("ChatParticipantes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.ConversaId)
            .IsRequired();

        builder.Property(p => p.UsuarioId)
            .IsRequired();

        builder.Property(p => p.UsuarioNome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.UltimaLeituraEm)
            .IsRequired(false);

        builder.Property(p => p.Ativo)
            .IsRequired();

        builder.HasIndex(p => new { p.ConversaId, p.UsuarioId })
            .IsUnique();

        builder.HasIndex(p => p.UsuarioId);
    }
}
