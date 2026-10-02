---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-02
spec: .specs/features/dashboard-kanban-navegacao/spec.md, .specs/features/correcoes-pre-deploy/spec.md
tags: [funcionalidade, gestão]
---

# Fila, Kanban e Dashboard

Três telas de gestão do dia a dia, exclusivas de **Atendentes e Admin**.

## Fila de Atendimento
**Para quê:** saber o que ainda não tem dono.
- Lista os chamados **Abertos**, ordenados por **prioridade**.
- É daqui que o atendente escolhe o que **assumir**.

## Kanban
**Para quê:** visão do fluxo inteiro num quadro.
- Uma coluna por status; cada cartão é um chamado.
- **Arrastar** o cartão entre colunas muda o status — respeitando as regras do
  [[Ciclo de Vida do Chamado]].
- **Clicar** no cartão abre o detalhe.
- Se outra pessoa alterou o chamado depois que o quadro foi carregado, mover o cartão é recusado:
  aparece o aviso de [[Acompanhamento do Chamado#Proteção contra edição simultânea|edição
  simultânea]] acima do quadro e o cartão volta à coluna em que estava.

## Dashboard
**Para quê:** a "foto do momento" da operação.
- Indicadores de hoje (ex.: resolvidos hoje, tempo médio de resolução).
- **Distribuição por situação** — quantos chamados estão em cada status agora.
- Chamados ativos por **prioridade**, por **área** e por **tipo** (clicar numa barra abre a lista filtrada).
- Cumprimento do **SLA** no mês. Ver [[SLA]].
- **Tudo é clicável:** clicar numa fatia ou barra abre a lista de chamados já filtrada por aquele
  recorte.

> **Dashboard × Relatório Mensal:** o dashboard mostra o **agora** e muda a todo momento; o
> [[Relatório Mensal]] mostra um **mês fechado** e serve para prestação de contas.

## Regras
- Solicitantes não acessam estas telas — o bloqueio é real, não só visual.
- As três telas se atualizam em tempo real.
