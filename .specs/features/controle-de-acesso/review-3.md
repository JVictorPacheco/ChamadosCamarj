# Review: controle-de-acesso — rodada 3

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-05
> **Escopo:** `git diff origin/develop...feature/controle-de-acesso` — 83 arquivos, +4137/-185 linhas (20 commits, `654b666..0365ad3`)
> **Veredito:** BLOQUEADO

Verificação feita pela leitura do código, sem subir backend/frontend e sem acessar o banco.
`dotnet test tests/ChamadosCamarj.UnitTests/`: **520/520**, 0 falhas. `npm --prefix frontend run build`: ok.
O achado R-01 foi **reproduzido** num projeto descartável no scratchpad (EF Core 9 InMemory, mesmo padrão
de repositório: `AsNoTracking` + `Include(Grupo)` + `Update` + `Detach` só do usuário) — fora do repositório.

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim | `AcessosQueries.cs:20-34` (lista todos, Admin como acesso total); `ControleAcessoPage.tsx:38-41` (busca por nome/e-mail); E2E `controle-de-acesso.spec.ts:22` |
| AC-02 | Sim | `AcessosQueries.cs:58-66` (`Padrao`/`Efetivo`/`Ajustavel`); `ControleAcessoPage.tsx:188-190` ("padrão do perfil"/"ajustado") |
| AC-03 | Parcial | `SalvarAcessosCommand.cs:47-58`; `Salvar_TirarModuloDeAtendente_GravaAjusteAuditaEAvisa`; E2E. **Falha** quando no mesmo "Salvar" mudam módulo e Chat de pessoa que tem Grupo — ver R-01 |
| AC-04 | Sim | `SalvarAcessosCommand.cs:99-114`; `VoltarAoPadrao_RemoveAjustesAuditaEAvisa`; E2E `:55-57` |
| AC-05 | Sim | Servidor: `PerfilRequisitanteGuard.ExigirAdmin` nos 5 handlers; Tela: `App.tsx:122-128` (`RequerAdmin` com "Você não tem permissão para acessar esta área."); E2E `:64`. Teste de servidor só para Salvar/Chat — ver R-03 |
| AC-06 | Sim | `SalvarAcessosCommand.cs:125-132` (módulos de Admin travados, inclusive o próprio); `DefinirChatDeAcessoCommandHandler` (Chat de qualquer um); `ControleAcessoPage.tsx:105-108`; E2E `:131` |
| AC-07 | Sim | Dashboard: `ModuloGuard` + `AplicarVisibilidade` em todas as contagens (`ChamadoRepository.cs:352-421`); Relatório: `ObterRelatorioMensalAutorizadoQuery` + `RelatorioEscopo` (`IdsVisiveis`); tela: `DashboardPage.tsx:49`, `RelatorioMensalPage.tsx:39`; E2E `:82` |
| AC-08 | Sim | `ModulosDeAcesso.cs:25-43`; `ObterDetalhe_Solicitante_NaoMostraKanbanNemFila`; `Solicitante_NuncaTemKanbanNemFila_MesmoComAjusteGravadoErrado` |
| AC-09 | Parcial | `SalvarAcessosCommand.cs:62-67` e `DefinirChatDeAcessoCommandHandler` despacham o `DefinirChatPerfilCommand` de sempre. Mesma falha do R-01 quando módulo e Chat mudam juntos |
| AC-10 | Sim | `UsuarioFormDialog.tsx` e `UsuariosPage.tsx` sem Chat; `AtualizarUsuarioPerfilCommand.ChatPerfil` opcional; `Handle_ChatPerfilNulo_NaoMexeNoChat` |
| AC-11 | Sim | `AcessoSignalRNotificationHandlers.cs`; `AppLayout.tsx:87-97`; `App.tsx:109-119` ("retirado"); E2E `:41-45` |
| AC-12 | Sim | Tela: `RequerModulo` (`App.tsx:109-119`); servidor: `ModuloGuard` (Dashboard, Relatório), `ChatPerfilGuard` (Chat); E2E `:47-49`, `:94-96`; `ObterRelatorioMensalAutorizadoQueryHandlerTests` |
| AC-13 | Sim | Mesmo evento; `AppLayout.tsx` monta o menu por `temModulo`; E2E `:57` |
| AC-14 | Parcial | `AuditoriaAcesso` gravada em Salvar, Voltar ao padrão, `DefinirChatPerfilCommandHandler`, mudança de perfil e reativação; tela `HistoricoAcessos`. No cenário do R-01 os módulos mudam **sem** linha de auditoria |
| AC-15 | Parcial | `AtualizarUsuarioPerfilCommandHandler` (zera ajustes, audita, avisa); logout na hora em `AppLayout.tsx:91-95`; E2E `:106`. Quem não estava conectado quando o perfil mudou não é desconectado e fica com token do perfil antigo — ver R-02 |
| AC-16 | Sim | `Padroes_SaoIguaisAoMenuDeAntesDaFeature`; migration com `default 0` (`20261002212922_AddControleDeAcesso.cs`) |
| AC-17 | Sim | Exceções de Application → `{ message }` pelo `ExceptionHandlingMiddleware`; 400 do `AcessosController` em `{ message }`. O 500 do R-01 sai com a mensagem genérica |
| AC-18 | Parcial | 520 testes passando; faltam testes de "não-Admin → 403" para listar/detalhar/voltar ao padrão e do módulo no handler de métricas — ver R-03 |
| AC-19 | Sim | `npm run build` ok; E2E cobre ajustar módulo, menu mudando e tela bloqueada |

