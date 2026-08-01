using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChamadosCamarj.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGrupo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GrupoId",
                table: "UsuariosPerfil",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPerfil_GrupoId",
                table: "UsuariosPerfil",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_Nome",
                table: "Grupos",
                column: "Nome",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UsuariosPerfil_Grupos_GrupoId",
                table: "UsuariosPerfil",
                column: "GrupoId",
                principalTable: "Grupos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuariosPerfil_Grupos_GrupoId",
                table: "UsuariosPerfil");

            migrationBuilder.DropTable(
                name: "Grupos");

            migrationBuilder.DropIndex(
                name: "IX_UsuariosPerfil_GrupoId",
                table: "UsuariosPerfil");

            migrationBuilder.DropColumn(
                name: "GrupoId",
                table: "UsuariosPerfil");
        }
    }
}
