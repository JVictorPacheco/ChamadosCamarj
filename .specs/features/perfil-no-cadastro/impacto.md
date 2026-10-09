# Análise de impacto — perfil-no-cadastro

> **Diff:** `origin/develop...HEAD` — 21 arquivos (11 de código, 5 de teste novos/alterados, 3 de spec, 2 de contrato) · **Data:** 2026-10-09 · **Gates:** `dotnet build` 0 erros · `dotnet test` 570/570 (eram 528; +42) · `npm run build` ok (frontend não foi tocado)
> **Veredito:** IMPACTO COBERTO — o único 🔴 (I-01) foi fechado em 2026-10-09: E2E 25/25 (1 worker) + verificação ao vivo 41/41 passando pelo pipeline real. Gates finais: build 0 erros, 573/573 testes, npm build ok.

## Resumo
- 🔴 0 (1 fechado) · 🟡 7 · ✅ 4

## 🔴 Impacto sem cobertura (decisão do usuário necessária)

### I-01 (FECHADO em 2026-10-09) — A ligação no `Program.cs` (`OnTokenValidated`) afeta TODAS as rotas e os dois hubs, e só o validador isolado tem teste
- **Mudou:** `src/ChamadosCamarj.WebApi/Program.cs` — `JwtBearerEvents.OnTokenValidated` passa a consultar o cadastro a cada pedido autenticado; registro do `CadastroClaimsValidator` e de `ConexoesTempoReal` na DI.
- **Quem depende:** qualquer pedido autenticado (todas as features), negociação e conexão dos hubs `/hubs/chamados` e `/hubs/chat` (token pela query string).
- **Antes × depois:** antes o token bastava; agora o pedido depende de uma consulta ao banco e do validador estar registrado.
- **Cobertura:** os 11 testes do validador e os 7 do handler de tempo real usam o validador/handler direto, com mock do repositório. **Nenhum teste passa pelo pipeline real** (o projeto de testes não tem servidor de integração). Se o registro na DI estivesse errado, `GetRequiredService` derrubaria todos os pedidos e a suíte continuaria verde.
- **Cenário de quebra:** deploy com o validador mal registrado → todo mundo recebe erro, e a suíte unitária não avisaria.
- **Proposta:** (1) rodar o E2E completo (25 testes, passa pelo pipeline real) — T10; (2) verificação ao vivo T13 com token de conta desativada (401), perfil trocado (escopo novo) e conexão do hub. Fecha o 🔴 sem código novo.

## 🟡 Impacto coberto

| Item | Consumidor fora do escopo | Teste que cobre |
|---|---|---|
| Claims `perfil`/`grupo_id` agora do cadastro | `CurrentUserService` e o `ObterContextoAcesso` usado pelo filtro de visibilidade de chamados (`ChamadoRepository`) e pelo `AcessoChamadoBehaviour` | `PerfilNoCadastroNasRegrasDeAcessoTests` (validador → contexto → `ChamadoPermissoes`), `CadastroClaimsValidatorTests.PerfilAtualizadoChegaNoCurrentUserENoContextoDeAcesso`; testes existentes de autorização verdes sem alteração |
| Idem | `ModuloGuard` (Dashboard/Relatório): a comparação "perfil do token × cadastro" passa a nunca disparar na prática (os claims já vêm do cadastro); fica como defesa em profundidade | Testes existentes do `ModuloGuard` verdes sem alteração |
| Idem | Controllers que mandam `_currentUser.Perfil` como `PerfilRequisitante` (Usuários, Grupos, Tipos, Acessos, Chat) | Testes existentes dos handlers (guarda `PerfilRequisitanteGuard`) verdes; valor novo coberto pelo validador |
| Conta desativada/apagada → 401 | Frontend (`api.ts:84` limpa o token e desloga; `/auth/me` já consulta `Ativo`; `useSignalR` revalida ao fechar) | Comportamento existente, sem alteração de código no frontend; fecha na verificação ao vivo (T13) |
| `ChamadosHub` ganhou construtor e reuso da regra de grupos | `ChamadosHubTests` (grupos de Atendimento/Admins; "hub não expõe método ao cliente") | Testes existentes verdes (2 só ajustados para o construtor novo) + `GruposDoPerfil_SegueOPerfil` |
| `AtualizarUsuarioPerfilCommandHandler` publica notificação nova | Histórico/auditoria de acessos, `AcessosAtualizados` (menu e desconexão da tela), `DefinirChatPerfilCommand` | 19 testes do handler (15 existentes sem alteração + 4 novos), incluindo "perfil muda → publica as duas" |
| Estado novo `Ativo`/equipe agora também derruba/reajusta conexões | Alertas de SLA para Admins/Atendimento (`SlaAlertaNotificador`), aviso de comentário interno | `CadastroDeAcessoAlteradoNotificationHandlerTests` (sai/entra nos grupos, `Abort` na desativação); `SlaAlertaNotificadorTests` existentes verdes |

