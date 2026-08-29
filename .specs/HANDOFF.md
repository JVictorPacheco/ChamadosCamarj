# Handoff

> ⚠️ **Este arquivo está sendo descontinuado.** O conteúdo de handoff foi unificado em `.specs/project/STATE.md`, que é a fonte única de verdade sobre o estado do projeto entre sessões. O arquivo abaixo é preservado como histórico da sessão de 2026-08-10, mas novas sessões não devem criar novos blocos aqui — use `STATE.md`.
>
> **Para retomar o trabalho:** leia `.specs/project/STATE.md` (seção "Sessão atual" no topo) e `CLAUDE.md` na raiz do projeto.

**Date:** 2026-08-10
**Session:** Hotfix URL API produção + sessão anterior (Dashboard/Kanban/Orquestração)
**Branch:** `develop` (mergeado em `main`)

## Completed ✓

### Hotfix — URL da API em produção (2026-08-10 tarde)
- `.env.production`: `VITE_API_BASE_URL` corrigido de `trycloudflare.com` (efêmero, expirado) para `https://chamados.okurumin.com.br/api`
- Build refeito, commitado e mergeado em `develop` e `main`
- Causa raiz: túnel efêmero caiu; API migrou para domínio fixo `okurumin.com.br` mas o `.env.production` nunca foi atualizado

### Mudança 1 — Dashboard clicável (click-through para lista filtrada)
- **Roscas "Distribuição por situação":** cada fatia navega para `/chamados?status={StatusChamado}`
- **Gráfico de barras por Categoria:** cada barra navega para `/chamados?categoriaId={Guid}`
- **Gráfico de barras por Prioridade:** cada barra navega para `/chamados?prioridade={PrioridadeChamado}`
- **Card KPI "Resolvidos Hoje":** navega para `/chamados?status=Resolvido`
- KPI "Tempo Médio" e "SLA (mês)" não são clicáveis (métricas informativas)

### Mudança 2 — Kanban com navegação para detalhe
- **Clique no card do Kanban** abre `/chamados/:id` (detalhe do chamado)
- **Drag and drop** continua funcionando (activationConstraint: distance=8 distingue clique de arraste)

### Dependências técnicas
- **Backend:** `PorCategoriaItem` agora inclui `Guid? CategoriaId` (antes só `CategoriaNome`). Novo record `CategoriaContagem` em Domain/Interfaces. `ContarPorCategoriaAsync` retorna `List<CategoriaContagem>` com ID e nome.
- **Frontend:** `ChamadosListPage` migrada de `useState` para `useSearchParams`. Filtros sincronizados bidirecionalmente com a URL (`status`, `prioridade`, `categoriaId`, `busca`, `slaStatus`).

### Code review — 5 pontos corrigidos
1. ✅ `categoriaId` com validação UUID (regex) antes de enviar à API
2. ✅ Kanban com paginação (botão "Carregar mais", merge deduplicado por ID)
3. ✅ SignalR filtra por tipo de evento no Kanban (evita refetch em MetricasAtualizadas)
4. ✅ `handlePrioridadeClick` com tipo próprio `PrioridadeClickData`
5. ✅ `buildQueryString` filtra `null` além de `undefined` (evita `?categoriaId=null`)

### Orquestração de IA + SDD
- **Processo documentado:** guia completo em `docs/GUIA-ORQUESTRACAO-SDD.md`
- Fluxo usado: @spec → @build-frontend → @review → correções → @review → merge
- Spec em `.specs/features/dashboard-kanban-navegacao/spec.md` + `tasks.md`
- Reviews em `review.md` e `review-fixes.md`

### Gate Checks
- 215 testes backend, 0 falhas
- 0 erros TypeScript
- Build limpo (frontend + backend)
- Mergeado em `develop` e `main`

## In Progress / Pending
- Deploy Azure: criar App Service no portal (GitHub Actions + guia prontos)
- Fase 4 Email: `EmailReceiverService` (IMAP) — depende de senha de app
- SLA: alertas SignalR + filtro por SLA na listagem
- Motivo: filtro por motivo na listagem de finalizados
- Dashboard: gráfico de evolução mensal do SLA
- Triagem por IA real (LLM): interface `ITriagemService` pronta, implementação atual é keyword-based

## Blockers
Nenhum.

## Context — MUITO IMPORTANTE PRA RETOMAR
- **Ler `.specs/project/STATE.md` primeiro** — regras de processo (Constitution) no topo.
- Branch: `develop` para trabalho, `main` para produção.
- **Orquestração:** usar `@spec` → `@build-backend`/`@build-frontend` → `@review` para cada feature nova.
- Guia completo em `docs/GUIA-ORQUESTRACAO-SDD.md`.
- Migration `AddOrigemHistoricoEntrada` já aplicada no Supabase real.
- SMTP configurado com senha de app do Gmail — reset de senha funcional.
- Produção: frontend em `https://chamados.okurumin.com.br`, backend via Cloudflare Tunnel.
- Ao abrir PR: base `develop`, não `main`.
