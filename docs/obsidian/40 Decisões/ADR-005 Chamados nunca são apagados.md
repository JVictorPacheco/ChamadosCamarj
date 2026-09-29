---
tipo: decisão
status: vigente
decisao: aceita
data: 2026-07-16
atualizado: 2026-09-29
tags: [decisão, dados]
---

# ADR-005 — Chamados nunca são apagados

## Contexto
Chamados finalizados se misturavam aos ativos nas telas do dia a dia, e surgiu a ideia de
apagá-los. Porém o [[Relatório Mensal]] e o histórico de auditoria dependem desses registros.

## Decisão
**Nenhum chamado é excluído.** Chamados finalizados (*Resolvido*, *Fechado*, *Cancelado*) saem das
telas operacionais e ficam numa tela própria de consulta: o [[Arquivo de Chamados]].

## Consequências
- ✅ Relatórios de meses passados continuam corretos para sempre.
- ✅ Auditoria completa: é sempre possível saber o que aconteceu com qualquer solicitação.
- ✅ Chamados podem ser reabertos se o problema voltar.
- ⚠️ A base cresce continuamente — irrelevante no volume atual.

## Exceção
**Anexos** podem ser removidos (por quem enviou ou pelo Admin), pois não afetam relatórios nem o
histórico. Ver [[Anexos]].
