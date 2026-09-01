# Chat Corporativo — Especificação

> **Status:** `Código mergeado em develop (PR #28) — Fase 8 e Fase 6 concluídas. Extensão de escopo (AC-46 a AC-52) implementada, verificada ao vivo (Playwright, restauração de acesso nas duas direções) e com gate checks completos em 2026-08-31.`
> **Branch:** `feature/chat-corporativo` (mergeada)
> **Criada em:** 2026-08-29
> **Atualizada em:** 2026-08-31 (extensão de escopo verificada e fechada)

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

### Extensão de escopo — Gerenciamento de membros de grupo (adicionada em 2026-08-31)

> Pedido do usuário ao validar o Bug #5 da Fase 8: grupo precisa funcionar como WhatsApp/Teams —
> lista de membros visível, com opção de iniciar conversa direta a partir dela. Detalhe completo em
> `review-fase8.md`, seção Bug #5.

- **AC-41:** Dado que sou participante de um grupo (qualquer `ChatPerfil` com acesso), quando abro
  a conversa, então consigo ver a lista completa de membros do grupo, com o status de presença de
  cada um.
- **AC-42:** Dado que sou o criador daquele grupo específico ou Admin do sistema, quando estou na
  lista de membros, então vejo opção de adicionar novo participante (entre os que têm acesso ao
  chat) e de remover um participante existente.
- **AC-43:** Dado que **não** sou o criador daquele grupo nem Admin, quando estou na lista de
  membros, então não vejo nenhuma opção de adicionar/remover — só visualizo.
- **AC-44:** Dado que estou na lista de membros de um grupo, quando clico num membro (que não seja
  eu mesmo), então uma conversa direta com essa pessoa é aberta — reaproveitando a mesma lógica já
  usada pelo painel de Presença (`CriarConversaCommandHandler`): se já existe uma conversa privada
  entre nós dois, continua ela; senão, cria uma nova.
- **AC-45:** Dado que um participante é adicionado ou removido de um grupo, então todos os membros
  veem uma mensagem de sistema registrando a ação, e o evento é auditado em `ChatHistorico`
  (`ParticipanteAdicionado`/`ParticipanteRemovido`) — mesmo padrão já usado pra outras ações do chat.

### Extensão de escopo — Restauração de acesso, refresh de perfil e polimento (adicionada em 2026-08-31)

> Pedido do usuário depois de testar a feature completa pela primeira vez. Dois problemas reais
> identificados: (1) o Bug #8b corrigiu a revogação avisando os outros participantes, mas conceder
> acesso de volta continuava mudo — nenhuma mensagem, ninguém avisado; (2) mesmo que avisássemos em
> tempo real, o link "Chat" na barra lateral só aparece com base no que veio no token de login — a
> pessoa restaurada não veria o link sem deslogar e logar de novo.
>
> **Decisão de arquitetura (verificada no código antes de especificar, não suposta):** existe uma
> conexão SignalR global (`SignalRProvider` → `/hubs/chamados`, montada na raiz do app) que fica
> ativa pra **qualquer** usuário logado, independente de ter acesso ao chat — ao contrário da conexão
> ao `ChatHub`, que só existe quando a pessoa está na tela `/chat`. É por esse canal (`ChamadosHub`,
> `Clients.User(id)` — o mesmo `SubClaimUserIdProvider` já é `IUserIdProvider` global, funciona em
> qualquer Hub) que a notificação de restauração precisa passar, não pelo `ChatHub`, senão nunca
> chegaria em quem está justamente sem acesso.

- **AC-46:** Dado que um Admin concede `ChatPerfil` a alguém que estava em `SemAcesso`, quando a
  mudança é salva, então uma mensagem de sistema `"[Nome] teve o acesso ao chat restaurado"` é
  criada em cada conversa onde a pessoa já era participante (os vínculos de participante não são
  removidos na revogação — só o acesso à tela é bloqueado), visível em tempo real pros outros
  participantes que estiverem com a conversa aberta.
- **AC-47:** Dado que a mudança do AC-46 aconteceu, então o usuário restaurado recebe um evento em
  tempo real (`ChatPerfilAtualizado`, via `ChamadosHub`/`Clients.User`) informando o novo
  `ChatPerfil` — chega mesmo que a pessoa não esteja na tela de chat, porque usa o canal global.
- **AC-48:** Dado que o frontend recebe o evento do AC-47, então atualiza `perfil.chatPerfil` no
  `AuthContext` (estado em memória + `localStorage`) sem precisar de logout/login — o link "Chat"
  na barra lateral aparece (ou some, no caso de revogação) dinamicamente.
