# Review: controle-de-acesso — rodada 1

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-05
> **Escopo:** `git diff origin/develop...feature/controle-de-acesso` — 64 arquivos, +3131/-163 linhas (12 commits, `654b666..bffe058`)
> **Veredito:** BLOQUEADO

Verificação feita pela leitura do código (backend e frontend não foram subidos, por falta de memória
na máquina). `dotnet test tests/ChamadosCamarj.UnitTests/` rodado nesta revisão: **506/506**, 0 falhas.

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim | `AcessosQueries.cs` (`ListarAcessosQueryHandler`); `ControleAcessoPage.tsx` (busca por nome/e-mail); E2E `controle-de-acesso.spec.ts` |
| AC-02 | Sim | `ObterAcessoUsuarioQueryHandler` (`Padrao`/`Efetivo`/`Ajustavel`); `ControleAcessoPage.tsx` (etiqueta "padrão do perfil"/"ajustado") |
| AC-03 | Sim | `SalvarAcessosCommand.cs:43-53`; `Salvar_TirarModuloDeAtendente_GravaAjusteAuditaEAvisa`; E2E |
| AC-04 | Sim | `VoltarAoPadraoCommandHandler`; `VoltarAoPadrao_RemoveAjustesAuditaEAvisa`; E2E |
| AC-05 | Parcial | Servidor: `PerfilRequisitanteGuard.ExigirAdmin` nos 4 handlers (`Salvar_QuemNaoEhAdmin_Forbidden`). Tela: `RequerAdmin` (`App.tsx:116-122`) só redireciona, **sem** "sem permissão" — ver R-04 |
| AC-06 | Sim (com regressão) | `SalvarAcessosCommand.cs:121,127`; `ControleAcessoPage.tsx:104`. A trava também tirou o ajuste de **Chat** de Admins — ver R-02 |
| AC-07 | **Não (na tela)** | Servidor ok: `ModuloGuard` + `AplicarVisibilidade` (Dashboard) e `IdsVisiveis` (Relatório). Tela: `DashboardPage.tsx:47` e `RelatorioMensalPage.tsx:37-45` continuam bloqueando todo Solicitante — ver R-01 |
| AC-08 | Sim | `ModulosDeAcesso.Ajustaveis`/`Efetivos`; `ObterDetalhe_Solicitante_NaoMostraKanbanNemFila`; `Solicitante_NuncaTemKanbanNemFila_MesmoComAjusteGravadoErrado` |
| AC-09 | Sim (só para não-Admin) | `SalvarAcessosCommand.cs:57-62` delega a `DefinirChatPerfilCommand`; `Salvar_MudarChat_UsaOComandoDeChatExistente_EAudita`. Para Admin, não há mais onde ajustar — R-02 |
| AC-10 | Sim | `UsuarioFormDialog.tsx`, `UsuariosPage.tsx` sem Chat; `AtualizarUsuarioPerfilCommand.ChatPerfil` opcional; `Handle_ChatPerfilNulo_NaoMexeNoChat`. API antiga de Chat continua aberta — R-06 |
| AC-11 | Sim | `RequerModulo` (`App.tsx:107-113`) + `AppLayout.tsx:78` (`AcessosAtualizados`); E2E "some do menu na hora" |
| AC-12 | Parcial | Servidor: Dashboard (2 handlers) e Relatório (`RelatoriosController.cs:45`); Chat inalterado. Tela: rota bloqueada, mas a mensagem é "Seu acesso a este módulo foi retirado." mesmo para quem nunca teve — R-04 |
| AC-13 | Sim | Mesmo evento; E2E (volta ao padrão faz o item reaparecer) |
| AC-14 | Sim (pela tela nova) | `AcessosTexto.DiferencasDeModulos`, `AuditoriaAcesso`, painel "Histórico de mudanças". Mudanças de Chat pela API antiga não entram — R-06 |
| AC-15 | Sim (pela edição) | `AtualizarUsuarioPerfilCommandHandler.cs:61-63,85-94`; `Handle_QuandoPerfilMuda_*`. Caminho de reativação não zera — R-03 |
| AC-16 | Sim | Colunas `default 0`; `Padroes_SaoIguaisAoMenuDeAntesDaFeature`; `modulosPadrao` no front para sessão antiga |
| AC-17 | Sim | Exceções do projeto (`BadRequest`/`Forbidden`/`NotFound`) com mensagens em português |
| AC-18 | Parcial | 506/506. Falta teste do ramo Solicitante do `RelatoriosController` (preenchimento de `IdsVisiveis`) — R-08 |
| AC-19 | Sim | E2E `controle-de-acesso.spec.ts` (ajustar, menu mudando, tela bloqueada); `npm run build` declarado no `impacto.md` (não rodado nesta revisão) |

