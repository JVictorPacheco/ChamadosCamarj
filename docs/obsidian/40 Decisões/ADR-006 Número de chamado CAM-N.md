---
tipo: decisão
status: vigente
decisao: aceita
data: 2026-07-19
atualizado: 2026-09-29
tags: [decisão, chamado]
---

# ADR-006 — Número de chamado CAM-N

## Contexto
Internamente cada chamado tem um identificador técnico longo, impossível de citar numa conversa
ou e-mail ("o chamado 3f2a9c…").

## Decisão
Todo chamado tem um **número amigável** no formato **`CAM-{número}`**, sem zeros à esquerda
(`CAM-1`, `CAM-42`).
- Gerado **pelo banco de dados** em sequência — duas aberturas simultâneas nunca recebem o mesmo
  número.
- Chamados que já existiam receberam números em ordem cronológica de abertura.
- A busca aceita `42` ou `CAM-42`.

## Consequências
- ✅ Referência simples em conversas, e-mails e relatórios.
- ✅ Sem risco de número repetido.
- ⚠️ A sequência pode ter "buracos" (ex.: uma abertura que falhou consome um número) — normal e
  sem impacto.

Ver [[Abertura de Chamados]].
