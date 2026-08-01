# Handoff

**Date:** 2026-08-01
**Session:** Resiliência de Anexos + UX Upload + Perfis (Meus Chamados / Fila)
**Branch:** `main` (`69be4bc`), 16 arquivos alterados

## Completed ✓

### Correções — Anexos/Storage
- **Arquivos órfãos:** rollback no `AdicionarAnexoCommandHandler`
- **CancellationToken:** propagado pro SDK Supabase
- **Logging:** `ILogger` no `SupabaseStorageService`
- **Teste:** rollback de órfão (216 testes total)

### UX — Anexos
- `UploadAnexoForm` na `ChamadoDetailPage` (upload direto)
- Spinner + skeleton "Enviando arquivo..." no `AnexosList` (`useIsMutating`)
- `ComentarioForm` notifica `AnexosList` via `onUploadChange`

### Perfis — Meus Chamados / Fila
- **Solicitante via grupo:** vê próprios chamados + grupo
- **Solicitante sem grupo:** só os que abriu
- **Atendente via grupo:** todos não-atribuídos (Fila) + atribuídos ao grupo
- Filtro de `solicitanteEmail` não duplica quando grupo ativo
- Novo campo `Perfil` em `ListarChamadosQuery` e repository

### Regras de visibilidade

| Perfil | Grupo | Vê |
|--------|-------|-----|
| Atendente | Sim/Não | Todos abertos + atribuídos ao grupo |
| Solicitante | Sim | Atribuídos ao grupo + próprios |
| Solicitante | Não | Só próprios |
| Admin | — | Todos |

### Comentários
- Públicos: todos veem
- Internos: só Admin/Atendente (Solicitante nunca)

### Gate Checks
- 216 testes backend, 0 falhas
- Frontend build limpo

### Pendência de Produção
- `Supabase__ServiceRoleKey` como env var (sem ela, `NullStorageService` quebra upload)

## Handoff anterior (2026-07-31)
...(mantido abaixo)



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
