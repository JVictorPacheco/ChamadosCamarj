# Review: Logout por Inatividade — rodada 3

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-02
> **Escopo:** `git diff develop...feature/logout-inatividade` (commits `f97ab9e`, `c044943`, `7f19f4b`, `036ba9b`, `2f6ebce`): 10 arquivos, +495/-12 linhas. São 5 arquivos de código: `useInactivityLogout.ts`, `logoutInatividade.ts`, `App.tsx`, `AppLayout.tsx` e `LoginPage.tsx`.
> **Veredito:** APROVADO COM RESSALVAS

## Achados da rodada 2

| Achado | Resolvido? | Evidência |
|---|---|---|
| R2-01 🔴 (suspensão renovava a sessão) | **Sim** | `registrarAtividade` confere `tempoParado()` pelo relógio de parede antes de renovar (`useInactivityLogout.ts:94-99`). `tempoParado` usa o maior valor entre `ultimaLocal` e a marca, o que cobre o atraso de até 15 s da gravação (`:74`). O primeiro gesto depois da suspensão desconecta em vez de renovar. `focus` e `visibilitychange` só chamam `verificar` (`:88-92`, `:116-117`), que expira ou reagenda e **nunca** renova a marca. A flag `expirado` impede que `aoExpirar` rode duas vezes na mesma instância do efeito. Ressalva: o listener de teclado roda **depois** dos handlers do React (R3-01). |
| R2-02 🟡 (área logada montava na reabertura vencida) | **Sim**, para URL protegida | `ProtectedRoute` confere antes de montar `SignalRProvider`/`AppLayout` (`App.tsx:83-85`). O efeito de `EncerrarSessaoPorInatividade` roda antes do efeito do `AuthProvider` (filhos primeiro), então `clearToken` acontece antes de `obterPerfilAtual` e o boot não chama `/auth/me`. Entrando pela raiz `/` ou por `/login`, uma chamada a `/auth/me` ainda sai (ver Sugestões). Ela é só leitura, e o resultado final continua sendo o logout. |
| R2-03 🟡 (aviso antigo num "Sair" normal) | **Sim** | `useEffect(limparLogoutPorInatividade, [])` no `AppLayout` (`:97`) fica declarado antes do hook. No caminho em que a expiração acontece na montagem, a limpeza roda primeiro e a marcação depois, então o aviso aparece. No StrictMode a ordem limpa → marca → limpa → marca também termina com a marca gravada. |

## Pontos pedidos, conferidos sem problema

- **`EncerrarSessaoPorInatividade` e o `logout` instável:** o `logout` vem do `value` memoizado por `[perfil]`, então só muda quando `perfil` muda. O efeito roda uma vez, o `logout` leva `perfil` a `null`, e o `ProtectedRoute` troca o componente por `<Navigate to="/login">`, que o desmonta. Não há loop. No StrictMode o efeito roda duas vezes, mas `logout` é idempotente. Se a resposta tardia de `/auth/me` devolver o perfil (ver Sugestões), o ciclo se repete **uma** vez e termina, porque a marca continua vencida.
- **Conferência durante a renderização do `ProtectedRoute`:** ela lê só a marca, sem `ultimaLocal`. Com a sessão ativa, a marca fica no máximo 15 s atrás do último gesto. Toda navegação feita pela pessoa passa antes por `pointerdown`/`keydown`, que regravam a marca se ela tiver mais de 15 s. Na pior hipótese, uma re-renderização sem gesto (evento de tempo real) entre 20:00 e 20:15 de inatividade desconecta até 15 s antes do hook. Não chega a ser falha.
- **`ultimaLocal` e a marca atrasada:** `Math.max` cobre os dois sentidos. Com várias abas, a marca de outra aba ativa adia o fim da sessão; o atraso de 15 s nunca encurta a sessão de quem está ativo nesta aba.
- **Primeiro login num navegador sem marca:** `registrarInicioDeSessao()` grava a marca antes de `loginComSenha` (`LoginPage.tsx:38`). `loginComSenha` é o único ponto de login em uso (`loginComGoogle` não tem chamador). Sem marca, `sessaoVencidaPorInatividade` devolve `false` (`ultima > 0`).
- **`focus`/`visibilitychange`:** o `focus` de elementos não borbulha até a `window`, então o listener só dispara quando a janela ganha foco. Nenhum dos dois renova a marca. Os dois são removidos no cleanup.
- **Clique como primeiro gesto depois da suspensão:** o `pointerdown` desconecta. O `setPerfil(null)` de evento discreto é aplicado antes do `click`, e `clearToken` é síncrono, então o clique não executa ação autenticada.

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim, com ressalva | Timer e conferência entre abas: `useInactivityLogout.ts:74-92`. Reabertura vencida antes de montar a área logada: `App.tsx:83-85`. Suspensão: `:94-99`, mais `focus`/`visibilitychange`. Ressalvas: Enter no chat como primeiro gesto depois da suspensão (R3-01); sessões abertas antes do deploy (R3-02); a tela continua visível até o primeiro gesto (Sugestões). Verificação na tela: tasks.md, cenários A–C. |
| AC-02 | Sim | Marca em `sessionStorage` nos dois caminhos: hook (`AppLayout.tsx:100-103`) e rota (`App.tsx:72`). Lida no `useState` da `LoginPage` e apagada no `onSubmit` e na montagem do `AppLayout`. Cenário D verificado. |
| AC-03 | Sim | Marca compartilhada e `verificar` reagendando pelo tempo restante. A conferência na rota e na montagem lê a mesma marca. Exceção durante a transição do deploy (R3-02). |
| AC-04 | Sim | Callback numa `ref`, efeito que depende só de `minutos`, sem `scroll`. Re-renderizações do `ProtectedRoute` não renovam nem reiniciam a contagem. |
| AC-05 | Sim | Conferência no `ProtectedRoute` e hook no `AppLayout`, iguais para os três perfis. A `LoginPage` não tem timer. |
| AC-06 | Sim | `npm run build` passou (tsc -b + vite). Os avisos são só de `@microsoft/signalr` e do tamanho do chunk, que já existiam. `npm run lint`: os mesmos 8 avisos de antes, nenhum nos arquivos alterados. |

