# Review: Logout por Inatividade — rodada 2

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-01
> **Escopo:** `git diff develop...feature/logout-inatividade` (commits `f97ab9e`, `c044943`, `7f19f4b`): 8 arquivos, +350/-12 linhas (4 de código: `useInactivityLogout.ts`, `logoutInatividade.ts`, `AppLayout.tsx`, `LoginPage.tsx`, +134/-11)
> **Veredito:** BLOQUEADO

## Achados da rodada 1

| Achado | Resolvido? | Evidência |
|---|---|---|
| R-01 🔴 (`scroll` automático mantém a sessão) | **Sim** | `scroll` saiu de `EVENTOS_DE_ATIVIDADE` (`useInactivityLogout.ts:7`). Conferi no Chromium 153 (Playwright, página estática): com o cursor parado sobre o documento rolado em `scrollY=1500`, inserir e remover um bloco no topo gerou **só** `scroll` (1500→1560→1500), sem nenhum `mousemove`/`pointermove` sintético. Uma mudança de layout sob o cursor sem rolagem também não gerou eventos. A dúvida das sugestões da rodada 1 fica respondida para o Chromium. |
| R-02 🟡 (sessão vencida reaberta) | **Parcial** | Os casos (a) navegador fechado e (b) aba descartada estão resolvidos pela conferência na montagem (`useInactivityLogout.ts:87-92`) e pela marca gravada antes do login (`LoginPage.tsx:38`). O caso (c), computador suspenso, **não**: a conferência na hora do evento, sugerida na rodada 1, não foi feita, e o AC-01 agora promete esse caso de forma explícita. Ver R2-01. A reabertura também deixa a área logada montar e fazer requisições antes de desconectar. Ver R2-02. |
| R-03 🟡 (rolagem interna não conta) | **Sim** | `wheel`, `touchstart`, `pointerdown` e `keydown` fazem bubbling até a `window` a partir de qualquer contêiner. Não há `stopPropagation` no `frontend/src`. |

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Parcial | Timer e conferência entre abas: `useInactivityLogout.ts:56-78`. Reabertura depois do prazo: `:87-92` (navegador fechado e aba descartada). **Falta o caso "computador suspenso"** (R2-01). A reabertura ainda mostra um quadro da área logada e dispara requisições com o token (R2-02). |
| AC-02 | Sim | `logoutInatividade.ts` (marca em `sessionStorage`), `AppLayout.tsx:96-99`, `LoginPage.tsx:24,108-112`. A mensagem fica até o próximo login, conforme a spec. Ela também pode aparecer **fora** do contexto (R2-03). |
| AC-03 | Sim | `localStorage` compartilhado, gravado no máximo a cada 15 s (`:71-78`). `verificar()` reagenda pelo tempo que falta (`:62-69`). A conferência na montagem lê a mesma marca, então abrir uma aba nova com outra aba ativa não desconecta (atraso máximo da marca: 15 s). |
| AC-04 | Sim | Callback numa `ref` e efeito dependente só de `minutos` (`:42-46,99`). Sem `scroll`, e o Chromium não gera `mousemove` sintético no *scroll anchoring* (teste acima). |
| AC-05 | Sim | Hook só no `AppLayout`, dentro do `ProtectedRoute` (`App.tsx:63-77`), igual para os três perfis. A `LoginPage` só lê e grava as marcas, sem timer. |
| AC-06 | Sim | `npm run build` passou (tsc -b + vite). `npm run lint`: os mesmos 8 avisos que já existiam (`useTheme`, `AuthContext:148`, `button`, `badge`, `sidebar`, `useSignalR` x2, `useChatSignalR`), nenhum nos arquivos alterados. |

## Achados

