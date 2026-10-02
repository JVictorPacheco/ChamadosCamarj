# Review: correcoes-pre-deploy — rodada 2

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-02
> **Escopo:** `git diff origin/develop...feature/correcoes-pre-deploy`: 46 arquivos, +1646/-142 linhas, 10 commits (`fa09eb3`..`aa9181d`)
> **Veredito:** APROVADO COM RESSALVAS (0 bloqueantes, 2 de atenção)

Verificação feita: leitura de spec, design, tasks, do review da rodada 1 com o tratamento, do diff
completo e do código ao redor: `Chamado.cs` (todos os métodos que alteram o chamado gravam
`DataAtualizacao`), `ChamadoConfiguration` (token de concorrência), `AplicarVisibilidade`/`PodeVerAsync`,
`ComentarChamadoCommandHandler`, os 14 commands `IRequerAcessoChamado`, `Program.cs` (ordem dos
behaviours), `SubClaimUserIdProvider`/`JwtTokenService` (`sub` = `UsuarioPerfil.Id`), `apiFetch`
(mescla os cabeçalhos sem perder o `Authorization`), `useChamado`, `useKanbanChamados`,
`FilaAtendimentoPage`, `useSignalR`, `AppLayout`, `AlterarPrioridadeModal`, e todos os chamadores das
10 funções de ação no frontend (todos passam a versão).
`dotnet test tests/ChamadosCamarj.UnitTests/` rodou aqui: **443 aprovados, 0 falhas**, sem `MSB3277`.
`favicon.png` conferido: 3.569 bytes, 64×64, mesmo nome no `index.html`. Não rodei backend, frontend,
`npm run build` nem E2E, por causa da restrição de memória.

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim | `ChamadoRepository.cs:224-241` (`ListarAtendentesQuePodemVerAsync` → `PodeVerAsync` → `AplicarVisibilidade`, sem duplicar a regra); `SlaAlertaNotificador.cs:35-44`; teste `Notificar_EnviaParaAdminsEParaOsAtendentesQueVeem` (com repositório mockado; pendência R-02 da rodada 1) |
| AC-02 | Sim | Mesmo método; o monitor não usa mais o grupo `Atendimento`; teste `Notificar_NuncaUsaOGrupoDeAtendimentoInteiro` |
| AC-03 | Sim | `ChamadosHub.cs:27-28,40-41` (grupo `Admins` pelo claim `perfil`); envio sempre para `Admins`; teste `OnConnectedAsync_SoAdminEntraNoGrupoAdmins` |
| AC-04 | Sim | Solicitante fora de `Admins` (teste do hub) e fora da consulta (`Perfil == Atendente`) |
| AC-05 | Sim (correção R-01 conferida) | `SlaAlertasEnviados.cs` usado em `SlaMonitorService.cs:49`; 5 testes em `SlaAlertasEnviadosTests`. A lógica é equivalente à anterior (inclusive Atenção → DentroPrazo → Atenção não repete). Ver R-02 desta rodada |
| AC-06 | Sim | `AdicionarAnexoCommandHandler.cs:26-29`, antes do upload; teste `Handle_ComComentarioQueNaoPertenceAoChamado_DeveRecusarSemFazerUpload` (verifica `UploadAsync` e `AdicionarAnexoAsync` nunca chamados) |
| AC-07 | Sim | `ChamadoRepository.cs:243-247` (`AnyAsync` com Id **e** ChamadoId). O teste é do handler com o mock devolvendo `false`; a consulta em si não tem teste (mesma limitação aceita no R-02 da rodada 1) |
| AC-08 | Sim | Testes `Handle_ComComentarioId_DeveVincularAnexoAoComentario` e `Handle_SemComentario_NaoConsultaComentario` |
| AC-09 | Sim | `VersaoChamadoBehaviour.cs`; `ChamadoPermissoes.AlteraChamado` (11 ações); os 11 commands declaram a ação certa; registrado depois do `AcessoChamadoBehaviour` (`Program.cs:52-56`); todos os métodos de `Chamado` que alteram estado gravam `DataAtualizacao`; não há `ExecuteUpdate` fora do seeder. Testes `VersaoDiferente_Recusa409ComAMensagemDoAC10`, `AlteraChamado_SoAsAcoesQueAlteram`. Ver R-01 desta rodada |
| AC-10 | Sim | Servidor: `VersaoChamado.MensagemConflito` no behaviour e em `ChamadoRepository.cs:45`. Tela: `recarregarSeConflito` em todos os hooks e no `TipoChamadoCampo`; mensagem no bloco de erro do detalhe, dos modais e da Fila. E2E `conflito.spec.ts` (detalhe) |
| AC-11 | Adiado | Decisão do usuário registrada na spec e no design |
| AC-12 | Sim (correção R-03 conferida) | Versão em microssegundos (`VersaoChamado.cs`, teste `De_IgnoraFracaoAbaixoDeMicrossegundo`); trava por `isFetching` em `BotoesAcao` (`ChamadoDetailPage.tsx:54,72`), nos botões dos 3 modais (`:150,156,162`), no `TipoChamadoCampo.tsx:18,41` e no Kanban (`KanbanBoard.tsx:29,51`). Teste de tela com várias ações seguidas segue como pendência aceita (R-05 da rodada 1) |
| AC-13 | Sim | `AlteraChamado` dá `false` para comentar/anexar; `AdicionarComentarioAsync`/`AdicionarAnexoAsync` não tocam no `Chamado`; teste `Comentario_NuncaDaConflito` |
| AC-14 | Sim | `KanbanBoard.tsx:57-74` (envia `chamado.versao`, faixa de erro, `invalidateQueries` no `finally`); E2E `kanban: mover cartão desatualizado…` (só confere o status pela API; pendência R-05 da rodada 1) |
| AC-15 | Sim | Nenhuma ocorrência de "categoria" em `frontend/e2e`; `abrirChamadoPelaTela` preenche Área e Tipo (`helpers.ts`); `admin.spec.ts` usa `/admin/tipos` |
| AC-16 | Sim (limpeza pendente de OK) | `PREFIXO = '[TESTE-E2E]'` em todo chamado criado (`chamados`, `fluxo-completo`, `conflito`) |
| AC-17 | Sim | `ChamadosCamarj.UnitTests.csproj` com `Microsoft.EntityFrameworkCore.Relational 9.*`; `dotnet test` aqui sem `MSB3277` |
| AC-18 | Sim | `favicon.png`: 3.569 bytes, 64×64, mesmo nome |
| AC-19 | Sim | `STATE.md` regra 4 (cita `/sdd`, as fases, as 2 aprovações e o OpenCode); `docs/GUIA-ORQUESTRACAO-SDD.md` com a seção do Claude Code no topo e a do OpenCode mantida |
| AC-20 | Sim | `BadRequestException`/`ConflictException` em português, tratadas pelo `ExceptionHandlingMiddleware` |
| AC-21 | Sim, com as pendências aceitas | 443 testes passando; AC-05 agora tem teste. Limites já aceitos: R-02 e R-05 da rodada 1 |
| AC-22 | Não verificado nesta revisão | Não rodei `npm run build` por falta de memória. Não há objeto literal tipado como `ChamadoResponse` fora de `types/api.ts` que quebre com o campo novo |
| AC-23 | Sim | Behaviour segue sem consultar quando não há `If-Match` (teste `SemVersaoInformada_SegueSemConsultar`); `cabecalhoVersao` não manda o cabeçalho sem versão |

