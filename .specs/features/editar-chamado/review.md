# Review: editar-chamado — rodada 1

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-02
> **Escopo:** `git diff origin/develop...feature/editar-chamado`: 26 arquivos, +1367/-61 linhas (19 de código e testes, o resto em `.specs/`)
> **Veredito:** APROVADO COM RESSALVAS

Verificação feita só pela leitura do código. Backend e frontend não foram executados. `dotnet test tests/ChamadosCamarj.UnitTests/` rodado nesta revisão: **474/474 aprovados**.

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim | `ChamadoPermissoes.cs:52-60` (`PodeEditar`, ramo "quem abriu e `ResponsavelId is null`"); `lib/permissoes.ts:15`; testes `Editar_QuemAbriu_PodeEnquantoNinguemAssumiu`, `Editar_QuemAbriuSemResponsavel_ChamaOHandler`. Na tela, só verificação ao vivo (ver R-04) |
| AC-02 | Sim | `PodeEditar` (com responsável, só o responsável); `AcessoChamadoBehaviour.cs:52-53` (403 com a mensagem da spec); teste `Editar_QuemAbriu_NaoPodeDepoisQueAlguemAssumiu` |
| AC-03 | Sim | `PodeEditar` vale para qualquer perfil; `[InlineData(Perfil.Atendente)]` em `Editar_QuemAbriu_PodeEnquantoNinguemAssumiu`. A visibilidade já inclui quem abriu (`ChamadoRepository.cs:200,210`) |
| AC-04 | Sim | `AbriuOChamado` compara o e-mail; teste `Editar_SolicitanteColegaDeGrupo_NaoPode`; tela em `permissoes.ts:15` |
| AC-05 | Sim | `PodeEditar` (`ResponsavelId == acesso.UsuarioId`); testes `Editar_ResponsavelAtual_Pode`, `Editar_ResponsavelAtual_ChamaOHandler` |
| AC-06 | Sim | Testes `Editar_OutroAtendenteQueVe_NaoPode(true)`, `Editar_AtendenteQueNaoEhResponsavel_DaForbidden` |
| AC-07 | Sim | Segundo assert de `Editar_ResponsavelAtual_Pode` (quem abriu e é responsável) |
| AC-08 | Sim | Regra sem estado: o responsável atual pode e o anterior passa a ser "outro Atendente". Coberto por `Editar_ResponsavelAtual_Pode` + `Editar_OutroAtendenteQueVe_NaoPode`. Não existe teste com a sequência de reatribuição |
| AC-09 | Sim | `PodeEditar` (Admin); teste `Editar_Admin_PodeQualquerChamadoNaoEncerrado` (com e sem responsável); a visibilidade do Admin é total (`ChamadoRepository.cs:188`) |
| AC-10 | Sim | `AcessoChamadoBehaviour.cs:48-49` (400 com `MensagemEdicaoEncerrado`, antes do 403); `Chamado.cs:251-252` (última defesa); `permissoes.ts:12`; testes `Editar_ChamadoEncerrado_DaBadRequestComAMensagem_InclusiveParaAdmin`, `Editar_Encerrado_NinguemPode_*`, `AtualizarDados_EmChamadoEncerrado_*`; E2E `chamado encerrado não mostra o botão Editar` |
| AC-11 | Sim | Na reabertura, `Chamado.Reabrir` zera o responsável (`Chamado.cs:151-153`). Teste `Editar_Reaberto_QuemAbriuVoltaAPoder`. Ver a sugestão sobre o status |
| AC-12 | Sim | `EditarChamadoModal.tsx:26-29` (só existe enquanto está aberto e começa com o texto e a versão atuais); E2E `Admin edita...` (`toHaveValue(titulo)`) |
| AC-13 | Sim | `EditarChamadoModal.tsx` (erros no campo e "Salvar" desabilitado, limites 200/5000 iguais a `AbrirChamadoCommandValidator`); servidor em `AtualizarChamadoCommandValidator`. Sem teste automatizado da tela |
| AC-14 | Sim | O modal é desmontado ao fechar (`ChamadoDetailPage.tsx:271`), então a próxima abertura recomeça do texto atual. Sem teste automatizado |
| AC-15 | Sim | `useAcoesChamado.ts` `invalidarChamado` invalida `['chamado', id]` e `['chamados']`, prefixo também da lista e do Kanban (`['chamados','kanban']`); E2E confere o título no detalhe |
| AC-16 | Sim | Tela em `EditarChamadoModal.tsx:49-52`; domínio em `Chamado.cs:253-254`; handler em `AtualizarChamadoCommandHandler.cs:40-41`; testes `AtualizarDados_SemMudanca_*`, `Handle_SemMudanca_NaoGravaNemRegistra` |
| AC-17 | Sim | Handler `:44-61` (JSON por campo, `UsuarioId`/`UsuarioNome` do controller); `TimelineHistorico.tsx:33-49,76`; testes `Handle_SoTitulo_*` e `Handle_TituloEDescricao_*`; E2E confere "Chamado editado" |
| AC-18 | Sim | Só os campos que mudaram entram no JSON; a tela itera `Object.keys(depois)` (`TimelineHistorico.tsx:41`); teste `Handle_SoTitulo_RegistraHistoricoSoComOTitulo` |
| AC-19 | Sim | `EditarChamadoModal.tsx:57-58` (409 → `conflito`, o modal fica aberto), `:92` (mensagem), `:94-100` ("Texto atual no chamado"); `recarregarSeConflito` atualiza o detalhe; E2E `conflito mantém o texto...`. Ressalva em R-01 e R-02 |
| AC-20 | Sim | `EditarChamadoModal.tsx:35-37` (passa a usar a versão nova após recarregar); o behaviour de acesso roda antes do de versão (`Program.cs:54-56`), então a permissão perdida dá 403. Segundo "Salvar" no E2E |
| AC-21 | Sim (comportamento herdado) | `AlteraChamado` não inclui comentar/anexar. Não foi alterado nesta feature |
| AC-22 | Sim | 404 continua primeiro (`AcessoChamadoBehaviour.cs:36-37`); a regra vale no servidor |
| AC-23 | Sim | 400/403/409 com `{ message }` (`ExceptionHandlingMiddleware`). Os erros de validação mantêm o formato `{ errors }` já existente (ver sugestões) |
| AC-24 | Sim | 474/474; os testes cobrem AC-01..AC-11 e AC-16..AC-18 contra a classe real (`ChamadoPermissoes`, `Chamado`) |
| AC-25 | Parcial | O build não foi executado nesta revisão. O E2E existe, mas "editar como quem abriu" e "botão ausente para quem não pode" só passam pelos ramos de Admin e de encerrado (R-04) |