### 🔴 Bloqueante R2-01 — Depois da suspensão do computador, a primeira mexida no mouse renova a sessão em vez de desconectar
- **Onde:** `frontend/src/hooks/useInactivityLogout.ts:71-78` (`registrarAtividade`)
- **Problema:** a conferência "passou do limite?" só existe em dois lugares: na montagem (`:87-92`) e quando o timer vence (`verificar`, `:62-69`). `registrarAtividade` grava "agora" e reinicia o timer **sem conferir** quanto tempo passou desde a última atividade gravada. Na suspensão a página não é remontada, então a conferência da montagem não roda. Sobra o timer, que conta no relógio monotônico do navegador. Esse relógio **para durante a suspensão** no macOS, no Linux e no Android, e no Windows o comportamento depende da plataforma. Além disso, o agendador do Chromium dá prioridade a eventos de entrada sobre timers, então mesmo um timer vencido pode rodar depois do primeiro `mousemove`. A spec, depois da rodada 1, diz no AC-01: "Se o sistema for reaberto (… computador suspenso) depois desse prazo, a desconexão acontece na hora, sem dar acesso à tela". A tabela de verificação não tem nenhum cenário de suspensão.
- **Cenário de falha:** um Admin fecha a tampa do notebook às 12:00, com o sistema aberto e 5 min de inatividade acumulados, e vai almoçar. Às 13:30 alguém abre a tampa e mexe no mouse. O timer da aba ainda conta cerca de 15 min, porque o relógio monotônico não andou na suspensão. O `mousemove` chama `registrarAtividade`, que grava 13:30 no `localStorage` e reagenda para 13:50. A pessoa usa o sistema como Admin, sem ter feito login.
- **Correção sugerida:** no começo de `registrarAtividade`, ler a marca e, se `ultima > 0 && Date.now() - ultima >= limiteMs`, chamar `aoExpirarRef.current()` e sair sem gravar. Isso usa o relógio de parede, que avança na suspensão. Conferir também no `visibilitychange` (ao voltar para `visible`) e no `focus`, para desconectar antes do primeiro gesto. Atenção à gravação limitada a 15 s: a marca pode estar até 15 s atrás do último gesto real, então a margem do limite deve considerar isso, ou a própria aba deve guardar o último gesto em memória e usar o maior dos dois. Acrescentar à tabela da T03 um cenário com o relógio de parede adiantado sem disparar os timers (no Playwright, `clock.setSystemTime` em vez de `runFor`/`fastForward`), seguido de um `mousemove`.

### 🟡 Atenção R2-02 — Reabertura vencida: a área logada monta e faz requisições com o token antes de desconectar
- **Onde:** `frontend/src/hooks/useInactivityLogout.ts:87-92` (conferência num `useEffect` do `AppLayout`), combinado com `frontend/src/layouts/AppLayout.tsx:44` (`useChatHeartbeat`, declarado antes do hook) e com as páginas filhas do `<Outlet>`.
- **Problema:** a conferência da reabertura roda num efeito passivo do `AppLayout`. Antes dela, no mesmo commit, rodam os efeitos dos filhos (as consultas TanStack Query da página) e o efeito do `useChatHeartbeat`, que envia `heartbeat()` na hora, com o token ainda válido, porque `clearToken` só acontece depois. Em montagem inicial o React pinta antes de rodar os efeitos passivos, então a área logada (barra lateral com nome e perfil, esqueleto da página) aparece por pelo menos um quadro. O AC-01 diz "sem dar acesso à tela".
- **Cenário de falha:** um Atendente com acesso ao chat fecha o navegador às 12:00 sem clicar em "Sair". Às 14:00 alguém abre o sistema: o `heartbeat` sai com o token e marca o Atendente como **Online** no chat. Em seguida o hook desconecta. Como a conexão SignalR nunca chega a abrir (o token já foi apagado quando o `SignalRProvider` roda o efeito dele), não há desconexão que marque Offline. Os colegas veem o Atendente "Online" até a presença cair sozinha. As requisições da página (`GET /chamados` etc.) também saem autenticadas, e as respostas vão para o cache do `queryClient`, que não é limpo no logout.
- **Correção sugerida:** fazer a conferência **antes** de renderizar a área logada. Por exemplo, uma função pura `sessaoVencidaPorInatividade()` chamada no `ProtectedRoute` (ou no estado inicial do `AuthProvider`), que, se vencida, marca o motivo, chama `logout()` e renderiza `<Navigate to="/login" replace />`. Como isso mexe em `App.tsx`/`AuthContext` (compartilhados), avisar antes, conforme a regra 5. Alternativa mínima: `useLayoutEffect` só para a conferência inicial, o que evita o quadro pintado, mas **não** evita o heartbeat nem as consultas dos filhos.

### 🟡 Atenção R2-03 — O aviso de inatividade persiste na aba e aparece depois de uma saída que não foi por inatividade
- **Onde:** `frontend/src/auth/logoutInatividade.ts:24-31` e `frontend/src/auth/LoginPage.tsx:38-39`. A marca só é apagada no `onSubmit` da tela de login **da própria aba** (o `sessionStorage` é por aba).
- **Problema:** com a mudança "a mensagem fica até o próximo login", a marca deixou de ter um fim garantido. Se a pessoa volta a entrar por outra aba, a marca continua na aba original. Duplicar a aba também copia o `sessionStorage`.
- **Cenário de falha:** as abas A e B são desconectadas por inatividade, cada uma com a marca no próprio `sessionStorage`. A pessoa faz login na aba B (só a marca de B é apagada) e depois recarrega a aba A, que entra direto porque o token é compartilhado. Mais tarde ela clica em **Sair** na aba A, ou o token expira e o 401 a desconecta: a tela de login mostra "Sua sessão foi encerrada por inatividade", que é falso. O mesmo acontece numa aba duplicada a partir da tela de login com o aviso.
- **Correção sugerida:** apagar a marca quando a área logada monta (no `AppLayout`, antes da conferência de inatividade), porque uma sessão ativa nesta aba significa que o aviso anterior já não vale. A limpeza no `onSubmit` pode continuar.

