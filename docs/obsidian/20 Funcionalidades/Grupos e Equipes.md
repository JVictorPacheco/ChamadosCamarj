---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-06
spec: .specs/features/area-e-tipo-do-chamado/spec.md
tags: [funcionalidade, equipes]
---

# Grupos e Equipes

## Para que serve
Permitir o **trabalho em equipe**: quando alguém sai de férias ou se ausenta, os colegas da mesma
equipe continuam enxergando e acompanhando os chamados dele.

> **Exemplo:** dois analistas do Reembolso estão na equipe "Reembolso". Um deles abre chamados e
> sai de férias. O outro passa a ver esses chamados e consegue acompanhar, comentar e anexar
> arquivos neles, sem precisar pedir acesso a ninguém.

## Equipes existentes
Reembolso · Credenciado · Comercial · Contas Médicas · Autorização/Auditoria · Atendimento

## O que é um "chamado da equipe"
Um chamado é da equipe quando **foi aberto por** um membro da equipe, está **sob a
responsabilidade** de um membro da equipe **ou** tem a **área** da equipe.

## Regras de negócio
- Cada usuário pertence a **no máximo uma** equipe.
- **Solicitante com equipe** vê os chamados que abriu **mais** os chamados da equipe. Nos chamados
  dos colegas pode **ver, comentar e anexar**, mas **não cancela**: cancelar é só de quem abriu.
- **Solicitante sem equipe** vê apenas os chamados que abriu.
- **Atendente** vê a fila (chamados sem responsável), os seus, os que ele mesmo abriu e os
  chamados da equipe, e pode atuar neles. Sem equipe, vê a fila, os seus e os que abriu.
- **Admin** vê todos os chamados, **mesmo pertencendo a uma equipe**.
- O **Admin** cria, edita e desativa equipes e define a equipe de cada usuário. Ver
  [[Administração]]. A troca de equipe de uma pessoa vale a partir do **próximo login** dela.
- A lista de equipes é também a lista de **áreas** da abertura de chamado: a área já vem com a
  equipe de quem abre. Ver [[Abertura de Chamados]] e [[ADR-008 Área do chamado é a equipe]].

As regras completas de quem vê e quem faz o quê estão em [[Perfis e Permissões]].

## Não confundir
- **Equipe** (esta nota) organiza quem enxerga e acompanha chamados.
- **Grupo de chat** é uma conversa coletiva no [[Chat Corporativo]]. São coisas independentes.
- **Tipo** do chamado (incidente, dúvida...) não tem relação com a equipe — é a natureza do pedido.
