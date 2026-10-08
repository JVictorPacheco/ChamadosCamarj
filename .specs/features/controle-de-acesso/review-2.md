# Review: controle-de-acesso — rodada 2

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-05
> **Escopo:** `git diff origin/develop...feature/controle-de-acesso` — 74 arquivos, +3657/-174 linhas (18 commits, `654b666..d419271`)
> **Veredito:** BLOQUEADO

Verificação feita pela leitura do código (backend e frontend não foram subidos, por falta de memória
na máquina; nada foi aplicado no banco). `dotnet test tests/ChamadosCamarj.UnitTests/` rodado nesta
revisão: **515/515**, 0 falhas. `npm run build` e E2E não foram rodados nesta revisão.

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim | `AcessosQueries.cs` (`ListarAcessosQueryHandler`); `ControleAcessoPage.tsx:38-41` (busca nome/e-mail); `Listar_AdminComoAcessoTotal`; E2E teste 1 |
| AC-02 | Sim | `ObterAcessoUsuarioQueryHandler` (`Padrao`/`Efetivo`/`Ajustavel`); `ControleAcessoPage.tsx` (etiqueta "padrão do perfil"/"ajustado"); `ObterDetalhe_Solicitante_NaoMostraKanbanNemFila` |
| AC-03 | Sim | `SalvarAcessosCommand.cs:43-53`; `Salvar_TirarModuloDeAtendente_GravaAjusteAuditaEAvisa`; E2E teste 1 |
| AC-04 | Sim | `VoltarAoPadraoCommandHandler`; `VoltarAoPadrao_RemoveAjustesAuditaEAvisa`; E2E teste 1 |
| AC-05 | Sim | Servidor: `PerfilRequisitanteGuard.ExigirAdmin` nos handlers (`Salvar_QuemNaoEhAdmin_Forbidden`, `DefinirChatDeAcesso_QuemNaoEhAdmin_Forbidden`). Tela: `RequerAdmin` com "Você não tem permissão para acessar esta área." (`App.tsx:121-128`); E2E teste 2 |
| AC-06 | Sim | Módulos de Admin travados (`AcessosGuard`, `SalvarAcessosCommand.cs:120-131`; `Salvar_AlvoAdmin_Recusa`, `Salvar_OProprioAdmin_Recusa`); Chat de Admin ajustável (`DefinirChatDeAcessoCommand`, `ControleAcessoPage.tsx:105-109`; `DefinirChatDeAcesso_ParaAdmin_UsaOComandoDeChat`) |
| AC-07 | Sim | Servidor: `ModuloGuard` + `AplicarVisibilidade` (Dashboard), `RelatorioEscopo` + `IdsVisiveis` (Relatório); `Handle_SolicitanteComModuloDashboard_*`, `Solicitante_SoOsChamadosQueEleVe_*`. Tela: `DashboardPage.tsx:49`, `RelatorioMensalPage.tsx:39-41` usam `temModulo`; E2E teste 3 |
| AC-08 | Sim | `ModulosDeAcesso.Ajustaveis`/`Efetivos`; `Salvar_KanbanParaSolicitante_Recusa`; `Solicitante_NuncaTemKanbanNemFila_MesmoComAjusteGravadoErrado` |
| AC-09 | Sim | `SalvarAcessosCommand.cs:57-63` e `DefinirChatDeAcessoCommand` delegam ao `DefinirChatPerfilCommand` (avisos + `ChatHistoricos`); `Salvar_MudarChat_UsaOComandoDeChatExistente_EAvisa` |
| AC-10 | Sim | `UsuarioFormDialog.tsx`/`UsuariosPage.tsx` sem Chat; `AtualizarUsuarioPerfilCommand.ChatPerfil` opcional; `Handle_ChatPerfilNulo_NaoMexeNoChat` |
| AC-11 | Sim | `RequerModulo` (`App.tsx:109-118`) + `AppLayout.tsx:78-80` (`AcessosAtualizados`); E2E teste 1 |
| AC-12 | Sim (servidor do Relatório sem teste) | Dashboard: `ModuloGuard` nos 2 handlers (`Handle_AtendenteSemModuloDashboard_DeveLancarForbidden`). Relatório: `RelatoriosController.cs:46` (sem teste — ver R-06). Chat: `ChatPerfilGuard` inalterado. Tela: `RequerModulo` com "sem permissão"; E2E teste 3 |
| AC-13 | Sim | Mesmo evento; E2E teste 1 (voltar ao padrão faz o item reaparecer) |
| AC-14 | **Parcial** | Gravação: `AcessosTexto.DiferencasDeModulos`, `DefinirChatPerfilCommandHandler.cs:54-59`, `AtualizarUsuarioPerfilCommandHandler.cs:85-90`. **Visualização:** o histórico não aparece para pessoas com perfil Admin (R-02); reativação de conta não é auditada (R-04) |
| AC-15 | Sim | `AtualizarUsuarioPerfilCommandHandler.cs:56-63,83-93`; `Handle_QuandoPerfilMuda_ZeraAjustesDeModulos_AuditaEAvisa`. (Na tela da pessoa, o perfil não acompanha — R-03) |
| AC-16 | Sim | Colunas `default 0`; `Padroes_SaoIguaisAoMenuDeAntesDaFeature`; `modulosPadrao` para sessão antiga (`AuthContext.tsx`) |
| AC-17 | Sim | `BadRequest`/`Forbidden`/`NotFound` com mensagens em português; `AcessosController` devolve `{ message }` nos 400 |
| AC-18 | Parcial | 515/515. Guarda e escopo do Relatório chamados no controller sem teste; `ObterMetricasQueryHandler` sem teste próprio (R-06) |
| AC-19 | Sim (declarado) | E2E `controle-de-acesso.spec.ts` (3 testes); `npm run build`/E2E 20/20 declarados no `review.md`, não rodados nesta revisão |

