# Chat Corporativo — Review da Fase 8 (Verificação Manual)

> **Data:** 2026-08-31
> **Quem testou:** usuário (2 contas reais) + Claude Code (conta sintética `teste.realtime@camarj.com.br`,
> criada só pra esse teste, sem tocar em nenhuma credencial real)
> **Como testou:** app local (backend `:5000` + frontend `:5173`) rodando contra o Supabase real
> **Resultado:** 6 bugs relatados pelo usuário + 2 extras encontrados durante os testes (heartbeat,
> `CriarGrupoDialog` usando endpoint Admin-only) — **todos os 8 corrigidos** em 2026-08-31.
> **Bloqueante para produção:** Falta só o sub-agente de `@review` independente (próximo passo)
> antes de `develop` poder ser promovida pra `main`.

---

## O que funciona (confirmado)

- Conversa 1:1: enviar, editar, deletar mensagem
- Anexo de arquivo (bucket `chat-arquivos` ok)
- Reação por emoji em mensagem existente (botões estáticos, não depende do seletor nativo)
- Criar grupo
- Admin concede acesso ao chat
- Mensagem em tempo real **em condição normal de conexão** (ver Bug #3 sobre resiliência)

---

## Bugs confirmados

### ✅ Bug #9 — CORRIGIDO — Heartbeat de presença falhava intermitente (500)

Corrigido em `ChatPresencaRepository.AdicionarOuAtualizarAsync`: mesmo padrão já usado em
`AdicionarReacaoAsync` — `catch (DbUpdateException ex) when (ex.InnerException is PostgresException
{ SqlState: PostgresErrorCodes.UniqueViolation })`, tratado como no-op. `dotnet build`/`test` ok (216).

### ✅ Bug #7 — CORRIGIDO — Resposta não aparecia como citação

Corrigido em `ListarMensagensQueryHandler`: novo método `ObterConteudosPorIdsAsync` no repositório
busca em lote (evita N+1) o conteúdo das mensagens citadas na página atual, com fallback
`"[arquivo]"` quando a mensagem original não tem texto — mesmo padrão já usado no preview do
`MensagemInput.tsx`. `dotnet build`/`test` ok (216).

### ✅ Bug #8b — CORRIGIDO — Revogação de acesso não avisava os outros participantes

Corrigido em `DefinirChatPerfilCommandHandler`: além de salvar a mensagem de sistema no banco (isso
já acontecia), agora também publica `ChatNovaMensagemNotification` pra cada conversa — mesmo evento
que `EnviarMensagemCommandHandler`/`EnviarArquivoCommandHandler` já usam — fazendo a mensagem
aparecer em tempo real pros outros participantes, não só na próxima vez que recarregarem a
conversa. `dotnet build`/`test` ok (216).

### ✅ Bug #4 — CORRIGIDO — Emoji do composer não funcionava

Substituído o hack de seletor nativo do SO por um picker próprio (grid de 24 emojis num popover
simples, mesmo padrão visual do `EMOJIS_RAPIDOS` de reação em `MensagemItem.tsx`). Sem biblioteca
nova. `npm run build` ok.

### ✅ Bug #8a — CORRIGIDO — causa raiz diferente da hipótese inicial

**A conexão SignalR nunca foi o problema** — confirmado ao vivo com log temporário nos dois lados:
o evento `Digitando` chega no servidor, o servidor emite `DigitandoIniciou`, e o cliente recebe.
**A causa real:** em `MensagemInput.tsx`, o debounce de "parar de digitar" (`usePararDigitarDebounce`)
nunca resetava `digitandoRef.current` para `false` depois de disparar. Resultado: `handleChange`'s
guard (`if (!digitandoRef.current)`) só deixava enviar o sinal de "começou a digitar" **uma vez por
mensagem inteira**. Se a pessoa pausasse mais de 3s no meio da mensagem (comum em textos mais
longos), o servidor avisava "parou de digitar" mas o cliente continuava achando que já tinha
avisado que "começou" — então, ao retomar a digitação, nada era reenviado. Na prática: o indicador
aparecia uma vez, brevemente, e nunca mais durante aquela mensagem — o que explica por que pareceu
"não funciona" num teste manual. Corrigido resetando o ref dentro do próprio timeout do debounce.
`npm run build` ok. Logs de diagnóstico temporários removidos de `ChatHub.cs` e `useChatSignalR.ts`
depois de confirmada a causa.

### ✅ Bug #5 — CORRIGIDO — Gerenciamento de membros do grupo (escopo expandido em 2026-08-31 pelo usuário)

**Sintoma relatado:** "Não tem a opção de remover ninguém, somente mensagem, que funciona." +
pedido explícito de escopo adicional: like WhatsApp/Teams, o grupo precisa ter uma lista de membros
visível, e clicar num membro deve abrir/continuar a conversa direta com ele.

**Causa raiz:** o endpoint de gerenciar participantes simplesmente não existe. `ChatController.cs`
só tem `POST /api/chat/grupos` (`CriarGrupoCommand`, participantes definidos na criação). Não há
`RemoverParticipanteCommand`/`AdicionarParticipanteCommand` nem rota equivalente. O evento SignalR
`ParticipanteRemovido` já está escutado no `useChatSignalR.ts:121` — código morto esperando uma
feature que nunca foi implementada, apesar de `AC-13` da spec exigir isso e do enum
`ChatAcao.ParticipanteRemovido` já existir para auditoria. Também não existe nenhuma tela de "ver
membros do grupo" — a lista de participantes só é escolhida uma vez, no `CriarGrupoDialog`, e
depois disso fica invisível.

**Boa notícia confirmada em 2026-08-31:** a parte de "clicar em alguém → iniciar ou continuar
conversa direta" **já está implementada e funcionando** para o painel de Presença
(`PresencaPanel.tsx` → `onIniciarConversa` → `CriarConversaCommandHandler`, que já verifica
`ObterPrivadaEntreUsuariosAsync` e reaproveita a conversa existente em vez de duplicar). O trabalho
novo aqui é só **reaproveitar esse mesmo fluxo** a partir da lista de membros do grupo — não
reinventar.

**Decisão de permissão (confirmada com o usuário em 2026-08-31):** só quem **criou aquele grupo
específico** + **Admin do sistema** podem adicionar/remover participante — mesmo padrão já usado
pra deletar mensagem (`podeDeletar = eAutor || eAdmin` em `MensagemItem.tsx`). Qualquer outro
participante do grupo só visualiza a lista de membros (sem editar).

**Escopo da correção:**
- Backend: `AdicionarParticipanteCommand` + `RemoverParticipanteCommand` + Handlers + Validators,
  restritos a `criador do grupo || Admin`, geram `ChatHistorico` (`ParticipanteAdicionado`/
  `ParticipanteRemovido`), publicam notificação SignalR + mensagem de sistema na conversa
- Backend: `POST /api/chat/grupos/{conversaId}/participantes` e
  `DELETE /api/chat/grupos/{conversaId}/participantes/{usuarioId}`
- Backend: `ListarParticipantesQuery` (ou reaproveitar dado já carregado na conversa) pra alimentar
  a tela de membros
- Frontend: tela/dialog "Membros do grupo" (acessível a partir do cabeçalho da conversa em grupo),
  listando todos os participantes com status de presença (reaproveita `PresencaBadge`)
- Frontend: nessa lista, clique num membro → reaproveita o `iniciarConversa` já existente em
  `ChatPage.tsx` (mesma função usada pelo `PresencaPanel`) — abre/continua a conversa direta
- Frontend: botões de adicionar/remover visíveis só pra quem tem permissão (`criador === eu || sou Admin`)

**Implementado e testado ao vivo em 2026-08-31** (Playwright, conta sintética + 2 contas de teste):
- Backend: `AdicionarParticipanteCommand`/`RemoverParticipanteCommand` + handlers + validators,
  `ObterConversaQuery` (detalhe + lista de participantes), 3 endpoints novos em `ChatController`
  (`GET/POST /chat/conversas/{id}`, `POST/DELETE /chat/grupos/{id}/participantes[/{usuarioId}]`),
  2 notifications SignalR novas (`ParticipanteAdicionado`/`ParticipanteRemovido`, com handlers em
  `ChatSignalRNotificationHandlers.cs`). Permissão: `ChatPerfilGuard.ExigirCriadorDaConversaOuAdmin`.
- Frontend: `MembrosGrupoDialog.tsx` (novo) — lista de membros com presença, remover (X) e
  "Adicionar participante" só pro criador/Admin, clique num membro reaproveita o `iniciarConversa`
  já existente (mesma lógica de find-or-create da Presença). Cabeçalho novo em `ChatPage.tsx` pra
  conversas de grupo, mostrando nome + contador de membros clicável.
- **Bug extra encontrado durante o teste ao vivo, fora do escopo original:** `CriarGrupoDialog.tsx`
  usava `GET /api/usuarios` (endpoint **Admin-only**) pra listar candidatos a participante — então
  qualquer usuário com `ChatPerfil = CriadorDeGrupo` mas `Perfil` diferente de Admin (ex: Atendente)
  via a lista de participantes sempre vazia e não conseguia criar grupo nenhum. Esse era o "achado
  solto" 403 em `/api/usuarios` registrado antes como fora do escopo — na verdade era do próprio
  chat. Corrigido trocando pra `/chat/presencas` (mesmo endpoint que `NovaConversaDialog` já usava
  corretamente).
- Verificado ao vivo: criar grupo (com o fix do 403) → abrir "Membros" → remover participante
  (mensagem de sistema em tempo real + contador atualiza + badge de não lida na sidebar) → adicionar
  de volta (mesma verificação). Tudo funcionou de primeira depois do fix do `CriarGrupoDialog`.
- `dotnet build`/`test` (216, 0 falhas) e `npm run build` limpos.

### 🔴 Bug #7 — Resposta (reply) não aparece como citação

**Sintoma relatado:** "Quando respondo é como se fosse uma mensagem nova."

**Causa raiz confirmada por leitura de código:** `ListarMensagensQueryHandler.cs:36` chama
`m.ToResponse(request.UsuarioId)` sem o parâmetro `respostaConteudo` (default `null` em
`ChatMappings.cs:8`). Só `EnviarMensagemCommandHandler` (retorno imediato pra quem envia) preenche
esse campo corretamente. A tela de listagem — que é o que efetivamente renderiza o histórico do
chat pra todo mundo — sempre recebe `respostaConteudo: null`, então a condição de renderização em
`MensagemItem.tsx:121` (`mensagem.respostaParaMensagemId && mensagem.respostaConteudo`) nunca é
satisfeita e a citação some.

**Escopo da correção:** em `ListarMensagensQueryHandler`, buscar em lote o conteúdo das mensagens
citadas (para as mensagens da página atual que têm `RespostaParaMensagemId`) e passar pro
`ToResponse`. Evitar N+1 — um único `WHERE Id IN (...)` pros IDs citados da página.

### 🔴 Bug #8b — Revogação de acesso só avisa quem perdeu

**Sintoma relatado:** "Quando eu tiro o acesso, a pessoa que está sem acesso consegue ver [que
perdeu], mas a pessoa que mandou não via que aquele usuário perdeu o acesso."

