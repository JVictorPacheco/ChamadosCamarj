# Perfil e Equipe Valem na Hora — Design Técnico

> Spec: `spec.md` (aprovada em 2026-10-09). Sem migration. Sem mudança no frontend.

---

## 1. Visão Geral da Solução

Toda leitura de perfil e equipe no servidor passa por dois pontos só: `CurrentUserService` (lê os claims `perfil` e `grupo_id` do `HttpContext.User`) e `ChamadosHub` (lê o claim `perfil` de `Context.User`). Os 39 pontos que dependem de perfil, em controllers e handlers, chegam a esses claims. Por isso **a correção é feita uma vez só, na validação do token**: logo depois que o JWT é aceito, o servidor consulta o cadastro, **recusa** quem foi desativado ou apagado e **troca** os claims `perfil` e `grupo_id` pelos valores atuais. Nenhum controller, handler ou tela precisa mudar, e uma rota nova já nasce protegida (AC-20).

Para o tempo real, onde a conexão fica aberta e o token só é validado na conexão, um registro das conexões ativas permite reajustar os grupos de quem mudou de perfil e derrubar a conexão de quem foi desativado.

## 2. Decisões técnicas

| # | Decisão | Motivo | AC |
|---|---|---|---|
| D1 | `JwtBearerEvents.OnTokenValidated` chama o `CadastroClaimsValidator` (classe nova, testável sem HTTP). | Ponto único; vale para API REST e para a negociação dos hubs. | 01–04, 06–08, 10, 11, 16, 20 |
| D2 | Usuário inexistente ou `Ativo == false` → `context.Fail(...)` → 401. | A tela já leva ao login em 401 (`lib/api.ts:84`). | 10, 11, 15 |
| D3 | Perfil e equipe do cadastro **substituem** os claims `perfil` e `grupo_id` (remove e adiciona no `ClaimsIdentity`). Sem equipe no cadastro → remove `grupo_id`. | `CurrentUserService`, `ObterContextoAcesso` e o hub passam a ver o valor atual sem mudar. | 01–04, 06–08, 16 |
| D4 | Consulta por chave primária que devolve só `Perfil`, `Ativo` e `GrupoId` (sem `Include` de Grupo, sem rastreamento), **com cache em memória de 15 s e invalidação imediata** feita pelo próprio repositório: `AtualizarAsync` e `AdicionarAsync` apagam o registro do usuário depois de gravar. Conta inexistente não é guardada em cache. | **Revisado em 2026-10-09** (decisão do usuário, depois da medição ao vivo): sem cache, cada pedido pagava ~160 ms de ida ao banco (Supabase), sobre ~530 ms de uma listagem. Invalidar no repositório cobre **todos** os caminhos que gravam usuário (edição, criação/reativação, Chat, módulos) sem depender de cada handler lembrar. Mudança feita pelo sistema vale no pedido seguinte; edição direta no banco, ou um 2º servidor de backend, demora até 15 s. | 01–04, 06–08, 10, 11, 18 |
| D5 | `AtualizarUsuarioPerfilCommandHandler` publica uma notificação nova quando mudam perfil, equipe ou situação da conta. | Hoje só publica quando o perfil muda; equipe e desativação não avisam o tempo real. | 13–15 |
| D6 | `ConexoesTempoReal` (singleton) guarda `ConnectionId → (usuarioId, HubCallerContext)` dos dois hubs, preenchido em `OnConnectedAsync` e limpo em `OnDisconnectedAsync`. | O `IHubContext` só consegue mexer em grupo e derrubar conexão sabendo o `ConnectionId`. | 13–15 |
| D7 | Handler da notificação (WebApi) reajusta, para cada conexão do usuário no `ChamadosHub`, a entrada nos grupos `Atendimento` e `Admins` conforme o perfil novo; se a conta foi desativada, chama `Abort()` nas conexões dos dois hubs. | Rebaixado deixa de receber alertas restritos; promovido passa a receber. A reconexão de desativado falha em D2. | 13–15 |
| D8 | `ModuloGuard` não muda. A comparação "perfil do token × cadastro" nele passa a nunca disparar (os claims já vêm do cadastro); fica como defesa em profundidade. | Remover é risco sem ganho. | 05, 17 |
| D9 | Anúncio na tela: nada novo. O `AcessosAtualizados` de perfil continua como está; conta desativada leva ao login no próximo pedido (401). | Spec "Fora de escopo". | 05, 10 |

## 3. Mudanças no Domain

- `IUsuarioPerfilRepository`: **novo método** `ObterIdentidadeAsync(Guid id, CancellationToken)` → `IdentidadeUsuario?` (record `Perfil`, `Ativo`, `GrupoId`). Aditivo.
- `UsuarioPerfilRepository` (Infrastructure): passa a receber `IMemoryCache` (singleton, `AddMemoryCache()` no `Program.cs`); `ObterIdentidadeAsync` usa o cache (15 s); `AtualizarAsync` e `AdicionarAsync` invalidam a entrada do usuário. Construtor muda (consumidor de produção: só a DI; testes de repositório ajustados).
- Risco aceito: uma leitura em andamento pode regravar no cache um valor lido antes de uma gravação concorrente; o valor velho vive no máximo 15 s.