## Achados

### 🔴 Bloqueante R-01 — "Salvar" que muda módulo e Chat juntos de uma pessoa com Grupo dá erro 500 e deixa os módulos gravados sem auditoria nem aviso
- **Onde:** `src/ChamadosCamarj.Application/Features/Acessos/Commands/SalvarAcessosCommand.cs:53-67`; `src/ChamadosCamarj.Infrastructure/Repositories/UsuarioPerfilRepository.cs:26,46-58`
- **Problema:** o `SalvarAcessosCommandHandler` grava os módulos com `AtualizarAsync(usuario)` e, na mesma requisição (mesmo `DbContext`), despacha o `DefinirChatPerfilCommand`, que carrega **outra cópia** do usuário e chama `AtualizarAsync` de novo. O repositório carrega com `Include(u => u.Grupo)`; `DbSet.Update` rastreia também o `Grupo`, e o `Detach` da linha 58 solta **só o usuário**. Na segunda chamada, o `Grupo` da nova cópia colide com o `Grupo` ainda rastreado → `InvalidOperationException` ("The instance of entity type 'Grupo' cannot be tracked because another instance with the same key value ... is already being tracked"). O comentário do próprio repositório já avisa do "salvar o mesmo usuário duas vezes por requisição"; o detach resolveu só para quem não tem Grupo. Os testes do handler usam Moq e não modelam o change tracker.
- **Cenário de falha:** Atendente Maria pertence a uma área (Grupo). O Admin abre "Ajustar acessos", desmarca Relatório mensal e muda o Chat de "Sem acesso" para "Participante" → Salvar. Os módulos são gravados (primeiro `SaveChanges`); o `DefinirChatPerfilCommand` lança a exceção → 500 "erro interno" na tela. Resultado: Maria perdeu o Relatório **sem** linha em `AuditoriaAcessos` (AC-14), **sem** `AcessosAtualizados` (o menu dela não muda — AC-11) e o Chat não foi alterado. Se o Admin tentar de novo, os módulos já não mudam (iguais ao gravado), só o Chat — e passa, então a retirada do Relatório fica para sempre fora do histórico. Reproduzido com EF Core 9 (InMemory) no scratchpad com o mesmo padrão de carga/atualização. O E2E não pega porque as contas de teste são criadas sem Grupo.
- **Correção sugerida:** no `UsuarioPerfilRepository.AtualizarAsync`, não arrastar o grafo: `_context.Entry(usuario).State = EntityState.Modified` (só a entidade, sem navegações) ou desanexar também o `Grupo`/limpar o `ChangeTracker` depois do `SaveChanges`; alternativamente, no `SalvarAcessosCommand`, mudar o Chat **antes** dos módulos e recarregar o usuário. Gravar módulos e Chat numa transação (`IUnitOfWork`) evita o estado parcial. Acrescentar um teste com provedor real em memória (SQLite/InMemory) para o caminho "módulo + Chat + Grupo", e rodar ao vivo com uma pessoa que tenha área. O mesmo defeito latente existe em `AtualizarUsuarioPerfilCommandHandler` quando alguém manda `chatPerfil` (a tela de Usuários deixou de mandar, mas o contrato aceita).

