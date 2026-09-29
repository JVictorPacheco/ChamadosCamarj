---
tipo: funcionalidade
status: vigente
atualizado: 2026-09-29
spec: .specs/features/grupos-equipes/spec.md
tags: [funcionalidade, equipes]
---

# Grupos e Equipes

## Para que serve
Permitir o **trabalho em equipe**: quando um atendente sai de férias ou se ausenta, os colegas da
mesma equipe continuam enxergando e tratando os chamados dele.

## Equipes existentes
Reembolso · Credenciado · Comercial · Contas Médicas · Autorização/Auditoria · Atendimento

## Regras de negócio
- Cada usuário pertence a **no máximo uma** equipe.
- O **Atendente** vê: os chamados sem responsável (fila), os seus e os dos **colegas da mesma
  equipe** — e pode atuar neles.
- Sem equipe, o Atendente vê apenas a fila e os próprios chamados.
- O **Admin** cria, edita e desativa equipes e define a equipe de cada usuário. Ver
  [[Administração]].
- A equipe também é **sugerida automaticamente** na abertura, junto com a categoria. Ver
  [[Abertura de Chamados]].

## Não confundir
- **Equipe** (esta nota) organiza quem atende chamados.
- **Grupo de chat** é uma conversa coletiva no [[Chat Corporativo]] — são coisas independentes.
- **Categoria** é o assunto do chamado; equipe é quem atende. Os nomes se parecem porque cada
  área costuma ter a sua categoria.
