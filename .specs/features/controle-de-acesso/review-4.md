# Review: controle-de-acesso — rodada 4

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-06
> **Escopo:** `git diff origin/develop...feature/controle-de-acesso` — 89 arquivos, +4593/-188 linhas (22 commits, `654b666..5dbdc17`)
> **Veredito:** APROVADO COM RESSALVAS

Verificação feita pela leitura do código, sem subir backend/frontend e sem acessar o banco. Rodados nesta revisão:
`dotnet test tests/ChamadosCamarj.UnitTests/` **528/528**, 0 falhas; `npm --prefix frontend run build` ok. E2E não
rodado (exige backend e banco real).

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim | `AcessosQueries.cs:20-34` (lista todos; Admin como acesso total); `ControleAcessoPage.tsx:38-41` (busca por nome/e-mail); E2E `controle-de-acesso.spec.ts:22` |
| AC-02 | Sim | `AcessosQueries.cs:55-66` (`Padrao`/`Efetivo`/`Ajustavel`); `ControleAcessoPage.tsx` `FormularioAcessos` ("padrão do perfil"/"ajustado") |
| AC-03 | Sim | `SalvarAcessosCommand.cs:47-58`; `Salvar_TirarModuloDeAtendente_GravaAjusteAuditaEAvisa`; E2E `:22`. O caminho "módulo + Chat de quem tem Área" (R-01 da rodada 3) agora grava: `UsuarioPerfilRepository.cs:51` + `UsuarioPerfilRepositoryTests.AtualizarAsync_DuasCopiasDoMesmoUsuarioComGrupoNaMesmaRequisicao_GravaAsDuas` (change tracker real) + E2E `:166` |
| AC-04 | Sim | `SalvarAcessosCommand.cs:99-114`; `VoltarAoPadrao_RemoveAjustesAuditaEAvisa`; E2E `:55-57` |
| AC-05 | Sim | Servidor: `PerfilRequisitanteGuard.ExigirAdmin` nos 5 handlers, agora testado em todos (`ListarDetalharEVoltarAoPadrao_QuemNaoEhAdmin_Forbidden`, `Salvar_QuemNaoEhAdmin_Forbidden`, `DefinirChatDeAcesso_QuemNaoEhAdmin_Forbidden`); tela: `RequerAdmin` (`App.tsx:122-128`); E2E `:64` |
| AC-06 | Sim | `AcessosGuard` (`SalvarAcessosCommand.cs:118-137`: alvo Admin e o próprio recusados); `DefinirChatDeAcessoCommand` (Chat de qualquer um); `PainelChatAdmin` agora nasce do cadastro lido de novo (`ControleAcessoPage.tsx:234-258`) |
| AC-07 | Sim | Dashboard: `ModuloGuard` + `ContextoAcesso` (`ObterMetricasQueryHandlerTests.Handle_SolicitanteComModuloDashboard_ContaSoNoEscopoDeSolicitante`, `ObterDistribuicaoQueryHandlerTests`); Relatório: `ObterRelatorioMensalAutorizadoQuery` + `RelatorioEscopo` (`IdsVisiveis`); E2E `:82` |
| AC-08 | Sim | `ModulosDeAcesso.cs:25-30`, `:36-43`; `ObterDetalhe_Solicitante_NaoMostraKanbanNemFila`; `Solicitante_NuncaTemKanbanNemFila_MesmoComAjusteGravadoErrado` |
| AC-09 | Sim | `SalvarAcessosCommand.cs:62-67` e `:171` despacham o `DefinirChatPerfilCommand` de sempre (avisos, `ChatHistoricos`, auditoria de acessos) |
| AC-10 | Sim | `UsuarioFormDialog.tsx` e `UsuariosPage.tsx` sem Chat; `AtualizarUsuarioPerfilCommand.ChatPerfil` opcional; `Handle_ChatPerfilNulo_NaoMexeNoChat` |
| AC-11 | Sim | `AcessoSignalRNotificationHandlers.cs`; `AppLayout.tsx:84-95`; `RequerModulo` (`App.tsx:109-119`); E2E `:41-45` |
| AC-12 | Sim | Tela: `RequerModulo`; servidor: `ModuloGuard` (Dashboard, Relatório), `ChatPerfilGuard` (Chat); E2E `:47-49`, `:94-96`; `ObterMetricasQueryHandlerTests`, `ObterRelatorioMensalAutorizadoQueryHandlerTests` |
| AC-13 | Sim | Mesmo evento; menu por `temModulo` (`AppLayout.tsx`); E2E `:57` |
| AC-14 | Sim | `AuditoriaAcesso` em Salvar, Voltar ao padrão, `DefinirChatPerfilCommandHandler`, mudança de perfil e reativação; `HistoricoAcessos` nos dois painéis; E2E `:166` confere "Relatório mensal" e "Chat" na auditoria de pessoa com Área |
| AC-15 | Parcial | Zera ajustes, audita e avisa (`AtualizarUsuarioPerfilCommandHandler.cs:57-94`); sistema aberto: `AppLayout.tsx:88-93` compara com o token; pessoa fora: boot do `AuthContext.tsx:103-117` (E2E `:143`). Fica de fora a aba aberta com o tempo real caído no momento da mudança — ver R-01 |
| AC-16 | Sim | `Padroes_SaoIguaisAoMenuDeAntesDaFeature`; migration com colunas `default 0` |
| AC-17 | Sim | Exceções de Application → `{ message }` pelo middleware; 400 do `AcessosController` em `{ message }` |
| AC-18 | Sim | 528 testes; regras de acesso de AC-05..AC-08, AC-12, AC-15 e AC-16 com teste (as lacunas do review-3 R-03 foram fechadas) |
| AC-19 | Sim | `npm run build` ok; E2E cobre ajustar módulo, menu mudando e tela bloqueada (registrado 24/24 no tratamento do review-3; não rodado aqui) |

