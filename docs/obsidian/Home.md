---
tipo: índice
status: vigente
atualizado: 2026-09-29
tags: [home]
---

# Portal de Chamados CAMARJ

Documentação funcional do **Portal de Chamados** da CAMARJ — o que o sistema faz, para quem, com
quais regras e por quê. Escrita para ser lida por quem **não** é da área técnica; os detalhes de
implementação ficam na seção Arquitetura e nas specs do projeto.

> **Em produção:** https://chamados.okurumin.com.br

---

## Por onde começar

| Se você quer… | Leia |
|---|---|
| Entender o sistema em 5 minutos | [[Visão Geral]] |
| Saber o significado de um termo | [[Glossário]] |
| Saber quem pode fazer o quê | [[Perfis e Permissões]] |
| Entender como um chamado anda | [[Ciclo de Vida do Chamado]] |

## Mapa do vault

**00 Visão** — o sistema em alto nível
- [[Visão Geral]] · [[Glossário]] · [[Perfis e Permissões]]

**10 Processos** — como o trabalho acontece
- [[Ciclo de Vida do Chamado]] · [[Fluxo de Atendimento]] · [[Encerramento e Cancelamento]]

**20 Funcionalidades** — uma nota por funcionalidade
- [[Abertura de Chamados]] · [[Acompanhamento do Chamado]] · [[Anexos]]
- [[Fila, Kanban e Dashboard]] · [[SLA]] · [[Relatório Mensal]] · [[Arquivo de Chamados]]
- [[Grupos e Equipes]] · [[Chat Corporativo]] · [[Acesso e Login]] · [[Administração]]
- [[Abertura por E-mail]] *(planejada)*

**30 Arquitetura** — visão técnica resumida
- [[Visão Técnica]] · [[Modelo de Dados]] · [[Infraestrutura e Deploy]]

**40 Decisões** — o porquê das escolhas (formato ADR)
- [[ADR-001 Google Workspace, não Azure AD]]
- [[ADR-002 Login por e-mail e senha]]
- [[ADR-003 Dev e produção no mesmo banco]]
- [[ADR-004 Hospedagem na Cloudflare]]
- [[ADR-005 Chamados nunca são apagados]]
- [[ADR-006 Número de chamado CAM-N]]
- [[ADR-007 Permissões aplicadas no servidor]]
- [[ADR-008 Área do chamado é a equipe]]
- [[ADR-009 Edição simultânea por versão do chamado]]

**90 Modelos** — para criar notas novas no mesmo padrão
- [[Modelo - Funcionalidade]] · [[Modelo - Decisão (ADR)]]

**99 Arquivo** — notas históricas, **não refletem o sistema atual**
- [[Azure AD]] · [[Login Google Workspace]] · [[Leitor de E-mails (script local)]]

---

## Onde acompanhar o andamento

Este vault descreve o sistema **como ele é** — não registra andamento, próximos passos ou
pendências. Para isso, a fonte oficial é a pasta `.specs/` do repositório:

| O quê | Arquivo |
|---|---|
| Estado atual, sessões, pendências | `.specs/project/STATE.md` |
| Roadmap (concluído e planejado) | `.specs/project/ROADMAP.md` |
| Débito técnico e riscos | `.specs/codebase/CONCERNS.md` |
| Especificação detalhada de cada funcionalidade | `.specs/features/<nome>/spec.md` |

> O Obsidian não exibe pastas que começam com ponto, por isso `.specs/` não aparece aqui — abra
> pelo editor de código ou pelo GitHub.

## Como manter este vault

- Cada nota tem **propriedades** no topo: `tipo`, `status` (`vigente`, `planejada` ou `obsoleta`),
  `atualizado` e, quando existe, `spec` (a especificação correspondente).
- **Regra do projeto:** uma funcionalidade só está pronta quando a nota dela aqui está atualizada.
- Nota que deixou de valer vai para `99 Arquivo` com `status: obsoleta` — não é apagada.
- Novas notas partem dos modelos em `90 Modelos`.
- Diagramas usam **Mermaid**, que o Obsidian e o GitHub desenham automaticamente.