## Achados

### 🟡 Atenção R-01: depois do primeiro conflito, a versão do modal acompanha qualquer recarga, e um terceiro conflito deixa de ser detectado
- **Onde:** `frontend/src/features/chamados/components/EditarChamadoModal.tsx:35-37`
- **Problema:** `conflito` nunca volta a `false`. Por isso o `useEffect` copia `chamado.versao` para `versao` em **toda** recarga posterior do chamado, e não só na recarga causada pelo 409. A intenção do design (§4) era passar a usar a versão que a pessoa viu junto com o aviso. O efeito real é que o modal fica sem proteção de concorrência até fechar.
- **Cenário de falha:** A abre o modal e B edita o título. A clica em "Salvar" e recebe o 409 com o "Texto atual no chamado". A demora mais de 30 s conferindo (o `staleTime` é 30 s) e, nesse meio tempo, C altera a descrição. A troca de aba e volta, e o `refetchOnWindowFocus` (padrão do TanStack, não desligado em `App.tsx:27-39`) recarrega o chamado. `versao` passa a ser a de C sem nenhum aviso. A clica em "Salvar" e a gravação é aceita, sobrescrevendo a descrição de C sem 409. Isso contraria o AC-19 ("B alterou o chamado (qualquer ação) → a gravação é recusada"). A caixa "Texto atual" muda de conteúdo, mas não aparece mensagem nenhuma.
- **Correção sugerida:** atualizar a versão uma única vez por conflito. Por exemplo, no `onError` do 409 guardar a versão recusada e, no efeito, só adotar `chamado.versao` quando ela for diferente da recusada, zerando `conflito` em seguida (`setConflito(false)`). O aviso pode continuar visível por outro estado. Um conflito seguinte volta a dar 409.

### 🟡 Atenção R-02: ao salvar de novo depois do conflito, campos que A não mexeu desfazem a mudança de B
- **Onde:** `EditarChamadoModal.tsx:26-28, 53-55` (envia sempre título **e** descrição do formulário)
- **Problema:** depois do 409, os campos que A não editou continuam com o texto de **antes** da mudança de B, e o segundo "Salvar" manda esse texto de volta.
- **Cenário de falha:** A abre o modal e corrige só a descrição. B corrige o título. A recebe o 409 e vê a caixa "Texto atual no chamado" com o título novo de B, mas o campo Título do modal continua com o título antigo. A clica em "Salvar" de novo (o caminho do AC-20) e o título de B volta ao antigo. O histórico registra "Título: <de B> → <antigo>", uma reversão que ninguém quis. A spec manda "conferir" (AC-19), mas a tela induz ao erro porque não separa o que A mudou do que ficou desatualizado.
- **Correção sugerida:** no conflito, para cada campo em que `valor digitado === original`, adotar o valor recarregado do servidor e atualizar `original` com o texto atual. Só os campos que A de fato mudou mantêm o texto dele, como pede o AC-19. Vale incluir um caso desses no E2E de conflito.