### 🟡 Atenção R-02 — Quem não estava conectado quando o perfil mudou nunca é desconectado: a tela assume o perfil novo, o token continua com o antigo
- **Onde:** `frontend/src/auth/AuthContext.tsx:103-108` (boot com `/auth/me` troca `perfil.tipo` sem comparar); `frontend/src/layouts/AppLayout.tsx:91` (a comparação só existe no evento em tempo real); `frontend/src/features/dashboard/DashboardPage.tsx:76` e `frontend/src/features/relatorio-mensal/RelatorioMensalPage.tsx:69` (403 vira "Serviço indisponível"); `ModuloGuard.cs:32-33`
- **Problema:** a decisão do review-2 R-03 (sair e entrar de novo) só vale para quem recebe o `AcessosAtualizados`. Se a aba estava fechada, o notebook suspenso ou a conexão SignalR caída, ao reabrir o `/auth/me` grava o **perfil novo** no navegador, mas o token (10 h, `AuthSettings.TokenExpiracaoHoras`) continua com o **antigo**. A partir daí os eventos seguintes trazem o mesmo perfil da tela e a pessoa nunca é desconectada. O `ModuloGuard` recusa Dashboard/Relatório com "Seu perfil mudou. Entre novamente para continuar.", mas as páginas mostram a mensagem genérica.
- **Cenário de falha:** Admin promove a Solicitante Maria a Atendente às 18h, com o navegador dela fechado. Às 8h do dia seguinte (token emitido às 14h, ainda válido) ela abre o portal: o menu mostra Kanban, Fila, Dashboard e Relatório (perfil novo); Kanban e Fila listam só os chamados dela (visibilidade de Solicitante do token) e "Assumir" dá 403; Dashboard e Relatório mostram "Serviço indisponível. Tente novamente em instantes." até o token vencer. O AC-15 diz "ao entrar, já vem com o perfil novo" — aqui ela "entrou" (reabriu) e não vem.
- **Correção sugerida:** no boot do `AuthContext`, se o `perfil` do `/auth/me` for diferente do salvo (que é o do login/token), fazer o mesmo logout com `marcarLogoutPorPerfilAlterado()`; considerar refazer o `/auth/me` no `onreconnected` do SignalR; nas duas páginas, exibir `error.message` quando o erro for 403.

### 🟡 Atenção R-03 — Guardas de Admin de listar/detalhar/voltar ao padrão e o módulo do handler de métricas sem teste (AC-18)
- **Onde:** `tests/ChamadosCamarj.UnitTests/Application/Acessos/AcessosCommandsTests.cs` (só `Salvar_QuemNaoEhAdmin_Forbidden` e `DefinirChatDeAcesso_QuemNaoEhAdmin_Forbidden`); `ObterMetricasQueryHandler` sem teste unitário
- **Problema:** AC-18 pede teste automatizado para cada regra de acesso (AC-05, AC-07, AC-12). `ListarAcessosQueryHandler`, `ObterAcessoUsuarioQueryHandler` e `VoltarAoPadraoCommandHandler` só são testados chamados como Admin; o handler de métricas do Dashboard não tem teste do `ModuloGuard` (só o de distribuição).
- **Cenário de falha:** uma refatoração remove o `PerfilRequisitanteGuard.ExigirAdmin` do `ListarAcessosQueryHandler` (ou o `ExigirAsync` do `ObterMetricasQueryHandler`) → um Atendente lista nome, e-mail, perfil e acessos de todos os usuários pelo `GET /api/acessos` (ou um Atendente sem o módulo volta a ver as métricas); os 520 testes continuam verdes.
- **Correção sugerida:** `[Theory]` com "Atendente"/"Solicitante" → `ForbiddenException` para Listar, Obter e VoltarAoPadrao; teste do `ObterMetricasQueryHandler` com `ModuloGuard` real (sem o módulo → 403; Solicitante com o módulo → passa o `ContextoAcesso` de Solicitante).

