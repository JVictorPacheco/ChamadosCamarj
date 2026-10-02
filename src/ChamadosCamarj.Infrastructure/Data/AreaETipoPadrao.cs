namespace ChamadosCamarj.Infrastructure.Data;

/// <summary>
/// Valores fixos da feature área e tipo do chamado (spec .specs/features/area-e-tipo-do-chamado)
/// e o SQL que preenche área e tipo dos chamados que ainda não têm. O mesmo SQL roda na migration
/// AddAreaETipoChamado e a cada subida da API (DatabaseSeeder): assim os chamados abertos pela versão
/// anterior, entre a migration e o deploy, também recebem área e tipo (design §3).
/// </summary>
public static class AreaETipoPadrao
{
    public static readonly Guid TipoIncidente = Guid.Parse("c1000000-0000-0000-0000-000000000001");
    public static readonly Guid TipoDuvida = Guid.Parse("c1000000-0000-0000-0000-000000000002");
    public static readonly Guid TipoSolicitacao = Guid.Parse("c1000000-0000-0000-0000-000000000003");
    public static readonly Guid TipoCustomizacao = Guid.Parse("c1000000-0000-0000-0000-000000000004");
    public static readonly Guid TipoMelhoria = Guid.Parse("c1000000-0000-0000-0000-000000000005");
    /// <summary>Só para chamados anteriores à mudança; inativo, não aparece na abertura.</summary>
    public static readonly Guid TipoNaoClassificado = Guid.Parse("c1000000-0000-0000-0000-000000000099");

    public static readonly Guid AreaFinanceiro = Guid.Parse("b1000000-0000-0000-0000-000000000007");
    public static readonly Guid AreaSuperETendencia = Guid.Parse("b1000000-0000-0000-0000-000000000008");

    public static readonly (Guid Id, string Nome, string Descricao, bool Ativo)[] Tipos =
    [
        (TipoIncidente, "Incidente", "Algo parou de funcionar ou está com erro", true),
        (TipoDuvida, "Dúvida", "Pergunta sobre como fazer algo", true),
        (TipoSolicitacao, "Solicitação", "Pedido de acesso, cadastro, segunda via e similares", true),
        (TipoCustomizacao, "Customização", "Ajuste ou personalização de algo que já existe", true),
        (TipoMelhoria, "Melhoria", "Sugestão para melhorar um processo ou funcionalidade", true),
        (TipoNaoClassificado, "Não classificado", "Chamados abertos antes da classificação por tipo", false),
    ];

    /// <summary>
    /// Idempotente. 1) cria um grupo (área) para cada categoria que ainda não tem grupo de mesmo nome;
    /// 2) área = grupo de mesmo nome da categoria, para chamados sem área; 3) tipo = "Não classificado"
    /// para chamados sem tipo.
    /// </summary>
    public static string SqlPreencherAreaETipo() => $"""
        INSERT INTO "Grupos" ("Id", "Nome", "Descricao", "Ativo", "DataCriacao")
        SELECT CASE cat."Nome"
                 WHEN 'Financeiro' THEN '{AreaFinanceiro}'::uuid
                 WHEN 'Super e Tendência' THEN '{AreaSuperETendencia}'::uuid
                 ELSE gen_random_uuid() END,
               cat."Nome", COALESCE(NULLIF(cat."Descricao", ''), 'Área ' || cat."Nome"), TRUE, now()
        FROM "Categorias" cat
        WHERE NOT EXISTS (SELECT 1 FROM "Grupos" g WHERE g."Nome" = cat."Nome")
        ON CONFLICT DO NOTHING;

        UPDATE "Chamados" c
        SET "AreaId" = g."Id"
        FROM "Categorias" cat
        JOIN "Grupos" g ON g."Nome" = cat."Nome"
        WHERE c."AreaId" IS NULL AND c."CategoriaId" = cat."Id";

        UPDATE "Chamados"
        SET "TipoId" = '{TipoNaoClassificado}'::uuid
        WHERE "TipoId" IS NULL;
        """;
}