### 🟡 Atenção R-03: a regra antiga de "Editar" continua escrita na spec de autorizacao-chamados e no Obsidian
- **Onde:** `.specs/features/autorizacao-chamados/spec.md:142-144` (AC-20) e `docs/obsidian/00 Visão/Perfis e Permissões.md:50`
- **Problema:** a mudança de regra C2 foi registrada só na spec nova. A spec de origem ainda diz "Solicitante não edita (inclusive um que abriu)… Atendente e Admin podem, nos chamados que conseguem ver", e a matriz do vault diz "Editar título e descrição: — / ✅ / ✅". As duas descrições estão erradas para o código desta branch.
- **Cenário de falha:** quem revalidar a feature autorizacao-chamados pelo AC-20 (Solicitante edita o próprio chamado sem responsável → espera 403) recebe 204 e abre uma "regressão". Ou um Atendente, lendo o vault, espera editar o chamado de um colega e recebe 403. A regra 6 da constitution (o vault reflete o sistema) ainda não está cumprida. A regra 2 pede que a spec seja atualizada antes do código, e a de origem não foi tocada.
- **Correção sugerida:** no AC-20 de `autorizacao-chamados/spec.md`, marcar que ele foi substituído pela spec `editar-chamado` (com data). Atualizar a linha da matriz e a nota "Acompanhamento do Chamado" no `/sdd-close`, como a spec já prevê no gate "Obsidian atualizado".

### 🟡 Atenção R-04: o lado da tela da regra de quem pode editar não tem teste automatizado fora do Admin
- **Onde:** `frontend/e2e/editar-chamado.spec.ts` (os 3 testes entram com a conta Admin de `E2E_EMAIL`) e `frontend/src/features/chamados/lib/permissoes.ts:14-15`
- **Problema:** o AC-25 pede E2E para "editar como quem abriu" e "botão ausente para quem não pode". O teste "editar" usa um Admin que também abriu o chamado, então `podeEditarChamado` decide pelo ramo `perfil.tipo === 'Admin'` (linha 13), e as linhas 14-15 (responsável e quem abriu) não são exercitadas. O "botão ausente" só cobre o encerrado (linha 12). A rastreabilidade da spec marca AC-12..AC-15 como ✅ apoiada nesse teste. A regra do servidor está bem coberta. A da tela depende só da verificação ao vivo (T09).
- **Cenário de falha:** uma alteração futura em `permissoes.ts:15`, por exemplo trocar a comparação de e-mail ou inverter a checagem de `responsavelId`, deixa de mostrar "Editar" ao Solicitante que abriu (AC-01) ou ao responsável (AC-05). Mesmo assim os 474 testes, o build e os 3 E2E continuam verdes.
- **Correção sugerida:** se o ambiente E2E tiver só a conta Admin, registrar na spec que o AC-25 foi cumprido parcialmente e por quê. Ou criar no E2E um Solicitante `[TESTE-E2E]` pela API de Admin, entrar com ele e cobrir o "editar como quem abriu" e o "botão some depois que o Admin assume".

## Constitution e contratos

- **Regra 1 (sem suposição silenciosa):** cumpre. As decisões estão na spec §6, aprovadas em 2026-10-02.
- **Regra 2 (spec antes do código):** cumpre na spec da feature (commit `4f8494d`, antes do `b55900a`). A spec de origem (`autorizacao-chamados` AC-20) ficou desatualizada (R-03).
- **Regra 3 (contrato avisado antes):** cumpre. C1–C5 estão no design e foram aprovados em `7f74cca`, antes do código.
- **Regra 4 (`/sdd`):** cumpre. Spec → design → tasks → implement → este review.
- **Regra 5 (cross-feature / `/analise-cod`):** cumpre. O `impacto.md` existe e a parte do relatório mensal foi marcada com honestidade como "só leitura de código". Conferi os consumidores que ele não lista: `AtualizarChamadoCommand`/`AtualizarDados` só são chamados pelo controller e pelos testes, e `AcaoChamado.Editar` só por `ChamadoPermissoes` e pelos behaviours. Nenhum outro leitor de `HistoricoEntrada` filtra ou enumera `AcaoHistorico` além do relatório. `DataAtualizacao` muda na edição e só alimenta a versão e notificações de outras ações. Não achei consumidor fora do escopo que tenha escapado.
- **Regra 6 (Obsidian):** pendente. `Perfis e Permissões.md:50` ainda tem a regra antiga (R-03). É gate do close, mas precisa entrar antes do PR.
- **Contratos previstos × feitos:** C1–C5 batem com o código, incluindo a ordem 404 → 400 (encerrado) → 403 → 409 e o `ChamadoEditado = 13`. O controller `Atualizar` não declara `ProducesResponseType` 400/403/409, o que é só documentação (Scalar).
- **Pontos de toque cross-feature:** `ChamadoPermissoes` tem as outras ações cobertas por testes da classe real (matriz completa do Admin e do Solicitante). `AcessoChamadoBehaviour` é coberto por `AcoesQueNaoDependemDoChamado_NaoFazemConsultaExtra` e pelos testes de Cancelar, que já existiam. `TimelineHistorico` mantém o ramo antigo idêntico para as outras ações. Para o relatório mensal só há leitura de código, já registrada no `impacto.md`.

