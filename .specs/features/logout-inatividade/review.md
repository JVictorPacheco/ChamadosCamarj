# Review: Logout por Inatividade — rodada 1

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-01
> **Escopo:** `git diff develop...feature/logout-inatividade` (commit `f97ab9e`): 6 arquivos, +243/-11 linhas (4 de código: `useInactivityLogout.ts`, `logoutInatividade.ts`, `AppLayout.tsx`, `LoginPage.tsx`)
> **Veredito:** BLOQUEADO

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Parcial | `frontend/src/hooks/useInactivityLogout.ts:51-74` (timer + conferência da última atividade) e `frontend/src/layouts/AppLayout.tsx:95-99` (liga o hook com `sair`). A regra de 20 min funciona com a tela parada, mas a contagem é reiniciada por rolagens que o próprio navegador faz quando a tela se atualiza (R-01) e não confere tempo vencido ao montar nem ao voltar da suspensão (R-02). |
| AC-02 | Sim | `frontend/src/auth/logoutInatividade.ts` (marca em `sessionStorage`), `AppLayout.tsx:97` (marca antes de `sair`), `frontend/src/auth/LoginPage.tsx:23-24,105-109` (lê no `useState`, limpa no `useEffect`, mostra o `Alert`). Texto igual ao da spec. Recarregar não repete a mensagem. |
| AC-03 | Sim | `useInactivityLogout.ts:6-25,53-61`: última atividade em `localStorage` (gravada no máximo a cada 15 s); quando o timer de uma aba vence, ela reagenda pelo que falta se outra aba teve atividade. Na pior das hipóteses a aba parada sai até 15 s antes dos 20 min contados da última interação real na outra aba (precisão aceita na seção 6 da spec). |
| AC-04 | Não | O callback numa `ref` e o efeito que depende só de `minutos` (`useInactivityLogout.ts:37-43,77`) resolvem o caso das re-renderizações. Mas `scroll` na `window` também dispara por *scroll anchoring* quando o tempo real insere conteúdo acima da área visível, e isso conta como atividade (R-01, confirmado no Chromium). |
| AC-05 | Sim | Hook chamado só no `AppLayout`, que só monta dentro do `ProtectedRoute` (`App.tsx:63-77`), igual para os três perfis. `LoginRoute` não monta o hook. |
| AC-06 | Sim | `npm run build` passou (tsc -b + vite). `npm run lint` sem avisos nos arquivos alterados; os 8 avisos que aparecem já existiam (`button`, `badge`, `AuthContext:148`, `useTheme`, `useSignalR`, `useChatSignalR`, `sidebar`). |

## Achados

### 🔴 Bloqueante R-01 — Atualização em tempo real gera evento `scroll` e mantém a sessão viva para sempre
- **Onde:** `frontend/src/hooks/useInactivityLogout.ts:3` (`'scroll'` em `EVENTOS_DE_ATIVIDADE`, ouvido na `window`) combinado com `frontend/src/layouts/AppLayout.tsx:64-67,268-275` (alerta de SLA inserido no topo do `SidebarInset` e removido 8 s depois) e com as telas que refazem a busca a cada evento do SignalR (`FilaAtendimentoPage.tsx:82`, `kanban/useKanbanChamados.ts:45`, `dashboard/hooks.ts:17,38`).
- **Problema:** quando um conteúdo é inserido ou removido acima da área visível de um documento que já foi rolado, o navegador ajusta `scrollY` sozinho (*scroll anchoring*, ligado por padrão no Chrome e no Edge) e dispara `scroll` na `window`. O hook não distingue essa rolagem de uma rolagem do usuário, então a trata como interação. Verifiquei no Chromium (Playwright, página estática): com o documento rolado até `scrollY=1500`, fazer `prepend` de um bloco de 60 px disparou `scroll` (`scrollY` 1500→1560), e removê-lo disparou outro (1560→1500), sem nenhum mouse ou teclado. Isso descumpre o AC-04 ("tempo real não conta como atividade nem atrasa o desligamento") e, por consequência, o AC-01. O teste da T03 usou relógio simulado sem eventos de tempo real, por isso não pegou o caso.
- **Cenário de falha:** um Atendente deixa a Fila (ou a lista de chamados ou o Kanban) rolada um pouco para baixo e sai da mesa sem bloquear o computador. O `SlaMonitorService` roda a cada 5 min e manda `SlaAtencao`/`SlaAtrasado` ao grupo de atendimento sempre que um chamado muda de faixa de SLA. Cada alerta é inserido no topo do `SidebarInset`: o anchoring gera `scroll`, que chama `registrarAtividade`, que grava "agora" no `localStorage` e reinicia o timer. 8 s depois o alerta sai e acontece um segundo `scroll`. Os novos chamados e mudanças de status que a Fila busca de novo também inserem linhas acima da área visível. Enquanto chegarem eventos com intervalo menor que 20 min (o normal num dia de expediente), a sessão do Atendente ou Admin fica aberta sem ninguém no computador, que é exatamente o risco que a feature deveria fechar.
- **Correção sugerida:** parar de usar `scroll` como sinal de atividade. Usar só eventos que nascem de uma ação física: `wheel`, `touchstart`/`touchmove` (registrados com `{ passive: true }`), `keydown`, `pointerdown` e `mousemove`/`pointermove`. Todos chegam à `window` por bubbling, inclusive quando a rolagem acontece num contêiner interno (o que também resolve o R-03). Depois, repetir o cenário da T03 com um evento de SLA simulado e a tela rolada.