### 🟡 Atenção R-04 — Painel "Ajustar Chat" do Admin nasce do cache da lista e pode gravar um valor velho
- **Onde:** `frontend/src/features/admin/ControleAcessoPage.tsx:235` (`useState(pessoa.chatPerfil)` — dado da lista `['acessos']`, fresco por 30 s); `:274`
- **Problema:** o review-2 R-05 fez o painel geral esperar a leitura nova do detalhe antes de montar o formulário; o painel de Chat do Admin continua iniciando o seletor com o valor da lista, embora já busque o detalhe (para o histórico).
- **Cenário de falha:** Admin A abre o Controle de acesso; em seguida o Admin B dá "Pode criar grupos" ao Admin X. Dentro de 30 s, A abre "Ajustar Chat" de X: o seletor mostra "Sem acesso" (lista velha) enquanto o histórico logo abaixo já mostra a mudança de B; A clica em Salvar sem mexer → o Chat de X é revogado (com aviso aos participantes das conversas).
- **Correção sugerida:** iniciar o seletor com `detalhe.chatPerfil` e só habilitar Salvar depois de `isFetchedAfterMount`, como no `PainelAcessos`.

### 🟡 Atenção R-05 — Contas `teste.acesso.*` desativadas que não passaram pelo E2E novo ainda guardam a senha publicada
- **Onde:** histórico do git (`TesteAcesso#2026`, removida em `9973908`); `review-2.md` (tratamento do R-01: "5 contas desativadas")
- **Problema:** o E2E novo redefine a senha só das 2 contas que usa (`teste.acesso.e2e` e `teste.acesso.sol.e2e`, via reativação pelo `POST /usuarios`). As outras 3 contas `teste.acesso.*` (criadas na verificação ao vivo) foram só desativadas: o hash da senha publicada continua no cadastro. Ativar uma conta pela tela de Usuários (`PUT` com `ativo: true`) não troca a senha.
- **Cenário de falha:** um Admin, arrumando a lista de Usuários, reativa "Teste Acesso ..." pelo seletor Ativo → a conta volta a abrir com `TesteAcesso#2026`, senha que está no histórico público do repositório.
- **Correção sugerida:** com OK do usuário (banco real), redefinir a senha das 5 contas para um valor aleatório descartado (ou excluí-las) — não basta estarem desativadas.

### 🟡 Atenção R-06 — Mudança de comportamento do review-2 entrou junto com o código, não antes (regras 2 e 3)
- **Onde:** commit `0365ad3` (spec AC-15 "desconectada na hora" + design §14 + código no mesmo commit); commit `9973908` (`SalvarAcessosRequest.PerfilEsperado` e a resposta 409 — contrato novo — commitados antes do design §14, que só entrou em `0365ad3`)
- **Problema:** na rodada 1 a sessão fez certo (spec/design ajustados em `7256d4c`, código depois). Na rodada 2 a spec do AC-15 e o design §14 foram commitados com o código ou depois dele. Pela história do git não dá para provar que a spec foi editada antes; também não dá para provar o contrário.
- **Cenário de falha:** o registro do processo (regra 2: "spec atualizada antes do código"; regra 3: "contrato avisado antes de aplicar") fica sem evidência; quem audita a feature não consegue saber se a decisão de logout veio antes ou foi documentada depois de implementada.
- **Correção sugerida:** nas próximas rodadas, commitar spec/design ajustados num commit `docs(sdd)` antes do `fix`; registrar no `review-2.md` (tratamento) a data/hora da aprovação do usuário para R-03 e R-05.

