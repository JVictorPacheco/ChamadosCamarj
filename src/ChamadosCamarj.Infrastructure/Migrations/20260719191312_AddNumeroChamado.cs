using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChamadosCamarj.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNumeroChamado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sequence criada antes da coluna existir e ligada a ela via OWNED BY —
            // assim ela morre sozinha se a coluna for removida (ver Down()).
            migrationBuilder.Sql(@"CREATE SEQUENCE ""ChamadosNumeroSeq"";");

            migrationBuilder.AddColumn<int>(
                name: "Numero",
                table: "Chamados",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"ALTER SEQUENCE ""ChamadosNumeroSeq"" OWNED BY ""Chamados"".""Numero"";");

            // Backfill cronológico (por DataCriacao, não por Id — Guid não tem ordem temporal)
            // dos chamados que já existiam antes desta migration.
            migrationBuilder.Sql(@"
                UPDATE ""Chamados"" c
                SET ""Numero"" = sub.rn
                FROM (
                    SELECT ""Id"", ROW_NUMBER() OVER (ORDER BY ""DataCriacao"") AS rn
                    FROM ""Chamados""
                ) sub
                WHERE c.""Id"" = sub.""Id"";
            ");

            // A sequence continua do maior número já atribuído no backfill acima.
            migrationBuilder.Sql(@"SELECT setval('""ChamadosNumeroSeq""', COALESCE((SELECT MAX(""Numero"") FROM ""Chamados""), 0));");

            migrationBuilder.Sql(@"ALTER TABLE ""Chamados"" ALTER COLUMN ""Numero"" SET DEFAULT nextval('""ChamadosNumeroSeq""');");
            migrationBuilder.Sql(@"ALTER TABLE ""Chamados"" ALTER COLUMN ""Numero"" SET NOT NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Chamados_Numero",
                table: "Chamados",
                column: "Numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chamados_Numero",
                table: "Chamados");

            // A sequence "ChamadosNumeroSeq" é removida automaticamente aqui (OWNED BY).
            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Chamados");
        }
    }
}
