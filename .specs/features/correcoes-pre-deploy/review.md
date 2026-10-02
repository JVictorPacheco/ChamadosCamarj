# Review: correcoes-pre-deploy — rodada 1

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-02
> **Escopo:** `git diff origin/develop...feature/correcoes-pre-deploy`: 43 arquivos, +1343/-118 linhas, 9 commits (`fa09eb3`..`1ba7615`)
> **Veredito:** BLOQUEADO (1 bloqueante, de correção pequena; o restante está correto)

Verificação feita: leitura de spec, design, tasks, diff completo e do código ao redor (`Chamado.cs`,
`AcessoChamadoBehaviour`, `AplicarVisibilidade`, handlers de escrita, `ChamadosHub`,
`SubClaimUserIdProvider`, `JwtTokenService`, `CurrentUserService`, `apiFetch`, Kanban, Fila).
`dotnet test tests/ChamadosCamarj.UnitTests/` rodou aqui: **438 aprovados, 0 falhas**, e não aparece
`MSB3277`. Não rodei backend, frontend, `npm run build` nem E2E, por causa da restrição de memória da
máquina.

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim | `ChamadoRepository.cs:224` (`ListarAtendentesQuePodemVerAsync` reaproveita `PodeVerAsync`/`AplicarVisibilidade`); `SlaAlertaNotificador.cs` envia por `Clients.Users`; teste `Notificar_EnviaParaAdminsEParaOsAtendentesQueVeem` (com o repositório mockado, ver R-02) |
| AC-02 | Sim | Mesmo método; `SlaMonitorService` deixou de usar o grupo `Atendimento`; teste `Notificar_NuncaUsaOGrupoDeAtendimentoInteiro` |
| AC-03 | Sim | `ChamadosHub.cs:15,28` (grupo `Admins` pelo claim `perfil`); `SlaAlertaNotificador` sempre envia para `Admins`; teste `OnConnectedAsync_SoAdminEntraNoGrupoAdmins` |
| AC-04 | Sim | O Solicitante não entra em `Admins` e a consulta filtra `Perfil == Atendente` (`ChamadoRepository.cs:228`); teste do hub com Solicitante |
| AC-05 | Sim no código, **sem teste automatizado** | `SlaMonitorService.cs:53-61` (`_notificados` inalterado). Ver **R-01** |
| AC-06 | Sim | `AdicionarAnexoCommandHandler.cs:27-28`, antes do upload; teste `Handle_ComComentarioQueNaoPertenceAoChamado_DeveRecusarSemFazerUpload` (verifica que `UploadAsync` não é chamado) |
| AC-07 | Sim | `ChamadoRepository.cs:244` (`AnyAsync` com Id **e** ChamadoId: comentário inexistente dá `false`). O teste só cobre o handler com o mock devolvendo `false` |
| AC-08 | Sim | Teste `Handle_ComComentarioId_DeveVincularAnexoAoComentario` (mock ajustado) e `Handle_SemComentario_NaoConsultaComentario` |
| AC-09 | Sim | `VersaoChamadoBehaviour.cs`, `ChamadoPermissoes.AlteraChamado` (11 ações); os 11 commands declaram a `AcaoChamado` correta (conferido em `Features/Chamados/Commands/*Command.cs`); registrado depois do `AcessoChamadoBehaviour` (`Program.cs:56`); testes `VersaoDiferente_Recusa409ComAMensagemDoAC10` e `AlteraChamado_SoAsAcoesQueAlteram` |
| AC-10 | Sim | Servidor: `VersaoChamado.MensagemConflito` no behaviour e no `ChamadoRepository.cs:45`. Tela: `recarregarSeConflito` em todos os hooks + `TipoChamadoCampo`; a mensagem aparece no bloco de erro já existente. E2E `conflito.spec.ts` (detalhe) |
| AC-11 | Adiado | Decisão do usuário registrada na spec e no design. Fora do escopo desta rodada |
| AC-12 | Parcial | Versão em microssegundos (`VersaoChamado.cs`, teste `De_IgnoraFracaoAbaixoDeMicrossegundo`) e botões do `BotoesAcao` travados durante `isFetching` (`ChamadoDetailPage.tsx:54,72`). Não há trava no Kanban, no `TipoChamadoCampo` nem nos botões que abrem os modais. Ver **R-03** e **R-05** |
| AC-13 | Sim | `AlteraChamado` dá `false` para comentar/anexar; comentário e anexo não mexem em `Chamado.DataAtualizacao` (conferido em `ChamadoRepository.AdicionarComentarioAsync`/`AdicionarAnexoAsync` e em `Chamado.cs`); teste `Comentario_NuncaDaConflito` |
| AC-14 | Sim | `KanbanBoard.tsx:60-69` (envia `chamado.versao`, faixa de erro, `invalidateQueries` no `finally`); E2E `kanban: mover cartão desatualizado…` (só confere o status pela API, ver R-05) |
| AC-15 | Sim | `frontend/e2e/*`: nenhuma ocorrência de "categoria"; `abrirChamadoPelaTela` preenche Área e Tipo (`helpers.ts`) |
| AC-16 | Sim (limpeza pendente de OK) | `PREFIXO = '[TESTE-E2E]'` em todos os títulos criados (`chamados`, `fluxo-completo`, `conflito`) |
| AC-17 | Sim | `ChamadosCamarj.UnitTests.csproj` com `Microsoft.EntityFrameworkCore.Relational 9.*`; `dotnet test` sem MSB3277 |
| AC-18 | Sim | `favicon.png` com 3.569 bytes, 64×64, mesmo nome (`index.html` intacto); conferido visualmente |
| AC-19 | Sim | `STATE.md`, regra 4 (cita `/sdd`, 2 aprovações e o OpenCode); `docs/GUIA-ORQUESTRACAO-SDD.md`, nova seção do Claude Code no topo, com a do OpenCode mantida |
| AC-20 | Sim | `BadRequestException`/`ConflictException` com mensagens em português, no formato já tratado pelo middleware |
| AC-21 | **Parcial** | `dotnet test` passa (438). Mas o AC-05 não tem teste automatizado (**R-01**) e a regra de quem recebe o alerta só é testada com mock (R-02) |
| AC-22 | Não verificado nesta revisão | Não rodei `npm run build` por falta de memória. A spec registra o build como ok. Não achei objeto literal tipado como `ChamadoResponse` que quebrasse com o campo novo |
| AC-23 | Sim | O behaviour segue sem consultar quando não há `If-Match` (teste `SemVersaoInformada_SegueSemConsultar`); `cabecalhoVersao` não manda o cabeçalho sem versão |

