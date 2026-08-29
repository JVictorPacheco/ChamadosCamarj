# Chat Corporativo — Especificação

> **Status:** `Em andamento`
> **Branch:** `feature/chat-corporativo`
> **Criada em:** 2026-08-29
> **Atualizada em:** 2026-08-29

---

## 1. Problema

**Situação atual:**
Não existe canal de comunicação direto entre usuários dentro do sistema. Toda comunicação acontece por e-mail ou ferramentas externas (WhatsApp, Teams), sem rastreabilidade e sem visibilidade de disponibilidade.

**Impacto:**
- Atendentes e admins não sabem quem está disponível no momento
- Comunicações internas sobre chamados ficam fragmentadas em ferramentas externas
- Sem histórico auditado de comunicações internas

**Solução esperada:**
Uma aba de chat corporativo integrada ao sistema, com presença em tempo real, mensagens 1:1 e grupos, controle de acesso granular pelo admin e log completo de todas as ações.

---

## 2. Fora de Escopo (V1)

- Notificações push do navegador (browser push API) — V2
- Mensagens de voz/áudio — V2
- Busca no histórico de mensagens — V2
- Status "Em reunião" — V2
- Fixar mensagem no topo — V2
- Preview automático de links — V2
- Export de histórico de conversa — V2
- Videochamada — fora do escopo permanentemente

---

## 3. User Stories

| ID | User Story |
|----|------------|
| US-01 | Como **Admin**, quero conceder ou revogar acesso ao chat por usuário (com nível: Participante ou CriadorDeGrupo), para que eu controle quem pode usar a funcionalidade. |
| US-02 | Como **qualquer usuário autenticado**, quero ver o status de presença (Online, Ausente, Offline) de todos os usuários do sistema, para que eu saiba quem está disponível no momento. |
| US-03 | Como **Participante ou CriadorDeGrupo**, quero enviar e receber mensagens 1:1 com outros usuários que têm acesso ao chat, para que eu me comunique internamente sem sair do sistema. |
| US-04 | Como **CriadorDeGrupo**, quero criar grupos de chat com nome e participantes selecionados, para que equipes se comuniquem coletivamente. |
| US-05 | Como **Participante ou CriadorDeGrupo**, quero enviar arquivos (PDF, imagens, Office, ZIP — máx 10MB) em conversas, para que eu compartilhe documentos sem usar e-mail. |
| US-06 | Como **Participante ou CriadorDeGrupo**, quero usar emojis e reagir a mensagens com emoji, para que a comunicação seja mais expressiva. |
| US-07 | Como **Participante ou CriadorDeGrupo**, quero editar minhas próprias mensagens (com registro de alteração), para que eu corrija erros sem perder o contexto. |
| US-08 | Como **Participante ou CriadorDeGrupo**, quero deletar minhas próprias mensagens (com rastro de auditoria), para que eu remova conteúdo enviado por engano. |
| US-09 | Como **Participante ou CriadorDeGrupo**, quero ver quando minha mensagem foi lida pelo destinatário, para que eu saiba se a comunicação chegou. |
| US-10 | Como **Participante ou CriadorDeGrupo**, quero ver o indicador "está digitando..." em tempo real, para que eu saiba que o outro está respondendo. |
| US-11 | Como **Participante ou CriadorDeGrupo**, quero receber notificações de novas mensagens via badge vermelho na aba do chat, para que eu perceba mensagens sem estar na tela do chat. |
| US-12 | Como **Participante ou CriadorDeGrupo**, quero responder a uma mensagem específica com citação, para que o contexto da resposta fique claro. |
| US-13 | Como **Admin**, quero que todas as ações do chat gerem log auditado, para que eu possa rastrear o histórico de comunicações e ações administrativas. |

---

## 4. Critérios de Aceitação

### US-01 — Controle de acesso pelo Admin

- **AC-01:** Dado que sou Admin na tela de gerenciamento de usuários, quando visualizo a lista, então vejo uma coluna "Chat" com o nível atual de cada usuário (`Sem Acesso`, `Participante`, `Criador de Grupo`).
- **AC-02:** Dado que sou Admin, quando altero o `ChatPerfil` de um usuário, então a mudança é aplicada imediatamente e o usuário afetado recebe ou perde acesso ao chat em tempo real via SignalR.
- **AC-03:** Dado que sou Admin e revogo o acesso de um usuário com conversas ativas, quando o acesso é revogado, então o usuário vê a mensagem "Seu acesso ao chat foi revogado" na tela e os demais participantes das conversas veem uma mensagem de sistema `[Nome] teve o acesso ao chat revogado` na conversa. As mensagens anteriores permanecem visíveis para os outros participantes.
- **AC-04:** Dado que sou Admin, quando concedo `ChatPerfil = CriadorDeGrupo`, então o usuário pode criar grupos além de participar de conversas 1:1.

### US-02 — Presença (todos os usuários)