## Constitution e contratos

- **Regra 1 (sem suposição silenciosa):** cumpre — decisões do Chat de Admin e do logout por mudança de perfil estão na spec §6/AC-15 como decisões do usuário.
- **Regra 2 (spec antes do código):** cumpre na rodada 1; sem evidência na rodada 2 (R-06).
- **Regra 3 (contrato avisado antes):** C1–C9 aprovados antes; `PUT /api/acessos/{id}/chat` e `ChatPerfil` obrigatório no design §13 antes do código; `PerfilEsperado`/409 e o campo `perfil` no evento `AcessosAtualizados` registrados no design §14 só depois do código de `9973908` (R-06). Todos internos à feature, sem consumidor externo.
- **Regra 4 (SDD):** cumpre — 3ª rodada de review independente.
- **Regra 5 (cross-feature / `/analise-cod`):** **não cumpre por completo.** O `impacto.md` declarou o `DefinirChatPerfilCommandHandler` e o `UsuarioPerfilRepository` cobertos, mas todos os testes desses caminhos usam Moq: a interação "duas gravações do mesmo usuário na mesma requisição" (já documentada como armadilha no próprio repositório) não foi tratada como consumidor e gerou o R-01. O boot do `AuthContext` (`/auth/me`) também não foi listado como consumidor da nova regra "perfil mudou → sair" (R-02).
- **Regra 6 (Obsidian):** pendente (gate desmarcado na spec) — esperado no fechamento; não é achado desta rodada.
- **Mudanças de contrato previstas × feitas:** batem com C1–C9 + §13/§14. `AcessosController` continua sem `[Produces("application/json")]` (`CONVENTIONS.md` §2.9, já apontado no review-2 como sugestão).
- **Pontos de toque cross-feature:** login, `/auth/me`, menu e rotas sem regressão para quem não tem ajuste (padrões por teste). Chat: auditoria a mais em todo caminho (teste). Usuários: Chat fora da tela; edição sem Chat não mexe no Chat (teste). Dashboard/Relatório: servidor coberto (métricas sem teste próprio — R-03).
- **Segredos no diff:** nenhuma senha, chave ou connection string no estado atual da branch; a senha antiga de teste continua no histórico (R-05). Senhas do E2E agora são aleatórias por rodada (`helpers.ts`).
- **Migration:** aditiva (2 colunas `int not null default 0` + tabela `AuditoriaAcessos`), já aplicada com OK do usuário — nada a fazer.

## Tratamento dos achados das rodadas anteriores (conferência)

| Achado | Tratamento resolve? | Observação |
|---|---|---|
| R1 R-01 (telas barravam Solicitante) | Sim | `temModulo` nas duas páginas; E2E com Solicitante |
| R1 R-02 (Chat de Admin) | Sim | `DefinirChatDeAcessoCommand` + "Ajustar Chat"; painel com valor inicial do cache (R-04 desta rodada) |
| R1 R-03 (reativação mantinha ajustes) | Sim | `CriarUsuarioPerfilCommandHandler` zera e audita |
| R1 R-04 (mensagens) | Sim | "retirado" × "sem permissão"; `RequerAdmin` com aviso |
| R1 R-05 (validação) | Sim | 400 sem `chatPerfil`/`modulos`; validador antes de gravar |
| R1 R-06 (auditoria de Chat por fora) | Sim, com efeito colateral | Auditoria no `DefinirChatPerfilCommandHandler`; o caminho combinado da tela nova quebra para quem tem Grupo (R-01) |
| R1 R-07 (token × cadastro) | Parcial | `ModuloGuard` recusa perfil divergente; pendência de Admin rebaixado no STATE; caso "reabriu depois" no R-02 |
| R1 R-08 / R2 R-06 (escopo do relatório) | Sim | `ObterRelatorioMensalAutorizadoQuery` testado; controller só repassa |
| R2 R-01 (senha no repositório) | Parcial | Código limpo; 3 contas desativadas ainda com a senha publicada (R-05) |
| R2 R-02 (histórico do Admin) | Sim | `HistoricoAcessos` no painel de Chat; E2E |
| R2 R-03 (estado misto após mudança de perfil) | Parcial | Resolvido para quem está conectado; não para quem reabre depois (R-02) |
| R2 R-04 (auditoria na reativação) | Sim | Linhas "Conta" e "Chat" |
| R2 R-05 (painel com dados velhos) | Sim no painel geral | `perfilEsperado` + 409 + leitura nova; painel de Chat do Admin ficou de fora (R-04) |