### Correções da rodada 1 conferidas no código

| Achado | Está no código? | Observação |
|---|---|---|
| R-01 | Sim | `DashboardPage`/`RelatorioMensalPage` usam `temModulo`; consulta do Relatório habilitada para o Solicitante |
| R-02 | Sim | `PUT /api/acessos/{id}/chat` + "Ajustar Chat" na linha do Admin. **Introduziu R-02 desta rodada** (histórico invisível para Admin) |
| R-03 | Sim, parcial | `VoltarAoPadraoDeModulos()` na reativação; **sem auditoria** (a sugestão da rodada 1 pedia "e auditar") — R-04 |
| R-04 | Sim | Mensagens distintas "retirado" × "sem permissão" × "esta área" |
| R-05 | Sim | `chatPerfil`/`modulos` nulos → 400 no controller; `SalvarAcessosCommandValidator` roda antes do handler (`ValidationBehaviour` sem restrição de tipo, MediatR 14) |
| R-06 | Sim, parcial | Auditoria "Chat" dentro do `DefinirChatPerfilCommandHandler` (3 caminhos). A reativação muda o Chat **sem** passar por ele — R-04 |
| R-07 | Servidor sim; tela não | `ModuloGuard` recusa perfil divergente do token. A tela mostra "Serviço indisponível" em vez da mensagem e o evento não leva o perfil — R-03 |
| R-08 | Parcial | Escopo extraído para `RelatorioEscopo` com 3 testes, mas guarda + escopo + `IChamadoRepository` continuam no controller, sem teste da ligação — R-06 |

Busca por checagens da regra antiga ("por perfil") restantes: `KanbanPage.tsx:12` e
`FilaAtendimentoPage.tsx:87` ainda barram `tipo === 'Solicitante'` — **coerente** com a spec (Solicitante
nunca recebe Kanban/Fila). No servidor, nenhuma checagem "Solicitante → 403" sobrou em Dashboard/Relatório.
`ListarChamadosQueryHandler` usa o perfil só para comentários internos (fora de escopo).

## Achados

