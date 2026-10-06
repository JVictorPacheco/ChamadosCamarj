---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-06
spec: .specs/features/relatorio-mensal/spec.md, .specs/features/controle-de-acesso/spec.md
tags: [funcionalidade, relatório]
---

# Relatório Mensal

## Para que serve
Prestação de contas: o andamento dos chamados em um **mês fechado**, pronto para apresentar à
superintendência no fechamento do mês.

## Quem usa
| Perfil | Vê |
|---|---|
| Admin | Relatório completo, incluindo a quebra por atendente |
| Atendente | Apenas os **próprios** números |
| Solicitante | Por padrão, sem acesso (bloqueio real). Se o Admin der a tela, vê os números **só dos chamados que já pode ver** |

O Admin pode tirar o relatório de um Atendente ou dá-lo a um Solicitante (ver [[Perfis e Permissões]]).

## Conteúdo
- **Seletor de mês.**
- **Totais** de abertos, resolvidos e cancelados, com **variação %** em relação ao mês anterior.
- Quebras **por área**, **por tipo** e **por atendente** (também na exportação).
- **Cumprimento do SLA** no mês. Ver [[SLA]].
- **Tempo médio de resolução.**

## Exportação
- **CSV** — para abrir no Excel.
- **PDF** — pela impressão do navegador, com layout próprio para impressão.

## Regras de negócio
- Os números vêm do **histórico** dos chamados: um chamado conta no mês em que o **evento
  aconteceu** (ex.: resolvido em março conta em março, mesmo que tenha sido aberto em fevereiro).
- Por isso o relatório de um mês passado **não muda** quando os chamados mudam de status depois —
  e por isso chamados nunca são apagados ([[ADR-005 Chamados nunca são apagados]]).

## Limites atuais
- Só por **mês fechado** — não há período livre.
