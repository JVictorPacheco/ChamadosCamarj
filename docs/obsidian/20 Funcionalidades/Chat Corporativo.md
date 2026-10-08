---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-06
spec: .specs/features/chat-corporativo/spec.md
tags: [funcionalidade, chat]
---

# Chat Corporativo

## Para que serve
Comunicação interna rápida e **registrada**, dentro do próprio portal — sem depender de
aplicativos pessoais de mensagem.

## Quem usa
Quem recebeu acesso do Admin, na tela *Controle de acesso* (ver [[Administração]]). O acesso ao chat é
independente do perfil de chamados e vale também para Admins (o próprio inclusive):

| Nível | Pode |
|---|---|
| Sem acesso | Não usa o chat |
| Participante | Conversas privadas e participação em grupos |
| Criador de grupo | Tudo acima + criar grupos |

Conceder ou revogar vale **na hora**: a pessoa ganha ou perde o acesso sem precisar sair do
sistema, e os participantes das conversas dela são avisados.

## O que dá para fazer
| Recurso | Detalhe |
|---|---|
| Conversa privada | Entre duas pessoas com acesso ao chat |
| Grupo | Nome + pelo menos 2 participantes; criador ou Admin adiciona e remove membros |
| Arquivos | Mesmas regras dos [[Anexos]]: PDF, imagens, Office, ZIP, até 10 MB. Imagens aparecem direto na conversa |
| Emojis e reações | Reagir a uma mensagem; clicar de novo remove a reação |
| Responder com citação | A resposta mostra a mensagem original; clicar leva até ela |
| Editar mensagem | Só o autor, até **24 horas** após o envio; aparece "(editado)" |
| Excluir mensagem | O autor exclui as suas; o Admin exclui qualquer uma. Aparece "[mensagem removida]" |
| "Digitando…" | Mostra quando o outro está escrevendo |
| Confirmação de leitura | "Visto" na conversa privada; "Visto por todos" no grupo |
| Mensagens não lidas | Contador vermelho no menu do chat, em qualquer tela |

## Presença
Todos veem o status dos colegas:

| Status | Quando |
|---|---|
| Online | Usando o sistema |
| Ausente | 5 minutos sem interação |
| Offline | 15 minutos sem interação, ou ao sair do sistema |

## Privacidade da confirmação de leitura
Em **Preferências**, cada pessoa pode desligar a confirmação de leitura. A regra é **recíproca**,
como no WhatsApp: quem desliga **deixa de mostrar e também deixa de ver** o "Visto". A leitura só
aparece quando os dois lados estão com a opção ligada.

## Auditoria
Toda ação no chat é registrada (acesso concedido/revogado, mensagem enviada/editada/excluída,
arquivo, grupo criado, membro adicionado/removido, reações). O **Admin** consulta esse histórico,
incluindo o **conteúdo original** de mensagens editadas ou excluídas.

> **Importante para os usuários:** "excluir" uma mensagem a remove da conversa, mas o conteúdo
> continua acessível ao Admin para fins de auditoria.