### 🔴 Bloqueante R-01 — Senha de conta de teste no repositório público; o E2E cria e deixa ativo um Atendente com essa senha no banco real
- **Onde:** `frontend/e2e/controle-de-acesso.spec.ts:6-7,15,63,85` (`SENHA = 'TesteAcesso#2026'`); contraria `frontend/e2e/helpers.ts:3` ("Credenciais NUNCA ficam no código")
- **Problema:** o repositório `JVictorPacheco/ChamadosCamarj` é **público** (`gh repo view`: `PUBLIC`) e a branch já está no `origin`. Os testes criam `teste.acesso.e2e@camarj.com.br` (perfil **Atendente**) e `teste.acesso.sol.e2e@camarj.com.br` com essa senha fixa no banco real (dev = prod) e **não** desativam nem apagam as contas ao final — dependem de uma limpeza manual "com OK do usuário" (T12). Cada nova rodada do E2E (que é gate obrigatório e vai rodar de novo após as correções deste review) recria a conta.
- **Cenário de falha:** depois da próxima rodada do E2E, qualquer pessoa que leia o repositório entra no portal de produção como `teste.acesso.e2e` / `TesteAcesso#2026`, com perfil Atendente: vê todos os chamados **sem responsável** do sistema inteiro (regra de visibilidade do Atendente em `ChamadoRepository.AplicarVisibilidade`), pode assumir/resolver chamados e usar Dashboard/Relatório. A janela fica aberta até alguém lembrar de apagar a conta; se a limpeza for esquecida, fica para sempre.
- **Correção sugerida:** senha das contas de teste vinda do ambiente (como `E2E_SENHA`) ou gerada aleatoriamente a cada execução; desativar as contas num `finally`/`afterAll` (`PUT /usuarios/{id}` com `ativo: false`); trocar a senha no histórico não resolve o que já foi publicado, então tratar `TesteAcesso#2026` como vazada (nunca reutilizar) e conferir no banco (só leitura, com OK do usuário) que as contas `teste.acesso.*` não estão ativas.

### 🔴 Bloqueante R-02 — Histórico de mudanças invisível para pessoas com perfil Admin (AC-14 não atendido na tela)
- **Onde:** `frontend/src/features/admin/ControleAcessoPage.tsx:105-109` (linha de Admin abre só o `PainelChatAdmin`) e `:245-289` (`PainelChatAdmin` sem histórico); o histórico só existe em `FormularioAcessos` (`:215`)
- **Problema:** AC-14 exige que **toda** mudança de acesso (módulo ou Chat) fique registrada **e que o Admin consiga ver o registro na pessoa, na tela de controle de acesso**. Com a correção do R-02 da rodada 1, o Chat de Admins passou a ser ajustável e é auditado (`DefinirChatPerfilCommandHandler`), mas para quem tem perfil Admin a tela só abre o diálogo de Chat, sem o histórico. O mesmo vale para o registro "Perfil" do AC-15 quando a pessoa é promovida a Admin: a linha é gravada, mas some da tela junto com todo o histórico anterior da pessoa. A API (`GET /api/acessos/{id}`) devolve a auditoria também para Admin — só a tela não mostra.
- **Cenário de falha:** Admin A tira o Chat do Admin B ("Ajustar Chat" → Sem acesso). Admin C, auditando, abre B no Controle de acesso: vê só o seletor de Chat, nenhum registro de quem mudou e quando. Ou: Atendente Maria teve o Relatório retirado e depois foi promovida a Admin; a linha "Perfil Atendente → Admin (ajustes zerados)" e o histórico de módulos dela ficam inacessíveis pela tela.
- **Correção sugerida:** no `PainelChatAdmin`, carregar `useAcessoUsuario(pessoa.id)` e exibir o mesmo bloco "Histórico de mudanças" (extrair o bloco para um componente compartilhado). Acrescentar ao E2E (ou teste de componente) a verificação do histórico num Admin.

