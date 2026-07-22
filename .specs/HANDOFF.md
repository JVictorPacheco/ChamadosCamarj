# Handoff

**Date:** 2026-07-21
**Feature:** Storage de Anexos (Fase 4, metade 1) — IMPLEMENTADO E VERIFICADO DE PONTA A PONTA
**Task:** Desde a última vez que este documento foi atualizado (18-19/07), 5 features novas foram feitas: Forçar Encerramento, Número do Chamado (`CAM-42`), busca por número, RBAC real do Dashboard/Kanban/Fila, e Storage de Anexos. Tudo já está em `develop` (e também em `main`, ver Context). Único bloqueio real que resta é o Client ID do Google (TI) e a senha de app do IMAP (Fase 4, metade 2 — Email).

## Completed ✓

- **Forçar Encerramento** (2026-07-19): Admin fecha um chamado direto de qualquer status não-final, com motivo obrigatório auditado no histórico. Bug real corrigido no caminho: `UsuarioId` sempre zerado no histórico desde o login Google real (fix: `MapInboundClaims = false`). Ver `.specs/features/forcar-encerramento/`.
- **Número do Chamado** (2026-07-19/20): `CAM-{número}` sequencial via sequence do Postgres, backfill cronológico dos chamados existentes, exibido em toda tela de lista/detalhe. Depois: busca por número no campo de busca já existente (`"42"` ou `"CAM-42"`). Ver `.specs/features/numero-do-chamado/`.
- **RBAC real do Dashboard/Kanban/Fila** (2026-07-20): as 3 telas ganharam bloqueio de verdade pro Solicitante (mesmo padrão do Relatório Mensal).
- **Storage de Anexos** (2026-07-20/21): upload/listagem/download de arquivo num chamado via Supabase Storage, verificado de ponta a ponta contra o Supabase real (upload de PDF, geração de URL assinada, download com conteúdo conferido byte a byte). Dois bugs reais corrigidos: API não subia sem a Service Role Key (fix: `NullStorageService` fallback), e o SDK do Supabase devolvia a URL assinada quebrada (fix: `TrimEnd('?')`). Ver `.specs/features/anexos-storage/`.
- **Incidente corrigido:** um PR foi acidentalmente aberto contra `main` em vez de `develop` e mergeado. Como as duas branches já estavam idênticas antes disso, a correção foi só um fast-forward simples de `develop` pros mesmos 2 commits — sem revert, sem conflito. `develop` e `main` estão sincronizadas de novo (`72451a3`).

## In Progress

Nada em execução.

## Pending

1. **Client ID real do Google (TI)** — login Google (T09/F5b) já implementado, só falta esse valor pra testar de ponta a ponta no navegador.
2. **Senha de app do IMAP** (suporte@/ti@camarj.com.br) — bloqueia a metade 2 da Fase 4 (Email → Chamado automático). Storage (metade 1) já está pronto.
3. **Decisão de hospedagem em produção** — também bloqueia o redirect URI de produção do OAuth.
4. Sem ordem confirmada além disso.

## Blockers

Nenhum bloqueio ativo — os itens acima são pendências externas (aguardando terceiros/decisões), não bugs nem trabalho travado.

## Context

- Branch: `develop`, sincronizada com `main` e com o remoto (`72451a3`).
- 216 testes de backend passando, builds limpos nos dois lados.
- Decisões e aprendizados completos em `.specs/project/STATE.md` — ler antes de mexer em auditoria/histórico, RBAC, Storage/Supabase, ou login.
- `user-secrets` agora tem `Supabase:Url`/`Supabase:ServiceRoleKey` configurados (verificar com `dotnet user-secrets list` se mudar de ambiente — não persiste entre clones/ambientes diferentes).