## Constitution e contratos

- **Regra 1 (sem suposição silenciosa):** cumpre. A mudança de comportamento do AC-02 ("fica até o próximo login") e o complemento do AC-01 estão registrados na spec.
- **Regra 3 (contrato compartilhado):** cumpre. A assinatura de `useInactivityLogout` não mudou. O novo export `registrarInicioDeSessao` só é usado pela `LoginPage`.
- **Regra 5 (código compartilhado):** a correção do R-02 evitou mexer no `AuthContext`, como declarado. A consequência é que só o login por senha grava a marca: `loginComGoogle` existe no `AuthContext` sem chamador hoje, mas se for religado (o fluxo de `design-t09-google-oauth.md`), a marca antiga **não** será renovada, e um login Google com uma marca de mais de 20 min no navegador será derrubado na hora. Hoje sem cenário, por isso fica em Sugestões. A análise de regressão do `AppLayout` não considerou a ordem de efeitos em relação ao `useChatHeartbeat` (R2-02).
- **Mudanças de contrato:** nenhuma de API. As chaves de storage são as mesmas da rodada 1.
- **Convenções (seção 3):** cumpre. Export nomeado, `Alert` do shadcn sem toast, hook global em `hooks/`, constante e marca em `auth/`.
- **Interações conferidas sem problema:**
  - *`registrarInicioDeSessao` antes de um login que falha:* grava "agora" sem sessão ativa. Isso é inofensivo: se não há token, não há o que manter vivo. Se há outra aba logada, digitar uma senha errada nesta máquina já é presença física. O próximo login regrava a marca. Um login que falha também apaga a marca do aviso, mas o `Alert` continua na tela, porque foi lido no `useState`. Ele só some se a pessoa recarregar depois da falha, o que é aceitável.
  - *Navegação dentro do efeito de montagem:* chamar `navigate` em `useEffect` é suportado pelo React Router. Junto, o `ProtectedRoute` também redireciona (`replace`) quando `perfil` vira `null`. No StrictMode (só em desenvolvimento), o efeito roda duas vezes e chama `logout` e `navigate('/login')` duas vezes. O `logout` é idempotente, e a única consequência é uma entrada a mais no histórico em desenvolvimento.
  - *`wheel`/`touchstart` passivos:* correto. O hook só lê o evento e nunca chama `preventDefault`, então `passive: true` não muda nada além de não bloquear a rolagem. `touchstart` dispara a cada novo gesto de arrastar, o suficiente para quem lê rolando com o dedo.
  - *Só teclado:* `keydown` na `window` cobre Tab, setas, PageDown e espaço, inclusive a rolagem por teclado. Sem `stopPropagation` no código, e os componentes Radix usados não interrompem o bubbling de `keydown`.
  - *`pointerdown` em telas de toque:* dispara junto com `touchstart` e não interfere no toque nem na rolagem (não é cancelável para rolagem). No toque, o navegador também emula `mousemove` no tap. Sem efeito colateral.
  - *Abertura de aba nova com sessão ativa em outra:* a marca tem no máximo 15 s de atraso, então a conferência da montagem não derruba ninguém.

## Sugestões (não bloqueiam)

- **Leitor de tela no modo de navegação:** NVDA e JAWS no "browse mode" movem o cursor virtual sem gerar `keydown` na página. Antes, o `scroll` causado pelo cursor virtual contava. Uma pessoa que lê um chamado longo só com o leitor de tela por 20 min seria desconectada. Isso é improvável e não tem usuário conhecido, mas, se houver, `focusin` (que o cursor virtual costuma disparar ao passar por elementos focáveis) é um sinal barato a acrescentar.
- **Login pelo Google:** se `loginComGoogle` for religado, mover `registrarInicioDeSessao()` para dentro de `loginComGoogle`/`loginComSenha` (ou para um ponto comum), para não depender de cada tela de login lembrar de chamá-lo.
- **Montagem conta como atividade:** `registrarAtividade()` na montagem (`:91`) renova a marca sem gesto. Se o Windows reiniciar sozinho (atualização) menos de 20 min depois do último uso e o Chrome restaurar a aba, a sessão ganha até mais 20 min. A extensão é limitada, então é aceitável. Usar a marca existente em vez de "agora" na montagem eliminaria o caso.
- **Cache entre usuários (anterior à feature):** o `logout` não limpa o `queryClient`. Depois de uma desconexão por inatividade, quem entrar na mesma aba com outra conta pode ver, por um instante, os dados em cache da conta anterior, até a nova busca terminar. Isso vale também para o "Sair" comum, mas é justamente o cenário de computador compartilhado que esta feature quer cobrir. Um `queryClient.clear()` no logout resolveria.
- Acrescentar à seção 5 da spec os cenários "suspensão (relógio de parede adiantado sem disparar timers) + mousemove" e "reabertura vencida não envia heartbeat".
