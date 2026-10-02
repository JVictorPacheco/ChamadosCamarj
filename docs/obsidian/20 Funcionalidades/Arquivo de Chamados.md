---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-02
spec: .specs/features/arquivo-de-chamados/spec.md
tags: [funcionalidade, arquivo]
---

# Arquivo de Chamados

## Para que serve
Tirar os chamados já terminados das telas do dia a dia **sem perder nada**: eles continuam
consultáveis numa tela própria.

## Quem usa
Todos os perfis, com a mesma regra de visibilidade de "Meus Chamados" (ver
[[Perfis e Permissões]]).

## O que aparece
Somente chamados **finalizados**: *Resolvido*, *Fechado* e *Cancelado*.

## Filtros
- **Período** (De / Até), pela data de abertura.
- **Status** (entre os três finalizados), **prioridade**, **área**, **tipo**.
- **Busca** por texto ou número (`CAM-42`).

## Regras de negócio
- A tela é **somente leitura** para consulta; ações (como reabrir) continuam no detalhe do chamado.
- Chamados **nunca são apagados**. Ver [[ADR-005 Chamados nunca são apagados]].
