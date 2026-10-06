---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-06
spec: .specs/features/fase-6-admin-log/spec.md, .specs/features/controle-de-acesso/spec.md
tags: [funcionalidade, admin]
---

# Administração

Telas exclusivas do **Admin**, no menu *Admin*.

## Usuários
- Cadastrar com **nome, e-mail, perfil, equipe e senha inicial**.
- Editar perfil, equipe e situação (ativo/inativo).
  - Mudar o perfil zera os ajustes de telas da pessoa e a desconecta, pedindo novo login.
  - Mudar a equipe vale a partir do próximo login da pessoa.
  - Cadastrar de novo um e-mail de conta desativada **reativa** a conta, já no padrão de telas do perfil.
- **Redefinir senha** de um usuário.
- Proteção: não é possível desativar nem rebaixar o **último Admin ativo**.

## Controle de acesso
Lista de todas as pessoas ativas, com busca, mostrando as telas e o acesso ao chat de cada uma.
- **Ajustar acessos** (Solicitantes e Atendentes): ligar e desligar telas (Arquivo, Kanban, Fila,
  Dashboard, Relatório Mensal) dentro do que o perfil permite, e definir o **acesso ao chat** (Sem
  acesso, Participante, Criador de grupo). Cada tela mostra se está no "padrão do perfil" ou "ajustado".
- **Voltar ao padrão do perfil**: desfaz todos os ajustes de telas da pessoa.
- **Ajustar Chat** (Admins): Admin tem todas as telas sempre; só o chat é ajustável, inclusive o próprio.
- **Histórico de mudanças** em cada painel: quem mudou, quando, o que era e o que ficou.
- A mudança vale na hora para a pessoa. Se o perfil dela mudou enquanto o painel estava aberto, salvar é
  recusado e o painel pede para reabrir.

As regras de quais telas cada perfil pode ter estão em [[Perfis e Permissões]]. Ver
[[ADR-010 Acesso por módulo ajustável por pessoa]].

## Tipos de chamado
- Criar, editar, desativar e reativar os tipos: Incidente, Dúvida, Solicitação, Customização e
  Melhoria são os iniciais.
- Um tipo desativado some da abertura, mas continua aparecendo nos chamados antigos.
- As edições do Admin ficam; o sistema não volta os nomes ao padrão quando reinicia.

## Áreas e Grupos (equipes)
As áreas usadas na abertura de chamados **são** as equipes cadastradas aqui.
- Criar, editar, ativar e desativar equipes. Ver [[Grupos e Equipes]].

## Ações de controle sobre chamados
Disponíveis na tela do chamado, só para o Admin:
- **Reatribuir** o responsável.
- **Alterar a prioridade** (recalcula o [[SLA]]).
- **Forçar o encerramento**, com motivo. Ver [[Encerramento e Cancelamento]].

## Auditoria
- Histórico completo de cada chamado.
- Histórico do chat, incluindo mensagens editadas e excluídas.
- Histórico de acessos de cada pessoa (telas, chat, perfil, reativação), na tela *Controle de acesso*.
