using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChamadosCamarj.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddControleDeAcesso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModulosConcedidos",
                table: "UsuariosPerfil",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ModulosRetirados",
                table: "UsuariosPerfil",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AuditoriaAcessos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AlteradoPorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlteradoPorNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Item = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Anterior = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Novo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaAcessos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaAcessos_UsuarioId",
                table: "AuditoriaAcessos",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaAcessos");

            migrationBuilder.DropColumn(
                name: "ModulosConcedidos",
                table: "UsuariosPerfil");

            migrationBuilder.DropColumn(
                name: "ModulosRetirados",
                table: "UsuariosPerfil");
        }
    }
}