### 🟡 Atenção R-02 — Sessão vencida volta a valer ao reabrir o sistema ou ao acordar o computador
- **Onde:** `frontend/src/hooks/useInactivityLogout.ts:62-72`: `registrarAtividade()` roda na montagem e a cada evento, e grava "agora" e reinicia o timer **sem antes conferir** se a última atividade gravada já passou do limite. A conferência só existe em `verificar()`, quando o timer vence.
- **Problema:** se o timer não teve como rodar (navegador fechado, aba descartada pelo "Economia de memória" do Chrome ou do Edge, aba congelada, computador suspenso), nada desconecta a sessão. O token dura 10 h (`AuthSettings.TokenExpiracaoHoras = 10`) e continua no `localStorage`, assim como o perfil. Na primeira montagem ou no primeiro evento, a sessão é renovada por mais 20 min. Isso responde à pergunta da primeira aba que lê uma "última atividade" velha: a aba **nunca lê** esse valor, ela sobrescreve. Daí vem o lado bom (um login novo nunca é derrubado por um valor antigo) e o lado ruim (uma sessão que já devia ter caído revive).
- **Cenário de falha:** (a) um Admin fecha o navegador às 12:00 sem clicar em "Sair". Às 13:30 outra pessoa abre o navegador no mesmo computador e acessa o sistema: entra direto como Admin. São 90 min sem nenhuma interação, sem logout. (b) A aba do sistema fica em segundo plano e o Chrome a descarta para liberar memória. Uma hora depois alguém clica na aba, a página recarrega, o `AppLayout` monta e a sessão continua. (c) O notebook entra em suspensão com a aba aberta. Se o relógio monotônico do timer não avançou durante a suspensão, o `mousemove` que acorda a tela chega antes do timer vencer e renova a sessão. Os casos (a) e (b) independem de detalhe do navegador; o (c) depende do SO e do navegador.
- **Correção sugerida:** em `registrarAtividade` (e na montagem), antes de gravar: se existe uma última atividade gravada e `Date.now() - ultima >= limiteMs`, chamar `aoExpirarRef.current()` em vez de renovar. Para não derrubar um login novo, gravar "agora" na chave de última atividade no `loginComSenha`/`loginComGoogle` do `AuthContext` (ou remover a chave no `logout`). Como isso mexe no `AuthContext`, que é compartilhado, avisar antes, conforme a regra 5 da constitution. Se o caso (a) for considerado fora do escopo ("nenhuma aba aberta"), registrar isso explicitamente na seção 2 da spec.

### 🟡 Atenção R-03 — Rolagem dentro de contêineres internos não conta como atividade
- **Onde:** `frontend/src/hooks/useInactivityLogout.ts:3,71`. `scroll` não faz bubbling, então rolar um elemento com `overflow-y-auto` não chega à `window`.
- **Problema:** o AC-01 lista "rolagem" como interação, mas a rolagem só é percebida quando o próprio documento rola. As áreas internas não contam: a lista de mensagens do chat (`MensagemList.tsx:92`), a lista de conversas (`ConversaList.tsx:54`), o painel do `ChatPage.tsx:169`, as listas dos diálogos. No mouse, a roda normalmente vem junto com algum `mousemove`. No toque, nenhum desses eventos acontece.
- **Cenário de falha:** um usuário num tablet ou num notebook com tela sensível ao toque lê uma conversa longa no `/chat` só arrastando o dedo na lista de mensagens, sem tocar em nada clicável, por 20 min. É desconectado no meio da leitura, mesmo tendo interagido o tempo todo.
- **Correção sugerida:** a mesma do R-01 (`wheel` + `touchstart`/`touchmove` passivos + `pointerdown`). Esses eventos fazem bubbling até a `window` a partir de qualquer contêiner.

## Constitution e contratos

