# Handoff

**Date:** 2026-07-15
**Feature:** Fase 6 (Admin/Log) + Fase 7 (Relatório Mensal) — ambas concluídas e MERGEADAS em `develop`
**Task:** Nada pendente de merge. Próximo: iniciar T09/T15 (login Google Workspace real).

## Completed ✓

- **PR #13 mergeado em `develop`** (2026-07-15T03:05Z) — "Feature/fase 6 admin log". Contém:
  - Fase 6 completa: T01-T14 (Reatribuir, Histórico/auditoria, Alterar Prioridade, Comentário interno), com correção de bugs (filtro de comentário interno, migration incompleta, histórico gravando "Sistema" fixo)
  - Fase 7 completa: Relatório Mensal (backend + frontend + exportação CSV/PDF + RBAC)
  - Retrabalho do Dashboard: rosca "Distribuição por situação" no lugar do gráfico de Tendência, KPIs simplificados, distinção Resolvido vs Encerrado, bug de data corrigido (`ObterTendenciaAsync` contava resolvidos pela data errada)
- 109 testes unitários de backend passando. `npm run build` e `dotnet build` limpos. Verificado manualmente/via Playwright ad-hoc nos 3 perfis (Admin/Atendente/Solicitante).
- Toda a documentação (`STATE.md`, `ROADMAP.md`, specs de `relatorio-mensal/`) atualizada e commitada.

## In Progress

Nada em execução.

## Pending

1. **T09** — Login real via Google Workspace: endpoint `POST /auth/google`, JWT, tabela `UsuarioPerfil` (mapeamento conta→perfil). Ver spec em `.specs/features/fase-6-admin-log/spec.md`.
2. **T15** — Frontend: substituir `ProfileSelector`/`AuthContext` mockado pelo fluxo OAuth real. Depende de T09.
3. Quando T09 entrar: trocar `UsuarioId`/`UsuarioNome` (hoje enviados pelo cliente nos commands de Reatribuir/AlterarPrioridade/Resolver/Fechar/Cancelar) por extração via claims do JWT no backend — está documentado como pendência técnica no `STATE.md`.
4. "Forçar encerramento" (Admin fechar/cancelar fora do fluxo normal) — item da spec da Fase 6 ainda não abordado.
5. Revisar se Dashboard/Kanban/Fila (soft-RBAC — só escondem o link da sidebar, sem bloqueio de rota) precisam do mesmo bloqueio real que o Relatório Mensal recebeu nesta sessão, ou se ficam assim até T09 trazer autenticação de verdade.
6. Fase 4 (Email/Storage) segue sem data — só entra se for repriorizada.

## Blockers

Nenhum.

## Context — MUITO IMPORTANTE PRA RETOMAR

- **A branch local pode estar em `feature/fase-6-admin-log` — essa branch JÁ FOI MERGEADA e não deve mais ser usada.** Ao retomar: `cd /Users/joaopacheco/ChamadosCamarj && git checkout develop && git pull`. Todo o trabalho descrito acima já está na `develop`.
- API e frontend rodando em background numa sessão anterior foram encerrados — reiniciar se for testar (`dotnet run --project src/ChamadosCamarj.WebApi`, `npm run dev` dentro de `frontend/`).
- Banco: Postgres local via Docker (`chamados-postgres`, `docker-compose.yml` na raiz) — não o Supabase compartilhado dev/prod mencionado em decisões mais antigas.
- Push/PR precisam ser feitos manualmente pelo usuário no terminal real dele — o ambiente do Claude Code não tem credenciais do GitHub configuradas (`git push` sempre falha aqui, mesmo com a branch trackeada corretamente).
- Decisões e aprendizados completos em `.specs/project/STATE.md` (seções Decisões e Aprendizados — muita coisa relevante registrada lá, ler antes de mexer em auditoria/histórico, RBAC ou métricas de dashboard/relatório).
- Spec/design/tasks completos do Relatório Mensal em `.specs/features/relatorio-mensal/` — úteis de referência se a Fase 7 for expandida (período livre, SLA em tempo real, etc.)
