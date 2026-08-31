using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class ChatMensagemReacaoConfiguration : IEntityTypeConfiguration<ChatMensagemReacao>
{
    public void Configure(EntityTypeBuilder<ChatMensagemReacao> builder)
    {
        builder.ToTable("ChatMensagemReacoes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.MensagemId)
            .IsRequired();

        builder.Property(r => r.UsuarioId)
            .IsRequired();

        builder.Property(r => r.UsuarioNome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Emoji)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(r => new { r.MensagemId, r.UsuarioId, r.Emoji })
            .IsUnique();
    }
}