## Tratamento da rodada 3 (conferência)

| Achado | Tratamento resolve? | Observação |
|---|---|---|
| R3 R-01 🔴 (500 em módulo + Chat de quem tem Área) | Sim | `Entry(usuario).State = Modified` só marca o usuário; o `Grupo` lido pelo `Include` não passa a ser rastreado, então a 2ª gravação não colide. Teste com o `ApplicationDbContext` real (EF InMemory) reproduz as duas gravações na mesma instância de contexto. O mesmo defeito latente em `AtualizarUsuarioPerfilCommandHandler` + `chatPerfil` também some (mesmo repositório). Efeito colateral fora do escopo: ver R-02 |
| R3 R-02 🟡 (perfil mudado com a pessoa fora) | Sim, para "aba fechada" | Boot compara `/auth/me` com o claim `perfil` do token e sai com o aviso; resposta de sessão já encerrada é ignorada (`AuthContext.tsx:109`). Não cobre a aba aberta com a conexão em tempo real caída na hora da mudança (R-01). As páginas de Dashboard/Relatório continuam mostrando "Serviço indisponível" num 403 (sugestão do R3 não aplicada, sem efeito prático depois do boot) |
| R3 R-03 🟡 (testes de guarda) | Sim | `[Theory]` Atendente/Solicitante para listar/detalhar/voltar; `ObterMetricasQueryHandlerTests` com `ModuloGuard` real. Os testes realmente falhariam sem as guardas (o mock de `ListarAsync` devolve `null` e `Verify(..., Never)`) |
| R3 R-04 🟡 (painel de Chat do Admin com cache) | Sim | `FormularioChatAdmin` só monta com `isFetchedAfterMount` e usa `detalhe.chatPerfil` |
| R3 R-05 🟡 (contas com senha publicada) | Sim (pelo registro) | Contas apagadas com OK do usuário; não conferido no banco (proibido nesta revisão). O texto `TesteAcesso#2026` continua no `review-2.md` versionado, mas não abre mais nada |
| R3 R-06 🟡 (ordem spec × código) | Parcial | Ver sugestão sobre o commit `dbbe8ab` |

## Achados

### 🟡 Atenção R-01 — Quem está com a aba aberta mas sem tempo real no instante da mudança de perfil não é desconectado
- **Onde:** `frontend/src/hooks/useSignalR.tsx:93` (`onreconnected` só marca conectado); `frontend/src/auth/AuthContext.tsx:102-123` (a comparação perfil do cadastro × token roda só no boot); `frontend/src/layouts/AppLayout.tsx:84-93` (só no evento)
- **Problema:** o AC-15 cobre dois casos — sistema aberto (evento `AcessosAtualizados`) e pessoa fora (boot). O evento não é reenviado depois de uma reconexão, e o `/auth/me` não é refeito ao reconectar. Quem estava com a tela aberta e a conexão em tempo real caída (Wi-Fi oscilou, VPN reconectou) no momento da mudança continua com o perfil antigo na tela **e** no token até recarregar a página, sair, ficar 20 min parado (inatividade) ou o token vencer (10 h). Os outros caminhos de servidor que não passam pelo `ModuloGuard` aceitam o perfil do token.
- **Cenário de falha:** Admin rebaixa a Atendente Maria a Solicitante às 10:00:05; o notebook dela trocou de rede às 10:00:00 e o SignalR reconectou às 10:00:20. Maria segue trabalhando: o menu ainda mostra Kanban e Fila, e "Assumir"/mover cartão funcionam no servidor com o token de Atendente até ela recarregar ou o token vencer. O AC-15 diz "se a pessoa estiver com o sistema aberto, ela é desconectada na hora".
- **Correção sugerida:** no `onreconnected` (e no `onclose` seguido de nova conexão) refazer o `/auth/me` e aplicar a mesma regra do boot — extrair a comparação do `AuthContext` para uma função reutilizável (`revalidarSessao()`), chamada no boot e na reconexão. Na mesma linha, a pendência de segurança do STATE ("Admin rebaixado mantém direitos até o token vencer") vale também para Atendente → Solicitante; vale ampliar o texto.

