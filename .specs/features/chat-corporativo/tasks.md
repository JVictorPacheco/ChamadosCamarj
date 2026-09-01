# Chat Corporativo — Tasks

> **Branch:** `feature/chat-corporativo` (mergeada em `develop` via PR #28 em 2026-08-31)
> **Spec:** `spec.md` | **Design:** `design.md`
> **Orquestração:** Claude Code (Sonnet 4.6 — spec; Opus 4.8 — backend e review; Sonnet 4.6 — frontend)
> **Gate checks:** `dotnet test` + `npm run build` antes de qualquer commit

---

## Fase 1 — Domain

- [x] Criar enum `ChatPerfil` em `Domain/Enums/`
- [x] Criar enum `ChatConversaTipo` em `Domain/Enums/`
- [x] Criar enum `ChatMensagemTipo` em `Domain/Enums/`
- [x] Criar enum `StatusPresenca` em `Domain/Enums/`
- [x] Criar enum `ChatAcao` em `Domain/Enums/`
- [x] Adicionar propriedade `ChatPerfil` em `UsuarioPerfil` com método `DefinirChatPerfil()`
- [x] Criar entidade `ChatConversa`
- [x] Criar entidade `ChatParticipante`
- [x] Criar entidade `ChatMensagem`
- [x] Criar entidade `ChatMensagemReacao`
- [x] Criar entidade `ChatPresenca`
- [x] Criar entidade `ChatHistorico`
- [x] Criar interface `IChatConversaRepository`
- [x] Criar interface `IChatMensagemRepository`
- [x] Criar interface `IChatPresencaRepository`
- [x] Criar interface `IChatHistoricoRepository`

---

## Fase 2 — Infrastructure

- [x] Criar migration `AddChatPerfilUsuario` (coluna em `UsuariosPerfil`, default `SemAcesso`) — **aplicada no Supabase real em 2026-08-31**
- [x] Criar migration `AddChatFeature` (6 novas tabelas) — **aplicada no Supabase real em 2026-08-31**
- [x] Criar `ChatConversaConfiguration` (Fluent API)
- [x] Criar `ChatParticipanteConfiguration`
- [x] Criar `ChatMensagemConfiguration`
- [x] Criar `ChatMensagemReacaoConfiguration`
- [x] Criar `ChatPresencaConfiguration`
- [x] Criar `ChatHistoricoConfiguration`
- [x] Implementar `ChatConversaRepository`
- [x] Implementar `ChatMensagemRepository`
- [x] Implementar `ChatPresencaRepository`
- [x] Implementar `ChatHistoricoRepository`
- [x] Criar `ChatPresencaWorker` (IHostedService — marca Ausente/Offline por inatividade)
- [x] Criar bucket `chat-arquivos` no Supabase Storage — **criado em 2026-08-31 (privado, sem limite de tamanho/mime no bucket — validação fica em `EnviarArquivoCommandValidator`)**
- [x] Registrar repositórios e worker em `Program.cs`

---

## Fase 3 — Application (Commands)

- [x] `DefinirChatPerfilCommand` + Handler + Validator
- [x] `CriarConversaCommand` + Handler + Validator
- [x] `CriarGrupoCommand` + Handler + Validator
- [x] `EnviarMensagemCommand` + Handler + Validator
- [x] `EnviarArquivoCommand` + Handler + Validator
- [x] `EditarMensagemCommand` + Handler + Validator
- [x] `DeletarMensagemCommand` + Handler + Validator
- [x] `AdicionarReacaoCommand` + Handler + Validator
- [x] `MarcarComoLidoCommand` + Handler
- [x] `AtualizarPresencaCommand` + Handler

---

## Fase 4 — Application (Queries + DTOs)

- [x] `ListarConversasQuery` + Handler
- [x] `ListarMensagensQuery` + Handler (paginado)
- [x] `ListarPresencasQuery` + Handler
- [x] `ListarHistoricoChatQuery` + Handler (Admin only)
- [x] Criar todos os DTOs (`ChatConversaResponse`, `ChatMensagemResponse`, `ChatReacaoResponse`, `ChatPresencaResponse`, `ChatArquivoResponse`, `ChatHistoricoResponse`)
- [x] Criar extension methods de mapeamento (`ChatMappings.cs`)

---

## Fase 5 — WebApi

- [x] Criar `ChatHub` em `WebApi/Hubs/`
- [x] Registrar `ChatHub` em `Program.cs` (`app.MapHub<ChatHub>("/hubs/chat")`)
- [x] Criar `ChatController` com todos os endpoints definidos em `design.md`
- [x] Criar `ChatPresencaController` (heartbeat + listagem de presença)
- [x] Adicionar `PATCH /api/usuarios/{id}/chat-perfil` em `UsuariosController`
- [x] Adicionar `GET /api/chat/historico` em `ChatController` (Admin only)

---

## Fase 6 — Testes Backend ✅ EXECUTADA em 2026-08-31

> Os 8 arquivos planejados foram criados, mais 2 extras (`AdicionarParticipante`/`RemoverParticipante`,
> handlers novos do Bug #5 que não tinham nenhuma cobertura). 70 testes novos, 286 no total do
> projeto (eram 216), 0 falhas. O gap ficou aberto de 2026-08-29 até 2026-08-31 — registrado em
> `STATE.md` como aprendizado: PR foi mergeado sem essa fase, e ninguém sinalizou isso com destaque
> antes do merge.

- [x] `DefinirChatPerfilHandlerTests` (ACs 01-04) — 9 testes
- [x] `ChatPresencaHandlerTests` (ACs 05-09) — 5 testes
- [x] `EnviarMensagemHandlerTests` (ACs 10-11) — 8 testes
- [x] `CriarGrupoHandlerTests` (ACs 12-14) — 7 testes
- [x] `EnviarArquivoHandlerTests` (ACs 15-17) — 5 testes (incluindo rollback de arquivo órfão)
- [x] `EditarMensagemHandlerTests` (ACs 21-22) — 7 testes
- [x] `DeletarMensagemHandlerTests` (ACs 23-25) — 8 testes
- [x] `ChatHistoricoHandlerTests` (ACs 35-36) — 4 testes
- [x] **Extra:** `AdicionarParticipanteHandlerTests` (ACs 41-45) — 10 testes
- [x] **Extra:** `RemoverParticipanteHandlerTests` (ACs 41-45) — 8 testes
- [x] Gate check: `dotnet test` — 286 testes, 0 falhas

---

## Fase 7 — Frontend

### Tipos e API
- [x] Adicionar tipos de chat em `types/api.ts`
- [x] Criar `features/chat/api.ts` com todas as funções HTTP

### Hooks
- [x] `useConversas.ts` (TanStack Query)
- [x] `useMensagens.ts` (TanStack Query, paginado)
- [x] `usePresencas.ts` (TanStack Query)
- [x] `useChat.ts` (mutations: enviar, editar, deletar, reagir, marcar lido)
- [x] `useChatSignalR.ts` (SignalR + heartbeat 30s com visibilitychange)

### Componentes
- [x] `PresencaBadge.tsx` — bolinha de status colorida
- [x] `PresencaPanel.tsx` — painel com todos os usuários e status
- [x] `ConversaItem.tsx` — item da lista de conversas
- [x] `ConversaList.tsx` — lista lateral com badge de não lidas
- [x] `MensagemItem.tsx` — bolha de mensagem (texto/arquivo/sistema + reações + ações)
- [x] `TypingIndicator.tsx` — "[Nome] está digitando..."
- [x] `MensagemList.tsx` — scroll de mensagens com load more
- [x] `MensagemInput.tsx` — input com emoji picker, upload, reply preview
- [x] `CriarGrupoDialog.tsx` — modal de criação de grupo
- [x] `ChatPerfilSelect.tsx` — select em Admin > Usuários
- [x] `ChatPage.tsx` — layout principal (split: lista + conversa ativa)
- [x] `NovaConversaDialog.tsx` — iniciar conversa privada pelo botão "+" ou pela aba Presença (adicionado em 2026-08-31, fora do tasks.md original)

### Integração
- [x] Adicionar rota `/chat` em `App.tsx` (dentro de `ProtectedRoute`)
- [x] Adicionar link "Chat" na sidebar (apenas para `ChatPerfil !== SemAcesso`)
- [x] Adicionar badge vermelho de não lidas no link do chat na sidebar
- [x] Adicionar coluna "Chat" na tela Admin > Usuários com `ChatPerfilSelect`
- [x] Gate check: `npm run build` — 0 erros (build local confirmado em 2026-08-31; há warnings de `INVALID_ANNOTATION` vindos de `node_modules/@microsoft/signalr`, não do código do projeto)

---

## Fase 8 — Verificação Manual (UI) ✅ EXECUTADA — 7 defeitos encontrados, TODOS CORRIGIDOS

> Executada em 2026-08-31 pelo usuário (2 contas reais) + confirmação ao vivo via Playwright
> (conta sintética `teste.realtime@camarj.com.br`, sem tocar em credencial real). Achados
> completos, causa raiz e correções em **`review-fase8.md`**. Badge de não lidas e ciclo completo
> de presença (Online→Ausente→Offline por inatividade) não foram verificados manualmente ainda —
> não bloqueiam, mas ficam como pendência pra próxima sessão.

- [x] Fluxo completo 1:1: enviar mensagem ✅, editar ✅, deletar ✅, reagir ✅, **reply ✅ (corrigido — Bug #7)**
- [x] Fluxo de grupo: criar ✅ (fix de bug extra no `CriarGrupoDialog` — usava endpoint Admin-only),
      **ver membros ✅ novo, adicionar/remover participante ✅ novo (Bug #5, testado ao vivo)**
- [x] Fluxo de arquivo: upload válido ✅; upload inválido — não testado ainda (não bloqueante)
- [ ] Presença: ciclo completo Online→Ausente→Offline por inatividade — não testado (não bloqueante)
- [ ] Badge de não lidas zerando ao abrir conversa — não testado isoladamente, mas o badge de
      contagem foi visto atualizando corretamente durante os testes de #5 (não bloqueante)
- [x] Admin: conceder acesso ✅; **revogar ✅ (corrigido — Bug #8b, agora avisa os outros participantes também)**
- [x] Typing indicator: **✅ (corrigido — Bug #8a, causa raiz era um ref que não resetava, não a conexão)**
- [x] Emoji: reação em mensagem existente ✅; **emoji no composer ✅ (corrigido — Bug #4, picker próprio)**
- [x] Mensagem em tempo real: ✅ funciona; **resiliência de conexão ✅ (corrigido — Bug #3, retry + aviso visual)**
- [x] **Bug extra #9:** heartbeat 500 intermitente — ✅ corrigido (upsert real)
- [x] **Bug extra encontrado durante os testes de #5:** `CriarGrupoDialog` usava endpoint Admin-only,
      quebrava criação de grupo pra qualquer `CriadorDeGrupo` não-Admin — ✅ corrigido

---

## Fase 9 — Polimento pós-teste do usuário (AC-46 a AC-52) ✅ EXECUTADA em 2026-08-31

> Disparada por feedback do usuário depois de testar a Fase 8 ao vivo: "o que acha quando o acesso
> voltar deixar a mensagem para a pessoa... o que acha que precisa para deixar mais profissional".
> Ver detalhes de arquitetura (por que o evento de restauração precisa ir pelo `ChamadosHub`, não
> pelo `ChatHub`) na nota acima do AC-46 em `spec.md`.

- [x] AC-46: mensagem de sistema "...teve o acesso ao chat restaurado" — simétrica à revogação
- [x] AC-47: `ChatPerfilAtualizadoNotification` publicada em toda mudança de `ChatPerfil`, entregue via `ChamadosHub`/`Clients.User` (canal global, funciona fora da tela `/chat`)
- [x] AC-48: `AuthContext.atualizarChatPerfil` — sidebar reflete o novo `ChatPerfil` sem logout/login
- [x] AC-49: emoji picker do composer fecha ao clicar fora ou pressionar Esc
- [x] AC-50: preview inline de imagem em mensagens de arquivo do tipo imagem
- [x] AC-51: spinner no botão de enviar enquanto a mensagem está pendente
- [x] AC-52: aviso específico de erro (não genérico) + retry visual (anel vermelho) no botão de enviar
- [x] Verificação ao vivo (Playwright, 2 contas sintéticas `teste.admin2`/`teste.alvo2`): AC-46/47/48
      confirmados nas duas direções (revogar e restaurar), inclusive com a aba do usuário afetado
      **sem reload** e fora da tela `/chat` — confirma que o canal usado é mesmo o `ChamadosHub`
- [ ] AC-49 a AC-52: só revisão de código + gate checks (`npm run build`), sem verificação ao vivo
      nesta sessão — pendência pra próxima rodada de teste manual do usuário

---

## Documentação (obrigatório — última etapa)

- [x] Atualizar `spec.md`: status ajustado para refletir realidade (não "Concluída" — ver spec.md)
- [x] Atualizar `tasks.md`: checkboxes marcados conforme o que está de fato implementado/verificado
- [x] Atualizar `.specs/project/STATE.md`: resumo da sessão de 2026-08-31
- [x] Atualizar `.specs/project/ROADMAP.md`: `chat-corporativo` marcado com status real (código mergeado, testes e verificação manual pendentes)

---

## Gate Checks Finais

- [x] `dotnet build` (solução inteira) — 0 erros, 0 avisos
- [x] `dotnet test` — **291 testes, 0 falhas** (286 + 5 novos de AC-46/47 em `DefinirChatPerfilHandlerTests.cs`)
- [x] `npm run build` — 0 erros (warnings só de `node_modules`, não do projeto)
- [x] PR aberto com base `develop`
- [x] PR revisado e mergeado em `develop` (2026-08-31, PR #28) — **não mergeado em `main`/produção ainda**
- [x] Review independente (sub-agente `@review`) executado — 1 achado bloqueante encontrado e corrigido
- [x] Fase 9 (AC-46 a AC-52) implementada, com gate checks completos e AC-46/47/48 verificados ao vivo
