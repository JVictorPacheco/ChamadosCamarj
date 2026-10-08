using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class AuditoriaAcessoConfiguration : IEntityTypeConfiguration<AuditoriaAcesso>
{
    public void Configure(EntityTypeBuilder<AuditoriaAcesso> builder)
    {
        builder.ToTable("AuditoriaAcessos");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.UsuarioId)
            .IsRequired();

        builder.Property(a => a.UsuarioNome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.AlteradoPorId)
            .IsRequired();

        builder.Property(a => a.AlteradoPorNome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.Item)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(a => a.Anterior)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(a => a.Novo)
            .IsRequired()
            .HasMaxLength(60);

        builder.HasIndex(a => a.UsuarioId);
    }
}
