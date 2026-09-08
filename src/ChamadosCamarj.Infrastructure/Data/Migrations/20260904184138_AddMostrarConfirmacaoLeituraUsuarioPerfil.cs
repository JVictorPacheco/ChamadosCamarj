using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChamadosCamarj.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMostrarConfirmacaoLeituraUsuarioPerfil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MostrarConfirmacaoLeitura",
                table: "UsuariosPerfil",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MostrarConfirmacaoLeitura",
                table: "UsuariosPerfil");
        }
    }
}
