# ⚠️ Concerns — Débito Técnico

> Identificados em 2026-06-18 durante mapeamento completo do código.
> Todos resolvidos em 2026-06-19. Detalhes técnicos completos em `.specs/codebase/CONCERNS.md`.

---

## ✅ Tudo resolvido

| ID | Problema | Status |
|----|----------|--------|
| C-01 | Conflito SQLite dev vs PostgreSQL migration | ✅ Migrado para Supabase (session pooler) em dev e prod |
| C-02 | Filtros de ListarChamados em memória | ✅ Query via IQueryable no banco |
| C-03 | `CategoriasController` bypassa CQRS | ✅ Usa MediatR |
| C-04 | `DatabaseSeeder.cs` não chamado | ✅ Seed centralizado |
| C-05 | Sem validators para Atribuir/Comentar | ✅ Validators criados |
| C-06 | Fechar/Cancelar sem Command/Endpoint | ✅ Implementados |
| C-07 | `ComentarioId` ausente na migration | ✅ FK adicionada |
| C-08 | IDs de seed hardcoded em dois lugares | ✅ Seed inline removido |
| C-09 | Sem testes | ✅ 48 testes (Domain + Application) |
| C-10 | `EnsureCreated()` em dev | ✅ Substituído por `MigrateAsync()` |

---

> *Nenhum débito técnico crítico pendente. Próximo passo: Frontend (Fase 3).* 🚀
