---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-01
spec: .specs/features/sla-tracking/spec.md
tags: [funcionalidade, sla]
---

# SLA — Prazos de Atendimento

## Para que serve
Dar a cada chamado um **prazo máximo de resolução** conforme a urgência, e deixar visível quem está
dentro ou fora do prazo.

## Prazos por prioridade
| Prioridade | Prazo para resolver |
|---|---|
| Urgente | 8 horas |
| Alta | 24 horas |
| Média | 16 horas |
| Baixa | 48 horas |

## Como é exibido
Cada chamado mostra um selo colorido:

| Situação | Cor | Quando |
|---|---|---|
| Dentro do prazo | 🟢 verde | Mais de 2 horas até o vencimento |
| Atenção | 🟡 amarelo | Faltam 2 horas ou menos |
| Atrasado | 🔴 vermelho | Prazo vencido — mostra há quanto tempo |

O selo aparece nos cartões, no detalhe e alimenta o indicador de cumprimento de SLA do
[[Fila, Kanban e Dashboard|Dashboard]] e do [[Relatório Mensal]].

## Alertas automáticos
A cada 5 minutos o sistema confere os prazos. Quando um chamado entra em **atenção** ou fica
**atrasado**, aparece um aviso na tela por alguns segundos ("CAM-123 — próximo do prazo!" /
"CAM-123 — PRAZO ESTOURADO!"), uma vez por situação.
- O aviso vai **só para Atendentes e Admins** conectados. Solicitantes não recebem.
- O aviso mostra só o número do chamado, sem título nem conteúdo.
- Hoje o aviso vai para **todos** os Atendentes, inclusive os que não enxergam aquele chamado
  (por exemplo, de outra equipe). Eles veem apenas o número.

## Regras de negócio
- O prazo é calculado **na abertura**, a partir da prioridade.
- **Mudar a prioridade recalcula o prazo** a partir do momento da mudança.
- Hoje o prazo corre em **horas corridas** (inclui noites, fins de semana e feriados).

## Pontos de atenção para o negócio
- **Horas corridas × horas úteis:** um chamado *Baixa* aberto na sexta à tarde vence no domingo.
  Se o combinado com as áreas for horário comercial, o cálculo precisa mudar.
- **Média = 16h:** a definição original falava em "12 a 16 horas"; o sistema usa 16h.
- Ainda não existe filtro da lista por situação de SLA na tela.
