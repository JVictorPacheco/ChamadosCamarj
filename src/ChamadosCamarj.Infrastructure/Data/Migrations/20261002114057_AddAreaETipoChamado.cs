using System;
using Microsoft.EntityFrameworkCore.Migrations;
using ChamadosCamarj.Infrastructure.Data;

#nullable disable

namespace ChamadosCamarj.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Área (= grupo) e Tipo do chamado. Compatível com a versão anterior ainda em produção até o
    /// deploy (dev = prod): colunas novas opcionais, CategoriaId vira opcional, nada é apagado.
    /// Spec/design: .specs/features/area-e-tipo-do-chamado (design §3).
    /// </summary>
    public partial class AddAreaETipoChamado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CategoriaId",
                table: "Chamados",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "AreaId",
                table: "Chamados",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TipoId",
                table: "Chamados",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TiposChamado",
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
                    table.PrimaryKey("PK_TiposChamado", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chamados_AreaId",
                table: "Chamados",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Chamados_TipoId",
                table: "Chamados",
                column: "TipoId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposChamado_Nome",
                table: "TiposChamado",
                column: "Nome",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chamados_Grupos_AreaId",
                table: "Chamados",
                column: "AreaId",
                principalTable: "Grupos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chamados_TiposChamado_TipoId",
                table: "Chamados",
                column: "TipoId",
                principalTable: "TiposChamado",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Tipos padrão (Guids fixos; "Não classificado" inativo, só para chamados antigos).
            foreach (var (id, nome, descricao, ativo) in AreaETipoPadrao.Tipos)
            {
                migrationBuilder.Sql(
                    $"INSERT INTO \"TiposChamado\" (\"Id\", \"Nome\", \"Descricao\", \"Ativo\", \"DataCriacao\") " +
                    $"VALUES ('{id}', '{nome}', '{descricao}', {(ativo ? "TRUE" : "FALSE")}, now()) ON CONFLICT DO NOTHING;");
            }

            // Uma área (grupo) para cada categoria que ainda não tem; depois área e tipo dos chamados.
            migrationBuilder.Sql(AreaETipoPadrao.SqlCriarAreasDasCategorias());
            migrationBuilder.Sql(AreaETipoPadrao.SqlPreencherAreaETipo());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chamados_Grupos_AreaId",
                table: "Chamados");

            migrationBuilder.DropForeignKey(
                name: "FK_Chamados_TiposChamado_TipoId",
                table: "Chamados");

            migrationBuilder.DropTable(
                name: "TiposChamado");

            migrationBuilder.DropIndex(
                name: "IX_Chamados_AreaId",
                table: "Chamados");

            migrationBuilder.DropIndex(
                name: "IX_Chamados_TipoId",
                table: "Chamados");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "Chamados");

            migrationBuilder.DropColumn(
                name: "TipoId",
                table: "Chamados");

            // CategoriaId continua opcional de propósito: chamados abertos pela versão nova não têm
            // categoria, e torná-la obrigatória de novo falharia.

        }
    }
}
