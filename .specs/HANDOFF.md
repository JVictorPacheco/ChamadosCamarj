# Handoff

**Date:** 2026-07-15
**Feature:** Fase 7 — Relatório Mensal (concluída) + retrabalho do Dashboard
**Task:** Fase 7 100% completa e verificada. Próximo: retomar T09/T15 (Google auth) da Fase 6.

## Completed ✓

- **Relatório Mensal (Fase 7) completo**: spec → design → tasks → execute (`.specs/features/relatorio-mensal/`). Backend (`GET /api/relatorios/mensal`, agregação via `HistoricoEntrada`, SLA, comparação com mês anterior) + frontend (página com seletor de mês, KPIs, rosca de SLA, quebra por categoria/atendente, exportação CSV/PDF).
- **Dashboard retrabalhado**: gráfico de Tendência (linha) virou rosca "Distribuição por situação" (Aguardando/Assumido/Resolvido/Encerrado/Cancelado — situação atual, não período). KPIs simplificados. Distinção Resolvido vs Encerrado adicionada (antes só existia Resolvido).
- **Bug corrigido**: `ObterTendenciaAsync` contava "resolvidos" pela data de criação do chamado, não de resolução.
- **RBAC corrigido**: Relatório Mensal bloqueia de verdade o Solicitante (não só esconde o link, como o resto do app faz) — expõe dado mais sensível (desempenho por atendente) que justificou o tratamento diferente.
- Verificado via Playwright ad-hoc (scripts temporários, removidos): números batendo em 2 meses (julho com dados, junho vazio), RBAC dos 3 perfis, CSV exportado com conteúdo correto.
- 109 testes unitários de backend passando. `npm run build` e `dotnet build` limpos.
- 9 commits nesta sessão, todos pushados em `feature/fase-6-admin-log` (branch segue com nome da Fase 6 por não ter sido renomeada, mas já contém Fase 6 completa T01-T14 + Fase 7 completa).

## In Progress

Nada em execução.

## Pending

1. Retomar T09/T15 (login Google Workspace real) — próxima prioridade.
2. Quando T09 entrar: trocar `UsuarioId`/`UsuarioNome` (hoje client-supplied) por claims do JWT no backend.
3. Criar PR de `feature/fase-6-admin-log` → `develop`.
4. "Forçar encerramento" (Admin fechar/cancelar fora do fluxo normal) — item da spec da Fase 6 ainda não abordado.
5. Considerar se Dashboard/Kanban/Fila (soft-RBAC, só escondem o link) precisam do mesmo bloqueio real que o Relatório Mensal recebeu, ou se ficam assim até o T09 trazer auth de verdade.

## Blockers

Nenhum.

## Context

- Branch local atual: `feature/fase-6-admin-log`, todos os commits pushados.
- API e frontend rodando em background (porta 5000 e 5173) — confirmar se ainda estão de pé antes de continuar, ou reiniciar.
- Banco: Postgres local via Docker (`chamados-postgres`), não o Supabase compartilhado dev/prod das decisões antigas.
- Decisões-chave em `.specs/project/STATE.md` (Decisões + Aprendizados, entradas de 2026-07-14/15) e `.specs/features/relatorio-mensal/` (spec/design/tasks completos, úteis se for expandir o relatório no futuro).