## Achados

### 🔴 Bloqueante R-01 — Solicitante com Dashboard ou Relatório mensal recebe "não está disponível para o seu perfil" (AC-07 não atendido na tela)
- **Onde:** `frontend/src/features/dashboard/DashboardPage.tsx:47-57`; `frontend/src/features/relatorio-mensal/RelatorioMensalPage.tsx:37-45` (arquivos **não tocados** pelo diff)
- **Problema:** o menu e a rota passaram a seguir o módulo (`temModulo`, `RequerModulo`), mas as duas páginas ainda têm a trava antiga por perfil: `if (perfil?.tipo === 'Solicitante') return <Alert>Esta área não está disponível para o seu perfil.</Alert>`. O Relatório ainda desliga a consulta (`useRelatorioMensal(..., !isSolicitante)`). O servidor foi corrigido; a tela não.
- **Cenário de falha:** Admin liga o Dashboard (ou o Relatório mensal) para um Solicitante → o item aparece no menu dele (AC-13 ok) → ao clicar, `RequerModulo` deixa passar e a página mostra "Esta área não está disponível para o seu perfil." / "Este relatório não está disponível para o seu perfil.". A US-02 inteira (o motivo de existir o "dar") não funciona para o usuário final. A verificação "ao vivo" e o `impacto.md` não pegaram porque testaram a API, e o `impacto.md` não listou as duas páginas como consumidoras.
- **Correção sugerida:** trocar a trava das duas páginas por `temModulo(perfil, 'Dashboard' | 'RelatorioMensal')` (ou removê-la, já que `RequerModulo` cobre) e, no Relatório, habilitar a consulta para o Solicitante sem `responsavelId`. Acrescentar no E2E um Solicitante de teste com Dashboard ligado abrindo a tela e vendo números.

### 🔴 Bloqueante R-02 — Não há mais como ajustar o Chat de um Admin (regressão do chat-corporativo)
- **Onde:** `src/ChamadosCamarj.Application/Features/Acessos/Commands/SalvarAcessosCommand.cs:121-128` (`AcessosGuard` recusa alvo Admin e o próprio Admin para **tudo**, inclusive o Chat); `frontend/src/features/admin/ControleAcessoPage.tsx:104` (sem botão para Admin); `UsuariosPage.tsx`/`UsuarioFormDialog.tsx` (seletor de Chat removido)
- **Problema:** antes, a lista de Usuários tinha o `ChatPerfilSelect` em **todas** as linhas, inclusive Admins (e o próprio Admin). O Chat de Admin **não** é automático: `ChatPerfilGuard.ExigirAcesso` só olha o `ChatPerfil`, sem exceção para Admin. A spec diz que Chat é "idem" para todos os perfis (tabela §1 e §4) e que o efeito do ajuste é "o mesmo de hoje" (AC-09); "Admin não ajustável" (AC-06) trata de **módulos**. Agora o único lugar de ajuste do Chat recusa Admins.
- **Cenário de falha:** Admin cria (ou promove) um novo Admin; o Chat dele nasce "Sem acesso" → nenhuma tela permite liberar → ele não usa o Chat. Idem para o próprio Admin que quer criar grupos. O único contorno é rebaixar a Atendente, ajustar e promover de novo (o que ainda gera auditoria de "Perfil" e notificações espúrias) ou chamar `PATCH /api/usuarios/{id}/chat-perfil` à mão.
- **Correção sugerida:** decidir com o usuário (regra 1 — é comportamento de produto): (a) permitir no painel só o Chat para alvo Admin (inclusive o próprio), mantendo os módulos travados; ou (b) Admin com Chat automático. Até decidir, a recusa para Admin no `SalvarAcessosCommand` deve valer só para módulos.

### 🟡 Atenção R-03 — Reativar uma conta pelo "Novo usuário" mantém os ajustes de módulos antigos (inclusive com troca de perfil)
- **Onde:** `src/ChamadosCamarj.Application/Features/Usuarios/Commands/CriarUsuarioPerfilCommandHandler.cs:31-43` (não tocado pelo diff; não listado no `impacto.md`)
- **Problema:** criar usuário com o e-mail de uma conta desativada reaproveita o registro (`Atualizar` + `Ativar`) e zera o Chat (`DefinirChatPerfil(request.ChatPerfil)`), mas **não** chama `VoltarAoPadraoDeModulos()`. Os ajustes antigos ficam valendo, sem aparecer para o Admin que "criou" a conta, e sem a regra do AC-15 quando o perfil muda nesse caminho.
- **Cenário de falha:** Solicitante com Dashboard e Relatório concedidos é desativado; meses depois o Admin cria de novo a conta (mesmo e-mail) como Solicitante → a pessoa volta já com Dashboard e Relatório, sem ninguém ter dado. Ou: Atendente com Arquivo retirado é desativado e recriado como Solicitante → fica sem Arquivo, que é padrão do novo perfil (contraria o espírito do AC-15).
- **Correção sugerida:** no ramo de reativação, chamar `existente.VoltarAoPadraoDeModulos()` (e auditar se havia ajuste); teste no `CriarUsuarioPerfil*Tests`.

