using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChamadosCamarj.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMotivoEncerramentoChamado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MotivoEncerramento",
                table: "Chamados",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoOutro",
                table: "Chamados",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotivoEncerramento",
                table: "Chamados");

            migrationBuilder.DropColumn(
                name: "MotivoOutro",
                table: "Chamados");
        }
    }
}
