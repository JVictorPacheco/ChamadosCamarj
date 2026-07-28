# Handoff

**Date:** 2026-07-28
**Session:** 5 features + boas práticas + deploy Azure + Grupos/Equipes — CONCLUÍDO
**Branch:** `develop`, ~60 arquivos modificados/criados

## Completed ✓

### Features
- **Toggle olhinho** 👁 — `PasswordInput` com forwardRef, 5 campos de senha
- **Tema claro CAMARJ** ☀️ — paleta branco + verde teal, ThemeProvider, toggle em Login/Resetar/AppLayout
- **Logo CAMARJ** 🏢 — no topo da sidebar + "Portal de Chamados"
- **Novas categorias** 📂 — 8 total: Credenciado, Comercial, Contas Médicas + rename Autorização/Auditoria
- **Grupos/Equipes** 👥 — entidade Grupo, migration, RBAC grupo, CRUD Admin, frontend completo

### Code Quality
- Frontend: `strict: true`, `useMemo` contextos, `isPending` (v5), `staleTime` global, PasswordInput acessível
- Backend: N+1 corrigido, autor via JWT, rate limiting, health checks, BadRequestException, JSON camelCase

### Infra
- GitHub Actions deploy Azure App Service Free (`.github/workflows/deploy-azure.yml`)
- Health checks `/health`, rate limiting `/auth/login`
- Docs: CONVENTIONS, STACK, STRUCTURE, TESTING, ROADMAP atualizados

### Numbers
- 218 testes backend passando
- 0 erros TypeScript
- Build limpo

## In Progress / Pending
Nada em execução.

## Blockers
Nenhum.

## Context — MUITO IMPORTANTE PRA RETOMAR
- **Ler `.specs/project/STATE.md` primeiro** — regras de processo (Constitution) no topo.
- Branch: `develop`, ~60 arquivos modificados (ainda não commitados).
- Migration `AddGrupo` NÃO aplicada no Supabase real — aplica automaticamente no próximo `dotnet run`.
- Para aplicar no Supabase real: `dotnet run` ou `dotnet ef database update`.
- Túnel Cloudflare precisa ser iniciado manualmente se for usar produção (até migrar pro Azure).
- Ao abrir PR: base `develop`, não `main`.