**Causa raiz confirmada por leitura de código:** `ChatAcessoRevogadoNotificationHandler.cs:75`
manda o evento `AcessoRevogado` só via `Clients.User(notification.UsuarioId.ToString())` — nunca
publica a mensagem de sistema no(s) grupo(s) de conversa do usuário revogado, apesar do `AC-03` da
spec exigir isso explicitamente ("os demais participantes das conversas veem uma mensagem de
sistema `[Nome] teve o acesso ao chat revogado`").

**Escopo da correção:** no `DefinirChatPerfilCommandHandler` (que dispara essa notificação ao
revogar), buscar todas as conversas ativas do usuário revogado, criar uma `ChatMensagem` tipo
`Sistema` em cada uma, e broadcastar via `Clients.Group(GrupoConversa(id))` — igual ao padrão já
usado pra outras mensagens de sistema (ex: participante adicionado/removido).

### 🔴 Bug #4 — Emoji do composer de mensagem não funciona

**Sintoma relatado:** "Emoji não funciona" (distinto do #6, que é a reação em mensagem existente —
esse funciona).

**Causa raiz confirmada por leitura de código:** `MensagemInput.tsx:109` (`abrirEmojiNativo`) tenta
abrir o seletor de emoji **nativo do sistema operacional** (o painel que abre com Win+. no Windows)
programaticamente, focando um `<input>` escondido. Isso não funciona em navegador desktop — só em
teclados virtuais mobile. É um erro de implementação, não de configuração.

**Escopo da correção:** trocar por um picker próprio (grid de emojis num popover), reaproveitando o
mesmo padrão visual já usado em `MensagemItem.tsx` pra reações (`EMOJIS_RAPIDOS`), só que com uma
lista maior. Sem biblioteca nova — mantém a convenção do projeto (sem dependências novas quando dá
pra resolver com o que já existe).

### 🟡 Bug #8a — Indicador "digitando..." não funciona

**Sintoma relatado:** "O digitando não funciona."

**Status da investigação:** confirmado ao vivo com Playwright — duas abas, mesma conversa, conexão
SignalR comprovadamente ativa (mensagens em tempo real funcionaram nas mesmas abas segundos antes),
mesmo assim o evento `DigitandoIniciou` nunca chegou na aba 2. A leitura de código não achou nada
obviamente errado: `MensagemInput.tsx` chama `onDigitando` corretamente no primeiro keystroke,
`useChatSignalR.ts` invoca `conn.invoke('Digitando', ...)` com guarda de conexão, `ChatHub.Digitando`
usa `Clients.OthersInGroup(...)`, `MensagemList.tsx` renderiza `digitandoNome` corretamente. Nenhum
erro apareceu no console nem nos logs do backend durante o teste (SignalR não loga invocação de
métodos de Hub por padrão, então a ausência de log não é conclusiva).

**Próximo passo:** instrumentar com log temporário nos dois lados (`ChatHub.Digitando` e o
`.catch()` de `emitirDigitando`) na hora de corrigir, pra isolar se o problema é no envio, no
recebimento, ou no roteamento do Hub.

### ✅ Bug #3 — CORRIGIDO — resiliência de conexão

**Sintoma relatado:** "Quando eu mando mensagem parece que não atualiza na hora... tenho que sair e
voltar."

**Reavaliação após teste ao vivo:** em condição normal, **funciona** — confirmado com duas abas
reais, mensagem apareceu instantaneamente sem reload. A hipótese original (conexão sempre quebrada)
estava errada.

**O que É um problema real, mesmo assim:** `useChatSignalR.ts` engole **todo** erro de conexão em
silêncio (`.catch(() => {/* falha silenciosa */})`), inclusive a tentativa inicial de `.start()`.
Se essa primeira negociação falhar por qualquer motivo transitório (rede, proxy corporativo, timing),
não existe retry nem aviso visual — o usuário fica sem tempo real pro resto da sessão, até recarregar
a página manualmente. Isso bate exatamente com "tenho que sair e voltar": não é o tempo real que é
lento, é a conexão inicial que às vezes não sobe e ninguém percebe.

**Correção aplicada:** `useChatSignalR.ts` ganhou retry manual com backoff (1s, 2s, 5s, 10s, 15s,
30s) especificamente pra tentativa inicial — `withAutomaticReconnect()` continua cobrindo quedas de
uma conexão já estabelecida, mas agora `onclose` (disparado quando o auto-reconnect desiste) e a
falha do primeiro `.start()` também acionam esse retry. Hook expõe `status` (`conectando` |
`conectado` | `reconectando` | `offline`); `ChatPage.tsx` mostra um aviso discreto ("Reconectando
ao chat em tempo real...") só depois de 1.5s sem conexão, pra não piscar em blips rápidos que o
auto-reconnect já resolve sozinho. `npm run build` ok.

---

## Bug extra encontrado (fora do checklist original)

### 🟡 Bug #9 — Heartbeat de presença falha intermitente (500)

**Como foi encontrado:** durante o teste ao vivo, `POST /api/chat/presenca/heartbeat` retornou 500.

**Causa raiz confirmada nos logs do backend:**
```
Npgsql.PostgresException: 23505: duplicate key value violates unique constraint "IX_ChatPresencas_UsuarioId"
```
Condição de corrida em `AtualizarPresencaCommandHandler`/`ChatPresencaRepository`: quando duas
chamadas de heartbeat quase simultâneas (plausível no React StrictMode em dev, mas também possível
em produção com múltiplas abas do mesmo usuário) tentam inserir presença pro mesmo `UsuarioId` ao
mesmo tempo, a segunda bate no índice único em vez de fazer upsert.

**Escopo da correção:** trocar a lógica de "buscar depois inserir" por um upsert real (ex:
`ON CONFLICT (UsuarioId) DO UPDATE` via SQL raw, ou catch específico de `DbUpdateException`/`23505`
com retry de update — mesmo padrão já usado em `AdicionarReacaoCommandHandler` pra reação duplicada,
que o próprio code review do PR #28 corrigiu).

---

## Achado adicional (não é do chat, mas apareceu durante o teste)

`GET /api/usuarios` retornou **403 Forbidden** pra uma conta com `Perfil: Admin`
(`suporte@camarj.com.br`) durante o teste. Não investigado ainda — anotado aqui pra não perder, mas
não bloqueia o chat especificamente. Verificar RBAC de `UsuariosController.Listar`.

---

## Review independente (sub-agente `@review`) — achado bloqueante corrigido

Ver `.specs/features/chat-corporativo/review-fase8-independente.md` pro relatório completo. Achado
principal, **CORRIGIDO** em seguida:

### ✅ Corrigido — `ChatPerfil` tinha 2 caminhos de escrita, um sem auditoria/notificação

Nesta mesma sessão, mais cedo, foi adicionado um campo "Chat" no dialog geral de edição de usuário
(`UsuarioFormDialog.tsx`) — pedido do usuário pra resolver a falta dessa opção na criação. Isso
criou um segundo caminho de escrita pra `ChatPerfil` (via `AtualizarUsuarioPerfilCommand`) que **não
tinha os mesmos efeitos colaterais** do caminho original (`DefinirChatPerfilCommand`, usado pelo
seletor da tabela de usuários): sem `ChatHistorico`, sem mensagem de sistema, sem notificação
SignalR. Ou seja, revogar acesso por essa tela reabria exatamente o Bug #8b por uma porta lateral.

**Correção:** `AtualizarUsuarioPerfilCommandHandler` agora replica os mesmos efeitos colaterais de
`DefinirChatPerfilCommandHandler` quando `ChatPerfil` muda (auditoria + mensagem de sistema +
notificação em tempo real, só quando revoga). `AtualizarUsuarioPerfilCommand` ganhou
`RequisitanteId`/`RequisitanteNome` pra isso. Testes atualizados (216, 0 falhas), build limpo.

**Achados não-bloqueantes do review independente:**
- ✅ Corrigido: timer de "parar de digitar" não era cancelado ao trocar de conversa rápido —
  `usePararDigitarDebounce` agora retorna `[agendar, cancelar]`, chamado no cleanup de troca de conversa
- ⬜ Follow-up (não bloqueante, baixo risco): janela de corrida residual mais estreita em
  `ChatPresencaRepository` — corrigir com upsert real (`ON CONFLICT ... DO UPDATE`) numa sessão futura
- ⬜ Follow-up (não bloqueante, pré-existente ao chat inteiro): citação de mensagem deletada mostra
  `"[arquivo]"` em vez de `"[mensagem removida]"` — mesmo padrão já usado no preview de resposta,
  não é regressão desta sessão

---

## Ordem de correção — TODOS OS ITENS CORRIGIDOS em 2026-08-31

1. ✅ Bug #9 (heartbeat)
2. ✅ Bug #7 (reply)
3. ✅ Bug #8b (revogação não avisa o outro lado)
4. ✅ Bug #4 (emoji do composer)
5. ✅ Bug #8a (digitando — causa raiz diferente da hipótese inicial)
6. ✅ Bug #3 (resiliência de conexão)
7. ✅ Bug #5 (gerenciamento de membros do grupo, escopo expandido) + bug extra do `CriarGrupoDialog`
8. ⬜ Investigar o 403 em `GET /api/usuarios` — **resolvido como efeito colateral do #5** (era o
   próprio `CriarGrupoDialog` chamando o endpoint errado, não um problema solto)

Todos os 7 bugs relatados pelo usuário na Fase 8 (mais o extra #9 e o extra do `CriarGrupoDialog`)
foram corrigidos, com `dotnet build`/`test`/`npm run build` limpos a cada passo, e verificação ao
vivo via Playwright pra tudo que dependia de tempo real ou múltiplas sessões. Falta só o sub-agente
de `@review` independente (próximo passo) antes de considerar `develop` pronta pra promoção.

---

## Addendum — Fase 9: polimento pós-teste do usuário (AC-46 a AC-52), 2026-08-31

> O review independente (seção acima) já estava resolvido quando o usuário testou a Fase 8 ao vivo
> e deu dois retornos: (1) sugeriu que a restauração de acesso devesse gerar uma mensagem simétrica
> à revogação, e (2) perguntou o que faltava pra deixar o chat "mais profissional". A resposta foi
> AC-46 a AC-52, todos implementados nesta mesma sessão, com instrução explícita do usuário de
> seguir o SDD à risca (spec antes de código).

**AC-46/47/48 — restauração simétrica + refresh de perfil sem relogar.** Ver a nota de arquitetura
em `spec.md` (antes do AC-46): o evento de restauração precisa viajar pelo `ChamadosHub` (conexão
global, ativa pra qualquer usuário logado), não pelo `ChatHub` (só existe na tela `/chat`) — do
contrário nunca chegaria em quem está justamente sem acesso ao chat. Verificado ao vivo com 2 contas
sintéticas (`teste.admin2@camarj.com.br`, `teste.alvo2@camarj.com.br`, senha `TesteChat123`, ambas
descartáveis — **pendente exclusão do Supabase**, não foi possível automatizar a limpeza nesta
sessão porque o classificador de modo automático bloqueou o acesso às credenciais de serviço do
Supabase via Bash; requer ação manual do usuário ou uma sessão com essa permissão liberada):

1. Aba A logada como `teste.alvo2` em `/chamados` (deliberadamente fora de `/chat`) — sidebar mostra
   o link "Chat".
2. Aba B logada como `teste.admin2`, revoga o `ChatPerfil` de `teste.alvo2` pela tela de usuários.
3. Sem recarregar a Aba A: o link "Chat" some sozinho da sidebar — confirma AC-47/AC-48.
4. Aba B restaura o `ChatPerfil` de volta pra "Participante".
5. Sem recarregar a Aba A: o link "Chat" reaparece sozinho — confirma a direção de restauração do
   AC-46/47/48, simétrica à revogação.
6. Console da Aba A confirmado com o WebSocket de `/hubs/chamados` autenticado como `teste.alvo2`
   (claim `sub`) o tempo todo — prova que o evento chegou pela conexão global pré-existente, não por
   uma reconexão nem por uma conexão específica do chat.

**AC-49 a AC-52 — polimentos de UX (emoji picker, preview de imagem, indicador de envio/retry).**
Implementados e cobertos pelos gate checks (`dotnet build`, `dotnet test` — 291 testes, `npm run
build`), mas **sem verificação manual ao vivo nesta sessão** — ficam como pendência pra próxima
rodada de teste do usuário, igual já é a prática registrada pra outros itens não-bloqueantes deste
documento.

**Gate checks finais desta rodada:** `dotnet build` (0 erros/avisos), `dotnet test` (291 testes, 0
falhas — 286 da Fase 6/8 + 5 novos cobrindo AC-46/47 em `DefinirChatPerfilHandlerTests.cs`), `npm
run build` (0 erros, mesmos warnings pré-existentes de `node_modules/@microsoft/signalr`).

---

## Addendum 2 — 2 achados no reteste do usuário pós-Fase 9, 2026-09-01

### ✅ Bug #10 — CORRIGIDO — Notificação de mensagem nova só chegava com a aba do chat aberta

**Relato do usuário:** "ainda acontece de alguém mandar mensagem e não aparecer a notificação na
aba do chat, somente quando eu clico na aba do chat".

**Causa raiz:** exatamente o mesmo padrão do AC-46/47/48 (ver nota de arquitetura em `spec.md`,
antes do AC-46) — `useChatSignalR` (que dispara `invalidateQueries(['chat','conversas'])` ao
receber `NovaMensagem`) só conecta ao `ChatHub`, e o `ChatHub` só existe enquanto o componente
`ChatPage` está montado. Fora da tela `/chat`, não tinha conexão nenhuma recebendo esse evento, então
o badge de não lidas na sidebar (`AppLayout`, `totalNaoLidas`) nunca era invalidado — só atualizava
ao entrar na tela de chat (que dispara um fetch novo por outro motivo).

**Correção:** `ChatNovaMensagemNotificationHandler` (WebApi) agora, além de avisar o grupo do
`ChatHub` (como antes), busca os participantes ativos da conversa e manda um evento leve
`ChatConversaAtualizada` pra cada um (exceto quem enviou a mensagem) via `ChamadosHub`/
`Clients.User` — mesmo canal global já usado pro `ChatPerfilAtualizado`. Frontend: novo evento
`ChatConversaAtualizada` em `signalr-events.ts`/`useSignalR.tsx`; `AppLayout` invalida
`['chat','conversas']` ao recebê-lo, atualizando o badge sem precisar estar na tela de chat.

**Status:** implementado, `dotnet build`/`test` (291, 0 falhas)/`npm run build` limpos — **ainda sem
reteste ao vivo do usuário** nesta sessão.

### ✅ Bug #11 — CORRIGIDO — Poucos emojis no composer

**Relato do usuário:** "tem poucos emojis, não? Tem que ver isso também".

**Causa raiz:** não é bug de lógica — `EMOJIS_COMPOSER` em `MensagemInput.tsx` era uma lista fixa e
curta (24 emojis) escolhida na correção original do Bug #4 (troca do seletor nativo do SO por um
picker próprio), sem preocupação de cobertura.

**Correção:** lista ampliada pra ~84 emojis cobrindo mais categorias (expressões, gestos, corações,
objetos/símbolos comuns em conversa de trabalho). Adicionado `max-h-56 overflow-y-auto` no container
do picker pra comportar as linhas extras sem estourar o layout.

**Status:** implementado, `npm run build` limpo — **ainda sem reteste ao vivo do usuário** nesta sessão.

---

## Addendum 3 — Review independente da Fase 9 (`review-fase9-independente.md`), 2 achados bloqueantes corrigidos, 2026-09-01

Pedido do usuário: "peço que use algum modelo avançado para fazer review e ver se precisa de algum
ajustes no cod, por mim aqui nos testes que eu fiz esta ok". Sub-agente independente (Opus) revisou
tudo que ainda não tinha sido revisado (Fase 9 + Bugs #10/#11 do Addendum 1/2 acima) e voltou com
veredito **BLOQUEANTE**, 2 achados. Relatório completo em `review-fase9-independente.md` (não
editado — fica como registro histórico do que o revisor encontrou). O usuário escolheu corrigir os 2
bloqueantes + os 2 achados 🟡 que o próprio revisor recomendou tratar primeiro.

### ✅ CORRIGIDO — Achado #1 (bloqueante): AC-47 não valia pro segundo caminho de escrita de ChatPerfil

Reincidência parcial do bloqueante da `review-fase8-independente.md` #1: aquela correção duplicou
código (auditoria + notificação) em `AtualizarUsuarioPerfilCommandHandler` em vez de extrair, e a
Fase 9 (feita depois) só evoluiu o `DefinirChatPerfilCommandHandler` original — os dois caminhos
divergiram nos mesmos 3 pontos de novo (AC-46/47/48 quebrados no dialog "Editar usuário").

**Correção definitiva desta vez:** eliminada a duplicação. `AtualizarUsuarioPerfilCommandHandler`
não mexe mais em `ChatPerfil` diretamente — quando `request.ChatPerfil` difere do atual, despacha
`DefinirChatPerfilCommand` via `IMediator.Send`, o mesmo comando que a tela de usuários já usava.
Um único handler decide o que acontece quando o `ChatPerfil` de alguém muda, não importa a tela.
3 testes novos em `AtualizarUsuarioPerfilHandlerTests.cs` cobrindo o despacho, a resposta refletindo
o novo perfil, e o caso de não-mudança (sem despacho).

### ✅ CORRIGIDO — Achado #2 (bloqueante): fan-out do Bug #10 alcançava usuários revogados

O filtro novo de `ChatConversaAtualizada` só considerava participação ativa, não `ChatPerfil` — um
usuário revogado continua como participante (revogar não remove ninguém de grupo), então continuava
recebendo o evento e sendo empurrado a recarregar a lista de conversas em tempo real. O revisor
também apontou a causa maior (pré-existente, não desta leva): nenhuma query de leitura do chat chama
`ChatPerfilGuard.ExigirAcesso` — fica registrado como débito conhecido, fora do escopo desta correção.

**Correção dentro do escopo:** `ChatNovaMensagemNotificationHandler` agora busca o `ChatPerfil` atual
dos participantes (`IUsuarioPerfilRepository.ListarPorIdsAsync`, método novo, uma query em lote — não
N+1) e filtra `ChatPerfil != SemAcesso` antes de mandar o evento. Defesa em profundidade no frontend:
`useConversas(enabled)` ganhou parâmetro opcional; `AppLayout` passa `Boolean(temAcessoChat)`, então
quem nunca teve acesso ao chat nem dispara mais `GET /chat/conversas`.

### ✅ CORRIGIDO — Achado 🟡 #3 (parcial: robustez): exceção no handler podia derrubar mensagem já salva

O handler novo do Bug #10 passou a fazer I/O de banco; qualquer falha ali (banco fora do ar,
`SendAsync` que estoure) subia pelo `_mediator.Publish` e derrubava `EnviarMensagemCommandHandler`
com 500 — **depois** da mensagem já persistida, mostrando o aviso de retry do AC-52 por engano e
arriscando duplicata no reenvio. Todo o bloco de fan-out (achado #2 incluído) agora está em
`try/catch` com `_logger.LogError` — falha em notificar nunca mais derruba um comando já commitado.
(A otimização de N+1 que o mesmo achado #3 apontava em `DefinirChatPerfilCommandHandler`/
`AtualizarUsuarioPerfilCommandHandler` — várias notificações reconsultando a mesma conversa que o
publisher já tinha em mãos — **não foi tratada**, fora do escopo que o usuário pediu para esta rodada.)

### ✅ CORRIGIDO — Achado 🟡 #5: `SignalRProvider` sem retry na conexão inicial

Toda a Fase 9 e os Bugs #10/#11 passaram a depender do `ChamadosHub` (conexão global). Um `start()`
que falhasse na primeira tentativa nunca era retentado — `withAutomaticReconnect` só cobre queda de
conexão já estabelecida. `useSignalR.tsx` ganhou o mesmo backoff manual (`ATRASOS_RETRY_MS`,
1s→30s) que `useChatSignalR.ts` já usava desde o Bug #3, por consistência. Comentário desatualizado
em `App.tsx` corrigido.

### Gate checks desta rodada
`dotnet build` (0 erros, 3 avisos pré-existentes não relacionados — `HasName` obsoleto em
`HistoricoEntradaConfiguration.cs`, nulabilidade de `IEmailSender` em `Program.cs`, nenhum deles
neste diff), `dotnet test` (**294 testes, 0 falhas** — 291 + 3 novos), `npm run build` (0 erros).

### O que ficou de fora nesta rodada (ver Addendum 4 abaixo — todo o resto foi corrigido depois)
- A otimização de N+1 do achado #3 em `EnviarMensagem`/`EnviarArquivo` (query nova por mensagem —
  tolerável, não é redundância; só a redundância de `DefinirChatPerfil` foi resolvida)
- Emoji sempre inserido no fim do texto, não na posição do cursor (nitpick 🟢)
- Batch endpoint pra preview de imagem (N requisições em conversas com muitas fotos — deferido por
  desproporção custo/benefício, ver Addendum 4)
- Acessibilidade do `<img>` clicável (sem `role`/handler de teclado)
- Demais nitpicks 🟢 sem impacto funcional

---

## Addendum 4 — Fechamento completo dos achados da review-fase9-independente.md, 2026-09-01

Pedido do usuário, verbatim: **"quero que vc resolva tudo, acha que demora muito, seria muito
trabalhoso?"** — depois de eu ter sugerido tratar só a lacuna de autorização (achado #2). Fechado
tudo que não tinha sido tratado no Addendum 3, em 3 blocos com gate check entre cada.

### ✅ Achado #2 (bloqueante) — fechado por completo
O Addendum 3 só mitigou o achado #2 filtrando o fan-out. A causa raiz (nenhuma query de leitura do
chat verificava `ChatPerfil`) foi fechada agora: `ChatPerfilGuard.ExigirAcesso` adicionado em
`ListarConversasQueryHandler`, `ListarMensagensQueryHandler`, `ObterConversaQueryHandler` e
`ObterArquivoMensagemQueryHandler` — as 4 queries de leitura do chat. Revogar acesso agora bloqueia
leitura de verdade, não só o link na sidebar. 13 testes novos cobrindo a guarda (`ForbiddenException`
pra `SemAcesso`, `NotFoundException` pra usuário inexistente, caminho feliz preservado) — incluindo
um teste que reproduz literalmente o cenário do achado: participante com vínculo intacto mas
`ChatPerfil = SemAcesso` ainda assim é bloqueado.

### ✅ Achado #4 — zero teste pros handlers de SignalR
`ChamadosCamarj.UnitTests.csproj` ganhou `ProjectReference` pra `ChamadosCamarj.WebApi`. 7 testes
novos em `ChatSignalRNotificationHandlersTests.cs` cobrindo `ChatNovaMensagemNotificationHandler`
(exclusão do remetente, exclusão de `ChatPerfil=SemAcesso`, `DestinatarioIds` pré-carregado pula a
consulta de conversa, conversa inexistente é no-op, exceção no fan-out não propaga) e
`ChatPerfilAtualizadoNotificationHandler`. Efeito colateral inofensivo: `dotnet build` passou a
mostrar 3 avisos `MSB3277` (conflito de versão do EF Core entre `Infrastructure` e `WebApi`, ambos
com `Version="9.*"` flutuante) — pré-existente, só ficou visível porque agora os dois entram no
mesmo grafo de dependência de um projeto; build e testes passam normalmente, não mexi na convenção
de versionamento (fora de escopo).

### ✅ Achado #3 (parcial: N+1) — resolvido no ponto que importava
`ChatNovaMensagemNotification` ganhou `DestinatarioIds` opcional. `DefinirChatPerfilCommandHandler`
(que publica uma notificação por conversa num loop) agora passa os participantes que já tinha em
mãos, evitando o handler refazer a mesma busca a cada iteração. O caminho comum
(`EnviarMensagem`/`EnviarArquivo`, 1 query nova por mensagem) foi deixado como está — não é
redundância, é uma busca genuinamente nova, e o próprio achado chamou isso de "tolerável".

### ✅ Achado #6 — timer de "digitando" sobrevive ao unmount
`cancelarPararDigitar()` agora é devolvido como cleanup do efeito de troca de conversa (`return
cancelarPararDigitar` em vez de chamar no corpo) — cobre troca de conversa E desmonte do componente.
Bônus do próprio achado: `enviar()` agora cancela o timer pendente no `onSuccess`, fechando o
"PararDigitar duplicado 3s depois" que o achado também mencionou.

### ✅ Achado #7 — texto de retry enganoso em erro de arquivo
Erro deixou de ser uma `string` única e virou `{ mensagem, origem: 'texto' | 'arquivo' }`. O texto de
retry e o destaque vermelho do botão de enviar só aparecem pra erro de `origem: 'texto'` (onde o
retry de fato funciona); erro de arquivo mostra "Escolha o arquivo novamente para tentar de novo." —
que agora também é verdade, porque `handleArquivo` limpa `fileInputRef.current.value` no `onError`
(sem isso, reescolher o MESMO arquivo não disparava `onChange`, deixando a pessoa sem conseguir
tentar de novo).

### ✅ Achado #8 (parcial) — preview de imagem
Corrigido: `gcTime` alinhado ao `staleTime` de 50min (antes só o comentário prometia isso — o padrão
de 5min do TanStack Query descartava o cache antes); `loading="lazy"` e `onError` no `<img>` (cobre
URL assinada que expira entre o fetch e a renderização, que antes mostrava o ícone de imagem quebrada
do navegador em vez do estado "Falha ao carregar" que o componente já sabia desenhar); botão de
download adicionado no branch de imagem, igual ao branch de arquivo genérico; bug do nitpick 🟢
(`tamanhoBytes && (...)` imprimindo o literal `"0"` pra arquivo de 0 bytes) corrigido nos dois
branches. **Deixado de fora, deliberadamente:** o endpoint em lote pra buscar N URLs assinadas de
uma vez (evitaria 1 requisição por imagem em conversas com muitas fotos) — desproporcional pro
volume de uso atual do sistema; documentado como débito, não escondido.

### ✅ Achado #9 — alerta duplicado ao revogar com `/chat` aberto
Considerei a sugestão do revisor (aposentar `ChatAcessoRevogadoNotification`/evento `AcessoRevogado`
do `ChatHub`), mas isso tocaria em código auditado por testes existentes e teria risco desproporcional
ao ganho — optei por uma correção mais cirúrgica: `AppLayout` agora sabe (via um ref sincronizado com
`useLocation()`) se a pessoa já está em `/chat` quando o evento de revogação chega, e nesse caso não
mostra o próprio aviso (porque `ChatPage` já mostra o dele e navega pra fora). Resultado igual ao
sugerido — sem alerta duplicado — com uma mudança bem menor.

### ✅ Achado #10 — AC-48 não cobria quem estava offline no momento da mudança
Endpoint novo `GET /auth/me` (`ObterPerfilAtualQuery`/`Handler`, `AuthController.Me`) revalida o
perfil atual direto do banco, não do JWT/localStorage. `AuthContext` chama isso uma vez no boot do
app (só se já existir token salvo) e substitui o `perfil` local pelo que voltar do servidor — falha
de rede aqui não desloga ninguém (mantém o snapshot salvo); um 401 de verdade (conta excluída/
desativada) já cai no `registrarLogoutAutomatico` existente. Fecha o AC-48 pra quem foi revogado (ou
promovido) enquanto estava deslogado ou com a aba fechada. 3 testes novos em
`ObterPerfilAtualHandlerTests.cs`.

### Gate checks finais desta rodada
`dotnet build` (0 erros; 6 avisos — 3 pré-existentes não relacionados + 3 `MSB3277` novos, inofensivos,
ver achado #4 acima), `dotnet test` (**316 testes, 0 falhas** — 294 + 19 do bloco de segurança/testes
+ 3 do `/auth/me`), `npm run build` (0 erros, mesmos warnings pré-existentes de terceiros).

### O que ficou de fora, de propósito, mesmo depois deste addendum
- Endpoint em lote pra preview de imagem (achado #8, ver acima)
- `ChatAcessoRevogadoNotification`/evento `AcessoRevogado` do `ChatHub` não foi removido — ficou
  redundante (o `ChatPerfilAtualizado` via `ChamadosHub` cobre o mesmo caso e mais), mas continua
  funcionando; aposentá-lo de vez fica pra uma sessão futura, se fizer sentido
- Nitpicks 🟢 sem impacto funcional (emoji na posição do cursor, acessibilidade de teclado no `<img>`)

---

## Addendum 5 — Bug real encontrado na verificação ao vivo pós-Addendum 4, corrigido, 2026-09-01

Pedido do usuário: "consegue testar algo e me retornar ou esta tudo certo? Que ai já subimos isso,
mas quero tudo certinho". Fiz verificação ao vivo (Playwright, contas `teste.admin2`/`teste.alvo2`)
de tudo que só tinha teste automatizado até então: o fechamento do achado #2 (guarda de leitura),
o Bug #10 (badge fora do chat), o emoji picker, o preview de imagem — e, mais importante, o achado
#1 corrigido no Addendum 4 (revogar/restaurar pelo dialog "Editar usuário").

### ✅ Confirmado ao vivo, sem achados
- **Bug #10 (badge fora do chat):** mensagem enviada via chamada de API direta (bypassando o
  browser, pra eliminar qualquer dúvida de identidade) fez o badge "Chat" na sidebar de uma sessão
  em `/chamados` (não `/chat`) subir de 1 → 2 **sem reload**, em tempo real.
- **Emoji picker:** os 84 emojis renderizam; fecha com Esc e com clique fora, confirmado nos dois casos.
- **Preview de imagem:** upload de PNG real renderizou como miniatura inline, com nome, tamanho e
  botão "Baixar imagem" — não caiu no fallback de erro.
- **Achado #2 (guarda de leitura):** confirmado indiretamente — depois de revogar o acesso de
  `teste.alvo2`, o próprio console do navegador mostrou `403 Forbidden` em `GET /chat/conversas` e
  `GET .../mensagens`, prova de que `ChatPerfilGuard.ExigirAcesso` está bloqueando leitura de verdade.
- **Retry de conexão (achado #5):** por acidente, o teste incluiu um restart real do backend no meio
  — a conexão `ChamadosHub` do `teste.alvo2` caiu (`ERR_CONNECTION_REFUSED` repetido) e **se
  reconectou sozinha** assim que o backend voltou, sem precisar recarregar a página.

### 🔴 Achado NOVO, não estava em nenhuma review — CORRIGIDO na hora

**Revogar ou restaurar acesso pelo dialog "Editar usuário" (`PUT /api/usuarios/{id}`) devolvia
500** — reproduzido ao vivo, não pego por nenhum teste automatizado (nem os 3 novos do Addendum 4).

**Causa raiz:** a correção do achado #1 (Addendum 4) fez `AtualizarUsuarioPerfilCommandHandler`
despachar `DefinirChatPerfilCommand` via `IMediator.Send` quando o `ChatPerfil` muda. Os dois
handlers compartilham o mesmo `DbContext` (escopo por requisição), mas cada um carrega sua **própria
cópia** de `UsuarioPerfil` via `ObterPorIdAsync` (que não rastreia — `AsNoTracking()`). O handler
externo já tinha *anexado* sua cópia ao change tracker do EF Core (`_dbSet.Update()` pros campos
gerais); quando o handler interno tentava anexar a *sua* cópia (mesma chave primária, instância C#
diferente), o EF Core rejeitava com
`InvalidOperationException: The instance of entity type 'UsuarioPerfil' cannot be tracked because
another instance with the same key value... is already being tracked`. Isso é uma classe de bug que
teste com Moq **não consegue pegar** — os mocks não modelam o change tracker real do EF Core; só
apareceu batendo na API de verdade.

**Correção:** `UsuarioPerfilRepository.AtualizarAsync` (e `AdicionarAsync`, por consistência) agora
desanexam a entidade do change tracker (`_context.Entry(usuario).State = EntityState.Detached`)
logo depois de salvar — então uma segunda gravação da mesma entidade, com uma instância C# diferente,
no mesmo `DbContext`, não colide mais.

**Verificação:** reproduzi o 500 ao vivo primeiro (confirmando o bug antes de mexer), corrigi,
rebuildei, reiniciei o backend, e repeti a chamada exata — `204 No Content`. Testei as duas direções
(revogar e restaurar) pela API direta, com `teste.alvo2` já carregado e conectado em `/chamados`:
o link "Chat" sumiu e voltou ao vivo, com o banner "Seu acesso ao chat foi restaurado." aparecendo
na restauração — confirma que o Addendum 4 (achado #1) está de fato corrigido ponta a ponta agora,
não só nos testes unitários.

### Gate checks desta rodada
`dotnet build` (0 erros), `dotnet test` (**316 testes, 0 falhas** — mesma contagem; este bug é
estruturalmente invisível a testes com Moq, então nenhum teste novo foi capaz de cobri-lo — ver nota
acima), `npm run build` não re-executado (nenhum arquivo de frontend mudou nesta rodada).

### Lição registrada
Handlers que despacham outro comando via `IMediator.Send` dentro do mesmo `DbContext` escopado por
requisição, quando ambos os lados tocam a mesma entidade, **precisam** ou (a) desanexar depois de
cada save, ou (b) compartilhar a mesma instância já carregada em vez de recarregar. A opção (a) foi
aplicada aqui por ser a menor mudança; a alternativa mais "correta" sugerida originalmente pela
`review-fase9-independente.md` (extrair a lógica pra um serviço de aplicação reutilizável, evitando
o re-fetch por completo) resolveria a mesma classe de problema de raiz e deve ser considerada se
mais handlers passarem a compor uns aos outros dessa forma.