## Achados

### 🟡 Atenção R3-01 — Depois da suspensão, Enter no chat envia a mensagem antes de a sessão cair
- **Onde:** `frontend/src/hooks/useInactivityLogout.ts:113-115` (listeners na `window` em fase de *bubble*) e `frontend/src/features/chat/components/MensagemInput.tsx:134-138`.
- **Problema:** o React 19 escuta os eventos no contêiner raiz (`#root`). Na fase de *bubble*, o raiz vem **antes** da `window`, então os handlers `onKeyDown`/`onPointerDown` dos componentes rodam antes de `registrarAtividade`. O `enviar()` do chat chama `mutate`. O `mutationFn` lê o token depois de alguns `await` internos, e essas microtarefas rodam no checkpoint logo depois do listener do raiz, ainda antes do listener da `window`. O token ainda existe nesse momento, então a requisição sai autenticada. A correção do R2-01 desconecta em seguida, mas a ação já foi feita.
- **Cenário de falha:** um Atendente deixa um rascunho no campo do chat e fecha a tampa do notebook às 12:00, e o timer não dispara durante a suspensão. Às 13:30 alguém abre a tampa e o primeiro gesto é **Enter** com o foco no campo. A mensagem é enviada em nome do Atendente, e só então a sessão cai.
- **Correção sugerida:** registrar os listeners de atividade em fase de captura: `window.addEventListener(evento, registrarAtividade, { capture: true, passive: true })`, e o mesmo `capture: true` no `removeEventListener`. A captura na `window` roda antes de qualquer handler do React. Como `logout` faz `clearToken` de forma síncrona, a requisição do handler sai sem token e volta 401, e o 401 só repete o `logout`, que é idempotente. A semântica de "atividade" não muda.

### 🟡 Atenção R3-02 — Sessões abertas antes do deploy: sem marca, a sessão vencida revive, e abas com o código antigo não contam atividade
- **Onde:** `frontend/src/hooks/useInactivityLogout.ts:41-43` (`ultima > 0`: sem marca = não vencida) e `:120-125` (a montagem grava "agora").
- **Problema:** antes do deploy nenhum navegador tem `chamados-camarj:ultima-atividade`. (a) Uma sessão com token válido (dura até 10 h) e sem marca passa pela conferência da rota e da montagem, e a montagem grava "agora": a sessão abandonada ganha mais 20 min. (b) Abas abertas antes do deploy continuam com o bundle antigo até serem recarregadas. Elas não gravam a marca, mas compartilham o token com as abas novas.
- **Cenário de falha:** (a) um Admin entra às 09:00 e fecha o navegador às 12:00 sem clicar em "Sair". O deploy é às 13:00. Às 14:00 outra pessoa abre o sistema nesse computador e entra direto como Admin, que é o caso que o AC-01 promete bloquear. (b) Um Atendente trabalha o dia todo numa aba aberta antes do deploy. Às 14:00 abre um link de chamado numa aba nova (bundle novo) e volta para a aba antiga. Às 14:20 a aba nova não vê atividade nenhuma e desconecta: o token é apagado e a próxima requisição da aba antiga recebe 401. O Atendente perde a sessão no meio do trabalho (AC-03).
- **Correção sugerida:** para (a), tratar "perfil salvo e chave ausente" como sessão vencida. É preciso distinguir chave ausente (`getItem` devolve `null`) de falha de leitura. Isso força **um** novo login de quem estava logado no deploy, o que é aceitável e mais seguro. O primeiro login não é afetado, porque a `LoginPage` grava a marca antes de entrar. Para (b) não há correção barata no código: avisar no PR ou no comunicado do deploy que é preciso recarregar as abas abertas. Se o (a) for aceito como risco de transição, registrar essa decisão na spec (regra 1).

