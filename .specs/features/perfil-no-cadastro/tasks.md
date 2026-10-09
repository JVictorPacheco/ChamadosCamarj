# Perfil e Equipe Valem na Hora — Tasks

> Spec: `spec.md` · Design: `design.md` · Executor: Claude Code — Sonnet 5.5
> Marcar `[x]` ao concluir. Commits atômicos por bloco. Gates: `dotnet build`, `dotnet test tests/ChamadosCamarj.UnitTests/`, `npm --prefix frontend run build`.

## Bloco 1 — Cadastro e aplicação (C2, C3)

- [x] **T1** (D4, C2) — Adicionar `IdentidadeUsuario` (record: Perfil, Ativo, GrupoId) e `IUsuarioPerfilRepository.ObterIdentidadeAsync`; implementar no repositório com projeção sem rastreamento, sem `Include`.
  **Pronto quando:** compila; teste de repositório (EF InMemory, como em `controle-de-acesso`) devolve a identidade de um usuário e `null` para um id inexistente.
- [x] **T2** (D5, C3, AC-13..15) — Criar `CadastroDeAcessoAlteradoNotification`; em `AtualizarUsuarioPerfilCommandHandler` publicá-la quando perfil, equipe ou situação da conta mudarem (e só então). Manter a publicação atual de `AcessosAtualizadosNotification`.
  **Pronto quando:** testes novos: muda só equipe → publica; muda só ativo → publica; nada muda → não publica; muda perfil → publica as duas. Testes existentes do handler seguem verdes.

## Bloco 2 — Validação do token (C1)

- [x] **T3** (D1–D4, AC-01..04, 06..08, 10, 11, 16) — Criar `CadastroClaimsValidator` (WebApi/Services): consulta `ObterIdentidadeAsync`; inexistente ou inativo → falha; senão troca os claims `perfil` e `grupo_id` (remove `grupo_id` se o cadastro não tem equipe).
  **Pronto quando:** testes unitários: perfil rebaixado, perfil promovido, equipe trocada, equipe removida, equipe colocada, conta desativada, conta apagada, nada mudou (claims iguais). Banco indisponível → a validação falha (nunca aceita token sem conferir).
- [ ] **T4** (C1, AC-20) [código pronto e compilando; aguarda a verificação ao vivo da T13] — Registrar o validador no `Program.cs` (DI) e chamá-lo em `OnTokenValidated`, preservando o `OnMessageReceived` (token do SignalR via query string).
  **Pronto quando:** `dotnet build` ok; teste de integração mínimo (ou verificação ao vivo, T13) mostra 401 com token de conta desativada e escopo novo com token de perfil trocado, na API e na negociação do hub.
- [x] **T5** (D8, AC-17) — Conferir `ModuloGuard`, `ObterContextoAcesso` e os testes existentes: nada a mudar no código, mas o teste de `ModuloGuard` "perfil do token diferente do cadastro" continua verde.
  **Pronto quando:** testes existentes de `ModuloGuard` e `CurrentUserServiceExtensions` verdes sem alteração.

## Bloco 3 — Tempo real (C4)

- [x] **T6** (D6) — Criar `ConexoesTempoReal` (singleton, thread-safe): registrar, remover, listar conexões por usuário, guardando o `HubCallerContext`. Registrar no DI.
  **Pronto quando:** testes: registra duas conexões do mesmo usuário, remove uma, lista a restante; concorrência simples sem exceção.
- [x] **T7** (D6) — `ChamadosHub` e `ChatHub`: registrar/remover a conexão em `OnConnectedAsync`/`OnDisconnectedAsync`. Extrair em `ChamadosHub` a regra "quais grupos de perfil" (Atendimento/Admins) para método estático reutilizado, sem alterar `EhAtendimento`/`EhAdmin`.
  **Pronto quando:** testes existentes de `EhAtendimento`/`EhAdmin` verdes; teste novo do cálculo de grupos por perfil.
- [x] **T8** (D7, AC-13..15) — `CadastroDeAcessoAlteradoNotificationHandler` (WebApi/Notifications): conta desativada → `Abort()` nas conexões do usuário nos dois hubs; senão reajusta, no `ChamadosHub`, as entradas em `Atendimento` e `Admins` conforme o perfil novo.
  **Pronto quando:** testes com `IHubContext`/grupos simulados: rebaixado Admin→Solicitante sai de `Admins` e `Atendimento`; Solicitante→Atendente entra em `Atendimento`; desativado tem `Abort()` chamado; reativado não faz nada.

## Bloco 4 — Não-regressão (pontos de toque do design §8)

- [ ] **T9** (AC-06..08) — Teste de visibilidade: usuário com equipe trocada (cadastro ≠ token original) passa a ver/não ver chamados da equipe pelo contexto de acesso atualizado; Atendente rebaixado é recusado em assumir/reatribuir/resolver/fechar.
  **Pronto quando:** testes novos verdes usando o mesmo caminho de claims (validador → `CurrentUserService`).
- [ ] **T10** (AC-17, AC-18) — Rodar E2E completo (25) e a suíte inteira; medir tempo de uma listagem antes/depois no ambiente local.
  **Pronto quando:** `dotnet test` verde (≥ 528 + novos), E2E 25/25 e diferença de tempo sem impacto perceptível, registrada em `impacto.md`.

## Bloco 5 — Frontend

Nenhuma mudança prevista (D9). Se a verificação ao vivo mostrar que a tela não leva ao login após 401 de conta desativada, abrir tarefa nova e parar para avisar.

## Bloco 6 — Fechamento da fase Implement

- [ ] **T11** — Rodar `/analise-cod` (cinco buscas obrigatórias: regra antiga espalhada, quem perde o caminho, todos os caminhos que gravam o estado, gravação real e não mock, abertura do sistema e reconexão). Zero 🔴 para seguir.
- [ ] **T12** (AC-01..21) — Preencher a rastreabilidade da spec com os testes reais.
- [ ] **T13** (AC-01..16) — Verificação ao vivo com contas de teste `teste.perfil.*` e chamados `[TESTE-PERFIL]`: rebaixar Admin→Solicitante e chamar rotas de administração; Atendente rebaixado tentando assumir; troca de equipe; desativar conta e reutilizar o token; conexão do hub de rebaixado/desativado; reativação. Criar e apagar os dados de teste **com OK do usuário** (ação em banco real).
- [ ] **T14** — Gates: `dotnet build`, `dotnet test tests/ChamadosCamarj.UnitTests/`, `npm --prefix frontend run build`.

## Analyze

| Checagem | Resultado |
|---|---|
| Todo AC tem tarefa | AC-01..04: T3/T4/T9 · 05: T5 · 06..08: T3/T9 · 09: T3 (regra substituída, decisão registrada) · 10..11: T3/T4 · 12: T3 (nada a mais: reativar volta ao cadastro) + T13 · 13..15: T2/T6/T7/T8 · 16: T3/T7 · 17..18: T5/T10 · 19: mensagens atuais preservadas, verificado em T13 · 20: T4 · 21: T14 |
| Toda tarefa aponta para AC/design | Sim |
| Todo contrato C1–C4 tem tarefa de teste | C1: T3/T4 · C2: T1 · C3: T2 · C4: T6/T7/T8 |
| Nada contradiz "Fora de escopo" | Sim: sem invalidar tokens, sem mudar regras de visibilidade, sem mudar validade do login |
| Constitution | Cumpre (design §6) |

Nota: AC-12 (reativar e entrar de novo) não precisa de código: o login já usa o cadastro; é coberto por T13.