- **AC-49:** Dado que o seletor de emoji do composer está aberto, quando clico fora dele ou pressiono
  Esc, então ele fecha — sem precisar escolher um emoji ou trocar de conversa (comportamento
  observado como incômodo, sinalizado também pelo review independente da Fase 8).
- **AC-50:** Dado que uma mensagem de arquivo é uma imagem (`image/jpeg`, `image/png`, `image/gif`,
  `image/webp`), então ela é exibida como preview inline na bolha da mensagem (miniatura clicável
  pra abrir em tamanho real), em vez do ícone genérico de arquivo usado pra outros tipos.
- **AC-51:** Dado que estou enviando uma mensagem, então o botão de enviar mostra um indicador visual
  de carregamento enquanto a requisição está pendente (em vez de só ficar desabilitado sem feedback).
- **AC-52:** Dado que o envio de uma mensagem falha, então o conteúdo digitado permanece no campo
  (já era o comportamento) e um aviso claro e específico aparece perto do botão de enviar — não um
  alerta genérico — permitindo tentar de novo com um clique.

### Critérios Transversais

- **AC-37:** Todos os endpoints do chat exigem autenticação JWT válida.
- **AC-38:** Endpoints que exigem `ChatPerfil` retornam `403` com mensagem em português para usuários sem acesso.
- **AC-39:** `dotnet test` passa sem falhas após a implementação.
- **AC-40:** `npm run build` passa sem erros ou warnings após a implementação.

---

## 5. Rastreabilidade

> **Atualizado em 2026-08-31 (3ª vez — Fase 6 concluída):** os 8 arquivos de teste planejados em
> `tasks.md` foram todos criados, mais 2 extras pros handlers novos de gerenciamento de grupo
> (`AdicionarParticipante`/`RemoverParticipante`, sem cobertura nenhuma antes). **70 testes novos**,
> todos passando (216 → 286 no total do projeto). ACs puramente de UI (exibição visual, sem lógica
> de handler por trás) continuam sem automação — só verificação manual — porque não há o que testar
> num handler pra eles; estão marcados como tal, não como lacuna.