## Achados

### 🟡 Atenção R-01 — A versão enviada acompanha a recarga em segundo plano, mesmo com modal ou diálogo aberto: a perda silenciosa ainda acontece
- **Onde:** `frontend/src/features/chamados/ChamadoDetailPage.tsx:47-51` e `:245-262` (a versão passada aos hooks e modais é sempre a do último `refetch`);
  `frontend/src/features/chamados/components/AlterarPrioridadeModal.tsx:32-33`;
  `frontend/src/features/chamados/hooks/useChamado.ts` (`refetchOnWindowFocus` padrão do TanStack, `staleTime` 0)
- **Problema:** o design (§4.3) aceita que o `refetchOnWindowFocus` atualize a versão, porque "os
  dados na tela" passam a ser os novos. Isso vale para a página, mas não quando a pessoa está dentro de
  um modal ou diálogo de confirmação: o refetch troca a versão por baixo do overlay, e a escolha que ela
  fez no modal continua a mesma. A ação seguinte sai com a versão nova e passa sem 409, sem que a pessoa
  tenha visto a alteração do outro.
- **Cenário de falha:** o Admin A abre "Alterar prioridade" no CAM-300 (Média) e marca "Alta". Antes
  de salvar, troca de aba para consultar algo. Nesse meio tempo o Admin B muda o CAM-300 para
  "Urgente". A volta à aba: o foco dispara o refetch, `chamado.versao` passa a ser a versão de B e o
  modal continua mostrando "Alta" marcado. A clica em Salvar, o servidor aceita (a versão confere) e a
  prioridade "Urgente" de B é apagada sem aviso. É o caso que o AC-09 quer evitar. O mesmo vale para o
  diálogo de Encerrar/Cancelar com observação e para Reatribuir.