## Constitution e contratos

- **Regra 1 (sem suposição silenciosa):** cumpre. As correções da rodada 2 estão registradas em `tasks.md`, com a verificação dos cenários B, C e D. A seção 5 da spec (rastreabilidade) não ganhou os cenários de suspensão nem de reabertura sem chamadas à API, que estão só em `tasks.md`.
- **Regra 3 (contrato compartilhado):** cumpre. `useInactivityLogout(minutos, aoExpirar)` não mudou. Há um novo export, `sessaoVencidaPorInatividade`, usado pelo `App.tsx` e pelo próprio hook.
- **Regra 5 (código compartilhado):** **não cumpre na documentação**. A correção do R2-02 alterou o `ProtectedRoute` em `App.tsx`, por onde passa toda rota autenticada. O parágrafo "Pontos de toque cross-feature" da seção 6 da spec ainda cita só `AppLayout` e `LoginPage`. Atualizar o parágrafo e a análise de regressão: a conferência roda a cada renderização do `ProtectedRoute` (a cada navegação e a cada mudança de `perfil`), sem efeito colateral enquanto a sessão está válida. O `AuthContext` não foi alterado.
- **Mudanças de contrato:** nenhuma de API ou backend. As chaves de storage não mudaram desde a rodada 1.
- **Convenções (seção 3):** cumprem. `EncerrarSessaoPorInatividade` é um componente local do `App.tsx`, como `LoginRoute` e `ProtectedRoute`. Hook global em `hooks/`, constante e marca em `auth/`, `Alert` do shadcn, sem toast.

## Sugestões (não bloqueiam)

- **Tela visível depois da suspensão até o primeiro gesto:** se o timer congelar durante a suspensão (depende do SO e do navegador), a página logada continua na tela depois de acordar até alguém tocar em algo. Ninguém consegue agir sem disparar o logout, mas dá para **ler** o que estava aberto, e o AC-01 diz "sem dar acesso à tela". Um `setInterval(verificar, 30_000)`, além do timeout, desconecta até 30 s depois de acordar, qualquer que seja o relógio monotônico.
- **Entrada por `/` ou `/login` com sessão vencida:** a primeira renderização não passa pelo `ProtectedRoute` (passa pelo `<Navigate>` do `*` ou pelo `LoginRoute`), então o efeito do `AuthProvider` chama `GET /auth/me` com o token. O endpoint é só leitura (`AuthController.cs:97`). A resposta chega depois do logout e grava o perfil de novo em `localStorage`/estado, o que faz `LoginRoute` → `ProtectedRoute` → `EncerrarSessaoPorInatividade` rodarem mais uma vez antes do logout final. O efeito é inofensivo, mas o "0 chamadas à API" do cenário C só vale para URL protegida. Para fechar de vez, o `.then` do boot pode ignorar a resposta se `getToken()` já for `null`.
- **Mousemove lê o `localStorage` a cada evento:** `tempoParado()` em `registrarAtividade` faz `getItem` dezenas de vezes por segundo. No Chromium isso é cache em memória e não pesa. Se pesar, basta conferir a marca só quando `Date.now() - ultimaLocal` passar de alguns segundos.
- **Relógio do sistema ajustado:** uma correção do relógio para a frente em mais de 20 min (NTP ao acordar uma máquina com relógio errado) desconecta quem está ativo, e uma correção para trás estende a sessão. É uma consequência inerente de usar o relógio de parede. Só registrar.
- **Montagem conta como atividade:** a sugestão da rodada 2 continua valendo. Usar a marca existente em vez de gravar "agora" na montagem resolve também o caso (a) do R3-02 quando há marca.
- **Cache entre usuários (anterior à feature):** `queryClient.clear()` no logout, como na rodada 2.
- Acrescentar à seção 5 da spec os cenários B, C e D de `tasks.md` e, depois da correção, "Enter no chat como primeiro gesto depois da suspensão".

## Verificação dos achados pela sessão principal (2026-10-02)

| Achado | Confirmado? | Tratamento |
|---|---|---|
| R3-01 🟡 | Sim | **Corrigido:** listeners na fase de captura (`capture: true`), removidos com o mesmo flag. |
| R3-02 🟡 | Sim | **Corrigido:** sessão sem marca (aberta antes do deploy) vale como vencida, e quem estiver logado no deploy entra de novo uma vez. Abas com o código antigo deixam de existir com o recarregamento forçado. Registrado no PR/STATE como efeito do deploy. |
| Constitution regra 5 | Sim | Spec §6 atualizada com `ProtectedRoute` como ponto de toque. |
