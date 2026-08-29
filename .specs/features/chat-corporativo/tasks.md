# Chat Corporativo — Tasks

> **Branch:** `feature/chat-corporativo`
> **Spec:** `spec.md` | **Design:** `design.md`
> **Orquestração:** Claude Code (Sonnet 4.6 — spec; Opus 4.8 — backend e review; Sonnet 4.6 — frontend)
> **Gate checks:** `dotnet test` + `npm run build` antes de qualquer commit

---

## Fase 1 — Domain

- [ ] Criar enum `ChatPerfil` em `Domain/Enums/`
- [ ] Criar enum `ChatConversaTipo` em `Domain/Enums/`
- [ ] Criar enum `ChatMensagemTipo` em `Domain/Enums/`
- [ ] Criar enum `StatusPresenca` em `Domain/Enums/`
- [ ] Criar enum `ChatAcao` em `Domain/Enums/`
- [ ] Adicionar propriedade `ChatPerfil` em `UsuarioPerfil` com método `DefinirChatPerfil()`
- [ ] Criar entidade `ChatConversa`
- [ ] Criar entidade `ChatParticipante`
- [ ] Criar entidade `ChatMensagem`
- [ ] Criar entidade `ChatMensagemReacao`
- [ ] Criar entidade `ChatPresenca`
- [ ] Criar entidade `ChatHistorico`
- [ ] Criar interface `IChatConversaRepository`
- [ ] Criar interface `IChatMensagemRepository`
- [ ] Criar interface `IChatPresencaRepository`
- [ ] Criar interface `IChatHistoricoRepository`

---

## Fase 2 — Infrastructure

- [ ] Criar migration `AddChatPerfilUsuario` (coluna em `UsuariosPerfil`, default `SemAcesso`)
- [ ] Criar migration `AddChatFeature` (6 novas tabelas)
- [ ] Criar `ChatConversaConfiguration` (Fluent API)
- [ ] Criar `ChatParticipanteConfiguration`
- [ ] Criar `ChatMensagemConfiguration`
- [ ] Criar `ChatMensagemReacaoConfiguration`
- [ ] Criar `ChatPresencaConfiguration`
- [ ] Criar `ChatHistoricoConfiguration`
- [ ] Implementar `ChatConversaRepository`
- [ ] Implementar `ChatMensagemRepository`
- [ ] Implementar `ChatPresencaRepository`
- [ ] Implementar `ChatHistoricoRepository`
- [ ] Criar `ChatPresencaWorker` (IHostedService — marca Ausente/Offline por inatividade)
- [ ] Criar bucket `chat-arquivos` no Supabase Storage
- [ ] Registrar repositórios e worker em `Program.cs`

---

## Fase 3 — Application (Commands)

- [ ] `DefinirChatPerfilCommand` + Handler + Validator
- [ ] `CriarConversaCommand` + Handler + Validator
- [ ] `CriarGrupoCommand` + Handler + Validator
- [ ] `EnviarMensagemCommand` + Handler + Validator
- [ ] `EnviarArquivoCommand` + Handler + Validator
- [ ] `EditarMensagemCommand` + Handler + Validator
- [ ] `DeletarMensagemCommand` + Handler + Validator
- [ ] `AdicionarReacaoCommand` + Handler + Validator
- [ ] `MarcarComoLidoCommand` + Handler
- [ ] `AtualizarPresencaCommand` + Handler

---

## Fase 4 — Application (Queries + DTOs)

- [ ] `ListarConversasQuery` + Handler
- [ ] `ListarMensagensQuery` + Handler (paginado)
- [ ] `ListarPresencasQuery` + Handler
- [ ] `ListarHistoricoChatQuery` + Handler (Admin only)
- [ ] Criar todos os DTOs (`ChatConversaResponse`, `ChatMensagemResponse`, `ChatReacaoResponse`, `ChatPresencaResponse`, `ChatArquivoResponse`, `ChatHistoricoResponse`)
- [ ] Criar extension methods de mapeamento (`ChatMappings.cs`)

---

## Fase 5 — WebApi

- [ ] Criar `ChatHub` em `WebApi/Hubs/`
- [ ] Registrar `ChatHub` em `Program.cs` (`app.MapHub<ChatHub>("/hubs/chat")`)
- [ ] Criar `ChatController` com todos os endpoints definidos em `design.md`
- [ ] Criar `ChatPresencaController` (heartbeat + listagem de presença)
- [ ] Adicionar `PATCH /api/usuarios/{id}/chat-perfil` em `UsuariosController`
- [ ] Adicionar `GET /api/chat/historico` em `ChatController` (Admin only)

