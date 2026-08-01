using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChamadosCamarj.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnviadoPorAnexo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EnviadoPorId",
                table: "Anexos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnviadoPorNome",
                table: "Anexos",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnviadoPorId",
                table: "Anexos");

            migrationBuilder.DropColumn(
                name: "EnviadoPorNome",
                table: "Anexos");
        }
    }
}
