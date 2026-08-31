using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class ChatPresencaConfiguration : IEntityTypeConfiguration<ChatPresenca>
{
    public void Configure(EntityTypeBuilder<ChatPresenca> builder)
    {
        builder.ToTable("ChatPresencas");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.UsuarioId)
            .IsRequired();

        builder.Property(p => p.UsuarioNome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.UltimoHeartbeat)
            .IsRequired();

        builder.HasIndex(p => p.UsuarioId)
            .IsUnique();
    }
}
