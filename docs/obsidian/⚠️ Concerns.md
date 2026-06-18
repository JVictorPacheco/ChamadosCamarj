# ⚠️ Concerns — Débito Técnico

> Identificados em 2026-06-18 durante mapeamento completo do código.
> Detalhes técnicos completos em `.specs/codebase/CONCERNS.md`.

---

## 🔴 Críticos

### C-01 — Conflito SQLite dev vs PostgreSQL migration
O app roda SQLite em dev (`chamadoscamarj.db`) mas a migration foi escrita com tipos PostgreSQL (`uuid`, `character varying`). Funciona agora com `EnsureCreated()`, mas vai quebrar ao migrar para Supabase.

**Resolver antes de subir para produção. Bloco 4.**

---

## 🟡 Médios

| ID | Problema | Status |
|----|----------|--------|
| C-02 | Filtros de `ListarChamados` em memória → no banco | ✅ Resolvido — PR #2 (Bloco 2) |
| C-03 | `CategoriasController` injeta repositório direto (quebra CQRS) | ✅ Resolvido — PR #1 (Bloco 1) |
| C-04 | `DatabaseSeeder.cs` não era chamado — seed duplicado em `Program.cs` | ✅ Resolvido — PR #1 (Bloco 1) |
| C-05 | Sem validators para `Atribuir` e `Comentar` | ✅ Resolvido — PR #1 (Bloco 1) |
| C-06 | `Fechar` e `Cancelar` sem Command/Endpoint | ✅ Resolvido — PR #1 (Bloco 1) |
| C-07 | `ComentarioId` existe no `Anexo.cs` mas ausente na migration | 🔧 Bloco 4 |

---

## 🟢 Baixos

| ID | Problema | Status |
|----|----------|--------|
| C-08 | IDs de seed inconsistentes entre Program.cs e Seeder | ✅ Resolvido — PR #1 (Bloco 1) |
| C-09 | Sem testes — `tests/` mencionado no README mas não existe | 🔧 Bloco 3 (próximo) |
| C-10 | `EnsureCreated()` pode deixar schema stale ao mudar o modelo | 🔧 Bloco 4 |