## Achados

### 🔴 Bloqueante R-01 — AC-05 sem teste automatizado (descumpre o AC-21)
- **Onde:** `src/ChamadosCamarj.WebApi/Services/SlaMonitorService.cs:53-61`; spec §5 (rastreabilidade do AC-05: "Leitura de código")
- **Problema:** o AC-21 exige teste automatizado para **cada** AC de US-01 a US-03. O AC-05 (não repetir o
  alerta quando o chamado continua na mesma situação) não tem nenhum teste: a rastreabilidade marca ✅
  com base só em leitura de código. A lógica de deduplicação continua presa num método `private` do
  `BackgroundService`, que acessa o `ApplicationDbContext` direto, e nenhum teste chega até ela. Este é
  o mesmo tipo de buraco ("envio sem teste") que o design diz ter fechado para o R-04 anterior, só que
  na outra metade do monitor.
- **Cenário de falha:** alguém troca a condição da linha 53 por `status == SlaStatus.Atencao`, ou
  remove a atribuição `_notificados[c.Id] = ...`. A suíte continua verde (438/438). Em produção, todo
  Admin e todo Atendente que vê o chamado passa a receber o mesmo alerta de "próximo do prazo" a cada
  5 minutos, até o chamado ser fechado.
- **Correção sugerida:** extrair a decisão para uma unidade pura e testável, por exemplo
  `SlaAlertaControle.DeveNotificar(Guid id, SlaStatus status)`, que guarda o dicionário e devolve o
  evento a disparar ou `null`, e cobrir com testes: 1ª vez em Atenção dispara; 2ª vez em Atenção não
  dispara; Atenção→Atrasado dispara; Atrasado repetido não dispara. Depois, trocar a linha do AC-05 na
  rastreabilidade pelo nome do teste.