- **AC-05:** Dado que sou qualquer usuário autenticado, quando acesso o sistema, então vejo um painel ou indicador com o status de presença de todos os usuários (`Online`, `Ausente`, `Offline`), independentemente de ter ou não acesso ao chat.
- **AC-06:** Dado que o frontend está ativo, quando o usuário não interage por 5 minutos, então seu status muda para `Ausente`.
- **AC-07:** Dado que o usuário está `Ausente`, quando passa 15 minutos sem interação, então seu status muda para `Offline`.
- **AC-08:** Dado que o usuário retoma a interação, quando o frontend envia o próximo heartbeat (a cada 30s), então o status volta para `Online` imediatamente.
- **AC-09:** Dado que o usuário faz logout, quando a sessão encerra, então seu status é marcado como `Offline` imediatamente.

### US-03 — Mensagens 1:1

- **AC-10:** Dado que tenho `ChatPerfil = Participante` ou `CriadorDeGrupo`, quando inicio uma conversa com outro usuário que também tem acesso ao chat, então a conversa é criada e a primeira mensagem é entregue em tempo real via SignalR.
- **AC-11:** Dado que estou em uma conversa 1:1, quando o outro participante perde o acesso ao chat, então vejo a mensagem de sistema `[Nome] teve o acesso ao chat revogado` na conversa.

### US-04 — Grupos

- **AC-12:** Dado que tenho `ChatPerfil = CriadorDeGrupo`, quando crio um grupo com nome e ao menos 2 participantes (todos com acesso ao chat), então o grupo é criado e todos os participantes recebem notificação via SignalR.
- **AC-13:** Dado que sou criador de um grupo, quando adiciono ou removo um participante, então todos os membros veem uma mensagem de sistema registrando a ação (`[Admin] adicionou [Nome]` / `[Admin] removeu [Nome]`).
- **AC-14:** Dado que `ChatPerfil = Participante`, quando tento criar um grupo, então recebo erro `403 Forbidden` com mensagem "Você não tem permissão para criar grupos".

### US-05 — Arquivos

- **AC-15:** Dado que estou em uma conversa, quando envio um arquivo, então são aceitos PDF, imagens (JPG, PNG, GIF, WebP), Office (DOCX, XLSX, PPTX) e ZIP com no máximo 10MB.
- **AC-16:** Dado que envio um arquivo acima de 10MB ou de tipo não permitido, quando o upload é tentado, então recebo erro `400` com mensagem descritiva em português.
- **AC-17:** Dado que um arquivo é enviado com sucesso, quando clico para visualizar, então recebo uma URL assinada do bucket `chat-arquivos` do Supabase Storage com validade de 1 hora.

### US-06 — Emojis e Reações

- **AC-18:** Dado que estou em uma conversa, quando envio uma mensagem com emoji (via teclado ou seletor), então o emoji é exibido corretamente para todos os participantes.
- **AC-19:** Dado que passo o mouse sobre uma mensagem, quando seleciono uma reação de emoji, então a reação é registrada e exibida abaixo da mensagem com contador para todos os participantes em tempo real.
- **AC-20:** Dado que já reagi a uma mensagem com um emoji, quando clico na mesma reação novamente, então minha reação é removida.

### US-07 — Edição de Mensagens

- **AC-21:** Dado que sou o autor de uma mensagem enviada há menos de 24 horas, quando edito o conteúdo, então a mensagem é atualizada para todos os participantes em tempo real com a label `(editado)` e o timestamp da última edição.
- **AC-22:** Dado que edito uma mensagem, então o conteúdo original é preservado no log de auditoria (`ChatHistorico`) com a ação `MensagemEditada`.

### US-08 — Exclusão de Mensagens

- **AC-23:** Dado que sou o autor de uma mensagem, quando a deleto, então todos os participantes veem `[mensagem removida]` no lugar do conteúdo original.
- **AC-24:** Dado que um Admin deleta qualquer mensagem, então o mesmo comportamento se aplica: `[mensagem removida]` para todos.
- **AC-25:** Dado que uma mensagem é deletada, então o conteúdo original é preservado no log de auditoria (`ChatHistorico`) com a ação `MensagemDeletada` — visível apenas para Admin.

### US-09 — Read Receipts

- **AC-26:** Dado que enviei uma mensagem em uma conversa 1:1, quando o destinatário abre a conversa, então minha mensagem exibe o indicador `Visto` com timestamp.
- **AC-27:** Dado que estou em um grupo, quando todos os participantes leram uma mensagem, então o indicador `Visto por todos` é exibido.

### US-10 — Typing Indicator

- **AC-28:** Dado que estou em uma conversa, quando o outro participante começa a digitar, então vejo `[Nome] está digitando...` em tempo real via SignalR.
- **AC-29:** Dado que o participante para de digitar por 3 segundos ou envia a mensagem, então o indicador desaparece.

### US-11 — Notificações