### 🟡 Atenção R-04 — Mensagens de recusa na tela diferentes da spec (AC-05 e AC-12)
- **Onde:** `frontend/src/App.tsx:107-122`
- **Problema:** AC-05 pede que quem não é Admin, ao abrir "Controle de acesso" pelo endereço, "receba 'sem permissão'"; `RequerAdmin` só redireciona para `/chamados`, sem aviso. AC-12 pede "sem permissão" ao abrir a tela de um módulo pelo endereço; `RequerModulo` mostra sempre "Seu acesso a este módulo foi retirado." — texto do AC-11, que só faz sentido para quem perdeu o módulo com a tela aberta.
- **Cenário de falha:** Solicitante (que nunca teve Kanban) digita `/atendimento/kanban` → vê "Seu acesso a este módulo foi retirado." (informação falsa). Atendente digita `/admin/acessos` → cai em "Meus chamados" sem explicação. O aviso fica no `location.state` e reaparece ao recarregar a página.
- **Correção sugerida:** distinguir os dois casos (ex.: aviso de "retirado" só quando o módulo existia no perfil anterior em memória; "Você não tem permissão para acessar esta tela." no acesso direto e no `RequerAdmin`), ou registrar na spec que o texto aprovado é outro.

### 🟡 Atenção R-05 — `PUT /api/acessos/{id}` sem validação: `chatPerfil` ausente revoga o Chat e valor inválido grava os módulos pela metade
- **Onde:** `src/ChamadosCamarj.WebApi/Controllers/AcessosController.cs:51,67`; `SalvarAcessosCommand.cs:43-62` (sem `AbstractValidator<SalvarAcessosCommand>`)
- **Problema:** `SalvarAcessosRequest.ChatPerfil` não é anulável; corpo sem o campo desserializa como `SemAcesso`. Um valor numérico fora do enum (o `JsonStringEnumConverter` aceita inteiros) passa pelo controller, os módulos são salvos (`AtualizarAsync`, linha 52) e só depois o `DefinirChatPerfilCommandValidator` recusa — sem auditoria nem aviso dos módulos já gravados.
- **Cenário de falha:** um script/integração do Admin manda `{ "modulos": ["Arquivo"] }` para tirar módulos → o Chat da pessoa é revogado em silêncio (com aviso aos participantes). Ou `{ "modulos": [...], "chatPerfil": 7 }` → 400, mas os módulos mudaram sem linha em `AuditoriaAcessos` (AC-14) e sem `AcessosAtualizados` (AC-11).
- **Correção sugerida:** validador do `SalvarAcessosCommand` (`ChatPerfil` `IsInEnum`, `Modulos` não nulo) — roda antes do handler pelo `ValidationBehaviour`; considerar `ChatPerfil?` no request (null = não mexe), como foi feito no `AtualizarUsuarioPerfilCommand`.

### 🟡 Atenção R-06 — Mudanças de Chat por fora da tela nova não entram na auditoria de acessos (AC-14) e o "um lugar só" (AC-10) vale só na tela
- **Onde:** `src/ChamadosCamarj.WebApi/Controllers/UsuariosController.cs:100-115` (`PATCH /api/usuarios/{id}/chat-perfil`, mantido); `AtualizarUsuarioPerfilCommandHandler.cs` (ainda aceita `chatPerfil`)
- **Problema:** a auditoria nova (`AuditoriaAcessos`) só é gravada pelo `SalvarAcessosCommand`. Os dois caminhos antigos continuam mudando o Chat (gravam só `ChatHistoricos`) e não publicam `AcessosAtualizados`.
- **Cenário de falha:** o Chat de alguém é alterado via `PATCH /usuarios/{id}/chat-perfil` (hoje o único jeito de mexer no Chat de um Admin — R-02) → o painel "Histórico de mudanças" da pessoa não mostra nada; o Admin que audita pela tela conclui que não houve mudança. AC-14 diz "qualquer mudança de acesso (módulo ou Chat)".
- **Correção sugerida:** gravar a linha de `AuditoriaAcesso` dentro do `DefinirChatPerfilCommandHandler` (ponto único, como o comentário do `AtualizarUsuarioPerfilCommandHandler` já defende) e tirar a gravação duplicada do `SalvarAcessosCommand`; ou remover/descontinuar o `PATCH` antigo (mudança de contrato — avisar antes, regra 3).