### 🟡 Atenção R-03 — Após mudança de perfil, a tela da pessoa fica num estado misto e a recusa "Seu perfil mudou. Entre novamente" aparece como "Serviço indisponível"
- **Onde:** `src/ChamadosCamarj.Application/Common/Notifications/AcessoNotifications.cs:10` (evento sem o perfil); `frontend/src/auth/AuthContext.tsx` (`atualizarAcessos` não muda `tipo`); `frontend/src/features/dashboard/DashboardPage.tsx:74-78` e `frontend/src/features/relatorio-mensal/RelatorioMensalPage.tsx:67-71` (mensagem genérica em qualquer erro); `ModuloGuard.cs:33`
- **Problema:** a parte de servidor do R-07 foi corrigida, mas a tela não: `AcessosAtualizados` (publicado no AC-15) leva os módulos do **novo** perfil, enquanto `perfil.tipo` continua o antigo até novo login/recarga. O servidor recusa Dashboard/Relatório com "Seu perfil mudou. Entre novamente para continuar.", mas as páginas exibem "Serviço indisponível. Tente novamente em instantes." — a instrução que resolveria o problema nunca chega à pessoa.
- **Cenário de falha:** Admin promove a Solicitante Maria a Atendente com ela logada → o menu dela ganha Kanban, Fila, Dashboard e Relatório na hora (AC-13) → Kanban mostra "não está disponível para o seu perfil" (`KanbanPage.tsx:12`, `tipo` ainda Solicitante) e Dashboard/Relatório mostram "Serviço indisponível. Tente novamente em instantes." Ela tenta de novo até o token vencer (`TokenExpiracaoHoras`), e o Admin recebe um chamado de "sistema fora do ar".
- **Correção sugerida:** incluir o perfil no `AcessosAtualizadosNotification` e, quando ele mudar, mostrar na tela "Seu perfil mudou. Entre novamente para continuar." (ou forçar o logout); nas duas páginas, exibir `error.message` quando o erro for 403.

### 🟡 Atenção R-04 — Reativar conta pelo "Novo usuário" muda módulos, Chat e perfil sem registro na auditoria de acessos (AC-14)
- **Onde:** `src/ChamadosCamarj.Application/Features/Usuarios/Commands/CriarUsuarioPerfilCommandHandler.cs:38-46`
- **Problema:** a correção do R-03 da rodada 1 zera os ajustes de módulos, mas não grava `AuditoriaAcesso` (a sugestão da rodada 1 pedia "e auditar se havia ajuste"). O mesmo caminho troca o Chat direto na entidade (`DefinirChatPerfil(request.ChatPerfil)`, sem o `DefinirChatPerfilCommand`), então a afirmação do tratamento do R-06 ("todo caminho que muda o Chat entra na auditoria") não vale aqui; e uma troca de perfil nesse caminho também não é auditada (AC-15 diz "a mudança fica na auditoria").
- **Cenário de falha:** Solicitante com Dashboard concedido e Chat "Participante" é desativado; o Admin recria a conta (mesmo e-mail) como Atendente → Dashboard concedido apagado, Chat vira "Sem acesso", perfil vira Atendente — o "Histórico de mudanças" da pessoa não mostra nada disso; o último registro continua sendo "Dashboard desligado → ligado".
- **Correção sugerida:** no ramo de reativação, gravar as diferenças de módulos (`AcessosTexto.DiferencasDeModulos`), a linha "Perfil" se mudou e a linha "Chat" se mudou (ou despachar o `DefinirChatPerfilCommand`); teste em `CriarUsuarioPerfilHandlerTests`.

### 🟡 Atenção R-05 — Painel com dados velhos grava ajustes calculados contra o perfil novo (retira módulos sem o Admin pedir)
- **Onde:** `frontend/src/features/admin/hooks/useUsuarios.ts:28,39,51` (só invalida `['usuarios']`); `frontend/src/App.tsx` (`staleTime: 30_000`); `SalvarAcessosCommand.cs:43-44` (o pedido não diz para qual perfil os módulos foram escolhidos)
- **Problema:** o painel monta os módulos a partir do detalhe em cache (`['acessos', id]`, fresco por 30 s); editar o perfil em Usuários não invalida `['acessos']`. O servidor recalcula os ajustes contra o perfil **atual** do cadastro, sem conferir se a tela estava vendo o mesmo perfil.
- **Cenário de falha:** Admin abre "Ajustar acessos" da Solicitante Maria (Arquivo + Dashboard) e fecha; vai em Usuários e a promove a Atendente; volta ao Controle de acesso em menos de 30 s, abre Maria (painel ainda de Solicitante, sem Kanban/Fila) e só troca o Chat → Salvar envia `["Arquivo","Dashboard"]` → o servidor grava `retirados = Kanban | Fila | RelatorioMensal`: a recém-promovida Atendente perde três módulos que ninguém tirou (fica na auditoria, mas como se o Admin tivesse desligado). O mesmo acontece com dois Admins trabalhando ao mesmo tempo.
- **Correção sugerida:** invalidar `['acessos']` nas mutações de Usuários; no servidor, receber o perfil que a tela viu (`perfilEsperado`) e recusar com 409 "O perfil desta pessoa mudou. Abra de novo." quando diferir.