## ✅ Itens sem consumidor fora do escopo
- `IdentidadeUsuario` / `ObterIdentidadeAsync` — só o validador usa; teste de repositório com EF real (sem rastreamento, sem colidir com gravações da requisição).
- `ConexoesTempoReal` — só os dois hubs e o handler novo.
- `CadastroDeAcessoAlteradoNotification` — só o handler novo.
- Chat (`ChatHub`) — só ganhou registro/remoção da conexão; nenhum grupo ou evento mudou.

## Pontos para a verificação ao vivo (T13) que a leitura de código não prova
- Endpoints `[AllowAnonymous]` (login, esqueci/resetar senha) com um token antigo de conta desativada no cabeçalho: devem seguir respondendo (a falha de autenticação não deve bloquear rota anônima).
- Queda do banco durante a validação: o handler do JwtBearer propaga a exceção (500 pelo middleware de erros), e não vira 401 — assim uma oscilação do banco não desloga todo mundo pelo `401` do frontend. Conferir.
- Reconexão do tempo real de conta desativada: o servidor recusa na negociação (401) e a tela revalida e vai ao login.
- Medir o tempo de uma listagem antes/depois (AC-18).

## Previsto × real
- Pontos de toque do design que de fato foram tocados: `Program.cs`, `ChamadosHub`, `ChatHub`, `AtualizarUsuarioPerfilCommandHandler`, repositório de usuário. `CurrentUserService`, `ObterContextoAcesso` e `ModuloGuard` **não** foram alterados (como previsto).
- Tocados mas NÃO previstos no design: nenhum. (`ChamadosHubTests` só teve o construtor ajustado.)
- Documentação funcional a atualizar no Close (regra 6): 4 notas dizem "equipe vale no próximo login" — `Perfis e Permissões`, `Administração`, `Grupos e Equipes`, `ADR-010` — e a `Visão Técnica` diz que "o perfil vem do token".

## Fechamento do I-01 (2026-10-09)
- **E2E:** 25/25 passando com 1 worker. Na primeira tentativa, com 2 workers em paralelo (máquina com pouca memória), 2 testes de `controle-de-acesso.spec.ts` falharam por tempo (aviso em tempo real demorou além do limite); os mesmos 2 passaram isolados e na rodada completa com 1 worker. Tratado como instabilidade de carga, não regressão: nenhum dos dois toca perfil/equipe/conta de forma diferente do que passava antes.
- **Ao vivo:** 41/41 contra o banco real (API + os dois hubs), antes e depois do cache de 15 s (T1b), mais demonstração em tela (conta desativada volta ao login no primeiro clique).
- **Cache (T1b):** não previsto na spec original; decisão do usuário após a medição (≈160 ms por pedido). Invalidação no repositório cobre todo caminho que grava usuário; edição direta no banco ou 2º servidor: até 15 s.
- Dados de teste (`teste.perfil.*`, `[TESTE-PERFIL]` e os `[TESTE-E2E]` criados por esta verificação) apagados com OK do usuário; banco conferido: 9 usuários e 96 chamados originais.