### 🟡 Atenção R-02 — A correção do repositório passou a gravar a troca de Área, mas a sessão aberta continua com a Área antiga no token; impacto.md não listou esse consumidor nem há registro de aviso ao usuário
- **Onde:** `src/ChamadosCamarj.Infrastructure/Repositories/UsuarioPerfilRepository.cs:51`; `src/ChamadosCamarj.Application/Common/JwtTokenService.cs:33` (claim `grupo_id`); `src/ChamadosCamarj.WebApi/Services/CurrentUserService.cs` (`GrupoId` lido do token) → `ContextoAcesso` → `ChamadoRepository.AplicarVisibilidade` (`:186-215`); `.specs/features/controle-de-acesso/impacto.md` (re-análise após o review-3)
- **Problema:** antes, trocar/tirar a Área em "Editar usuário" não era gravado (bug antigo). Com o `Entry(...).State = Modified`, passa a ser gravado — mudança de comportamento da feature Usuários/autorizacao-chamados feita dentro desta branch. A visibilidade dos chamados usa a Área do **token** (`grupo_id`), não a do cadastro, e nada desconecta nem avisa a pessoa quando só a Área muda. O `impacto.md` conferiu os 7 handlers que chamam `AtualizarAsync`, mas não o consumidor do dado agora gravado (o `ContextoAcesso`/`AplicarVisibilidade` e o `ListarAtendentesQuePodemVerAsync`, que usa a Área do cadastro). A própria análise diz "Avisar o usuário", mas o STATE/ONDE PARAMOS e os avisos de deploy não registram isso (regra 5: tocar em código compartilhado de outra feature é avisado antes).
- **Cenário de falha:** depois do deploy, o Admin tira a Atendente Ana da Área "Faturamento" (ela mudou de setor). O cadastro grava "sem Área", o Admin vê a lista atualizada e entende que valeu. Ana, logada desde cedo, continua vendo no Kanban/Fila/Dashboard todos os chamados com Área = Faturamento até sair e entrar de novo (token de 10 h); ao mesmo tempo, os alertas de SLA desses chamados já não chegam para ela (`ListarAtendentesQuePodemVerAsync` usa o cadastro). Estado misto, sem aviso a ninguém.
- **Correção sugerida:** (1) registrar no STATE/avisos de deploy que mudar a Área passa a valer, mas só no próximo login da pessoa, e levar ao usuário a decisão de manter assim ou tratar como o perfil (desconectar quando `grupoId` muda: incluir `grupoId` no `AcessosAtualizados`/`/auth/me` e comparar com o claim `grupo_id`); (2) acrescentar este consumidor ao `impacto.md`; (3) atualizar a nota do Obsidian de Usuários/Áreas (regra 6) no fechamento.

## Constitution e contratos

- **Regra 1 (sem suposição silenciosa):** cumpre — R-02 e R-05 da rodada 3 têm decisão do usuário registrada.
- **Regra 2 (spec antes do código):** cumpre no conteúdo (a decisão do AC-15 "pessoa fora" é do usuário e está na spec), mas o commit `dbbe8ab` (spec/design) foi feito 19 s antes do `5dbdc17` e já descreve resultados da implementação ("corrida achada no E2E", dados apagados) — ver sugestão.
- **Regra 3 (contrato avisado antes):** nada de contrato novo nesta rodada (`perfilDoToken` é interno ao frontend; pacote `EntityFrameworkCore.InMemory` só no projeto de testes).
- **Regra 4 (SDD):** cumpre — 4ª rodada independente, a pedido do usuário.
- **Regra 5 (cross-feature / `/analise-cod`):** parcial — o `UsuarioPerfilRepository` (compartilhado por 7 handlers de 4 features) foi alterado com efeito fora do escopo (Área passa a ser gravada); a análise listou os chamadores, mas não o consumidor do dado (token `grupo_id`), e não há registro de aviso ao usuário (R-02).
- **Regra 6 (Obsidian):** pendente, esperado no fechamento; incluir o efeito sobre a Área (R-02).
- **Mudanças de contrato previstas × feitas:** batem com C1–C9 + §13/§14/§15. `AcessosController` continua sem `[Produces("application/json")]`.
- **Pontos de toque cross-feature:** login/`/auth/me`/menu/rotas sem regressão para quem não tem ajuste (testes de padrão); Chat com auditoria em todo caminho (teste); repositório de usuários agora testado com change tracker real (3 testes); Dashboard/Relatório com `ModuloGuard` real nos testes.
- **Segredos no diff:** nenhuma senha, chave ou connection string no código da branch; E2E com senha aleatória por rodada (`helpers.ts`). O `review-2.md` versionado ainda cita a senha antiga de teste, cujas contas foram apagadas.
- **Migration:** aditiva, já aplicada com OK do usuário — nada novo nesta rodada.