- **AC-30:** Dado que estou em qualquer tela do sistema, quando recebo uma mensagem nova no chat, então o ícone do chat na sidebar exibe um badge vermelho com o número de mensagens não lidas.
- **AC-31:** Dado que abro a conversa com mensagens não lidas, então o badge é zerado automaticamente.
- **AC-32:** Dado que tenho mensagens não lidas em múltiplas conversas, então o badge exibe a soma total de mensagens não lidas.

### US-12 — Resposta com Citação

- **AC-33:** Dado que estou em uma conversa, quando seleciono "Responder" em uma mensagem, então minha nova mensagem é enviada com a citação da mensagem original exibida acima.
- **AC-34:** Dado que clico na citação de uma resposta, então a conversa rola até a mensagem original.

### US-13 — Auditoria (todos os perfis)

- **AC-35:** Dado que qualquer ação ocorre no chat (lista completa abaixo), então um registro é criado em `ChatHistorico` com: `usuarioId`, `usuarioNome`, `acao`, `detalhe`, `dataHora`.
- **AC-36:** Dado que sou Admin, quando acesso os logs de chat, então vejo o histórico completo de ações incluindo conteúdo de mensagens deletadas.

**Ações que geram log obrigatório:**

| Ação | Enum `ChatAcao` |
|------|----------------|
| Acesso concedido | `AcessoConcedido` |
| Acesso revogado | `AcessoRevogado` |
| Mensagem enviada | `MensagemEnviada` |
| Mensagem editada (com conteúdo anterior) | `MensagemEditada` |
| Mensagem deletada (com conteúdo original) | `MensagemDeletada` |
| Arquivo enviado | `ArquivoEnviado` |
| Grupo criado | `GrupoCriado` |
| Grupo deletado | `GrupoDeletado` |
| Participante adicionado ao grupo | `ParticipanteAdicionado` |
| Participante removido do grupo | `ParticipanteRemovido` |
| Reação adicionada | `ReacaoAdicionada` |
| Reação removida | `ReacaoRemovida` |

### Critérios Transversais

- **AC-37:** Todos os endpoints do chat exigem autenticação JWT válida.
- **AC-38:** Endpoints que exigem `ChatPerfil` retornam `403` com mensagem em português para usuários sem acesso.
- **AC-39:** `dotnet test` passa sem falhas após a implementação.
- **AC-40:** `npm run build` passa sem erros ou warnings após a implementação.

---

## 5. Rastreabilidade

> Preencher após implementação.

| Critério | Arquivo de Teste | Método | Status |
|----------|-----------------|--------|--------|
| AC-01 | `ChatPerfilHandlerTests.cs` | `Handle_Admin_ExibeColunaChatNaListagem` | ⬜ Pendente |
| AC-02 | `DefinirChatPerfilHandlerTests.cs` | `Handle_Admin_AlteraPerfilComSucesso` | ⬜ Pendente |
| AC-03 | `DefinirChatPerfilHandlerTests.cs` | `Handle_RevogarAcesso_EnviaMensagemSistema` | ⬜ Pendente |
| AC-04 | `DefinirChatPerfilHandlerTests.cs` | `Handle_ConcederCriadorDeGrupo_PermiteCrearGrupo` | ⬜ Pendente |
| AC-05 a AC-09 | `ChatPresencaHandlerTests.cs` | `Handle_Heartbeat_*` | ⬜ Pendente |
| AC-10 a AC-11 | `EnviarMensagemHandlerTests.cs` | `Handle_*` | ⬜ Pendente |
| AC-12 a AC-14 | `CriarGrupoHandlerTests.cs` | `Handle_*` | ⬜ Pendente |
| AC-15 a AC-17 | `EnviarArquivoHandlerTests.cs` | `Handle_*` | ⬜ Pendente |
| AC-21 a AC-22 | `EditarMensagemHandlerTests.cs` | `Handle_*` | ⬜ Pendente |
| AC-23 a AC-25 | `DeletarMensagemHandlerTests.cs` | `Handle_*` | ⬜ Pendente |
| AC-35 a AC-36 | `ChatHistoricoHandlerTests.cs` | `Handle_*` | ⬜ Pendente |
| AC-18 a AC-20, AC-26 a AC-34 | Manual (UI) | Verificação visual | ⬜ Pendente |

---

## 6. Dependências

- Depende de: Supabase Storage (bucket `chat-arquivos` — criar antes da implementação)
- Depende de: SignalR Hub já existente (reusar ou criar `ChatHub` separado)
- Depende de: `UsuarioPerfil` (adicionar campo `ChatPerfil`)
- Bloqueia: nenhuma feature futura identificada

---

## 7. Gate Checks

- [ ] `dotnet test` — X testes, 0 falhas
- [ ] `npm run build` — 0 erros, 0 warnings
- [ ] ACs verificados (testes automatizados + verificação manual da UI)
- [ ] `spec.md` atualizada com status final e rastreabilidade preenchida
- [ ] `STATE.md` atualizado com resumo da sessão
- [ ] PR aberto com base `develop`
