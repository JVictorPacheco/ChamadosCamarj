# Handoff

**Date:** 2026-07-31
**Session:** Fundamentos de engenharia — concorrência, idempotência, triagem, tema, UX
**Branch:** `main`, 38 arquivos modificados/criados

## Completed ✓

### Fundamentos de Engenharia (Gaps 1-4)
- **Gap #1 — Concorrência otimista:** `IsConcurrencyToken()` no `DataAtualizacao` do Chamado → 409 Conflict se dois atendentes modificarem o mesmo chamado
- **Gap #2 — Idempotência:** Filtro `[Idempotent]` + `Idempotency-Key` opcional em POST/PATCH. Frontend: helper `gerarIdempotencyKey()`
- **Gap #3 — Auto-triagem:** `KeywordTriagemService` → 8 categorias × 6 grupos mapeados por palavras-chave. Endpoint `POST /api/chamados/sugerir-triagem`. Botão "Sugerir categoria" na `AbrirChamadoPage`
- **Gap #4 — Observabilidade:** Enum `OrigemEntrada` (Humano/Ia) + campo `Origem` no `HistoricoEntrada`. Migration `AddOrigemHistoricoEntrada`

### Tema + UX
- Tema verde menta: claro `#d7efe5`, escuro `#06241a`. Padrão agora é claro
- Favicon logo CAMARJ + título "CAMARJ - Portal de Chamados"
- Reset de senha: `FrontendBaseUrl` corrigido, CORS com `okurumin.com.br`, logs SMTP melhorados
- `Email:SmtpSenha` configurada via user-secrets (local) — em produção precisa ser env var `Email__SmtpSenha`

### Numbers
- 215 testes backend passando
- 12 testes E2E Playwright passando
- 0 erros TypeScript
- Build limpo

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
- Migration `AddOrigemHistoricoEntrada` já aplicada no Supabase real.
- SMTP configurado com senha de app do Gmail — reset de senha funcional.
- Produção: frontend em `https://chamados.okurumin.com.br`, backend via Cloudflare Tunnel.
- Ao abrir PR: base `develop`, não `main`.
- Em Produção, setar env var `Email__SmtpSenha` no servidor do backend.