### 🟡 Atenção R-02 — A regra de quem recebe o alerta (Atendente ativo + visibilidade) não tem teste automatizado
- **Onde:** `src/ChamadosCamarj.Infrastructure/Repositories/ChamadoRepository.cs:224-241`
- **Problema:** a parte do AC-01/AC-02 que decide quem recebe vive inteira em
  `ListarAtendentesQuePodemVerAsync`: o filtro `Perfil == Atendente && Ativo` (mitigação do design §10
  para "Atendente inativo recebendo alerta") e a chamada a `PodeVerAsync`. Os testes do notificador
  mockam esse método e só provam que o envio vai para "os ids que o repositório devolveu". O projeto
  não tem testes de repositório, então a evidência é só a verificação ao vivo da T13.
- **Cenário de falha:** alguém remove `&& u.Ativo` ou troca `PodeVerAsync` por `true` numa refatoração.
  A suíte continua verde, e um Atendente desligado, ou um de outra equipe, volta a receber o número e a
  situação de prazo de chamados que não pode ver. É o vazamento que a US-01 fecha.
- **Correção sugerida:** um teste com `UseInMemoryDatabase` (ou SQLite) para esse método, com 3
  Atendentes (vê; não vê; inativo) e 1 Solicitante. Se não houver tempo, registrar a dívida no
  STATE.md, e não marcar AC-01/AC-02 como cobertos por teste automatizado.

### 🟡 Atenção R-03 — 409 falso (AC-12) continua possível fora dos botões do `BotoesAcao`
- **Onde:** `frontend/src/features/chamados/kanban/KanbanBoard.tsx:56-69`;
  `frontend/src/features/chamados/components/TipoChamadoCampo.tsx:20,39`;
  `frontend/src/features/chamados/ChamadoDetailPage.tsx:150,156,162`
- **Problema:** a trava por `isFetching` (design §4.3) só existe nos botões Assumir, Resolver,
  Encerrar, Cancelar e Reabrir. Em três outros pontos a versão enviada pode ser a anterior à própria
  ação da pessoa:
  - **Kanban:** a atualização otimista copia o cartão com `{ ...c, status }` e mantém a `versao`
    antiga até a recarga terminar;
  - **`TipoChamadoCampo`:** o select só fica desabilitado durante `isPending`, não durante a recarga;
  - os botões **Reatribuir / Alterar prioridade / Forçar encerramento** não usam `isPending` nem
    `recarregando`.