- **Correção sugerida:** congelar a versão no momento em que o modal ou o diálogo abre (por exemplo, `useState`/`useRef` preenchido no `onClick` que abre o modal, passado como `versao` ao hook) em vez de usar a prop viva. Outra opção: desligar o refetch do chamado enquanto um modal estiver aberto. Se o comportamento atual for aceito, registrar a decisão com o usuário (regra 1), porque o design só tratou dos dados visíveis na página.

### 🟡 Atenção R-02 — Se o envio aos Atendentes falhar, o alerta se perde de vez para eles
- **Onde:** `src/ChamadosCamarj.WebApi/Services/SlaAlertasEnviados.cs:21-22` (marca como avisado antes do envio);
  `src/ChamadosCamarj.WebApi/Services/SlaMonitorService.cs:49-58` (um único `try/catch` em volta do laço inteiro);
  `src/ChamadosCamarj.WebApi/Services/SlaAlertaNotificador.cs:39-41` (envia aos Admins e só depois consulta o banco)
- **Problema:** `EventoANotificar` grava a situação no dicionário antes de `NotificarAsync` rodar. O
  código anterior tinha a mesma ordem, mas o envio era um único `SendAsync` para um grupo. Agora o envio
  depende de `ListarAtendentesQuePodemVerAsync`, que faz 1 + N consultas ao banco, depois de os Admins
  já terem recebido o alerta. Uma falha nesse ponto marca o chamado como avisado sem que os Atendentes
  recebam nada. Além disso, a exceção interrompe o laço, e os outros chamados daquela volta só são
  verificados 5 minutos depois.
- **Cenário de falha:** o CAM-410 passa de Atenção para Atrasado. Na volta do monitor, os Admins recebem
  "PRAZO ESTOURADO!" e, em seguida, a consulta dos Atendentes falha (timeout ou queda momentânea da
  conexão com o Postgres). O `catch` só registra no log. Na volta seguinte, `EventoANotificar` devolve
  `null` porque o CAM-410 já está marcado como Atrasado, e o Atendente responsável nunca recebe o alerta
  de prazo estourado do chamado dele (AC-01).