---

## Fase 6 — Testes Backend

- [ ] `DefinirChatPerfilHandlerTests` (ACs 01-04)
- [ ] `ChatPresencaHandlerTests` (ACs 05-09)
- [ ] `EnviarMensagemHandlerTests` (ACs 10-11)
- [ ] `CriarGrupoHandlerTests` (ACs 12-14)
- [ ] `EnviarArquivoHandlerTests` (ACs 15-17)
- [ ] `EditarMensagemHandlerTests` (ACs 21-22)
- [ ] `DeletarMensagemHandlerTests` (ACs 23-25)
- [ ] `ChatHistoricoHandlerTests` (ACs 35-36)
- [ ] Gate check: `dotnet test` — 0 falhas

---

## Fase 7 — Frontend

### Tipos e API
- [ ] Adicionar tipos de chat em `types/api.ts`
- [ ] Criar `features/chat/api.ts` com todas as funções HTTP

### Hooks
- [ ] `useConversas.ts` (TanStack Query)
- [ ] `useMensagens.ts` (TanStack Query, paginado)
- [ ] `usePresencas.ts` (TanStack Query)
- [ ] `useChat.ts` (mutations: enviar, editar, deletar, reagir, marcar lido)
- [ ] `useChatSignalR.ts` (SignalR + heartbeat 30s com visibilitychange)

### Componentes
- [ ] `PresencaBadge.tsx` — bolinha de status colorida
- [ ] `PresencaPanel.tsx` — painel com todos os usuários e status
- [ ] `ConversaItem.tsx` — item da lista de conversas
- [ ] `ConversaList.tsx` — lista lateral com badge de não lidas
- [ ] `MensagemItem.tsx` — bolha de mensagem (texto/arquivo/sistema + reações + ações)
- [ ] `TypingIndicator.tsx` — "[Nome] está digitando..."
- [ ] `MensagemList.tsx` — scroll de mensagens com load more
- [ ] `MensagemInput.tsx` — input com emoji picker, upload, reply preview
- [ ] `CriarGrupoDialog.tsx` — modal de criação de grupo
- [ ] `ChatPerfilSelect.tsx` — select em Admin > Usuários
- [ ] `ChatPage.tsx` — layout principal (split: lista + conversa ativa)

### Integração
- [ ] Adicionar rota `/chat` em `App.tsx` (dentro de `ProtectedRoute`)
- [ ] Adicionar link "Chat" na sidebar (apenas para `ChatPerfil !== SemAcesso`)
- [ ] Adicionar badge vermelho de não lidas no link do chat na sidebar
- [ ] Adicionar coluna "Chat" na tela Admin > Usuários com `ChatPerfilSelect`
- [ ] Gate check: `npm run build` — 0 erros, 0 warnings

---

## Fase 8 — Verificação Manual (UI)

- [ ] Fluxo completo 1:1: enviar mensagem, editar, deletar, reagir, reply
- [ ] Fluxo de grupo: criar, adicionar participante, remover participante
- [ ] Fluxo de arquivo: upload válido, upload inválido (tipo/tamanho)
- [ ] Presença: abrir sistema → Online; inatividade → Ausente → Offline; retomar → Online
- [ ] Badge de não lidas: receber mensagem em outra tela → badge aparece → abrir conversa → badge zera
- [ ] Admin: conceder acesso → usuário vê chat; revogar → mensagem de sistema na conversa
- [ ] Typing indicator: digitar → aparece para o outro; parar → some

---

## Documentação (obrigatório — última etapa)

- [ ] Atualizar `spec.md`: preencher tabela de rastreabilidade, marcar status como `Concluída`
- [ ] Atualizar `tasks.md`: todos os checkboxes marcados
- [ ] Atualizar `.specs/project/STATE.md`: resumo da sessão, decisões tomadas, aprendizados
- [ ] Atualizar `.specs/project/ROADMAP.md`: marcar `chat-corporativo` como ✅

---

## Gate Checks Finais

- [ ] `dotnet test` — X testes, 0 falhas
- [ ] `npm run build` — 0 erros, 0 warnings
- [ ] PR aberto com base `develop`
- [ ] PR revisado (Opus 4.8) e mergeado em `develop`
