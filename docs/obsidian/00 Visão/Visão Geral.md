---
tipo: visão
status: vigente
atualizado: 2026-10-02
spec: .specs/project/PROJECT.md
tags: [visão]
---

# Visão Geral

## O que é

O **Portal de Chamados** é o sistema interno da CAMARJ para registrar, distribuir, acompanhar e
medir as solicitações que os colaboradores fazem às equipes de atendimento. Cada solicitação vira
um **chamado** com número, responsável, prioridade, prazo e histórico completo.

Além dos chamados, o portal tem um **chat corporativo** para conversas internas em tempo real.

## Problema que resolve

Antes do portal, as solicitações chegavam de forma informal — e-mail, telefone, mensagem — e:

- não havia registro único nem número para referenciar uma solicitação;
- ninguém sabia o status, a prioridade ou o prazo de um pedido;
- não existia histórico por categoria nem métrica de desempenho para apresentar à gestão.

## Quem usa

| Perfil | Quem é | O que faz, em resumo |
|---|---|---|
| **Solicitante** | Colaboradores da CAMARJ | Abre chamados e acompanha os seus |
| **Atendente** | Equipes de atendimento | Assume, trata e resolve chamados da sua equipe |
| **Admin** | Gestão do sistema | Tudo do Atendente + cadastros, reatribuição e controle total |

Detalhes em [[Perfis e Permissões]].

## O que o sistema oferece hoje

- **Chamados de ponta a ponta** — abertura com sugestão automática de área e tipo, número `CAM-N`,
  comentários públicos e internos, anexos e histórico de tudo o que aconteceu.
  → [[Abertura de Chamados]], [[Acompanhamento do Chamado]]
- **Trabalho em equipe** — chamados visíveis para a equipe inteira do atendente, cobrindo férias e
  ausências. → [[Grupos e Equipes]]
- **Prazos (SLA)** — cada chamado tem prazo conforme a prioridade, com alerta visual de atraso.
  → [[SLA]]
- **Gestão do atendimento** — fila por prioridade, quadro Kanban e dashboard em tempo real.
  → [[Fila, Kanban e Dashboard]]
- **Prestação de contas** — relatório mensal com exportação para apresentar à superintendência.
  → [[Relatório Mensal]]
- **Memória** — chamados finalizados nunca são apagados; ficam consultáveis no arquivo.
  → [[Arquivo de Chamados]]
- **Comunicação interna** — chat com conversas privadas e em grupo. → [[Chat Corporativo]]

## Fora do escopo atual

- Abertura automática de chamados por e-mail — planejada, ver [[Abertura por E-mail]].
- Aplicativo móvel — o portal é web; decisão de começar pela web.
- Notificações fora do navegador (push, desktop).

## Números de referência

| Item | Quantidade |
|---|---|
| Perfis de acesso | 3 |
| Áreas (equipes) | 8 |
| Tipos de chamado | 5 |
| Equipes (grupos) | 6 |
| Faixas de prioridade / SLA | 4 |
