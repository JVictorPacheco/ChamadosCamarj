# Chat Corporativo — Review Independente da Fase 9 + Bugs #10/#11 (`@review`)

**Data:** 2026-09-01
**Escopo:** working tree não commitado contra `develop` (`13f6937`) — **apenas** a leva posterior à
revisão de `review-fase8-independente.md`: Rodada A (Fase 9, AC-46 a AC-52) e Rodada B (Bugs #10 e #11).
**Revisor:** sub-agente `@review` independente, sem participação na implementação.
**Veredito:** **BLOQUEANTE** — dois achados 🔴. O AC-47 ("evento em toda mudança de `ChatPerfil`")
não se sustenta no segundo caminho de escrita, e o fan-out novo de Bug #10 alcança usuários com
acesso revogado, expondo em tempo real uma lacuna de autorização nas queries de leitura do chat.

---

## Resumo

Li os diffs de todos os arquivos listados no escopo, mais os arquivos vizinhos necessários para
julgar corretude: `ChatConversaRepository`, `ChatHub`/`ChamadosHub`, `SubClaimUserIdProvider`,
`ChatMappings`, `ChatMensagem`, `ChatPerfilGuard`, `EnviarMensagemCommandHandler`,
`ListarConversasQueryHandler`, `AtualizarUsuarioPerfilCommandHandler`, `App.tsx`, `useConversas.ts`,
`useChatSignalR.ts`, `ChatPage.tsx`.

O núcleo da Rodada A está bem construído. A distinção `revogou` / `restaurou` em
`DefinirChatPerfilCommandHandler` está correta e bem justificada (troca lateral
`Participante → CriadorDeGrupo` não gera mensagem de sistema — verificado no código e coberto por
teste). A decisão arquitetural de mandar `ChatPerfilAtualizado` e `ChatConversaAtualizada` pelo
`ChamadosHub` em vez do `ChatHub` está certa e é a única que resolve o problema real: o `ChatHub` só
tem conexão dentro de `/chat`. O `SubClaimUserIdProvider` garante que `Clients.User(id)` case com o
`UsuarioPerfil.Id`, e `Clients.User` atinge **todas** as conexões daquele usuário — multi-aba e
multi-dispositivo estão corretos por construção. Usuário desconectado é no-op silencioso, sem
exceção — comportamento adequado nos dois handlers.

`DefinirChatPerfilHandlerTests.cs` cobre AC-46/47 de forma adequada: restauração gera mensagem de
sistema em cada conversa e publica em tempo real; restauração não dispara `ChatAcessoRevogado`;
`[Theory]` cobre as três transições publicando `ChatPerfilAtualizado` com o valor novo; troca lateral
não toca em conversa nenhuma. Não achei buraco relevante nessa parte específica.

O que trava a leva são dois pontos que ninguém verificou: (a) o AC-47 foi implementado só em um dos
dois handlers que escrevem `ChatPerfil`, e (b) o fan-out do Bug #10 aponta um holofote sobre a
ausência de guarda de `ChatPerfil` nas queries de leitura do chat.

Gate checks rodados por mim: **build 0/0, 291 testes aprovados, `npm run build` exit 0**.

---

## Achados por Severidade

### 🔴 Bloqueante

#### 1. `ChatPerfilAtualizado` não é publicado "em toda mudança de `ChatPerfil`" — o segundo caminho de escrita ficou de fora, de novo

**Onde:**
- `src/ChamadosCamarj.Application/Features/Usuarios/Commands/AtualizarUsuarioPerfilCommandHandler.cs:72-104`
- comparar com `src/ChamadosCamarj.Application/Features/Chat/Commands/DefinirChatPerfil/DefinirChatPerfilCommandHandler.cs:50-94`

O bloqueante #1 da revisão anterior foi corrigido **parcialmente**: `AtualizarUsuarioPerfilCommandHandler`
ganhou auditoria (`ChatHistorico`) e o bloco de revogação (mensagem de sistema +
`ChatNovaMensagemNotification` + `ChatAcessoRevogadoNotification`), copiados literalmente do handler
original. Mas a Fase 9, feita depois, só evoluiu o handler original. O resultado é que os dois
caminhos divergiram de novo, agora em três pontos:

| Comportamento | `DefinirChatPerfilCommandHandler` (PATCH `/usuarios/{id}/chat-perfil`) | `AtualizarUsuarioPerfilCommandHandler` (PUT `/usuarios/{id}`, campo "Chat" do `UsuarioFormDialog`) |
|---|---|---|
| `ChatPerfilAtualizadoNotification` (AC-47) | ✅ sempre que o perfil muda | ❌ **nunca** |
| Mensagem de sistema ao **restaurar** (AC-46) | ✅ | ❌ só trata `revogouChat` |
| Frontend reflete sem relogar (AC-48) | ✅ | ❌ |

**Impacto concreto:** um Admin que revoga o acesso pelo dialog "Editar usuário" — a forma mais óbvia
de editar um usuário no painel, e onde o campo "Chat" fica bem visível — deixa a pessoa afetada com
o link "Chat" ainda na barra lateral e o `chatPerfil` antigo em `localStorage` até ela deslogar e
logar de novo. O `ChatPerfilGuard` bloqueia as escritas no servidor (o perfil é lido do banco, não do
JWT), então não vira brecha de escrita — mas AC-48 simplesmente não funciona nesse caminho, e AC-46
(restauração anunciada) também não. E ninguém percebe, porque o Admin vê o mesmo resultado nas duas
telas.

**Por que os testes não pegam:** `DefinirChatPerfilHandlerTests.cs` testa só o handler correto.
`AtualizarUsuarioPerfilHandlerTests.cs` não verifica publicação de notificação nenhuma.

**Sugestão:** a duplicação literal de ~35 linhas entre os dois handlers é a causa raiz — é garantido
que vão divergir de novo na próxima mudança. Extrair para um serviço de aplicação
(`IChatAcessoService.AplicarMudancaDeChatPerfilAsync(usuario, perfilAnterior, perfilNovo, requisitante...)`)
ou fazer `AtualizarUsuarioPerfilCommandHandler` despachar `DefinirChatPerfilCommand` via `IMediator`
quando `chatPerfilMudou`. Qualquer uma resolve as três divergências de uma vez.

---

#### 2. `ChatConversaAtualizada` é entregue a participantes com `ChatPerfil = SemAcesso`, e a query que ele manda o cliente recarregar não tem guarda de `ChatPerfil`

**Onde:**
- `src/ChamadosCamarj.WebApi/Notifications/ChatSignalRNotificationHandlers.cs:40-45` (filtro do fan-out)
- `src/ChamadosCamarj.Application/Features/Chat/Queries/ListarConversas/ListarConversasQueryHandler.cs` (sem guarda)
- `frontend/src/layouts/AppLayout.tsx:41,53-55`

O filtro do fan-out novo é:

```csharp
var destinatarios = conversa.Participantes
    .Where(p => p.Ativo && p.UsuarioId != mensagem.AutorId)
    .Select(p => p.UsuarioId.ToString());
```

Está certo quanto a *participação* (ativos, exclui o remetente — `AutorId` do `ChatMensagemResponse`
é o autor real; para mensagens de sistema é `Guid.Empty`, então ninguém é excluído indevidamente).
O problema é o que ele **não** filtra: revogar acesso ao chat, por decisão explícita e documentada do
próprio handler ("revogar não remove ninguém de grupo nenhum, só bloqueia a tela; os vínculos
continuam intactos"), **mantém a pessoa como participante ativo**. Logo, todo usuário revogado
continua na lista de destinatários e recebe `ChatConversaAtualizada` a cada mensagem trocada em
qualquer conversa da qual ele ainda é membro.

O evento em si não carrega payload, então ele sozinho não vaza conteúdo. O problema é o efeito:

1. `AppLayout.tsx:41` chama `useConversas()` **incondicionalmente**, para qualquer usuário logado —
   sem `enabled: temAcessoChat`. A query fica sempre ativa.
2. `AppLayout.tsx:53-55` invalida `['chat','conversas']` ao receber o evento → refetch imediato.
3. `ListarConversasQueryHandler` **não chama `ChatPerfilGuard.ExigirAcesso`**. Verifiquei: no
   projeto inteiro, `ExigirAcesso` só aparece em `EnviarMensagem`, `EnviarArquivo`, `CriarConversa`
   e `AdicionarReacao` — **nenhuma query de leitura tem essa guarda**. `ListarMensagens`,
   `ObterConversa` e `ObterArquivoMensagem` checam apenas *participação ativa*, que o revogado
   continua tendo.

Resultado: **revogar acesso ao chat não revoga leitura**. Um usuário revogado continua podendo
chamar `GET /api/chat/conversas` (nome das conversas, preview da última mensagem, contagem de não
lidas), `GET .../mensagens` (histórico completo) e baixar anexos — e, depois desta leva, o navegador
dele faz isso **automaticamente e em tempo real**, empurrado pelo servidor, a cada mensagem nova.
A UI esconde o link "Chat" (`temAcessoChat`), mas os dados estão na rede e no cache do TanStack
Query. `[ProducesResponseType(StatusCodes.Status403Forbidden)]` está declarado no
`ChatController.ListarConversas` mas nada no handler produz esse 403.

**Ressalva de escopo, para triagem honesta:** a ausência de guarda nas queries de leitura é
**pré-existente** ao diff que estou revisando — não foi introduzida aqui. O que é desta leva é o
fan-out que passa a acionar essa leitura ativamente para exatamente o público que deveria estar
bloqueado, transformando uma lacuna estática em um feed vivo. Classifico como 🔴 porque contradiz
diretamente o AC-03 e porque a mitigação parcial dentro do escopo desta leva é barata.

**Sugestão:** (a) mínimo, dentro do escopo: filtrar o fan-out por `ChatPerfil != SemAcesso` (exige
carregar os perfis, ou — melhor — passar os destinatários já resolvidos na própria notificação, ver
achado #3); (b) frontend: `useConversas()` com `enabled: Boolean(temAcessoChat)` — hoje ele dispara
uma chamada de chat até para quem nunca teve acesso; (c) correto de verdade:
`ChatPerfilGuard.ExigirAcesso` nas quatro queries de leitura do chat.

---

### 🟡 Não bloqueante (atenção recomendada)

#### 3. `ChatNovaMensagemNotificationHandler`: N+1 real nos loops, query redundante no caso comum, e agora capaz de derrubar o comando inteiro
**Arquivo:** `src/ChamadosCamarj.WebApi/Notifications/ChatSignalRNotificationHandlers.cs:26-46`

Três coisas distintas:

- **N+1 confirmado.** `DefinirChatPerfilCommandHandler:76` já carrega **todas** as conversas do
  usuário com `.Include(c => c.Participantes)` (`ChatConversaRepository.ListarConversasComUsuarioAsync`),
  e então publica uma notificação por conversa dentro do `foreach`. Cada publish faz o handler
  refazer `ObterPorIdAsync` — que é o *mesmo* `SELECT ... Include(Participantes)` da conversa que o
  publisher já tem em mãos. Usuário em 30 conversas → 30 queries redundantes por revogação/restauração.
  `AtualizarUsuarioPerfilCommandHandler` tem o mesmo padrão.
- **Query extra por mensagem no caminho comum.** `EnviarMensagemCommandHandler` carrega só o
  `ChatParticipante` (`ObterParticipanteAsync`), não a conversa — então aqui é 1 roundtrip
  genuinamente novo por mensagem enviada, não uma redundância. Tolerável em volume de intranet, mas
  evitável.
- **Jeito mais barato (resolve os três):** estender o record para
  `ChatNovaMensagemNotification(Guid ConversaId, object Mensagem, IEnumerable<Guid>? DestinatarioIds = null)`
  e deixar cada publisher preencher quando já tiver a conversa carregada (é o caso de
  `DefinirChatPerfil`, `AtualizarUsuarioPerfil`, `AdicionarParticipante`, `RemoverParticipante`),
  caindo no `ObterPorIdAsync` só quando vier `null`. Isso também dá o ponto natural para aplicar o
  filtro de `ChatPerfil` do achado #2.
- **Regressão de robustez.** O handler era `Task Handle(...) => hub.SendAsync(...)`, sem I/O de
  banco. Agora ele acessa o `DbContext` e faz `Task.WhenAll`. A estratégia padrão de `Publish` do
  MediatR é sequencial e propaga exceções: qualquer falha aqui (banco indisponível, `DbContext`
  descartado, uma `SendAsync` que estoure) sobe pelo `_mediator.Publish` e faz o
  `EnviarMensagemCommandHandler` retornar 500 — **depois** de a mensagem já ter sido persistida. O
  usuário vê "erro ao enviar" (com o novo aviso de retry do AC-52, inclusive) e, ao reenviar,
  duplica a mensagem. Um `try/catch` com log ao redor do bloco de fan-out — ou um
  `INotificationPublisher` que não propague — fecha isso.

#### 4. O código novo de maior risco não tem teste nenhum
**Arquivos:** `ChatSignalRNotificationHandlers.cs` (todo), `tests/ChamadosCamarj.UnitTests/ChamadosCamarj.UnitTests.csproj`

`ChatNovaMensagemNotificationHandler` concentra a lógica que decide **quem recebe o quê** — é o
ponto exato do achado #2 — e não tem uma linha de teste. Nem ele nem
`ChatPerfilAtualizadoNotificationHandler`. Causa estrutural: o projeto de testes referencia só
`ChamadosCamarj.Domain` e `ChamadosCamarj.Application`; os handlers de SignalR vivem em
`ChamadosCamarj.WebApi`, fora do alcance. Casos que mereceriam teste e hoje ninguém garante:
participante inativo é excluído; remetente é excluído; conversa inexistente é no-op;
`Mensagem` que não seja `ChatMensagemResponse` cai no `return` silencioso (hoje todos os publishers
mandam `ChatMensagemResponse`, mas o record declara `object Mensagem` — nada impede alguém de mandar
um anônimo amanhã e o fan-out sumir sem erro). Adicionar `ProjectReference` para `WebApi` no
`.csproj` de testes destrava tudo isso, ou mover os handlers para uma pasta testável.

#### 5. `SignalRProvider` não tem retry na conexão inicial — e a leva inteira depende dela
**Arquivo:** `frontend/src/hooks/useSignalR.tsx:64-71`

Toda a Rodada A e B passou a depender do `ChamadosHub` (`/hubs/chamados`). Essa conexão faz
`conn.start().catch(() => setIsConnected(false))` e mais nada. `withAutomaticReconnect()` **só cobre
queda de uma conexão que chegou a ser estabelecida** — um `start()` que falha (API reiniciando,
blip de rede no momento do login) não é retentado nunca, e depois de esgotadas as 4 tentativas
padrão do reconnect, `onclose` dispara e também não há retry manual. Em qualquer um desses casos o
usuário fica a sessão inteira sem `ChatPerfilAtualizado` e sem `ChatConversaAtualizada`, em silêncio
— exatamente o sintoma que os Bugs #10/AC-47 queriam eliminar. É assimétrico: o `ChatHub`
(`useChatSignalR.ts`) ganhou retry manual com backoff 1s→30s e `status` exposto na correção do Bug #3;
o hub global, que agora carrega mais responsabilidade, não. O comentário em `App.tsx:68-70` reconhece
a limitação, mas reconhecer não é mitigar.

#### 6. `MensagemInput`: o timer de debounce continua vivo depois do unmount
**Arquivo:** `frontend/src/features/chat/components/MensagemInput.tsx:88-95`

A correção criou `cancelarPararDigitar` (bom), mas o chamou **no corpo** do efeito de troca de
conversa, não no cleanup:

```tsx
useEffect(() => {
  setConteudo(''); setErro(''); setMostrarEmojiPicker(false)
  digitandoRef.current = false
  cancelarPararDigitar()          // ← corpo, não cleanup
}, [conversaId, cancelarPararDigitar])
```

Funciona para troca de conversa (o efeito reroda), mas **não** para desmontagem: sair de `/chat` com
um timer pendente deixa o `setTimeout` disparar até 3s depois, chamando `onPararDigitar` sobre uma
conexão SignalR já parada. Trocar por `return cancelarPararDigitar` resolve e cobre os dois casos.
Menor: `enviar()` no `onSuccess` chama `onPararDigitar` mas não cancela o timer pendente — 3s depois
sai um `PararDigitar` duplicado.

#### 7. AC-52: o texto de retry aparece em erro de arquivo, onde o retry não existe
**Arquivo:** `frontend/src/features/chat/components/MensagemInput.tsx:160-170, 202-207, 274-283`

`erro` é um estado único compartilhado por `enviarMensagem` e `enviarArquivo`, e o alerta agora
concatena sempre `"Toque em enviar para tentar de novo."`. Para texto está correto — verifiquei que
`enviar()` só limpa `conteudo` no `onSuccess`, então o texto é preservado e o clique reenvia de fato.
Para arquivo a instrução é falsa em dois níveis: o botão "enviar" está `disabled` (`!conteudo.trim()`)
e, mesmo habilitado, enviaria texto e não o arquivo. Pior: `handleArquivo` só limpa
`fileInputRef.current.value` no `onSuccess` — depois de uma falha, **reescolher o mesmo arquivo não
dispara `onChange`**, então o usuário literalmente não consegue tentar de novo sem escolher outro
arquivo antes. Sugestão: limpar `fileInputRef.current.value` também no `onError`, e condicionar o
sufixo do texto à origem do erro (dois estados, ou um `{ mensagem, origem }`).

#### 8. Preview de imagem: uma requisição HTTP por imagem, sem lazy-load e sem tratamento de falha do `<img>`
**Arquivo:** `frontend/src/features/chat/components/MensagemItem.tsx:24-34, 207-232`

O hook está corretamente posicionado antes do early-return de `Sistema` (o comentário na linha 72
está certo, e confirmei que não há outro hook depois do return — Rules of Hooks OK). O
`enabled: ehImagem && !mensagem.deletada` combinado com o branch `mensagem.deletada` vir **antes**
no ternário evita o estado "isPending eterno" que `enabled: false` produziria no TanStack v5 — está
correto, mas por uma dependência sutil entre duas partes distantes do arquivo; um comentário ali
ajudaria. Pontos abertos:

- Cada mensagem-imagem dispara seu próprio `obterUrlArquivo(mensagemId)`. Uma conversa com 40 fotos
  = 40 requisições ao abrir. Um endpoint em lote (`POST /chat/arquivos/urls` com N ids), no mesmo
  espírito do `ObterConteudosPorIdsAsync` que resolveu o Bug #7, evitaria isso.
- `staleTime: 50min` sem `gcTime`: o padrão de `gcTime` é 5min, então a promessa do comentário
  ("cache client-side por 50min") só vale enquanto o componente estiver montado. Sem impacto
  funcional, mas o comentário afirma mais do que o código entrega.
- Sem `loading="lazy"` no `<img>`, e sem `onError` — se a URL assinada expirar entre o fetch e a
  renderização, o usuário vê o ícone de imagem quebrada do navegador, não o estado "Falha ao
  carregar" que o componente sabe desenhar (esse só cobre falha da *query*, não do carregamento).
- O branch de imagem não oferece botão de download (só clique abrindo em aba nova), ao contrário do
  branch de arquivo genérico.

#### 9. Alerta duplicado ao revogar acesso com a tela `/chat` aberta
**Arquivos:** `frontend/src/layouts/AppLayout.tsx:57-66`, `frontend/src/features/chat/ChatPage.tsx:33-36`

Com a pessoa em `/chat`, a revogação dispara dois eventos por dois hubs independentes:
`AcessoRevogado` (ChatHub → `ChatPage` mostra alerta + `navigate('/chamados')` em 3s) e
`ChatPerfilAtualizado` (ChamadosHub → `AppLayout` mostra "Seu acesso ao chat foi revogado."). Os dois
alertas aparecem empilhados, em ordem não determinística. Não quebra nada, mas é ruído — e sugere que
`ChatAcessoRevogadoNotification` virou redundante agora que `ChatPerfilAtualizado` cobre o caso pelo
canal global e alcança também quem *não* está em `/chat`. Vale considerar aposentar o evento antigo
e deixar `ChatPage` reagir ao `chatPerfil` do `AuthContext`.

#### 10. AC-48 não vale para quem estava offline no momento da mudança
**Arquivo:** `frontend/src/auth/AuthContext.tsx:32-42, 44`

`perfil` (incluindo `chatPerfil`) é hidratado exclusivamente de `localStorage` no boot e nunca
revalidado contra o servidor — só um novo login traz `chatPerfil` fresco. Quem foi revogado enquanto
estava deslogado (ou com a aba fechada) volta com o link "Chat" visível, clica, e leva 403 nas
escritas. Comportamento pré-existente, mas é o limite prático da garantia "sem logout/login" que o
AC-48 anuncia — vale registrar como tal. Um `GET /auth/me` no boot fecharia isso.

---

### 🟢 Nitpicks (informativo)

- **`{mensagem.tamanhoBytes && (...)}` renderiza `0`** (`MensagemItem.tsx:228` e `:238`): arquivo de
  0 bytes faz o React imprimir o literal "0" na bolha. Padrão pré-existente na linha 238, copiado
  para o branch de imagem novo. `!= null` ou `> 0` resolve.
- **`ChatAcao` não tem `AcessoRestaurado`** (`Domain/Enums/ChatAcao.cs`): restauração é auditada como
  `AcessoConcedido`, indistinguível de uma concessão inicial exceto pelo `perfilAnterior` no JSON de
  detalhe. Aceitável, mas o `detalhe` vira a única fonte da distinção.
- **`atualizarChatPerfil` sem `useCallback`** (`AuthContext.tsx:77`): a identidade muda a cada
  mudança de `perfil` (via o `useMemo([perfil])`), o que rederruba e refaz o `subscribe` do
  `AppLayout` a cada evento de perfil. Sem bug — o `setPerfil` é funcional, então não há closure
  velha, e o `unsub` é devolvido corretamente — mas é churn desnecessário.
- **`localStorage.setItem` dentro do updater de `setPerfil`** (`AuthContext.tsx:78-84`): efeito
  colateral em função de atualização de estado; o StrictMode do React 19 invoca updaters duas vezes
  em dev. É idempotente aqui, então inofensivo, mas o lugar canônico seria um `useEffect` sobre
  `perfil`.
- **`setTimeout(..., 6000)` sem `clearTimeout`** (`AppLayout.tsx:65`): timer não cancelado no
  unmount, e dois eventos em menos de 6s fazem o primeiro timer apagar o segundo aviso cedo. Mesmo
  padrão já existente em `slaAlerta` (linha 50).
- **`SendAsync("ChatConversaAtualizada", cancellationToken)`** (`ChatSignalRNotificationHandlers.cs:45`):
  confirmei que resolve corretamente para a sobrecarga `SendAsync(string, CancellationToken)` — o
  token não vai como argumento serializado. Mas o idioma é ambíguo à leitura (as sobrecargas
  `(string, object, CancellationToken)` e `(string, CancellationToken)` são ambas aplicáveis, e só a
  regra de conversão exata desempata). Mesmo padrão pré-existente em
  `ChatAcessoRevogadoNotificationHandler:106`. Um `default` explícito ou um comentário evitaria a
  releitura.
- **Emoji sempre inserido no fim** (`MensagemInput.tsx:172-175`): `inserirEmoji` concatena ao final
  em vez de na posição do cursor, não devolve o foco ao `<textarea>` e não dispara o sinal de
  "digitando" (só `handleChange` faz isso) — quem manda só emoji nunca aparece como digitando. A
  lista em si está limpa: 84 emojis, **zero duplicatas** (verifiquei programaticamente — importa
  porque o `key={emoji}` quebraria com repetição).
- **Clique fora do picker está correto**: `mousedown` no `document` com early-return para
  `emojiPickerRef` **e** `emojiBotaoRef` — sem esse segundo guard, clicar no botão fecharia (mousedown)
  e reabriria (click). Bem feito.
- **`<img>` clicável sem `role`/handler de teclado** (`MensagemItem.tsx:219-224`): abre em aba nova
  só no mouse. Acessibilidade menor.
- **`useConversas()` incondicional no `AppLayout`** (`AppLayout.tsx:41`): dispara
  `GET /api/chat/conversas` para todo usuário logado, inclusive quem nunca teve acesso ao chat.
  Pré-existente, mas relacionado ao achado #2 e resolvido pelo mesmo `enabled`.
- **`lastEvent` no `useMemo` do contexto** (`useSignalR.tsx:80`): muda a cada evento SignalR, então
  todo consumidor de `useSignalR()` rerenderiza a cada evento, mesmo os que só usam `subscribe`.
  Pré-existente; `subscribe` é estável (`useCallback` com `[]` sobre um `Set` em ref), então não há
  loop de efeito.

---

## Verificações que fiz e deram certo (para o registro)

| Ponto verificado | Resultado |
|---|---|
| `Clients.User` com múltiplas conexões (multi-aba/multi-dispositivo) | ✅ SignalR entrega a todas as conexões do `UserIdentifier`; `SubClaimUserIdProvider` lê `sub`, que casa com `UsuarioPerfil.Id` |
| Usuário desconectado | ✅ `Clients.User(...)` é no-op silencioso, sem exceção — nenhum dos dois handlers quebra |
| Query de participantes: ativos vs inativos | ✅ `p.Ativo` filtra corretamente; `ObterPorIdAsync` **tem** `.Include(c => c.Participantes)`, então a coleção não vem vazia (era o risco real com lazy loading desativado) |
| Exclusão do remetente | ✅ `p.UsuarioId != mensagem.AutorId`; para mensagem de sistema `AutorId == Guid.Empty`, logo ninguém é excluído por engano |
| `ChatConversaAtualizada` indo para não-participante | ✅ Não acontece — só participantes. O problema é outro (achado #2: participante *revogado*) |
| Payload do `ChatConversaAtualizada` | ✅ Vazio — não carrega conteúdo de mensagem |
| `revogou` / `restaurou` mutuamente exclusivos, troca lateral neutra | ✅ Lógica correta e coberta por teste |
| Rules of Hooks em `MensagemItem` | ✅ `useUrlPreviewImagem` chamado antes do único early-return; nenhum hook depois dele |
| Listeners do picker removidos | ✅ `removeEventListener` para `mousedown` e `keydown` no cleanup, com guard `if (!mostrarEmojiPicker) return` |
| `queryKey` da invalidação bate com a query | ✅ `['chat','conversas']` idêntico ao de `useConversas` |
| Texto digitado preservado no erro de envio (AC-52) | ✅ `setConteudo('')` só no `onSuccess` |
| Cobertura de AC-46/47 em `DefinirChatPerfilHandlerTests.cs` | ✅ Adequada — restauração, não-revogação, `[Theory]` das 3 transições, troca lateral, no-op |

---

## Gate Checks (executados por mim, de forma independente)

| Check | Comando | Resultado obtido |
|---|---|---|
| Build da solução | `dotnet build` (raiz) | ✅ **0 Erro(s), 0 Aviso(s)** — 5 projetos, 3,57s |
| Testes unitários | `dotnet test --no-build` | ✅ **291 aprovados, 0 falhas, 0 ignorados, 291 total** (184 ms) |
| Build do frontend | `npm run build` (em `frontend/`) | ✅ exit code 0, `built in 763ms`. Warnings: 2× `INVALID_ANNOTATION` em `node_modules/@microsoft/signalr/dist/esm/Utils.js` e 1× aviso de chunk >500 kB — todos pré-existentes, de terceiros/bundling, não deste diff |

---

## Veredito Final

**BLOQUEANTE**, por dois achados.

O achado **#1** é uma reincidência parcial do bloqueante da revisão anterior: a correção daquele
problema foi feita por cópia literal de código em vez de extração, e a Fase 9 — desenvolvida depois —
só evoluiu a cópia original. AC-46, AC-47 e AC-48 valem apenas para o `ChatPerfilSelect` da tabela;
pelo dialog "Editar usuário" nada disso acontece. Enquanto os dois handlers duplicarem a lógica, isso
vai se repetir a cada mudança.

O achado **#2** é o mais sério em termos de consequência: o fan-out novo do Bug #10 revela que
"revogar acesso ao chat" nunca revogou **leitura** — nenhuma das quatro queries de leitura do chat
chama `ChatPerfilGuard.ExigirAcesso`, e o revogado permanece participante ativo por decisão de
design. A lacuna é pré-existente e eu a marco como tal, mas esta leva passou a empurrar ativamente o
navegador do usuário revogado a recarregar a lista de conversas — com preview da última mensagem — a
cada mensagem nova. Antes era uma porta destrancada; agora tem alguém batendo nela em tempo real.

Os achados 🟡 #3 a #10 não travam a promoção, mas #3 (exceção no handler de notificação derrubando um
comando já commitado) e #5 (conexão global sem retry inicial, carregando funcionalidade crítica) são
os que eu trataria primeiro na sequência — ambos produzem falhas silenciosas ou confusas em produção,
que é a classe de problema mais cara de diagnosticar depois.

Nada do que reportei aqui foi corrigido por mim: não editei nenhum arquivo de código.