## Sugestões (não bloqueiam)

- `RequerModulo`: acrescentar `key={modulo}` nos cinco `<RequerModulo>` (`App.tsx:140-152`) para o `tinhaAoAbrir` não ser reaproveitado entre rotas de módulo.
- `ObterRelatorioMensalQueryHandler`: converter `IdsVisiveis` para `HashSet<Guid>` uma vez (hoje `List.Contains` em 8 listas de eventos).
- Auditoria "Perfil" grava "(ajustes zerados)" mesmo quando a pessoa não tinha ajuste — gravar o sufixo só se `TemAjusteDeModulos` era verdadeiro antes.
- E2E: `entrarComo` fica fora do `try`; se o login da conta de teste falhar, o `finally` não desativa a conta (a senha é aleatória, então o risco é baixo).
- `SalvarAcessosCommand.cs` reúne três comandos, dois handlers e o `AcessosGuard`; e os validadores estão em `Commands/` e não em `Validators/` — separar segue o padrão de pastas do resto da Application.
- `spec.md` §5 e o cabeçalho do `impacto.md` ainda citam "506 testes, E2E 19/19"; os números atuais são 520 e 22.
- `AcessosController`: `[Produces("application/json")]` como os demais controllers.

---

## Tratamento (sessão principal, 2026-10-06)

3ª rodada com bloqueante → levado ao usuário (limite do fluxo). Decisões: R-02 "pede login de novo", R-05
"apagar as contas agora", e uma **4ª rodada** de review depois das correções.

| Achado | Confirmado? | O que foi feito | Prova |
|---|---|---|---|
| R-01 🔴 | Sim — reproduzido com o `ApplicationDbContext` real (EF InMemory): `Grupo ... is already being tracked` | `AtualizarAsync` marca só o usuário como alterado (sem `Update()` no grafo). **Achado extra:** com o código antigo, trocar/tirar a Área em "Editar usuário" não era gravado (bug anterior a esta feature, também em produção) — a mesma correção resolve. | `UsuarioPerfilRepositoryTests` (3 testes; os 3 falham com o código antigo); E2E "Pessoa com Área: trocar a Área grava, e salvar módulo + Chat juntos funciona e audita" |
| R-02 🟡 | Sim | Boot do `AuthContext`: perfil do `/auth/me` ≠ perfil do token → sai com "Seu perfil foi alterado. Entre novamente."; tempo real também compara com o token. O E2E achou uma corrida (2ª resposta do `/auth/me` ressuscitava a sessão já encerrada) — resposta de sessão que acabou agora é ignorada. | E2E "Perfil mudado com a pessoa fora do sistema…" (16/16 em repetição) |
| R-03 🟡 | Sim | Testes de não-Admin → 403 para listar/detalhar/voltar ao padrão; `ObterMetricasQueryHandlerTests` com `ModuloGuard` real. | 528 testes |
| R-04 🟡 | Sim | `PainelChatAdmin` monta o formulário só com a leitura nova do cadastro. | build; E2E do painel |
| R-05 🟡 | Sim | Com OK do usuário: 5 contas, 4 chamados de teste, 4 históricos, 45 linhas de auditoria de acessos e 1 do chat apagados numa transação (sobra conferida: 0). | console no scratchpad |
| R-06 🟡 | Procede como processo | Decisão do usuário veio antes do código, mas spec e código saíram no mesmo commit. Nesta rodada a spec/design foram commitados antes (`dbbe8ab`). | git log |

Gates: `dotnet build` 0 erros · `dotnet test` **528/528** · `npm run build` ok · lint só avisos antigos · E2E **24/24**.
