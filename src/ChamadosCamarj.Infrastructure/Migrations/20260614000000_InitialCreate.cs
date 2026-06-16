using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace ChamadosCamarj.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chamados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Prioridade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SolicitanteNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SolicitanteEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ResponsavelId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponsavelNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataLimite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Origem = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chamados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chamados_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Anexos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChamadoId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeArquivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CaminhoStorage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoArquivo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anexos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anexos_Chamados_ChamadoId",
                        column: x => x.ChamadoId,
                        principalTable: "Chamados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comentarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChamadoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Autor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Conteudo = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comentarios_Chamados_ChamadoId",
                        column: x => x.ChamadoId,
                        principalTable: "Chamados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Índices
            migrationBuilder.CreateIndex("IX_Chamados_Status", "Chamados", "Status");
            migrationBuilder.CreateIndex("IX_Chamados_Prioridade", "Chamados", "Prioridade");
            migrationBuilder.CreateIndex("IX_Chamados_SolicitanteEmail", "Chamados", "SolicitanteEmail");
            migrationBuilder.CreateIndex("IX_Chamados_ResponsavelId", "Chamados", "ResponsavelId");
            migrationBuilder.CreateIndex("IX_Chamados_DataLimite", "Chamados", "DataLimite");
            migrationBuilder.CreateIndex("IX_Chamados_CategoriaId", "Chamados", "CategoriaId");
            migrationBuilder.CreateIndex("IX_Anexos_ChamadoId", "Anexos", "ChamadoId");
            migrationBuilder.CreateIndex("IX_Comentarios_ChamadoId", "Comentarios", "ChamadoId");
            migrationBuilder.CreateIndex("IX_Categorias_Nome", "Categorias", "Nome", unique: true);

            // Seed categorias padrão
            migrationBuilder.InsertData("Categorias", new[] { "Id", "Nome", "Descricao", "Ativa", "DataCriacao" },
                new object[,]
                {
                    { Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567891"), "Autorização", "Pedidos de autorização", true, DateTime.UtcNow },
                    { Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567892"), "Atendimento", "Atendimento geral", true, DateTime.UtcNow },
                    { Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567893"), "Super e Tendência", "Assuntos de supervisão e tendências", true, DateTime.UtcNow },
                    { Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567894"), "Reembolso", "Solicitações de reembolso", true, DateTime.UtcNow },
                    { Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567895"), "Financeiro", "Assuntos financeiros", true, DateTime.UtcNow }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Anexos");
            migrationBuilder.DropTable(name: "Comentarios");
            migrationBuilder.DropTable(name: "Chamados");
            migrationBuilder.DropTable(name: "Categorias");
        }
    }
}
