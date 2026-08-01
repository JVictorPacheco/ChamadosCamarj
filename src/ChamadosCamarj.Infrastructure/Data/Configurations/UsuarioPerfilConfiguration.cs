using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChamadosCamarj.Domain.Entities;

namespace ChamadosCamarj.Infrastructure.Data.Configurations;

public class UsuarioPerfilConfiguration : IEntityTypeConfiguration<UsuarioPerfil>
{
    public void Configure(EntityTypeBuilder<UsuarioPerfil> builder)
    {
        builder.ToTable("UsuariosPerfil");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Perfil)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.SenhaHash)
            .HasMaxLength(500);

        builder.Property(u => u.GrupoId)
            .IsRequired(false);

        builder.HasOne(u => u.Grupo)
            .WithMany(g => g.Usuarios)
            .HasForeignKey(u => u.GrupoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(u => u.Email)
            .IsUnique();
    }
}
