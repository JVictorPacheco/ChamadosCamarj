# Handoff

**Date:** 2026-06-23
**Feature:** Fase 3 — Portal do Solicitante (frontend)
**Task:** Execute em andamento. Fases 1, 2 e 3 do `tasks.md` concluídas (T1-T9). Próxima: Fase 4 (T10, T11, T12).

## Completed ✓

- **Fase 1 (backend, API-01):** T1 (`ObterComentariosPorChamadoAsync`), T2 (`ComentarioResponse`/`ListarComentariosQuery`/Handler + testes), T3 (endpoint `GET /chamados/{id}/comentarios`, verificado via curl). 3 commits na branch `feature/fase3-bloco1-comentarios-api` (pushada, PR ainda não criado — `gh` CLI não disponível neste ambiente; link de criação ficou no output do push).
- **Fase 2 (frontend foundation):** T4 (scaffold Vite+React+TS, alias `@`), T5 (TailwindCSS v4 + shadcn/ui + **tema dark Camarj**, ver decisão abaixo), T6 (React Router + TanStack Query + React Hook Form, `App.tsx` envolto nos providers).
- **Fase 3 (infra paralela):** T7 (`types/api.ts`, conferido contra os DTOs reais do backend), T8 (`lib/api.ts` — `apiFetch`/`ApiError`, conferido contra `ExceptionHandlingMiddleware` real), T9 (`AuthContext`/`useAuth`, 3 perfis mockados).
- Tudo commitado na branch `feature/fase3-bloco2-frontend-foundation` (branched de `origin/develop`, **não** inclui as mudanças de backend da bloco1 — só precisa delas a partir da T13). Pushada, PR ainda não criado.

## Decisão de design tomada nesta sessão (2026-06-23)

Usuário mandou uma imagem de referência (`Downloads\Exemplo_Imagem_Camarj_Chamado.jpeg`, dashboard interno da Camarj) e pediu pra usar a paleta de cores + o menu lateral. Isso mudou o design aprovado original:

- **Tema:** dark mode único (sem toggle), paleta extraída por inspeção visual (aproximada, não pixel-perfect) registrada em `.specs/features/frontend-portal-solicitante/design.md` → seção "Tema Visual (Dark Mode)". Aplicada em `frontend/src/index.css` (bloco `.dark`), forçada via `<html class="dark">` em `index.html`.
- **AppLayout:** trocado de header simples (design original) pra **sidebar fixa** (shadcn `Sidebar`). Documentado em `design.md` (seção AppLayout) e `tasks.md` (T11 reescrita).
- Tipografia da referência (serifa + mono caixa-alta) ficou **fora de escopo**, registrada como ideia adiada em `STATE.md`.
- Verificação visual do tema feita com Playwright ad-hoc (instalado num diretório `/tmp` descartável, não comitado) já que não há `chromium-cli`/MCP de browser neste ambiente: `bg #0a1413`, `fg #ededE8`, sem erros de console — confere com os tokens definidos.

## In Progress

- Nada em execução. Sessão pausada a pedido do usuário ("espera até amanhã") logo antes de escrever o arquivo da **T10**.

## Pending

1. **T10** (`ProfileSelector.tsx`, tela `/login`): tentativa de escrita foi **rejeitada pelo usuário** (parar antes de prosseguir) — nenhum arquivo foi criado, nada a desfazer. Recomeçar do zero amanhã. Depende de T9 (✅) e T5 (✅), ambas prontas.
2. Depois: T11 (`AppLayout` — sidebar, já redesenhada pra usar o componente `Sidebar` do shadcn) e T12 (wiring de rotas em `App.tsx`, inclui montar `AuthProvider` que ainda não está em lugar nenhum da árvore de componentes).
3. Dois PRs pendentes de abertura manual pelo usuário (sem `gh` CLI no ambiente):
   - `feature/fase3-bloco1-comentarios-api` → `develop`
   - `feature/fase3-bloco2-frontend-foundation` → `develop`
4. Pendência antiga ainda válida: nenhuma, decisão do teste E2E (Playwright em T22) já foi confirmada e incorporada ao `tasks.md` nesta mesma sessão, antes do trabalho de Execute.

## Blockers

Nenhum.

## Context

- Branch local atual: `feature/fase3-bloco2-frontend-foundation` (pushada). Continuar nela amanhã para T10-T12 (mesma fatia "Frontend Foundation/Shell").
- Dev server e API ficaram rodando em background durante a sessão — **ambos finalizados** antes de pausar.
- Sem alterações de código pendentes no working tree, só ruído de tooling não relacionado (`.agents/`, `.mcp.json`, `docs/.obsidian/workspace.json`, `skills-lock.json`, e uma diferença de line-ending em `docs/obsidian/🏗️ Arquitetura.md`) — não tocar, não fazem parte do trabalho.
- Decisões-chave em `.specs/project/STATE.md` e `design.md` (Tema Visual + AppLayout) — ler antes de continuar T10/T11.
