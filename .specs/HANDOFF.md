# Handoff

**Date:** 2026-07-21/22
**Feature:** Revisão de processo (SDD) + qualidade de código + MCP do shadcn/ui — CONCLUÍDO
**Task:** Depois das 5 features da sessão anterior (Forçar Encerramento, Número do Chamado, busca por número, RBAC real, Storage de Anexos), esta sessão comparou nosso uso do skill `tlc-spec-driven` com a metodologia canônica de Spec-Driven Development (specdriven.ai) e revisou qualidade de código das features recentes contra documentação oficial (MCP `microsoft-learn` + `context7`). **Leia `.specs/project/STATE.md` primeiro** — a seção "🧭 Regras de Processo (Constitution)" no topo é permanente e deve ser seguida em toda sessão futura.

## Completed ✓

- **3 gaps de processo identificados e adotados como regra permanente** (não só documentados): (1) pergunta de clarificação sem resposta não vira suposição silenciosa em decisão difícil de reverter; (2) spec atualizada antes do código, mesmo em extensões pequenas; (3) mudança de contrato entre camadas sinalizada antes de aplicar, não só relatada depois. Ver `.specs/project/STATE.md`, seção "Constitution".
- **1 bug de segurança real corrigido**: `AdicionarAnexo` (Controller) passava `arquivo.FileName` direto pro Command sem sanitizar — corrigido com `Path.GetFileName()`, conforme a doc oficial da Microsoft. **PR #18 mergeado em `develop`** (`dd166e0`, 2026-07-22T01:48:55Z), desta vez com o `base` certo.
- **MCP oficial do shadcn/ui instalado** (`.mcp.json`, escopado a este projeto) — só fica disponível numa sessão nova/reload. Não existe MCP dedicado de React (a lib não tem CLI/tooling própria); Context7 (genérico) já cobre esse papel.
- **Frontend das features recentes checado contra TanStack Query v5 oficial: sem desvio.** Backend teve 1 achado real (acima) + 2 pontos menores sem ação ainda (ver STATE.md).
- **Toda a documentação revisada e sincronizada** (`.specs/` + `docs/obsidian/`) — nada estava faltando ao final desta sessão.

## In Progress

Nada em execução.

## Pending

1. **Client ID real do Google (TI)** — login Google (T09/F5b) já implementado, só falta esse valor pra testar de ponta a ponta no navegador.
2. **Senha de app do IMAP** (suporte@/ti@camarj.com.br) — bloqueia a metade 2 da Fase 4 (Email → Chamado automático). Storage (metade 1) já está pronto e verificado.
3. **Decisão de hospedagem em produção** — também bloqueia o redirect URI de produção do OAuth.
4. **Testar comportamento de upload >10MB** (`[RequestSizeLimit]`) — a doc oficial alerta que pode aparecer como reset de conexão em vez de 4xx limpo. Não verificado ainda.
5. Sem ordem confirmada além disso.

## Blockers

Nenhum bloqueio ativo — os itens acima são pendências externas (aguardando terceiros/decisões) ou verificações menores, não bugs nem trabalho travado.

## Context — MUITO IMPORTANTE PRA RETOMAR

- **Ler `.specs/project/STATE.md` de cima pra baixo antes de qualquer coisa** — a seção "🧭 Regras de Processo (Constitution)" é uma regra permanente de como conduzir specify→design→tasks→execute neste projeto daqui pra frente, não um item histórico.
- Branch: `develop`, sincronizada com `main` e com o remoto (`dd166e0`).
- 216 testes de backend passando, builds limpos nos dois lados.
- `.mcp.json` foi criado nesta sessão (MCP do shadcn/ui) — se esta é uma sessão nova, ele já deve estar disponível; usar pra consultar componentes shadcn atuais (props/variantes/exemplos) em vez de confiar só na memória de treinamento.
- `user-secrets` tem `Supabase:Url`/`Supabase:ServiceRoleKey` configurados (verificar com `dotnet user-secrets list` se mudar de ambiente — não persiste entre clones/ambientes diferentes).
- Ao abrir um PR neste repo: **sempre conferir o dropdown `base`** — já aconteceu de ir pra `main` por engano (não fixa `develop` como lembrança).