### 🟡 Atenção R-07 — Token antigo × cadastro: após mudança de perfil, a tela e parte do servidor continuam no perfil antigo
- **Onde:** `AcessosAtualizadosNotification` (sem o perfil); `frontend/src/lib/modulos.ts:14`; `AppLayout.tsx:78`; `ObterMetricasQueryHandler`/`ObterDistribuicaoQueryHandler`/`RelatoriosController.cs:48-55` (escopo pelo `ContextoAcesso` do token, guarda pelo cadastro)
- **Problema:** o `ModuloGuard` lê o cadastro (bom), mas o escopo dos números vem do perfil do **token**, e o evento `AcessosAtualizados` não leva o novo perfil, então `perfil.tipo` no navegador não muda até novo login/`/auth/me`.
- **Cenário de falha:** (1) Atendente é rebaixado a Solicitante e o Admin, em seguida, lhe dá o Dashboard → o guarda passa (cadastro: Solicitante com Dashboard), mas os números saem com a visibilidade de **Atendente** do token (inclui chamados sem responsável de todo o sistema), contrariando o AC-07 "nunca os do sistema todo" enquanto o token durar (`TokenExpiracaoHoras`). (2) Admin rebaixado a Atendente recebe `AcessosAtualizados`, mas `temModulo` devolve `true` para `tipo === 'Admin'` e o menu de Administração continua; `/api/acessos` segue aceitando pelo perfil do token. O item (2) é herdado do padrão já existente em Usuários, não é escalada nova.
- **Correção sugerida:** incluir o perfil no evento e atualizar `perfil.tipo` no `atualizarAcessos`; no Dashboard/Relatório, montar o `ContextoAcesso` com o perfil do cadastro já carregado pelo `ModuloGuard` (ex.: `ExigirAsync` devolver o `UsuarioPerfil`). Registrar no STATE como risco conhecido o resto (token com perfil antigo).

### 🟡 Atenção R-08 — Escopo do Relatório do Solicitante sem teste automatizado e com repositório no controller
- **Onde:** `src/ChamadosCamarj.WebApi/Controllers/RelatoriosController.cs:45-57`
- **Problema:** a regra "Solicitante vê só os chamados que ele vê" (AC-07) vive no controller, que chama `IChamadoRepository` direto (a arquitetura do projeto é Controller → MediatR → Handler → Repositório) e não tem teste. O teste `Handle_ComIdsVisiveis_*` só prova o filtro **quando** `IdsVisiveis` chega preenchido; nada garante que o controller o preencha. AC-18 pede teste automatizado para AC-07/AC-12.
- **Cenário de falha:** uma refatoração do controller (ex.: trocar o `else if` por outra checagem de perfil) deixa `idsVisiveis = null` para o Solicitante → ele passa a ver os números do sistema inteiro; os 506 testes continuam verdes.
- **Correção sugerida:** levar o escopo para o handler (a query recebe o `ContextoAcesso`/usa `ICurrentUserService` e `ModuloGuard`, como o Dashboard) e cobrir Admin/Atendente/Solicitante com teste de handler.

## Constitution e contratos

- **Regra 1 (sem suposição silenciosa):** cumpre no que está na spec; R-02 exige decisão do usuário (Chat de Admin) antes da correção.
- **Regra 2 (spec antes do código):** cumpre — spec aprovada e design/tasks commitados antes do primeiro `feat`.
- **Regra 3 (contrato avisado antes):** cumpre — C1–C9 aprovados (tasks.md); remoção do `ChatPerfilSelect` registrada no `impacto.md` como "não listado no design", sem consumidor externo.
- **Regra 4 (SDD):** cumpre — esta é a fase de review independente.
- **Regra 5 (cross-feature / `/analise-cod`):** **não cumpre por completo.** O `impacto.md` declarou "IMPACTO COBERTO", mas deixou passar consumidores fora do diff: `DashboardPage.tsx` e `RelatorioMensalPage.tsx` (R-01 — classificados como cobertos por E2E que só usa Admin e por verificação via API), `CriarUsuarioPerfilCommandHandler` (R-03) e o Chat de Admins na tela de Usuários (R-02; o item "`ChatPerfilSelect` removido — único consumidor era a lista de Usuários" não considerou que essa lista era a única tela que ajustava o Chat de Admins).
- **Regra 6 (Obsidian):** ainda não feita (gate desmarcado na spec) — esperado na fase de fechamento; não é achado desta rodada.
- **Mudanças de contrato previstas × feitas:** batem com C1–C9. Divergência menor: o design (§4) previa a checagem de Admin "no controller **e** no handler"; está só no handler (igual ao `UsuariosController`, sem brecha — o handler sempre roda).
- **Pontos de toque cross-feature:** `AppLayout`/`App.tsx`/login/`/auth/me` sem regressão para quem não tem ajuste (padrões conferidos por teste). Dashboard/Relatório: servidor coberto; telas com regressão funcional para o caso novo (R-01). Tela de Usuários/Chat: regressão para Admins (R-02).
- **Migration `AddControleDeAcesso`:** aditiva (2 `AddColumn int not null default 0` + `CreateTable`), compatível com a versão em produção; já aplicada com OK do usuário — nada a fazer.