### 🟡 Atenção R-06 — Guarda e escopo do Relatório continuam no controller, sem teste da ligação (R-08 da rodada 1 só em parte)
- **Onde:** `src/ChamadosCamarj.WebApi/Controllers/RelatoriosController.cs:20-22,46-51`
- **Problema:** o escopo virou `RelatorioEscopo` (testado), mas quem chama `ModuloGuard.ExigirAsync` e `RelatorioEscopo.ResolverAsync` — e injeta `IChamadoRepository` — ainda é o controller (a arquitetura do projeto é Controller → MediatR → Handler → Repositório, `.specs/codebase/ARCHITECTURE.md`). O projeto não testa controllers, então nenhuma das duas chamadas tem teste; AC-18 pede teste para AC-07/AC-12.
- **Cenário de falha:** uma refatoração do controller remove a linha 46 ou passa `responsavelId` direto para a query → um Atendente sem o módulo Relatório volta a ver o relatório, ou um Solicitante com o módulo vê os números do sistema inteiro; os 515 testes continuam verdes.
- **Correção sugerida:** mover guarda + escopo para o `ObterRelatorioMensalQueryHandler` (com `ICurrentUserService` e `ModuloGuard`, como o Dashboard), mantendo `IdsVisiveis`/`ResponsavelId` só como detalhe interno, e testar o handler para os 3 perfis e para "sem o módulo → 403".

## Constitution e contratos

- **Regra 1 (sem suposição silenciosa):** cumpre — o Chat de Admin (R-02 da rodada 1) foi decidido pelo usuário e registrado na spec (§6) e no design (§13).
- **Regra 2 (spec antes do código):** cumpre — `7256d4c` (spec/design ajustados) antecede `718122f`/`e9f0af7` (código).
- **Regra 3 (contrato avisado antes):** cumpre — a rota nova `PUT /api/acessos/{id}/chat` e `SalvarAcessosRequest.ChatPerfil` obrigatório estão no design §13 antes do código; a nova dependência do `DefinirChatPerfilCommandHandler` (chat-corporativo) está declarada no tratamento do R-06.
- **Regra 4 (SDD):** cumpre — esta é a 2ª rodada de review independente.
- **Regra 5 (cross-feature / `/analise-cod`):** cumpre em boa parte — a re-análise do `impacto.md` reconheceu os furos da rodada 1 e conferiu os consumidores do `DefinirChatPerfilCommandHandler` e do `ModuloGuard`. Não foram tratados como consumidores: o cache `['acessos']` frente às mutações de Usuários (R-05) e a tela de Dashboard/Relatório frente à nova recusa "Seu perfil mudou" (R-03).
- **Regra 6 (Obsidian):** pendente (gate desmarcado) — esperado no fechamento.
- **Convenção do projeto violada:** credenciais no código de E2E (`helpers.ts:3`) — R-01.
- **Mudanças de contrato previstas × feitas:** batem com C1–C9 + ajustes do §13. Divergência menor: `AcessosController` sem `[Produces("application/json")]` (`CONVENTIONS.md` §2.9).
- **Pontos de toque cross-feature:** login, `/auth/me`, menu global e rotas sem regressão para quem não tem ajuste (padrões cobertos por teste). Chat: auditoria a mais em todo caminho, com teste. Usuários: Chat removido da tela, edição sem Chat não mexe no Chat (teste). Dashboard/Relatório: servidor coberto; mensagem de erro na tela para o caso novo (R-03).
- **Migration:** inalterada desde a rodada 1 (aditiva, já aplicada com OK do usuário) — nada a fazer.

