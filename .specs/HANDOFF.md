# Handoff

**Date:** 2026-07-14
**Feature:** Fase 7 — Relatório Mensal (antecipada)
**Task:** Specify ainda não começado. Fase 6 (T09/T15) pausada de propósito até o relatório sair.

## Completed ✓

- Fase 6 T01-T14 completos e verificados via Playwright: reatribuir, alterar prioridade, histórico com usuário real, comentário interno oculto do Solicitante. Frontend reescrito em `frontend/src/features/chamados/`, arquivos órfãos em `src/ChamadosCamarj.Web/` apagados.
- Bugs corrigidos: filtro de comentário interno (endpoint não repassava perfil), migration `AddHistoricoEntrada` incompleta, histórico gravando "Sistema" fixo em vez do usuário real.
- Dashboard (Fase 5) corrigido: agora mostra Cancelados e Resolvidos (total, não só hoje), e "Abertos" detalha assumidos vs em espera.
- 2 commits pushados em `feature/fase-6-admin-log` (`8c76baf`, `c971d0f`, `eb1dca3` — rework frontend, docs, fix dashboard).
- Testes unitários (96) e typecheck sempre verificados após cada mudança.

## In Progress

Nada em execução. Sessão pausada a pedido do usuário antes de iniciar o Specify do Relatório Mensal.

## Pending

1. **Specify do Relatório Mensal** (Fase 7, prioridade imediata): definir com o usuário — período (mês calendário fechado?), métricas exatas (totais por status/categoria/atendente, comparação com mês anterior, SLA cumprido vs estourado?), formato de exportação (PDF? CSV? os dois?), quem acessa (só Admin, ou Atendente também vê o próprio desempenho?).
2. Depois do relatório: retomar T09/T15 (Google Workspace auth).
3. Quando T09 entrar: trocar `UsuarioId`/`UsuarioNome` (hoje client-supplied) por claims do JWT.
4. Criar PR de `feature/fase-6-admin-log` → `develop` (T01-T14 completos e verificados).
5. "Forçar encerramento" (Admin fechar/cancelar fora do fluxo normal) — item da spec da Fase 6 ainda não abordado.

## Blockers

Nenhum.

## Context

- Branch local atual: `feature/fase-6-admin-log`, com os commits já pushados pro remoto.
- API e frontend rodando em background (porta 5000 e 5173) — confirmar se ainda estão de pé antes de continuar, ou reiniciar.
- Banco: Postgres local via Docker (`chamados-postgres`), não o Supabase compartilhado dev/prod das decisões antigas.
- Decisões-chave em `.specs/project/STATE.md` (seção Decisões: "Ordem Fase 6 vs Fase 7", "Relatório mensal") e `ROADMAP.md` (Fase 7 marcada EM ANDAMENTO).
- Push feito manualmente pelo usuário no terminal real dele — este ambiente (Claude Code) não tem credenciais do GitHub configuradas, sempre vai falhar `git push` aqui.