- **Regra 1 (sem suposição silenciosa):** cumpre. A regra de negócio (20 min) vem de decisão confirmada em 2026-07-18. O texto da mensagem e o "sem aviso prévio" estão marcados como **proposta** na spec.
- **Regra 2 (spec antes do código):** não dá para verificar a ordem, porque spec e código estão no mesmo commit `f97ab9e`. A spec descreve o comportamento implementado e as decisões técnicas (seção 6).
- **Regra 3 (contrato compartilhado):** cumpre. A assinatura de `useInactivityLogout(minutos, aoExpirar)` não mudou, e o hook não tinha consumidores antes.
- **Regra 5 (código compartilhado):** `AppLayout` está declarado como ponto de toque na seção 6. A mudança só acrescenta a chamada do hook e não altera `sair`, os assinantes do SignalR nem o heartbeat. A análise de regressão desse ponto não cobriu a interação com os eventos de tempo real que o `AppLayout` exibe (alerta de SLA), e é daí que vem o R-01. A correção do R-02 provavelmente mexe no `AuthContext`, que também é compartilhado: avisar antes.
- **Regra 6 (Obsidian):** pendente. O gate "STATE, ROADMAP e Obsidian (`Acesso e Login`)" está desmarcado na spec, o que é esperado antes da fase de fechamento.
- **Mudanças de contrato:** nenhuma de API/backend. As chaves novas de storage são `chamados-camarj:ultima-atividade` (`localStorage`) e `chamados-camarj:logout-por-inatividade` (`sessionStorage`), sem colisão com as existentes (`:token`, `:perfil`, `camarj-theme`).
- **Interações conferidas sem problema:**
  - *401:* se o 401 vem antes, `perfil = null` desmonta o `AppLayout` e o cleanup do hook roda, sem mensagem de inatividade (correto). Um 401 que chega depois do logout por inatividade só repete o `logout`, que é idempotente.
  - *Heartbeat do chat:* faz só chamadas HTTP, sem eventos de DOM, e para quando o `AppLayout` desmonta.
  - *SignalR:* re-renderizações não reiniciam o timer, por causa da `ref`. A exceção é o efeito colateral de layout descrito no R-01.
  - *StrictMode:* montagem, limpeza e montagem de novo removem o timer e os listeners antes de recriar, sem vazamento. A gravação extra no `localStorage` é inofensiva. O `useState(lerSaiuPorInatividade)` roda duas vezes antes do efeito de limpeza e lê `'1'` nas duas.
  - *Convenções da seção 3:* export nomeado, `Alert` do shadcn, sem toast, tokens de tema, hook global em `hooks/`. Cumpre.

## Sugestões (não bloqueiam)

- Depois de corrigir o R-01, conferir no Chrome real (não só com o relógio simulado) se mudanças de layout sob um cursor parado geram `mousemove` sintético. Se gerarem, é a mesma classe de problema do R-01; trocar `mousemove` por `pointermove` não resolve, mas exigir deslocamento real (comparar `screenX`/`screenY` com o evento anterior) resolve.
- Acessibilidade: o `Alert` tem `role="alert"` e é inserido junto com a tela de login, então os leitores de tela costumam anunciar a mensagem. Para garantir, o foco poderia ir para o campo de e-mail quando a tela abre por inatividade. A variante padrão (não `destructive`) é adequada, já que a mensagem não é um erro do usuário.
- Ouvir o evento `storage` da chave do token (ou do perfil) faria as outras abas irem para o login assim que uma delas sai, em vez de cada uma esperar o próprio timer ou o primeiro 401. É comportamento anterior à feature e só aparece aqui.
- Acrescentar à tabela de verificação da spec (seção 5) um cenário de evento de tempo real com a tela parada e rolada, para o AC-04 ficar testado de fato.

## Verificação dos achados pela sessão principal (2026-10-01)

| Achado | Confirmado? | Tratamento |
|---|---|---|
| R-01 🔴 | Sim | **Corrigido:** `scroll` saiu da lista de atividade; entraram `pointerdown`, `wheel` e `touchstart` (passivos). Verificado na tela: 21 min só com `scroll` automático → desconecta. |
| R-02 🟡 | Sim | **Corrigido sem tocar no `AuthContext`:** ao abrir, se a última atividade passou do limite, desconecta na hora; a tela de login grava a marca **antes** de entrar, então um login novo não é derrubado por uma marca antiga. Verificado nos dois casos. |
| R-03 🟡 | Sim | **Corrigido junto com o R-01:** `wheel` e `touchstart` borbulham até a `window`, inclusive a partir das áreas com rolagem interna do chat. |
| Extra (achado na verificação) | — | Quando a desconexão acontecia logo na abertura, a mensagem sumia antes de aparecer (a tela de login é montada mais de uma vez no redirecionamento). Agora a mensagem fica até o próximo login. |