## Sugestões (não bloqueiam)

- Pendentes da rodada 1, ainda válidos: `HashSet<Guid>` para `IdsVisiveis` no handler do Relatório; limpar o `avisoModulo` do `history.state` (sobrevive a recarregar e não tem botão de fechar); refazer `/auth/me` no `onreconnected` do SignalR; `SalvarAcessosCommand` grava módulos e Chat em `SaveChanges` separados, sem transação.
- `DashboardPage`: para o Solicitante com o módulo, o texto vazio "Nenhum chamado no sistema." sugere o sistema todo; algo como "Nenhum chamado para mostrar." fica correto para todos.
- `RequerModulo` guarda `tinhaAoAbrir` em `useState`; como as 5 rotas de módulo usam o mesmo componente na mesma posição da árvore, o React pode reaproveitar o estado ao navegar entre elas (ex.: botão Voltar). Hoje isso só leva a mostrar "retirado" para quem de fato teve o módulo, mas um `key={modulo}` no elemento deixa a intenção explícita.
- E2E do R-01 da rodada 1 só confere a ausência da mensagem antiga e o título; conferir também que não aparece "Serviço indisponível" (ou que um KPI carregou) pegaria um 403 do servidor.
- `spec.md` §5 e o cabeçalho do `impacto.md` ainda citam "506 testes, E2E 19/19"; atualizar para os números atuais no fechamento.
- `AcessosController`: acrescentar `[Produces("application/json")]` como os demais controllers.

---

## Tratamento (sessão principal, 2026-10-05)

| Achado | Decisão | O que foi feito | Prova |
|---|---|---|---|
| R-01 🔴 | corrigir já (OK do usuário) | 5 contas `teste.acesso.*` desativadas no banco (login → 401). E2E com senha aleatória por rodada (`contaDeTeste`) e `desativarContaDeTeste` no `finally`. A senha antiga segue no histórico do git, mas não abre mais nada. | E2E 22/22; consulta no banco |
| R-02 🔴 | "Ajustável na tela nova" (usuário) | `PainelChatAdmin` busca o detalhe e mostra o histórico (componente `HistoricoAcessos`, o mesmo do painel geral). | E2E "Painel de Chat de um Admin mostra o histórico de mudanças" |
| R-03 🟡 | "Sai e entra de novo" (usuário) | `AcessosAtualizadosNotification` + payload levam o `perfil` do cadastro; `AppLayout` compara com o da sessão e, se diferente, desconecta e a tela de login mostra "Seu perfil foi alterado. Entre novamente." (`auth/logoutPerfilAlterado.ts`, mesmo mecanismo do aviso de inatividade). Spec AC-15 e design §14 atualizados antes do código. | `AcessoSignalRNotificationHandlersTests` (payload com perfil), `AcessosCommandsTests`, `AtualizarUsuarioPerfilHandlerTests`; E2E "Admin muda o perfil de quem está com o sistema aberto…" |
| R-04 🟡 | corrigir | Reativação grava "Conta" (e "Chat", se mudou) na auditoria de acessos. | `CriarUsuarioPerfilHandlerTests` |
| R-05 🟡 | corrigir | Servidor: `perfilEsperado` → 409 se mudou. Tela: manda `perfilEsperado: detalhe.perfil`; o painel espera a leitura nova (`refetchOnMount: 'always'` + `isFetchedAfterMount`) antes de montar o formulário; criar/editar usuário invalida `['acessos']`. | `AcessosCommandsTests` (conflito); build |
| R-06 🟡 | corrigir | `ObterRelatorioMensalAutorizadoQuery` (módulo + escopo + relatório); controller só repassa. | `ObterRelatorioMensalAutorizadoQueryHandlerTests` |

Gates após o tratamento: `dotnet build` 0 erros (3 avisos antigos) · `dotnet test` **520/520** · `tsc -b` ok · lint só avisos antigos · E2E **22/22** (5 de controle de acesso).