- **Correção sugerida:** separar a decisão da marcação, por exemplo `EventoANotificar` sem efeito e um
  `Registrar(id, status)` chamado depois de `NotificarAsync` concluir. Outra opção: fazer um `try/catch`
  por chamado que desfaz a marcação. Cobrir com um teste em `SlaAlertasEnviadosTests` ("sem registro, a
  próxima verificação avisa de novo").

## Correções da rodada 1, conferidas

| Achado | Declarado | Conferido no código |
|---|---|---|
| R-01 🔴 (AC-05 sem teste) | Extraído para `SlaAlertasEnviados` + 5 testes | **Sim.** A lógica é equivalente à original e os testes falham se a condição de repetição ou a gravação for removida. O ponto novo é o R-02 acima (ordem marcar → enviar), que existia antes mas ficou mais provável com a consulta ao banco |
| R-03 🟡 (409 falso fora do `BotoesAcao`) | Kanban ignora o soltar durante a recarga; Tipo e botões dos modais travados | **Sim.** `KanbanBoard.tsx:29,51` (retorna antes da atualização otimista), `TipoChamadoCampo.tsx:41`, `ChamadoDetailPage.tsx:150,156,162`. Não introduziu regressão; ver as sugestões sobre o soltar ignorado sem aviso |
| R-02, R-04, R-05, R-06 | Pendências aceitas / ação do usuário | Sem mudança de avaliação. Continuam valendo para o close: registrar no STATE.md e levar o R-04 e o R-06 ao usuário |

## Constitution e contratos

- **Regra 1 (pergunta sem resposta não vira suposição):** cumpre. O R-01 desta rodada pede uma decisão
  explícita, se o comportamento atual for mantido.
- **Regra 2 (spec antes do código):** cumpre. O `fa09eb3` (spec/design/tasks) vem antes do código.
- **Regra 3 (contrato avisado antes):** cumpre. C1–C7 batem com o código, e a rodada de correções não
  criou contrato novo: `SlaAlertasEnviados` é interno ao WebApi e o `TipoChamadoCampo` reaproveita o
  `recarregarSeConflito` exportado.
- **Regra 4 (orquestração SDD):** cumpre. Esta é a 2ª rodada de review por sub-agente novo, e a regra
  foi atualizada pelo AC-19.
- **Regra 5 (não quebrar fora do escopo):** cumpre no que foi possível verificar por leitura.
  - `SignalRNotificationHandlers` continua com `Atendimento`.
  - Os grupos `Todos` e `Atendimento` estão intactos.
  - `AplicarVisibilidade` não foi alterado.
  - O behaviour só age em `IRequerAcessoChamado` de alteração, e não há `Send` do MediatR aninhado nos handlers de chamado (só nos de Chat, que não são de chamado).
  - O `apiFetch` preserva o `Authorization` ao receber `headers`.
- **Regra 6 (Obsidian):** ainda pendente. O gate está desmarcado na spec e é feito no close.
- **Convenções:** sem biblioteca de toast, `isPending`/`useIsFetching`, mapeamento manual, repositório no
  código novo. O `SlaMonitorService` ainda acessa o `ApplicationDbContext` direto, como antes desta
  feature.
- **Pontos cross-feature:** `ChamadosHub`, pipeline do MediatR e `ChamadoMappings` têm teste. A tela de
  detalhe, os modais, a Fila e o Kanban têm só E2E e verificação ao vivo, com os limites do R-05 da
  rodada 1.

## Sugestões (não bloqueiam)

- **Kanban:** o soltar ignorado durante a recarga não dá nenhum aviso; o cartão só volta ao lugar. Como o
  quadro recarrega a cada `StatusAlterado`/`ChamadoCriado` de qualquer pessoa (via SignalR), em horário de
  movimento isso pode parecer um arraste que "não pegou". Uma dica curta ("Atualizando o quadro, tente de
  novo") resolve.
- **Kanban:** ainda sobra uma janela de 409 falso enquanto a própria requisição está em voo. Se a pessoa
  arrastar o mesmo cartão de novo antes da resposta, o `recarregando` ainda é `false` e o cartão otimista
  mantém a versão antiga. A janela é de centenas de ms. Dá para fechar travando o arraste enquanto houver
  movimento pendente (por exemplo, um `useState` com o id em voo).
- **Spec §5:** a rastreabilidade cita "438 testes", e a suíte agora tem 443. Atualizar no close.
- **Sugestões da rodada 1 ainda abertas:** `If-Match: *`, a consulta por Atendente no alerta, o anexo
  ligado a comentário interno, o erro antigo que fica na tela no `BotoesAcao` e a faixa do Kanban sem
  botão de fechar. Nenhuma mudou de avaliação.

---

## Tratamento dos achados (sessão principal, 2026-10-02)

| Achado | Confirmado? | Tratamento |
|---|---|---|
| R-01 🟡 | Sim | **Corrigido:** `ChamadoDetailPage` guarda a versão no momento em que abre o diálogo (resolver/encerrar/cancelar/reabrir) ou um modal (reatribuir/prioridade/forçar encerramento) e passa essa versão às ações; recarga em segundo plano não troca mais a versão por baixo. Após um 409 dentro do modal, a pessoa fecha, confere e reabre (versão nova capturada). |
| R-02 🟡 | Sim | **Corrigido:** `SlaAlertasEnviados.EventoANotificar` não registra mais; o monitor chama `RegistrarEnvio` só depois do envio. Teste `EnvioQueFalhou_TentaDeNovoNaProximaVerificacao`. Efeito aceito: se o envio falhar depois de avisar os Admins, eles podem receber o mesmo alerta de novo na próxima verificação. |

Decisão do usuário (2026-10-02) sobre o R-04 da rodada 1: **aceito como está** (alerta segue o grupo do banco; telas seguem o token até o próximo login).