## Sugestões (não bloqueiam)

- `ObterRelatorioMensalQueryHandler`: `visiveis.Contains` sobre uma `List` em 8 consultas (mês, anterior, 6 meses de evolução) — converter para `HashSet<Guid>` uma vez.
- `AppLayout`: o aviso de módulo retirado não tem botão de fechar e sobrevive a recarregar a página (está no `history.state`); limpar o state depois de exibir.
- Eventos perdidos durante uma queda da conexão SignalR (ex.: notebook suspenso) só se refletem no próximo `/auth/me`; considerar refazer o `/auth/me` no `onreconnected` (vale também para o `ChatPerfilAtualizado`).
- `SalvarAcessosCommand`: módulos e Chat são gravados em `SaveChanges` separados, sem transação; uma falha no meio deixa estado parcial sem auditoria. Baixa probabilidade, mas o `IUnitOfWork` já existe no projeto.
- A listagem do Controle de acesso exibe "Admin — acesso total" também para o próprio Admin; um rótulo "(você)" ajudaria a entender por que não há botão.
- E2E de AC-05 cobre só Atendente; acrescentar um Solicitante é barato.

---

## Tratamento dos achados (sessão principal, 2026-10-05)

| Achado | Confirmado? | Tratamento |
|---|---|---|
| R-01 🔴 | Sim | **Corrigido:** `DashboardPage` e `RelatorioMensalPage` usam `temModulo`. E2E novo: Solicitante com Dashboard abre a tela. |
| R-02 🔴 | Sim | **Decisão do usuário (2026-10-05): Chat de Admin ajustável na tela nova.** `PUT /api/acessos/{id}/chat` (`DefinirChatDeAcessoCommand`) para qualquer pessoa, inclusive Admin e o próprio; "Ajustar Chat" na linha do Admin. Spec (AC-06) e design (§13) atualizados antes do código. |
| R-03 🟡 | Sim | **Corrigido:** reativar conta pelo "Novo usuário" volta ao padrão de módulos + teste. |
| R-04 🟡 | Sim | **Corrigido:** "retirado" só para quem tinha o módulo com a tela aberta; senão "Você não tem permissão para acessar este módulo."; `RequerAdmin` mostra "Você não tem permissão para acessar esta área." (E2E). |
| R-05 🟡 | Sim | **Corrigido:** sem `chatPerfil` (ou módulos) → 400; validador do comando recusa Chat inválido antes de gravar. |
| R-06 🟡 | Sim | **Corrigido:** auditoria "Chat" dentro do `DefinirChatPerfilCommandHandler` — todo caminho (tela nova, edição de usuário, rota antiga) é auditado + teste. |
| R-07 🟡 | Sim | **Corrigido (parte nova):** `ModuloGuard` recusa quando o perfil do cadastro difere do token ("Seu perfil mudou. Entre novamente para continuar.") + teste. **Pendência registrada (pré-existente, fora do escopo):** Admin rebaixado mantém os direitos de Admin do token até ele vencer — as rotas de Admin checam o perfil do token. |
| R-08 🟡 | Sim | **Corrigido:** escopo extraído para `RelatorioEscopo.ResolverAsync` com 3 testes (Admin, Atendente, Solicitante). |
| Constitution regra 5 | Sim | O `impacto.md` não pegou R-01..R-03. Re-análise registrada nele; skill `/analise-cod` melhorada (procurar as checagens da regra antiga e caminhos de criação/reativação). |

Verificação após as correções: `dotnet test` **515/515**; E2E **20/20**; ao vivo **22/22**.
