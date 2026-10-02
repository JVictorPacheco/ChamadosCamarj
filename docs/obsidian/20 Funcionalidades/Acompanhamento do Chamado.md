---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-02
spec: .specs/features/fase-6-admin-log/spec.md
tags: [funcionalidade, chamado]
---

# Acompanhamento do Chamado

## Para que serve
Concentrar numa única tela tudo sobre um chamado: situação, prazo, conversa, arquivos e histórico.

## Quem usa
Todos os perfis, cada um vendo o que lhe cabe. Ver [[Perfis e Permissões]].

## A tela de detalhe
| Bloco | Conteúdo |
|---|---|
| Cabeçalho | Número `CAM-N`, título, status, prioridade, área, tipo, responsável. O Atendente pode corrigir o tipo |
| Prazo | Selo colorido do [[SLA]] com o tempo restante ou o atraso |
| Ações | Botões que mudam conforme o status e o perfil (assumir, resolver, encerrar, cancelar, reabrir, reatribuir, prioridade, forçar encerramento) |
| Comentários | Conversa do chamado, com anexos por comentário |
| Anexos | Arquivos do chamado. Ver [[Anexos]] |
| Histórico | Linha do tempo de tudo o que aconteceu |

## Comentários
- **Públicos** — visíveis para todos os envolvidos; é o canal com o solicitante.
- **Internos** — só Atendentes e Admin veem e escrevem. Servem para alinhamento da equipe sem
  expor ao solicitante.
- É possível anexar arquivos ao comentar.

## Histórico (auditoria)
Registrado automaticamente, sem ação do usuário. Cada entrada tem **quem**, **o quê** e **quando**:
criação, assumir, reatribuir, resolver, fechar, cancelar, reabrir, mudança de status, mudança de
prioridade, comentário e encerramento forçado — incluindo o **motivo**, quando houver.

O histórico também distingue ações feitas por **pessoas** das feitas **automaticamente pelo
sistema**, preparando o terreno para automações futuras.

## Tempo real
Mudanças feitas por outra pessoa aparecem sem recarregar a página.

## Proteção contra edição simultânea
Se duas pessoas alterarem o mesmo chamado ao mesmo tempo, a segunda recebe um aviso de conflito
em vez de sobrescrever silenciosamente a alteração da primeira.