## Sugestões (não bloqueiam)

- `SalvarAcessosCommandHandler` grava módulos, depois Chat, e só então a auditoria dos módulos e o aviso (`SalvarAcessosCommand.cs:56-75`): uma falha no meio (ex.: erro de banco ao gravar a mensagem de sistema do Chat) deixa os módulos mudados sem linha de auditoria nem aviso. Gravar a auditoria dos módulos logo após o `AtualizarAsync`, ou usar o `IUnitOfWork`/transação, fecha a janela.
- `UsuarioPerfilRepository.AtualizarAsync`: o `Detach` não roda se o `SaveChangesAsync` lançar (ex.: `grupoId` inexistente) — `try/finally` evita deixar o usuário preso no tracker para o resto da requisição.
- `PUT /usuarios/{id}` devolve `grupoId` novo com `grupoNome` da Área antiga (o `Grupo` carregado pelo `Include` não é atualizado). A tela ignora a resposta hoje; outro consumidor pode estranhar.
- `PUT /usuarios/{id}` sem `grupoId` agora **tira** a Área (antes era ignorado para quem tinha Área). As duas telas mandam o campo; o helper do E2E (`desativarContaDeTeste`) não manda — inofensivo para contas de teste, mas vale documentar no contrato.
- `RequerModulo`: `key={modulo}` nos cinco `<RequerModulo>` (`App.tsx:140-152`) — sem isso o `tinhaAoAbrir` pode ser reaproveitado entre rotas de módulo (ex.: botão Voltar), trocando "sem permissão" por "retirado". Sugestão já feita na rodada 3.
- E2E "Pessoa com Área": cada rodada grava no banco real uma entrada de `ChatHistoricos` ("acesso concedido" ao Teste Acesso E2E), visível na auditoria do Chat, e coloca a conta de teste por instantes na primeira Área real. Considerar limpar ou usar uma Área de teste.
- Processo (regra 2): commitar spec/design ajustados quando a decisão é tomada, antes de começar o código — o `dbbe8ab` já traz fatos que só a implementação revelou (a corrida no E2E), o que mostra que foi escrito depois.
- `spec.md` §5 e o cabeçalho do `impacto.md` ainda citam "506 testes, E2E 19/19" (hoje 528 e 24); cabeçalho da spec com "Atualizada em: 2026-10-02".
- `ObterRelatorioMensalQueryHandler`: converter `IdsVisiveis` para `HashSet<Guid>` uma vez.
- Dashboard/Relatório: num 403, mostrar `error.message` (ex.: "Seu perfil mudou. Entre novamente para continuar.") em vez de "Serviço indisponível".

---

## Tratamento (sessão principal, 2026-10-06)

| Achado | Decisão | O que foi feito | Prova |
|---|---|---|---|
| R-01 🟡 | corrigir (mesma regra já decidida) | `revalidarSessao()` no evento `online` e na reconexão do SignalR. No teste, a simulação de offline do navegador **não derruba** o WebSocket já aberto — então o gatilho que o E2E prova é o `online`; o da reconexão fica sem E2E (só leitura de código). | E2E "Perfil mudado durante uma queda de conexão…" (repetido 4×) |
| R-02 🟡 | usuário: "vale no próximo login" | Pendência no STATE + aviso de deploy (o bug antigo de Área não gravada também vai no aviso). | — |
| Sugestão "PUT /usuarios sem grupoId tira a Área" | conferido | As duas telas que chamam (`UsuariosPage.alternarAtivo` e `UsuarioFormDialog`) sempre mandam `grupoId`. Só chamada direta à API seria afetada. | leitura de código |

Gates: `dotnet test` 528/528 · `npm run build` ok · lint só avisos antigos · E2E **25/25**.
