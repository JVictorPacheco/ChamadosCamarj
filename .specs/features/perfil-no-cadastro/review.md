# Review: perfil-no-cadastro — rodada 1

> **Revisor:** sub-agente independente (Claude Code — Sonnet 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-09
> **Escopo:** `git diff origin/develop...feature/perfil-no-cadastro` — 22 arquivos, +1160/-17 linhas (11 de código de produção, 6 de teste, 4 de spec/design/tasks/impacto + 1 de contrato)
> **Veredito:** APROVADO COM RESSALVAS

`dotnet test` rodado na branch: 573/573 verdes. Nenhum bloqueante encontrado; 4 pontos de atenção.

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim | `Program.cs` (OnTokenValidated, ~l.166) + `CadastroClaimsValidator.cs:27-43` troca `perfil`; testes `CadastroClaimsValidatorTests.PerfilRebaixado_*`, `PerfilNoCadastroNasRegrasDeAcessoTests` |
| AC-02 | Sim | idem; `PerfilNoCadastroNasRegrasDeAcessoTests.AtendenteRebaixado*` |
| AC-03 | Sim | `CadastroClaimsValidatorTests.PerfilPromovido_*` |
| AC-04 | Sim (por construção) | Único leitor de `perfil`/`grupo_id` fora do hub é `CurrentUserService.cs:34,40`; `JwtTokenService` só emite. Grep em `src` não achou outro `FindFirst("perfil")`/`IsInRole`/`[Authorize(Roles` |
| AC-05 | Sim | sem mudança no frontend nem em `AcessosAtualizadosNotification` |
| AC-06..08 | Sim | `grupo_id` substituído/removido (`CadastroClaimsValidator.cs:39-40`); testes `Equipe*` |
| AC-09 | Sim | decisão em spec §6 e design D3 |
| AC-10, 11 | Sim | `CadastroClaimsValidator.cs:33-34` + `context.Fail` em `Program.cs`; `ContaDesativada_*`, `ContaApagada_*` |
| AC-12 | Sim (sem código) | login já usa o cadastro; cache invalidado em `UsuarioPerfilRepository.AtualizarAsync` |
| AC-13, 14 | Sim | `CadastroDeAcessoAlteradoNotificationHandler.cs:36-47`; testes do handler |
| AC-15 | Sim, com ressalva R-02/R-03 | `Handler.cs:29-34` (`Abort`), reconexão recusada no `OnTokenValidated` |
| AC-16 | Sim | `ChamadosHub.cs:32-35` lê o claim já trocado |
| AC-17 | Sim | 528 testes antigos verdes; E2E 25/25 relatado |
| AC-18 | Parcial na evidência | cache 15 s (`UsuarioPerfilRepository.cs:20,40-56`); medição relatada (~636 vs ~690 ms) é diferença pequena e de uma amostra só, mas o mecanismo é correto |
| AC-19 | Sim | mensagens atuais não foram tocadas |
| AC-20 | Sim | ponto único no JwtBearer; vale para REST e para a negociação dos hubs |
| AC-21 | Sim | 573/573 |

## Achados

### 🟡 Atenção R-01 — Cache pode ser repovoado com valor velho logo após a invalidação (desativação "vale na hora" falha por até 15 s)
- **Onde:** `src/ChamadosCamarj.Infrastructure/Repositories/UsuarioPerfilRepository.cs:40-56` (leitura/`Set`) e `:72,83` (`Remove`)
- **Problema:** `ObterIdentidadeAsync` faz miss -> consulta o banco -> `Set`. Se a consulta começou antes do `SaveChanges` do Admin e o `Set` acontece depois do `Remove`, o valor antigo volta ao cache com validade cheia de 15 s. O design (§3) aceita o risco, mas ele contradiz a promessa "no pedido seguinte" da spec (AC-01..11, §6) justamente no caso em que a pessoa está ativa fazendo pedidos (o miss é causado por pedido dela).
- **Cenário de falha:** cache de Maria expirou; Maria dispara um pedido (miss, lê `Ativo = true`); o Admin desativa Maria e o repositório faz `Remove`; o pedido de Maria grava `Ativo = true` no cache. Pelos próximos até 15 s, todos os pedidos de Maria passam e os hubs aceitam reconexão. A janela é estreita (milissegundos), mas Maria é quem mais gera misses.
- **Correção sugerida:** contador de geração por usuário (ou `ConcurrentDictionary<Guid,int>` de versão) lido antes da consulta e conferido antes do `Set`; ou também invalidar no `Dispose` do escopo; ou aceitar e registrar o limite na spec como "até 15 s também no caso de corrida".

### 🟡 Atenção R-02 — Conexão aberta entre calcular os grupos e ser registrada fica fora do reajuste/derrubada
- **Onde:** `src/ChamadosCamarj.WebApi/Hubs/ChamadosHub.cs:24-37` (`Registrar` é chamado depois do `AddToGroupAsync`); `ChatHub.cs:22-23`
- **Problema:** `ConexoesTempoReal.Registrar` só roda no fim de `OnConnectedAsync`. O handler de notificação (`DoUsuario`) não enxerga a conexão antes disso.
- **Cenário de falha:** Admin conecta (token válido, perfil Admin lido do cadastro, entra em `Admins`); no meio do `OnConnectedAsync` o Admin do sistema rebaixa essa pessoa; o handler roda, `DoUsuario` não a encontra; em seguida a conexão é registrada. Ela fica em `Admins` e `Atendimento` pelo tempo de vida da conexão (horas; só reconectar corrige) e continua recebendo alertas de SLA. Mesmo cenário com desativação: a conexão nunca é abortada.
- **Correção sugerida:** chamar `_conexoes.Registrar(...)` como primeira linha de `OnConnectedAsync` (antes dos `AddToGroupAsync`) e, depois de entrar nos grupos, reler o perfil atual (via repositório) e reajustar; ou, no mínimo, registrar primeiro para reduzir a janela. Em `ChatHub` registrar antes do `AddToGroupAsync`.

### 🟡 Atenção R-03 — Notificação de tempo real depende de nada falhar antes dela, e uma falha não se recupera sozinha
- **Onde:** `src/ChamadosCamarj.Application/Features/Usuarios/Commands/AtualizarUsuarioPerfilCommandHandler.cs:~85-106` (publicação depois da auditoria e da `AcessosAtualizadosNotification`)
- **Problema:** o banco já foi gravado (e o cache invalidado) em `AtualizarAsync`; depois disso `DefinirChatPerfilCommand`, `_auditoriaAcesso.AdicionarAsync` e a publicação de `AcessosAtualizados` podem lançar. Se lançarem, `CadastroDeAcessoAlteradoNotification` não é publicada. Na nova tentativa do Admin, `ativoAnterior`/`grupoAnterior`/`perfilAnterior` já refletem o novo estado, então a comparação não acusa mudança e a notificação nunca mais é publicada.
- **Cenário de falha:** Admin desativa João; o `INSERT` da auditoria falha (oscilação do banco) -> 500 ao Admin; João está desativado e continua com WebSocket aberto no `ChamadosHub`/`ChatHub` recebendo `ChamadoCriado`/`StatusAlterado` (grupo `Todos`) e mensagens de chat até reconectar. Repetir a ação não derruba a conexão.
- **Correção sugerida:** publicar `CadastroDeAcessoAlteradoNotification` logo depois de `AtualizarAsync` (antes de chat/auditoria), pois a segurança não deve depender da auditoria; ou envolver auditoria/AcessosAtualizados em try/catch com log.

### 🟡 Atenção R-04 — Regra 6 da constitution ainda aberta (documentação funcional) e spec com cabeçalho/gates desatualizados
- **Onde:** diff não contém nada em `docs/obsidian/`; `.specs/features/perfil-no-cadastro/spec.md` cabeçalho `Status: Pendente`, §8 Gate Checks desmarcados
- **Problema:** o impacto.md lista 4 notas que ainda dizem "equipe vale no próximo login" (`Perfis e Permissões`, `Administração`, `Grupos e Equipes`, `ADR-010`) e a `Visão Técnica` diz que "o perfil vem do token"; falta o ADR novo.
- **Cenário de falha:** mergear sem a fase Close deixa a documentação funcional afirmando o comportamento oposto ao do sistema (a regra 6 diz que a feature só está pronta com o vault refletindo o código).
- **Correção sugerida:** fazer na fase Close, antes do PR: atualizar as 5 notas + novo ADR, marcar gates e status da spec.

## Respostas aos pontos de atenção do pedido

- **(a) Leitura de perfil/equipe/conta fora do ponto único:** nenhum outro. Grep em `src` por `FindFirst`, `FindAll`, `Claims`, `IsInRole`, `ClaimTypes.Role`, `HttpContext.User`, `[Authorize(Roles` só encontra `CurrentUserService`, `ChamadosHub`, `SubClaimUserIdProvider` (só `sub`), o validador e o `JwtTokenService` (emissão). `ChamadosHub.EhAdmin/EhAtendimento(ClaimsPrincipal)` ficaram sem uso fora da classe (ver Sugestões). Os métodos do `ChatHub` não leem perfil.
- **(b) Gravações de `UsuarioPerfil` fora do repositório:** não encontrei. Todos os `Atualizar/Adicionar` de usuário (Login rehash, ResetarSenha, RedefinirSenha, Criar/Reativar, DefinirChatPerfil, DefinirPreferenciaLeitura, SalvarAcessos, AtualizarUsuarioPerfil) usam o repositório; `GrupoRepository` só grava `Grupo` e não existe exclusão de grupo (a FK com `SetNull` não é acionada). `DatabaseSeeder` só semeia com tabela vazia, na subida. Edição direta no banco continua dentro dos 15 s aceitos.
- **(c) Corrida leitura x invalidação:** confirmada, ver R-01.
- **(d) JwtBearer:** `context.Fail` em `OnTokenValidated` torna a autenticação falha: rota protegida -> 401 (fallback policy `RequireAuthenticatedUser`); rota `[AllowAnonymous]` segue como anônima (o middleware só define `User` quando há principal), coerente com a verificação ao vivo relatada. Exceção lançada no evento (banco fora) é relançada pelo `JwtBearerHandler` após `AuthenticationFailed`; como `ExceptionHandlingMiddleware` está antes de `UseAuthentication` (`Program.cs:245` x `:259`), vira 500 e não 401 (não desloga todo mundo; também afeta rota anônima enviada com cabeçalho `Authorization`, aceitável). Negociação do SignalR com token na query: coberta, o mesmo evento roda. Em long polling cada requisição revalida (barato por causa do cache).
- **(e) `ConexoesTempoReal`:** `ConcurrentDictionary`, remoção em `OnDisconnectedAsync` como primeira instrução (sem vazamento visível; `Abort()` dispara o `OnDisconnectedAsync`). `HubCallerContext.Abort()` fora do contexto da invocação é suportado. Falha: só a janela de R-02 e o fato de ser memória de um único processo (já registrado no design).
- **(f) Cobertura do handler:** perfil e ativo cobertos; equipe (`GrupoId`) é publicada mas o handler não faz nada com ela porque nenhum grupo de tempo real depende de equipe (os avisos por equipe saem por `ListarAtendentesQuePodemVerAsync`, consulta ao banco no momento do envio). Hubs coerentes: `ChatHub` só é derrubado na desativação; `ChamadosHub` também é reajustado. Reativação: nada a fazer (conexão já foi abortada). Ver R-02 e R-03.
- **(g) Dado de token que deveria ter vindo do cadastro:** nenhum encontrado. `ModuloGuard` ainda compara cadastro (sem cache, `ObterPorIdAsync`) com o claim; passa a divergir só dentro da janela de R-01 e então devolve 403 "perfil mudou" (falha segura).

## Constitution e contratos

- Regra 1: cumpre (decisões respondidas em 2026-10-09).
- Regra 2: cumpre (spec §6 e design D4 revisados antes do cache T1b).
- Regra 3: C1-C4 aprovados; o cache (D4 revisado) e o construtor novo de `UsuarioPerfilRepository` (injeção de `IMemoryCache`) foram aprovados pelo usuário e estão no design §3. Batem com o código.
- Regra 4: cumpre (fluxo `/sdd`, review independente).
- Regra 5: `/analise-cod` feito; `impacto.md` não omitiu consumidor relevante. A classificação "coberto" para `ChatHub`/`ChamadosHub` apoia-se em testes com mock; o risco real (ligação em `Program.cs`) foi fechado por E2E e verificação ao vivo, não por teste automatizado — não há teste que fixe o comportamento `AllowAnonymous` + token inválido nem 500 por banco fora (ver Sugestões).
- Regra 6: pendente para o Close (R-04).
- Mudanças de contrato previstas x feitas: batem; nenhuma não prevista (`ChamadosHubTests` só ajustou construtor).
- Pontos de toque cross-feature: `Program.cs`, `ChamadosHub`, `ChatHub`, `AtualizarUsuarioPerfilCommandHandler`, repositório — cobertos por teste unitário e E2E; a ligação do pipeline só tem verificação manual.

## Sugestões (não bloqueiam)

- `ChamadosHub.EhAdmin(ClaimsPrincipal)` e `EhAtendimento(ClaimsPrincipal)` ficaram sem chamadores em produção (só testes); remover ou manter com intenção explícita.
- Comentário de `ModuloGuard.cs` ("o escopo vem do perfil do token") ficou desatualizado: agora vem do cadastro.
- Teste de integração mínimo (`WebApplicationFactory`) para o `OnTokenValidated`: 401 em rota protegida, anônimo passa, banco fora -> 500. Hoje só a verificação ao vivo prova isso.
- `ChatHub.EntrarConversa` permite entrar em qualquer grupo de conversa sem checar participação (preexistente, fora do escopo; vale registrar no ROADMAP de segurança).
- O grupo `Todos` do `ChamadosHub` recebe `ChamadoCriado`/`StatusAlterado` independentemente de perfil/equipe (preexistente, fora do escopo desta feature).
- AC-18: registrar mais de uma amostra de medição (a diferença 636 x 690 ms está dentro do ruído).

## Tratamento dos achados (sessão principal, 2026-10-09)

| Achado | Resultado |
|---|---|
| R-01 corrida leitura × invalidação do cache | **Corrigido** com "geração" por usuário: o valor lido só vai ao cache se nenhuma gravação ocorreu durante a leitura. Teste que reproduz: `UsuarioPerfilRepositoryCorridaTests.GravacaoNoMeioDaLeitura_ValorVelhoNaoVaiParaOCache`. |
| R-02 conexão fora do registro durante a entrada nos grupos | **Corrigido**: `ChamadosHub` e `ChatHub` registram a conexão como primeira ação. Teste: `ChamadosHubTests.OnConnectedAsync_RegistraAConexaoAntesDeEntrarNosGrupos`. Resta uma janela de milissegundos entre ler o claim e o reajuste do handler, aceita (a conexão se corrige ao reconectar). |
| R-03 aviso de tempo real dependia da auditoria | **Corrigido**: a notificação sai logo depois da gravação, antes de Chat/auditoria/aviso de menu. Teste: `Handle_SeAAuditoriaFalhar_ODesativarJaAvisouOTempoReal`. |
| R-04 regra 6 (Obsidian) e cabeçalho da spec | Fica para a fase Close (já previsto). |
| Sugestão: comentário do `ModuloGuard` desatualizado | Corrigido. |
| Sugestão: `WebApplicationFactory` para o pipeline | Não feita: o projeto não tem servidor de integração; o pipeline segue provado por E2E (25/25) e ao vivo (41/41). Registrada como pendência no fechamento. |
| Sugestões preexistentes (`ChatHub.EntrarConversa` sem checar participação; grupo `Todos` sem filtro de perfil) | Fora do escopo; vão para o ROADMAP de segurança no fechamento. |
| Sugestão: mais de uma amostra de AC-18 | A medição ao vivo foi de 30 pedidos por rodada (2 rodadas); o ganho (≈54 ms) fica no ruído, mas o mecanismo está provado por teste de cache. Registrado. |

Gates após as correções: `dotnet build` 0 erros · `dotnet test` 576/576.
