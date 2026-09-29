---
tipo: visão
status: vigente
atualizado: 2026-09-29
spec: .specs/features/fase-6-admin-log/spec.md
tags: [visão, permissões]
---

# Perfis e Permissões

Todo usuário tem **um perfil** (Solicitante, Atendente ou Admin) e, opcionalmente, **uma equipe**
([[Grupos e Equipes]]). O acesso ao chat é controlado à parte (ver no fim desta nota).
Usuários são cadastrados pelo Admin — não existe autocadastro. Ver [[Administração]].

## Os três perfis

- **Solicitante** — colaborador que precisa de algo. Abre chamados e acompanha os seus.
- **Atendente** — membro de uma equipe de atendimento. Trata os chamados da fila e da sua equipe.
- **Admin** — gestor do sistema. Pode tudo o que o Atendente pode, mais os cadastros e as ações
  de controle (reatribuir, mudar prioridade, forçar encerramento).

## Quais chamados cada perfil vê

| Perfil | Vê |
|---|---|
| Solicitante | Os chamados que **ele abriu** |
| Atendente | Chamados **sem responsável** (a fila), os **seus** e os dos **colegas da mesma equipe** |
| Admin | **Todos** |

## Matriz de ações sobre chamados

| Ação | Solicitante | Atendente | Admin |
|---|:-:|:-:|:-:|
| Abrir chamado | ✅ | ✅ | ✅ |
| Comentar (público) | ✅ | ✅ | ✅ |
| Comentar **interno** / ver comentários internos | — | ✅ | ✅ |
| Anexar arquivo | ✅ | ✅ | ✅ |
| Remover anexo | só os seus | só os seus | qualquer um |
| Assumir (Aberto → Em andamento) | — | ✅ | ✅ |
| Resolver | — | ✅ | ✅ |
| Encerrar (Resolvido → Fechado) | — | ✅ | ✅ |
| Cancelar (enquanto Aberto ou Em andamento) | ✅ | ✅ | ✅ |
| Reabrir chamado finalizado | — | ✅ | ✅ |
| Reatribuir para outro atendente | — | — | ✅ |
| Alterar prioridade | — | — | ✅ |
| Forçar encerramento | — | — | ✅ |

## Acesso às telas

| Tela | Solicitante | Atendente | Admin |
|---|:-:|:-:|:-:|
| Abrir chamado, Meus chamados, Arquivo | ✅ | ✅ | ✅ |
| Fila, Kanban, Dashboard | — | ✅ | ✅ |
| Relatório Mensal | — | ✅ só os próprios números | ✅ completo |
| Admin: Usuários, Categorias, Grupos | — | — | ✅ |
| Chat | depende do acesso ao chat | depende do acesso ao chat | depende do acesso ao chat |

> O bloqueio das telas é **real**: não basta esconder o link — quem não tem permissão e tenta
> acessar pelo endereço recebe uma mensagem de acesso negado.

## Acesso ao chat

Independente do perfil acima, o Admin define o nível de cada pessoa no chat:

| Nível | Pode |
|---|---|
| Sem acesso | Nada no chat (ainda aparece na lista de presença) |
| Participante | Conversar em privado e participar de grupos |
| Criador de grupo | Tudo do Participante + criar grupos |

Detalhes em [[Chat Corporativo]].

## Regras de proteção

- O sistema **impede remover ou desativar o último Admin ativo** — nunca fica sem administrador.
- Toda ação relevante sobre um chamado é registrada no histórico com o autor real (quem estava
  logado), não um nome digitado.
