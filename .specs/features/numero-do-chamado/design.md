# Número do Chamado — Design

**Spec**: `.specs/features/numero-do-chamado/spec.md`
**Status**: Draft

---

## Arquitetura

Coluna nova (`Numero`, `int`) em `Chamados`, gerada por uma **sequence do Postgres** (`ChamadosNumeroSeq`) — não pela aplicação. Isso evita qualquer condição de corrida entre duas aberturas simultâneas (o Postgres garante atomicidade de `nextval()`). O `Guid Id` não muda em nada — `Numero` é só um campo adicional, gerado automaticamente no INSERT, igual o `DataCriacao` já é preenchido por padrão do banco hoje.

```mermaid
graph TD
    A[AbrirChamadoCommandHandler] --> B[new Chamado(...) sem Numero]
    B --> C[INSERT via EF Core]
    C --> D[Postgres: default nextval do Numero]
    D --> E[EF lê o valor gerado de volta pro objeto em memória]
    E --> F[ChamadoResponse.Numero populado]
```

## Migration — estratégia de backfill

A migration precisa rodar contra o Supabase real (mesmo banco de dev/produção), com chamados já existentes. Ordem das operações, tudo numa única migration (`AddNumeroChamado`):

1. `ALTER TABLE "Chamados" ADD COLUMN "Numero" integer;` (nullable por enquanto)
2. `CREATE SEQUENCE "ChamadosNumeroSeq";`
3. Backfill cronológico: `UPDATE "Chamados" SET "Numero" = sub.rn FROM (SELECT "Id", ROW_NUMBER() OVER (ORDER BY "DataCriacao") AS rn FROM "Chamados") sub WHERE "Chamados"."Id" = sub."Id";`
4. `SELECT setval('"ChamadosNumeroSeq"', COALESCE((SELECT MAX("Numero") FROM "Chamados"), 0));` — próximo `nextval()` continua depois do maior número já atribuído
5. `ALTER TABLE "Chamados" ALTER COLUMN "Numero" SET DEFAULT nextval('"ChamadosNumeroSeq"');`
6. `ALTER TABLE "Chamados" ALTER COLUMN "Numero" SET NOT NULL;`
7. `CREATE UNIQUE INDEX "IX_Chamados_Numero" ON "Chamados" ("Numero");`
8. `ALTER SEQUENCE "ChamadosNumeroSeq" OWNED BY "Chamados"."Numero";` — a sequence morre junto se a coluna for removida algum dia

`Down()`: remove o índice e a coluna (a sequence some sozinha por causa do `OWNED BY`).

**Por que backfill cronológico (`ORDER BY DataCriacao`), não por `Id`:** `Guid` não tem ordem cronológica nenhuma — numerar por `Id` daria números aleatórios sem relação com "o que aconteceu primeiro". `DataCriacao` é a única fonte confiável de ordem temporal (mesmo raciocínio já usado no Arquivo de Chamados e no Relatório Mensal).

## Componentes

### `Chamado.Numero` (Domain)
- **Purpose**: expor o número gerado pelo banco
- **Location**: `Chamado.cs`
- **Interface**: `public int Numero { get; private set; }` — sem parâmetro no construtor (não é decisão da aplicação, é gerada pelo banco)

### `ChamadoConfiguration` (Infrastructure)
- Mapeia `Numero` com `HasDefaultValueSql("nextval(...)")` + `ValueGeneratedOnAdd()`, mesmo padrão de `DataCriacao` (que já usa `HasDefaultValueSql`)

### `ChamadoResponse.Numero` (Application)
- Campo `int Numero` adicionado ao DTO existente e ao mapeamento em `ChamadoMappings.ToResponse()`. **Não** inclui um campo `NumeroFormatado` no backend — a formatação `CAM-{numero}` fica só no frontend, numa função única reaproveitada em todo lugar que exibe (evita duplicar a regra de formatação em cada response)

### Frontend
- `types/api.ts`: `numero: number` no `ChamadoResponse`
- Nova função `formatarNumeroChamado(numero) => \`CAM-${numero}\`` em `frontend/src/lib/` — único lugar com a regra de formatação
- Exibido em `ChamadoCard.tsx` (cobre Lista, Arquivo, Fila de Atendimento e Kanban — o `KanbanCard.tsx` só embrulha o `ChamadoCard` pro drag-and-drop, sem duplicar) e `ChamadoDetailPage.tsx` (cabeçalho)

## Fora deste design

Busca por número, reset anual, e-mail (Fase 4) — ver "Out of Scope" no spec.
