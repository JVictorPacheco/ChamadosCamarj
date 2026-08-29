using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChamadosCamarj.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChatFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatConversas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CriadoPorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatConversas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatHistoricos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Acao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Detalhe = table.Column<string>(type: "text", nullable: true),
                    ConversaId = table.Column<Guid>(type: "uuid", nullable: true),
                    MensagemId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatHistoricos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatPresencas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UltimoHeartbeat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatPresencas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatMensagens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversaId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AutorNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Conteudo = table.Column<string>(type: "text", nullable: true),
                    ConteudoOriginal = table.Column<string>(type: "text", nullable: true),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Deletada = table.Column<bool>(type: "boolean", nullable: false),
                    EditadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RespostaParaMensagemId = table.Column<Guid>(type: "uuid", nullable: true),
                    NomeArquivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CaminhoStorage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TipoArquivo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMensagens_ChatConversas_ConversaId",
                        column: x => x.ConversaId,
                        principalTable: "ChatConversas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatParticipantes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversaId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    UltimaLeituraEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatParticipantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatParticipantes_ChatConversas_ConversaId",
                        column: x => x.ConversaId,
                        principalTable: "ChatConversas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMensagemReacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MensagemId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Emoji = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMensagemReacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMensagemReacoes_ChatMensagens_MensagemId",
                        column: x => x.MensagemId,
                        principalTable: "ChatMensagens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversas_CriadoPorId",
                table: "ChatConversas",
                column: "CriadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistoricos_ConversaId",
                table: "ChatHistoricos",
                column: "ConversaId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistoricos_DataCriacao",
                table: "ChatHistoricos",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMensagemReacoes_MensagemId_UsuarioId_Emoji",
                table: "ChatMensagemReacoes",
                columns: new[] { "MensagemId", "UsuarioId", "Emoji" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMensagens_ConversaId_DataCriacao",
                table: "ChatMensagens",
                columns: new[] { "ConversaId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatParticipantes_ConversaId_UsuarioId",
                table: "ChatParticipantes",
                columns: new[] { "ConversaId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatParticipantes_UsuarioId",
                table: "ChatParticipantes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatPresencas_UsuarioId",
                table: "ChatPresencas",
                column: "UsuarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatHistoricos");

            migrationBuilder.DropTable(
                name: "ChatMensagemReacoes");

            migrationBuilder.DropTable(
                name: "ChatParticipantes");

            migrationBuilder.DropTable(
                name: "ChatPresencas");

            migrationBuilder.DropTable(
                name: "ChatMensagens");

            migrationBuilder.DropTable(
                name: "ChatConversas");
        }
    }
}
