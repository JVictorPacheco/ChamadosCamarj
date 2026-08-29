using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class ChatConversaConfiguration : IEntityTypeConfiguration<ChatConversa>
{
    public void Configure(EntityTypeBuilder<ChatConversa> builder)
    {
        builder.ToTable("ChatConversas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.Nome)
            .IsRequired(false)
            .HasMaxLength(150);

        builder.Property(c => c.CriadoPorId)
            .IsRequired();

        builder.Property(c => c.Ativa)
            .IsRequired();

        builder.HasMany(c => c.Participantes)
            .WithOne(p => p.Conversa)
            .HasForeignKey(p => p.ConversaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Mensagens)
            .WithOne(m => m.Conversa)
            .HasForeignKey(m => m.ConversaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.CriadoPorId);
    }
}
