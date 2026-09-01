using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChamadosCamarj.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChatPerfilUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChatPerfil",
                table: "UsuariosPerfil",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "SemAcesso");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatPerfil",
                table: "UsuariosPerfil");
        }
    }
}
