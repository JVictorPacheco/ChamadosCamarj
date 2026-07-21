using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChamadosCamarj.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHistoricoEntrada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoricoEntradas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChamadoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Acao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DetalheAnterior = table.Column<string>(type: "text", nullable: true),
                    DetalheNovo = table.Column<string>(type: "text", nullable: true),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoEntradas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoEntradas_ChamadoId",
                table: "HistoricoEntradas",
                column: "ChamadoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoEntradas_DataHora",
                table: "HistoricoEntradas",
                column: "DataHora");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricoEntradas");
        }
    }
}
