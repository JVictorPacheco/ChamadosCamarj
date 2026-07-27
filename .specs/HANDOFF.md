# Handoff

**Date:** 2026-07-27
**Feature:** Feature auth-email-senha CONCLUÍDA + docs cleanup — CONCLUÍDO
**Task:** Backend + frontend do login por e-mail e senha implementados e verificados. Migration `AddSenhaHashUsuarioPerfil` corrigida (Up/Down). Confirmation dialogs de UX (Encerrar, Cancelar, Resolver, Desativar/Reativar, Logout) adicionados. Documentação (README, AGENTS, STATE, HANDOFF, spec, tasks) sincronizada com o estado atual. **Leia `.specs/project/STATE.md` primeiro** — a seção "🧭 Regras de Processo (Constitution)" no topo é permanente e deve ser seguida em toda sessão futura.

## Completed ✓

- **Feature auth-email-senha CONCLUÍDA**: backend + frontend completos — `LoginPage.tsx` trocou Google Login por email+senha, `UsuarioFormDialog.tsx` ganhou campo de senha obrigatório, `UsuariosPage.tsx` com botão "Redefinir senha", `AuthContext.tsx` com `loginComSenha`, `api.ts` com `login()`/`redefinirSenha()`. Migration `AddSenhaHashUsuarioPerfil` com `Up()`/`Down()` completos. 218 testes passando, `npm run build` limpo.
- **Confirmation dialogs**: Encerrar, Cancelar, Resolver, Desativar/Reativar usuário, Logout — todos com modal de confirmação.
- **Docs cleanup**: README.md, AGENTS.md, STATE.md, HANDOFF.md, `.specs/features/auth-email-senha/spec.md` e `tasks.md` atualizados.
- **ROADMAP.md**: seção duplicada removida.

## In Progress

Nada em execução.

## Pending

1. **Senha de app do IMAP** (suporte@/ti@camarj.com.br) — bloqueia a metade 2 da Fase 4 (Email → Chamado automático). Storage (metade 1) já está pronto e verificado.
2. **Decisão de hospedagem em produção** — também bloqueia o redirect URI de produção do OAuth (se o login Google um dia voltar a ser usado).
3. **Testar comportamento de upload >10MB** (`[RequestSizeLimit]`) — a doc oficial alerta que pode aparecer como reset de conexão em vez de 4xx limpo. Não verificado ainda.
4. Sem ordem confirmada além disso.

## Blockers

Nenhum bloqueio ativo — os itens acima são pendências externas (aguardando terceiros/decisões) ou verificações menores, não bugs nem trabalho travado.

## Context — MUITO IMPORTANTE PRA RETOMAR

- **Ler `.specs/project/STATE.md` de cima pra baixo antes de qualquer coisa** — a seção "🧭 Regras de Processo (Constitution)" é uma regra permanente de como conduzir specify→design→tasks→execute neste projeto daqui pra frente, não um item histórico.
- Branch: `develop`, sincronizada com `main` e com o remoto (`dd166e0`).
- 218 testes de backend passando, builds limpos nos dois lados.
- `.mcp.json` foi criado nesta sessão (MCP do shadcn/ui) — se esta é uma sessão nova, ele já deve estar disponível; usar pra consultar componentes shadcn atuais (props/variantes/exemplos) em vez de confiar só na memória de treinamento.
- `user-secrets` tem `Supabase:Url`/`Supabase:ServiceRoleKey` configurados (verificar com `dotnet user-secrets list` se mudar de ambiente — não persiste entre clones/ambientes diferentes).
- Ao abrir um PR neste repo: **sempre conferir o dropdown `base`** — já aconteceu de ir pra `main` por engano (não fixa `develop` como lembrança).