- **Cenário de falha:** no Kanban, A arrasta CAM-10 de Aberto para Em Andamento e, antes de a lista
  recarregar (centenas de ms), arrasta de Em Andamento para Resolvido. A segunda chamada leva a versão
  de antes da primeira e recebe 409 "Outra pessoa alterou este chamado", sem que ninguém mais tenha
  mexido. O mesmo acontece ao trocar o Tipo duas vezes seguidas, ou quando o Admin altera a prioridade
  e abre e confirma "Reatribuir" na sequência. Isso contradiz o AC-12 ("várias ações seguidas de A sem
  recarregar a página").
- **Correção sugerida:** no Kanban, bloquear o arraste (ou ignorar o `dragEnd`) enquanto
  `useIsFetching({ queryKey: ['chamados','kanban'] }) > 0`; no `TipoChamadoCampo`, usar
  `disabled={isPending || recarregando}`; nos três botões de modal, usar `disabled={isPending}`, já
  que o `isPending` inclui `recarregando`.

### 🟡 Atenção R-04 — O alerta usa perfil e grupo do banco, e a lista usa os do token: a "mesma regra" diverge enquanto o token não é renovado
- **Onde:** `ChamadoRepository.cs:224-241` (lê `UsuariosPerfil`) × `CurrentUserService` (lê
  `perfil`/`grupo_id` do JWT); `ChamadosHub.EhAdmin` (lê o token)
- **Problema:** o AC-01 diz "pela mesma regra da lista de chamados". A regra é a mesma, mas as
  entradas não: a lista e o detalhe montam o `ContextoAcesso` a partir do token, e o alerta monta a
  partir do banco. Enquanto o token antigo for válido (`TokenExpiracaoHoras`), os dois resultados
  divergem.
- **Cenário de falha:** o Admin move o Atendente João do grupo TI para o RH. João continua logado com o
  token do TI e recebe o alerta "CAM-500 — PRAZO ESTOURADO!" de um chamado do RH atribuído a outra
  pessoa. Ao clicar, recebe 404, porque a API usa o token do TI. No sentido inverso, ele deixa de ser
  avisado de chamados do TI que ainda aparecem na lista dele. Outro caso: um Solicitante promovido a
  Atendente no banco, ainda com o token de Solicitante, passa a receber alertas de todos os chamados
  sem responsável, que a lista dele não mostra.
- **Correção sugerida:** aceitar e documentar a divergência na spec e na nota do Obsidian (ela dura no
  máximo o tempo do token e segue o banco, que é a fonte "mais nova"). Se não for aceitável, invalidar
  ou renovar o token quando perfil, grupo ou status mudar. Pedir a decisão ao usuário (regra 1).

### 🟡 Atenção R-05 — Os E2E de tela não testam o que o AC-12 e o AC-14 descrevem
- **Onde:** `frontend/e2e/conflito.spec.ts`
- **Problema:**
  - **AC-12:** o teste só faz uma ação depois de um 409. O AC pede "várias ações seguidas de A sem
    recarregar" (ex.: Assumir → Resolver → Encerrar), que é justamente o caso do 409 falso por versão
    velha.
  - **AC-14:** o teste do Kanban confere o status pela API (`atual.status === 'Aberto'`), o que já
    vale mesmo que a tela deixe o cartão na coluna errada. Ele não verifica que o cartão voltou para
    a coluna Aberto na tela.
- **Cenário de falha:** alguém remove `recarregando` do `isPending` (`ChamadoDetailPage.tsx:72`) ou o
  `invalidateQueries` do `finally` do Kanban. Os dois E2E continuam passando: o primeiro porque não
  encadeia ações; o segundo porque só consulta a API.
- **Correção sugerida:** no teste do detalhe, depois do Assumir, clicar Resolver → Confirmar →
  Encerrar e esperar o status final, sem 409. No teste do Kanban, conferir que o título está dentro
  da coluna "Aberto" (localizador escopado pela coluna).

### 🟡 Atenção R-06 — A senha da conta Admin real continua no histórico do git
- **Onde:** histórico de `frontend/e2e/*.spec.ts` em `origin/develop` (removida neste diff, substituída
  por `E2E_EMAIL`/`E2E_SENHA`)
- **Problema:** a mudança para variáveis de ambiente está certa, mas a senha da conta Admin de
  produção (`suporte@…`, o banco de dev é o de produção) continua em commits já publicados. O diff não
  piora a situação, mas também não a resolve.
- **Cenário de falha:** qualquer pessoa com acesso de leitura ao repositório (ou a um clone ou fork
  antigo) roda `git log -p -- frontend/e2e` e entra como Admin em produção.
- **Correção sugerida:** trocar a senha dessa conta antes do deploy (ação do usuário) e registrar no
  STATE.md. Reescrever o histórico não é necessário se a senha for trocada.

## Constitution e contratos

- **Regra 1 (pergunta sem resposta não vira suposição):** cumpre. O AC-11 e os contratos C1–C7 foram
  decididos com o usuário (design §11). O R-04 é uma decisão de produto implícita que vale levar ao
  usuário.
- **Regra 2 (spec antes do código):** cumpre. O commit `fa09eb3` (spec/design/tasks) vem antes de todo
  o código.
- **Regra 3 (contrato avisado antes):** cumpre. C1–C7 estão listados e aprovados no design, e o código
  bate com eles: `Versao` no fim do record, antes dos opcionais; `If-Match` opcional; mensagem do 409
  unificada; 3 métodos novos em `IChamadoRepository`; grupo `Admins` mais `Clients.Users`; behaviour
  global que só age em `IRequerAcessoChamado` de alteração; 400 no anexo. Nenhuma mudança de contrato
  fora do previsto.
- **Regra 4 (orquestração SDD):** cumpre. Esta revisão é o review por sub-agente novo.
- **Regra 5 (não quebrar fora do escopo):** cumpre no que foi possível verificar.
  - `SignalRNotificationHandlers` continua usando `Atendimento`.
  - Os grupos `Todos` e `Atendimento` estão intactos (testes do hub).
  - `AplicarVisibilidade` não foi alterado.
  - O behaviour deixa passar requests que não são de chamado e as ações de Ver/Comentar/Anexar
    (incluindo `RemoverAnexoCommand`, que usa `Anexar`).
  - Não existem envios MediatR aninhados nos handlers de chamado que pudessem herdar o `If-Match`.
  - O `ChamadoResponse` ganhou um parâmetro posicional, e não há outra construção dele além de
    `ChamadoMappings`.
- **Regra 6 (Obsidian):** planejada para o close (gate ainda desmarcado na spec). Fica pendente.
- **Convenções:** sem toast library (faixa com `Alert` no Kanban), `isPending`/`useIsFetching`,
  mapeamento manual, repositório em vez de `DbContext` no código novo. O `SlaMonitorService` continua
  usando `ApplicationDbContext` direto, o que já existia antes desta feature.
- **Pontos cross-feature:** `ChamadosHub`, `Program.cs` (pipeline) e `ChamadoMappings` têm cobertura
  por teste. A tela de detalhe, os modais e o Kanban têm só E2E, com os limites do R-05.

## Sugestões (não bloqueiam)

- `VersaoLidaAccessor.Normalizar` trata `If-Match: *` e listas (`"a", "b"`) como uma versão literal, o
  que dá 409. Nenhum cliente nosso manda isso hoje. Se um dia a API for usada por terceiros, tratar
  `*` como "sem checagem", conforme a RFC 9110.
- `ListarAtendentesQuePodemVerAsync` faz uma consulta por Atendente. Com o `_notificados` vazio a cada
  reinício, a primeira volta do monitor faz (chamados atrasados × Atendentes) consultas. Hoje é
  aceitável, mas dá para fazer numa consulta só, aplicando a regra por Atendente com `Any`.
- `ComentarioPertenceAoChamadoAsync` não olha o tipo do comentário: um Solicitante pode ligar um anexo
  a um comentário **interno** do próprio chamado (o id não é exposto a ele pela tela, mas pode ser
  adivinhado ou vazado). Vale conferir como `ListarAnexos` trata anexo de comentário interno para
  Solicitante.
- No `BotoesAcao`, o erro de uma ação anterior (ex.: o 409 do Assumir) continua na tela depois de outra
  ação bem-sucedida, porque cada mutation guarda o próprio `error`. Esse comportamento já existia, mas
  agora o 409 o torna mais visível. Dá para chamar `reset()` das outras mutations ao disparar uma nova.
- A faixa de erro do Kanban não tem botão de fechar e só some no próximo arraste.
- Na rastreabilidade da spec, a linha "AC-20..AC-22: 438 testes, builds ok" pode separar o AC-22
  (`npm run build`), para deixar claro que ele foi rodado na sessão de implementação.

---

## Tratamento dos achados (sessão principal, 2026-10-02)

| Achado | Confirmado? | Tratamento |
|---|---|---|
| R-01 🔴 | Sim | **Corrigido:** regra de não repetir alerta extraída para `SlaAlertasEnviados` (WebApi/Services) com 5 testes (`SlaAlertasEnviadosTests`); `SlaMonitorService` passa a usá-la. |
| R-02 🟡 | Sim | **Pendência:** o filtro `Ativo` e a regra de visibilidade de `ListarAtendentesQuePodemVerAsync` só são cobertos por mock + verificação ao vivo (69/69). O projeto não tem testes de repositório contra banco. |
| R-03 🟡 | Sim | **Corrigido:** Kanban ignora soltar cartão enquanto o quadro recarrega; campo de Tipo e botões que abrem os modais (Reatribuir, Prioridade, Forçar encerramento) ficam desabilitados enquanto o chamado recarrega. |
| R-04 🟡 | Sim | **Pendência (decisão de negócio):** alerta usa perfil/grupo do banco; telas usam os do token até o próximo login. Levado ao usuário no fechamento. |
| R-05 🟡 | Sim | **Pendência:** E2E de conflito não encadeia várias ações nem confere a coluna do cartão na tela; AC-12 coberto por teste unitário (precisão da versão) + verificação ao vivo via API. |
| R-06 🟡 | Sim | **Ação do usuário:** a senha antiga do Admin segue no histórico do git (já não funciona: 401 em 2026-10-02). A senha atual foi informada na conversa desta sessão → recomendar a troca. |
