---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-02
spec: .specs/features/fase-6-admin-log/spec.md
tags: [funcionalidade, admin]
---

# Administração

Telas exclusivas do **Admin**, no menu *Admin*.

## Usuários
- Cadastrar com **nome, e-mail, perfil, equipe e senha inicial**.
- Editar perfil, equipe e situação (ativo/inativo).
- Definir o **acesso ao chat** (Sem acesso, Participante, Criador de grupo). Ver
  [[Chat Corporativo]].
- **Redefinir senha** de um usuário.
- Proteção: não é possível desativar nem rebaixar o **último Admin ativo**.

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