## Sugestões (não bloqueiam)

- **Corrida rara entre a checagem e a gravação:** se o chamado for encerrado entre o `VersaoChamadoBehaviour` e o `ObterPorIdComTrackingAsync` do handler, `Chamado.AtualizarDados` lança `InvalidOperationException`, que o middleware devolve como **500 "Ocorreu um erro interno."**, e não como o 400 do AC-10. O padrão é o mesmo das outras ações, então não é regressão. No handler, dá para converter em `BadRequestException(ChamadoPermissoes.MensagemEdicaoEncerrado)`.
- **Espaços nas pontas:** a comparação do AC-16 é exata (`Chamado.cs:253` e `EditarChamadoModal.tsx:49`). "Título " conta como mudança e gera entrada no histórico só com espaço. Vale avaliar `Trim()` na tela antes de enviar.
- **AC-11 × domínio:** a spec diz que o chamado reaberto "fica Em andamento". `Chamado.Reabrir` deixa `Aberto` (`Chamado.cs:151`), e o texto da confirmação na tela também diz "Em Andamento". A regra de edição funciona nos dois casos. Corrigir o texto da spec, ou da tela, evita confusão.
- **Formato de erro de validação:** um PUT inválido fora da tela recebe `{ errors: [...] }`, que é o formato do `ValidationBehaviour` existente, não `{ message }` (AC-23). Não é alcançável pela tela, que valida antes.
- **Botão "Salvar" durante a recarga do conflito:** em `EditarChamadoModal.tsx:106`, "Salvar" não fica desabilitado enquanto o chamado recarrega depois do 409. Um clique rápido dá um segundo 409, que é inofensivo. Dá para usar `useIsFetching(['chamado', id])`, como já faz o `BotoesAcao`.
- **Testes da tela:** AC-13, AC-14 e o AC-16 do lado da tela (fechar sem chamar a API) só foram verificados ao vivo. Se for fácil, um E2E curto de "salvar sem mudar não cria 'Chamado editado'" fecha a lacuna.
- **`impacto.md`:** diz "dotnet build 0 avisos". Nesta revisão o build mostrou `CS8634` em `Program.cs:83`, que é anterior à feature e fica fora do diff. Vale só ajustar a frase.

---

## Tratamento dos achados (sessão principal, 2026-10-02)

| Achado | Confirmado? | Tratamento |
|---|---|---|
| R-01 🟡 | Sim | **Corrigido:** depois de um 409 o modal adota a versão **uma vez só** (a da recarga causada pelo conflito); recargas posteriores voltam a ser detectadas como conflito. |
| R-02 🟡 | Sim | **Corrigido:** campo que a pessoa não mudou vai com o valor **atual** do chamado, preservando o que a outra pessoa gravou nele. |
| R-03 🟡 | Sim | **Corrigido no close:** AC-20 de `autorizacao-chamados` marcado como substituído; `Perfis e Permissões` e `Acompanhamento do Chamado` no Obsidian com a regra nova. |
| R-04 🟡 | Sim | **Pendência registrada:** o frontend não tem framework de teste unitário; os ramos "quem abriu"/"responsável" de `lib/permissoes.ts` ficam cobertos pela verificação na tela com contas não-Admin (close) e pela regra do servidor (testes + 23/23 ao vivo), que é quem decide. |
| Sugestão: 500 se encerrado no meio | Sim | **Corrigido:** handler converte em 400 com a mensagem do AC-10 + teste. |
| Sugestão: "Salvar" habilitado durante a recarga pós-conflito | Sim | **Corrigido** (junto com o R-01). |
| Sugestão: reaberto "Em andamento" na spec | Sim | **Spec corrigida** (reabrir leva a Aberto). O diálogo de reabrir na tela também diz "Em Andamento" — pré-existente, registrado como pendência no STATE. |
| Sugestão: espaços nas pontas contam como edição | Sim | Mantido: mesmo comportamento da abertura (que também não apara); registrado. |
| Sugestão: "0 avisos" no impacto.md | Sim | Texto corrigido. |
