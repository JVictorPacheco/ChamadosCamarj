# ⚠️ Concerns — Débito Técnico

> Identificados em 2026-06-18 durante mapeamento completo do código.
> Detalhes técnicos completos em `.specs/codebase/CONCERNS.md`.

---

## 🔴 Críticos

### C-01 — Conflito SQLite dev vs PostgreSQL migration
O app roda SQLite em dev (`chamadoscamarj.db`) mas a migration foi escrita com tipos PostgreSQL (`uuid`, `character varying`). Funciona agora com `EnsureCreated()`, mas vai quebrar ao migrar para Supabase.

**Resolver antes de subir para produção.**

### C-02 — Filtros de ListarChamados em memória
`ListarChamadosQueryHandler` carrega **todos os chamados** do banco e depois filtra em C#. Com volume, vira gargalo grave.

---

## 🟡 Médios

| ID | Problema | Onde |
|----|----------|------|
| C-03 | `CategoriasController` injeta repositório direto (quebra CQRS) | `WebApi/Controllers/CategoriasController.cs` |
| C-04 | `DatabaseSeeder.cs` não é chamado — seed duplicado em `Program.cs` | `Infrastructure/Data/DatabaseSeeder.cs` |
| C-05 | Sem validators para `Atribuir` e `Comentar` | `Application/Features/Chamados/Validators/` |
| C-06 | `Fechar` e `Cancelar` existem no Domain mas sem Command/Endpoint | `Application/Features/Chamados/Commands/` |
| C-07 | `ComentarioId` existe no `Anexo.cs` mas ausente na migration | `Infrastructure/Migrations/` |

---

## 🟢 Baixos

| ID | Problema |
|----|----------|
| C-08 | IDs de seed hardcoded em dois lugares (Program.cs usa GUIDs fixos, Seeder não) |
| C-09 | Sem testes — `tests/` mencionado no README mas não existe |
| C-10 | `EnsureCreated()` pode deixar schema stale ao mudar o modelo |