| Critério | Arquivo de Teste | Status |
|----------|-----------------|--------|
| AC-01 (coluna "Chat" na listagem) | — (UI pura, sem handler) | ✅ Verificado manualmente |
| AC-02 a AC-04 | `DefinirChatPerfilHandlerTests.cs` (9 testes) | ✅ Implementado e verificado manualmente |
| AC-03 (revogação avisa os outros) | `DefinirChatPerfilHandlerTests.cs` — `Handle_AoRevogarAcesso_DeveCriarMensagemSistemaEmCadaConversaAtivaEPublicarEmTempoReal` | ✅ Corrigido (Bug #8b), com teste automatizado cobrindo o comportamento novo |
| AC-05 a AC-09 | `ChatPresencaHandlerTests.cs` (5 testes) | ✅ Implementado; heartbeat 500 corrigido (Bug #9) |
| AC-10 a AC-11 | `EnviarMensagemHandlerTests.cs` (8 testes) | ✅ Implementado e verificado manualmente; resiliência de conexão corrigida (Bug #3) |
| AC-12 a AC-14 | `CriarGrupoHandlerTests.cs` (7 testes) | ✅ Implementado e verificado manualmente ao vivo |
| AC-15 a AC-17 | `EnviarArquivoHandlerTests.cs` (5 testes, incluindo rollback de arquivo órfão) | ✅ Implementado; upload válido verificado manualmente, upload inválido só por teste automatizado ainda |
| AC-18 (emoji do composer) | — (UI pura) | ✅ Corrigido (Bug #4, picker próprio) e verificado manualmente |
| AC-19 a AC-20 (reação em mensagem) | — (UI pura, lógica no `AdicionarReacaoCommandHandler` sem teste ainda) | ✅ Verificado manualmente; teste automatizado é débito residual (não fazia parte da Fase 6 original) |
| AC-21 a AC-22 | `EditarMensagemHandlerTests.cs` (7 testes, incluindo limite de 24h) | ✅ Implementado e verificado manualmente |
| AC-23 a AC-25 | `DeletarMensagemHandlerTests.cs` (8 testes, autor/Admin/idempotência) | ✅ Implementado e verificado manualmente |
| AC-26 a AC-27 (read receipts) | — | ⬜ Não implementado nem verificado — fora do escopo do que foi tocado nesta sessão |
| AC-28 a AC-29 (digitando) | — (UI pura) | ✅ Corrigido (Bug #8a) e verificado ao vivo (Playwright, 2 sessões) |
| AC-30 a AC-32 (badge de não lidas) | — | ⬜ Não verificado manualmente de forma isolada (visto funcionando de passagem durante os testes de #5) |
| AC-33 a AC-34 (citação/reply) | `EnviarMensagemHandlerTests.cs` — `Handle_ComResposta_DevePopularRespostaConteudoComOTextoDaMensagemOriginal` | ✅ Corrigido (Bug #7) e verificado |
| AC-35 a AC-36 | `ChatHistoricoHandlerTests.cs` (4 testes) | ✅ Implementado; tela de histórico do Admin não verificada manualmente ainda |
| AC-41 a AC-45 (gerenciamento de grupo, novo) | `AdicionarParticipanteHandlerTests.cs` (10 testes) + `RemoverParticipanteHandlerTests.cs` (8 testes) | ✅ Implementado e verificado ao vivo — ver Bug #5 em `review-fase8.md` |
| AC-46 (mensagem de sistema na restauração) | `DefinirChatPerfilHandlerTests.cs` — `Handle_AoRestaurarAcesso_DeveCriarMensagemSistemaEmCadaConversaAtivaEPublicarEmTempoReal` | ✅ Implementado e verificado ao vivo (Playwright, 2 contas sintéticas) |
| AC-47 (evento `ChatPerfilAtualizado` via `ChamadosHub`) | `DefinirChatPerfilHandlerTests.cs` — `Handle_QuandoChatPerfilMuda_DevePublicarChatPerfilAtualizadoComONovoValor` (Theory, 3 casos) + `Handle_AoRestaurarAcesso_NaoDevePublicarChatAcessoRevogadoNotification` | ✅ Implementado e verificado ao vivo — confirmado via console que o WS de `/hubs/chamados` recebeu o evento autenticado como o usuário certo, sem estar na tela `/chat` |
| AC-48 (refresh de `perfil.chatPerfil` sem relogar) | — (UI pura, `AuthContext.atualizarChatPerfil`) | ✅ Verificado ao vivo nas duas direções: link "Chat" some/reaparece na sidebar sem reload, em aba já aberta, ao mudar o perfil em outra aba |
| AC-49 (fechar emoji picker ao clicar fora / Esc) | — (UI pura) | ✅ Implementado; **não verificado ao vivo nesta sessão** — só revisão de código (listeners `mousedown`/`keydown` em `MensagemInput.tsx`) |
| AC-50 (preview inline de imagem) | — (UI pura) | ✅ Implementado; **não verificado ao vivo nesta sessão** — só revisão de código (`MensagemItem.tsx`) |
| AC-51 (spinner no botão de enviar) | — (UI pura) | ✅ Implementado; **não verificado ao vivo nesta sessão** — só revisão de código |
| AC-52 (aviso específico + retry em falha de envio) | — (UI pura) | ✅ Implementado; **não verificado ao vivo nesta sessão** — requer simular falha de rede, não tentado |

---

## 6. Dependências

- Depende de: Supabase Storage (bucket `chat-arquivos` — **criado em 2026-08-31**, depois da implementação, não antes como o plano original previa)
- Depende de: SignalR Hub já existente (reusar ou criar `ChatHub` separado)
- Depende de: `UsuarioPerfil` (adicionar campo `ChatPerfil`)
- Bloqueia: nenhuma feature futura identificada

---

## 7. Gate Checks

- [x] `dotnet build` — 0 erros, 0 avisos (solução completa)
- [x] `dotnet test` — 316 testes, 0 falhas (216 base + 70 da Fase 6 + 5 de AC-46/47 + 3 do fix de
      duplicação de ChatPerfil + 19 de guarda de autorização/handlers de SignalR + 3 do `/auth/me` —
      ver `review-fase8.md` addendums 3 e 4 pro detalhe completo da review-fase9-independente.md)
- [x] `npm run build` — 0 erros (warnings só de `node_modules/@microsoft/signalr`, não do projeto)
- [x] ACs verificados — AC-46/47/48 ao vivo (Playwright, nas duas direções: revogar e restaurar); AC-49 a AC-52 implementados e cobertos por `dotnet build`/`npm run build`, mas **sem verificação manual ao vivo nesta sessão** (ver rastreabilidade acima)
- [x] `spec.md` atualizada com status final e rastreabilidade preenchida (refletindo o que falta, não fabricando conclusão)
- [x] `STATE.md` atualizado com resumo da sessão
- [x] PR aberto com base `develop` (PR #28, mergeado em 2026-08-31)
