---
tipo: visão
status: vigente
atualizado: 2026-10-06
spec: .specs/features/autorizacao-chamados/spec.md, .specs/features/editar-chamado/spec.md, .specs/features/controle-de-acesso/spec.md
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

"Chamados da equipe" são os **abertos por** um colega da mesma equipe, os **sob responsabilidade**
de um colega da mesma equipe e os com a **área** da equipe. Ver [[Grupos e Equipes]].

| Perfil | Vê |
|---|---|
| Solicitante | Os chamados que **ele abriu** e, se tiver equipe, os **chamados da equipe** |
| Atendente | Chamados **sem responsável** (a fila), os **seus**, os que **ele abriu** e os **chamados da equipe** |
| Admin | **Todos**, mesmo pertencendo a uma equipe |

Um chamado fora dessa lista **não existe** para o usuário: ele não aparece em nenhuma tela,
lista, busca ou número do Dashboard, e tentar abri-lo pelo endereço resulta em "Chamado não
encontrado".

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
| Cancelar (enquanto Aberto ou Em andamento) | só os que abriu | ✅ | ✅ |
| Editar título e descrição (chamado não encerrado) | os que abriu, enquanto ninguém assumiu | os que abriu (enquanto ninguém assumiu) e os que assumiu | todos |
| Reclassificar o tipo do chamado | — | ✅ | ✅ |
| Reabrir chamado finalizado | — | ✅ | ✅ |
| Reatribuir para outro atendente | — | — | ✅ |
| Alterar prioridade | — | — | ✅ |
| Forçar encerramento | — | — | ✅ |

## Acesso às telas (módulos)

O perfil define o **padrão** de telas de cada pessoa. O Admin pode dar ou tirar telas **pessoa a pessoa**
na tela *Controle de acesso* (ver [[Administração]]). A tabela mostra o padrão e o que pode ser ajustado:

| Tela | Solicitante | Atendente | Admin |
|---|:-:|:-:|:-:|
| Abrir chamado, Meus chamados | ✅ fixo | ✅ fixo | ✅ fixo |
| Arquivo | ✅ ajustável | ✅ ajustável | ✅ fixo |
| Fila, Kanban | — nunca | ✅ ajustável | ✅ fixo |
| Dashboard | — ajustável (pode ganhar) | ✅ ajustável | ✅ fixo |
| Relatório Mensal | — ajustável (pode ganhar) | ✅ ajustável, só os próprios números | ✅ completo, fixo |
| Admin: Usuários, Controle de acesso, Tipos de chamado, Áreas e Grupos | — | — | ✅ |
| Chat | depende do acesso ao chat | depende do acesso ao chat | depende do acesso ao chat |

- **Admin tem sempre todas as telas** — só o acesso ao chat dele é ajustável (inclusive o próprio).
- **Fila e Kanban são de atendimento**: Solicitante nunca recebe essas telas.
- Quem ganha Dashboard ou Relatório vê **só os números dos chamados que já pode ver** — ganhar a tela não
  amplia a visibilidade de chamados.
- O controle é da **tela inteira**. Ações dentro das telas (comentário interno, cancelar, anexar etc.)
  continuam seguindo a matriz de ações acima.
- A mudança vale **na hora**: o menu da pessoa se atualiza sozinho e, se ela estiver numa tela que
  perdeu, sai dela com o aviso "Seu acesso a este módulo foi retirado.".
- **Mudança de perfil** (em *Usuários*): os ajustes de telas da pessoa são zerados — ela passa ao padrão do
  perfil novo; o acesso ao chat é mantido. Ela é desconectada com o aviso "Seu perfil foi alterado. Entre
  novamente." — na hora, se estiver com o sistema aberto, ou quando abrir o sistema/voltar a ter conexão.
- **Mudança de equipe** (em *Usuários*): vale a partir do **próximo login** da pessoa.
- Toda mudança de acesso (telas, chat, perfil, reativação de conta) fica no **histórico de acessos** da
  pessoa, com quem mudou, quando, antes e depois. Ver [[ADR-010 Acesso por módulo ajustável por pessoa]].

> O bloqueio é **real** e acontece no sistema, não só na tela: esconder um botão ou link é só
> conveniência. Quem tenta uma ação sem permissão, mesmo usando ferramentas do navegador, recebe
> "acesso negado"; quem tenta abrir um chamado que não pode ver recebe "não encontrado". Ver
> [[ADR-007 Permissões aplicadas no servidor]].
>
> Todas as ações de chamado valem só sobre chamados que a pessoa **pode ver**. A tabela de ações
> acima vale dentro desse limite. Na abertura, o solicitante registrado é sempre quem está logado.

## Acesso ao chat

Independente do perfil acima, o Admin define o nível de cada pessoa no chat, na tela *Controle de acesso*:

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