## 4. Mudanças no Application

- `AtualizarUsuarioPerfilCommandHandler`: compara perfil, equipe e `Ativo` antes e depois; se algum mudou, publica `CadastroDeAcessoAlteradoNotification(UsuarioId, Perfil, Ativo, GrupoId)` (record novo em `Common/Notifications`). Mantém a publicação atual de `AcessosAtualizadosNotification` quando o perfil muda.

## 5. Mudanças na WebApi

- `Services/CadastroClaimsValidator` (novo): D1–D3.
- `Program.cs`: `OnTokenValidated` chamando o validador (junto do `OnMessageReceived` já existente).
- `Hubs/ConexoesTempoReal` (novo, singleton registrado no `Program.cs`).
- `Hubs/ChamadosHub` e `Hubs/ChatHub`: registrar e remover a conexão em `OnConnectedAsync`/`OnDisconnectedAsync`. `ChamadosHub` passa a reaproveitar a mesma regra de grupos num método público estático usado também pelo handler (sem duplicar a regra de quem é Atendimento/Admin).
- `Notifications/CadastroDeAcessoAlteradoNotificationHandler` (novo): D7.

## 6. Constitution Check

| Regra | Cumpre? | Justificativa |
|---|---|---|
| 1 Pergunta sem resposta não vira suposição | Sim | As 4 decisões de produto foram respondidas em 2026-10-09. |
| 2 Spec antes do código | Sim | Spec aprovada antes do design. |
| 3 Contrato compartilhado sinalizado antes | Sim | C1–C4 aprovados pelo usuário em 2026-10-09. |
| 4 Fluxo SDD | Sim | Via `/sdd`. |
| 5 Não quebrar fora do escopo | Sim, com testes | Seção 8; `/analise-cod` depois da implementação. |
| 6 Obsidian atualizado | A fazer no Close | Perfis e Permissões, Administração, Tempo Real; novo ADR (conferir cadastro a cada pedido). |

## 7. Mudanças de contrato (a aprovar)

| Id | Mudança | Quem é afetado |
|---|---|---|
| C1 | Autenticação global: a cada pedido autenticado o servidor consulta o cadastro e **substitui** `perfil` e `grupo_id`; conta desativada/apagada → 401. | Toda a API e os dois hubs. Quem não mudou: nada muda. Custo: uma consulta por chave primária por pedido. |
| C2 | `IUsuarioPerfilRepository.ObterIdentidadeAsync` (método novo, aditivo). | Só o validador; mocks existentes não quebram. |
| C3 | Notificação nova `CadastroDeAcessoAlteradoNotification`, publicada na edição de usuário. | Só o handler novo; `AcessosAtualizadosNotification` segue igual. |
| C4 | Tempo real: registro de conexões (`ConexoesTempoReal`); `ChamadosHub` e `ChatHub` registram em conectar/desconectar; conta desativada tem a conexão derrubada e grupos são reajustados na hora. | Quem usa alertas de SLA, avisos do atendimento e chat. O evento `AcessosAtualizados` e o formato das mensagens não mudam. |

## 8. Pontos de toque cross-feature e não-regressão

| Arquivo | Quem mais depende | Proteção |
|---|---|---|
| `Program.cs` (autenticação) | Todas as features | Teste do validador (perfil, equipe, desativado, apagado, sem equipe, sem mudança); E2E completo (25) deve seguir verde; verificação ao vivo. |
| `CurrentUserService` / `ObterContextoAcesso` | `autorizacao-chamados`, `grupos-equipes`, `controle-de-acesso`, `editar-chamado`, chat | Não mudam: ganham o valor atual dos claims. Testes existentes seguem; caso novo: filtro de chamados com equipe trocada. |
| `ChamadosHub` | Alertas de SLA (`correcoes-pre-deploy`), avisos de comentário interno | Testes de `EhAtendimento`/`EhAdmin` existentes; teste novo do reajuste de grupos. |
| `ChatHub` | Chat corporativo | Só registro de conexão; E2E do chat. |
| `AtualizarUsuarioPerfilCommandHandler` | `controle-de-acesso` (histórico, `AcessosAtualizados`), `chat-corporativo` | Testes existentes mantidos; novos para equipe e ativo. |
| `ModuloGuard` | Dashboard e Relatório | Sem mudança. |

## 9. Riscos

- **Falha do banco na validação do token:** passa a derrubar o pedido (hoje o token sozinho bastava). Resposta: 401/500 conforme o erro; sem "falhar aberto" (não aceitar token sem conferir).
- **Desempenho:** mais uma consulta por pedido e por conexão de hub; medir no teste ao vivo (AC-18).
- **`OnTokenValidated` e o `DbContext` do escopo:** a consulta é sem rastreamento, para não colidir com o rastreador do EF (problema já visto em `UsuarioPerfilRepository.AtualizarAsync`).
