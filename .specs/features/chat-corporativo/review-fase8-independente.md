# Chat Corporativo — Review Independente da Fase 8 (`@review`)

**Data:** 2026-08-31
**Escopo:** `git diff` não commitado (working tree) — 8 defeitos da Fase 8 (7 relatados + 1 extra) corrigidos numa sessão anterior, conforme descrito em `review-fase8.md`.
**Revisor:** sub-agente `@review` independente, sem participação na implementação das correções.
**Veredito:** **BLOQUEANTE** — um achado crítico (#1) reabre exatamente o Bug #8b (AC-03) por um caminho paralelo não coberto por nenhuma correção nem teste. Todo o resto do lote (Bugs #9, #7, #4, #8a, #3, #5 + fix do `CriarGrupoDialog`) está correto e consistente com o que `review-fase8.md` afirma.

---

## Resumo

O lote revisado toca: 3 endpoints novos em `ChatController` (`ObterConversa`, `AdicionarParticipante`, `RemoverParticipante`), 2 handlers novos (`AdicionarParticipanteCommandHandler`, `RemoverParticipanteCommandHandler`), 1 query nova (`ObterConversaQueryHandler`), correções em `ChatPresencaRepository`, `ListarMensagensQueryHandler`, `DefinirChatPerfilCommandHandler`, `MensagemInput.tsx`, `useChatSignalR.ts`, e um componente novo `MembrosGrupoDialog.tsx`. Também inclui uma mudança não anunciada em `review-fase8.md`: `ChatPerfil` passou a ser parâmetro de `CriarUsuarioPerfilCommand`/`AtualizarUsuarioPerfilCommand`, com um campo "Chat" novo no dialog geral de edição de usuário (`UsuarioFormDialog.tsx`).

Todos os 6 bugs relatados pelo usuário e os 2 extras (heartbeat, `CriarGrupoDialog`) foram, de fato, corrigidos como descrito — verifiquei cada um lendo o código, não apenas o relato. O problema é uma mudança adicional, fora do escopo documentado, que compromete justamente a garantia que o Bug #8b tinha acabado de estabelecer.

Gate checks (build/test/npm build) rodados por mim de forma independente: **todos passam limpos**, números batem exatamente com o que `review-fase8.md`/`spec.md` reportam.

---

## Achados por Severidade

### 🔴 Bloqueante

#### 1. `ChatPerfil` agora tem dois caminhos de escrita — um deles sem auditoria, sem notificação em tempo real e sem mensagem de sistema

**Onde:**
- `src/ChamadosCamarj.Application/Features/Usuarios/Commands/AtualizarUsuarioPerfilCommandHandler.cs:46`
- `frontend/src/features/admin/components/UsuarioFormDialog.tsx:212-233` (campo "Chat" novo, `Select` com `chatPerfil`)

**O que acontece:**
Existiam (e continuam existindo) dois lugares na UI de Admin para alterar o `ChatPerfil` de um usuário:

1. **`ChatPerfilSelect.tsx`** (coluna "Chat" na tabela de `UsuariosPage`) → `PATCH /usuarios/{id}/chat-perfil` → `DefinirChatPerfilCommandHandler`. Esse handler faz **tudo** que a spec exige: grava `ChatHistorico` (`AcessoConcedido`/`AcessoRevogado`, com `perfilAnterior`/`perfilNovo`), e — desde a correção do Bug #8b nesta mesma sessão — cria uma `ChatMensagem` de sistema em cada conversa ativa do usuário revogado e publica `ChatNovaMensagemNotification` (tempo real para os outros participantes) além de `ChatAcessoRevogadoNotification` (tempo real para o próprio revogado).

2. **Campo "Chat" novo dentro do dialog geral "Editar usuário"** (`UsuarioFormDialog.tsx`) → `PUT /usuarios/{id}` → `AtualizarUsuarioPerfilCommand` → `AtualizarUsuarioPerfilCommandHandler.cs:46`, que só faz:
   ```csharp
   usuario.Atualizar(request.Nome, request.Perfil, request.GrupoId);
   usuario.DefinirChatPerfil(request.ChatPerfil);   // setter puro, sem side effects
   await _usuarioPerfilRepository.AtualizarAsync(usuario, cancellationToken);
   ```
   `UsuarioPerfil.DefinirChatPerfil()` (`Domain/Entities/UsuarioPerfil.cs:32`) é um setter simples — não gera `ChatHistorico`, não dispara nenhuma notificação SignalR, não cria mensagem de sistema em conversa nenhuma.

**Impacto:** se um Admin revogar (ou conceder) acesso ao chat usando o dialog geral de edição — que é a forma mais natural de "editar um usuário" no painel Admin, e agora tem um campo "Chat" bem visível ali dentro — nenhum dos participantes das conversas do usuário afetado vê a mensagem de sistema exigida pelo **AC-03**, o usuário revogado não é avisado em tempo real (só ao relogar e pegar um token novo), e a ação não fica registrada em `ChatHistorico`, violando **AC-35** ("qualquer ação no chat gera log auditado") e **AC-36** (Admin precisa ver histórico completo). Ou seja: o mesmo bug que a correção do #8b acabou de fechar (revogação não avisa os outros participantes) reabre por uma porta lateral que nem existia antes desta sessão.

**Por que isso não apareceu nos testes/gate checks:** os únicos testes tocados (`AtualizarUsuarioPerfilHandlerTests.cs`, `AtualizarUsuarioPerfilValidatorTests.cs`) foram só ajustados mecanicamente para aceitar o novo parâmetro posicional (`ChatPerfil.SemAcesso` em todos os casos) — nenhum deles verifica que a mudança de `ChatPerfil` por esse caminho gera auditoria ou notificação, porque esse comportamento nunca existiu ali e não é o que esses testes cobrem.

**Não documentado:** `review-fase8.md` não menciona em nenhum momento a adição de `ChatPerfil` aos comandos de Usuario nem o campo novo no `UsuarioFormDialog`. `STATE.md` (sessão de 2026-08-31 parte 2) também não menciona essa mudança entre os 8 achados corrigidos. É uma alteração de escopo sem spec, sem AC correspondente, e sem o aviso que a Constitution do projeto exige para mudança de contrato/comportamento (`CONVENTIONS.md` §4.7, regra 2 e 3).

**Sugestão:** ou (a) remover o campo "Chat" de `UsuarioFormDialog.tsx` e manter `ChatPerfilSelect.tsx` como único caminho, ou (b) fazer `AtualizarUsuarioPerfilCommandHandler` publicar via `IMediator` o mesmo `DefinirChatPerfilCommand` (ou extrair a lógica de auditoria/notificação para um serviço de domínio compartilhado) quando `request.ChatPerfil` for diferente do valor atual. Qualquer uma resolve; o estado atual (dois caminhos, comportamento divergente) não deveria ir para `main`.

---

### 🟡 Não bloqueante (atenção recomendada)

#### 2. Janela de corrida residual em `ChatPresencaRepository.AdicionarOuAtualizarAsync`
**Arquivo:** `src/ChamadosCamarj.Infrastructure/Repositories/Chat/ChatPresencaRepository.cs:31-54`

O fix do Bug #9 cobre corretamente o caso comum (duas requisições simultâneas que ambas veem "não existe" e tentam `Add` — a segunda cai no `catch (DbUpdateException ... UniqueViolation)` e vira no-op). Mas existe uma janela mais estreita, não coberta: `AtualizarPresencaCommandHandler` já faz uma consulta própria (`ObterPorUsuarioAsync`) *antes* de chamar `AdicionarOuAtualizarAsync`, que faz uma segunda consulta interna (`existente`). Se uma requisição concorrente inserir a presença exatamente entre essas duas consultas, a chamada corrente cai no branch `_dbSet.Update(presenca)` com uma entidade cujo `Id` (gerado ao construir `new ChatPresenca(...)` no handler, pois a primeira consulta retornou `null`) não corresponde a nenhuma linha existente no banco — o `UPDATE` afeta 0 linhas, silenciosamente, sem exceção. Não reproduz o 500 relatado (esse caso está mesmo coberto), mas é uma perda silenciosa de heartbeat em uma janela ainda menor. Baixo risco prático, mas vale registrar — um upsert real (`ON CONFLICT ... DO UPDATE`) fecharia toda a classe de problema de uma vez, como o próprio `review-fase8.md` já cogitava como alternativa.

#### 3. Timer de "parar de digitar" não é limpo ao trocar de conversa
**Arquivo:** `frontend/src/features/chat/components/MensagemInput.tsx:24-44` (`usePararDigitarDebounce`)

O `useEffect` que limpa estado ao trocar `conversaId` (linha 64-69) reseta `digitandoRef.current = false`, mas não cancela um `setTimeout` pendente do debounce (`timerRef` fica isolado dentro do hook, sem `useEffect` de cleanup nem acesso externo para cancelar). Se o usuário digitar, trocar de conversa em menos de 3s, o timer antigo ainda dispara e chama `onPararDigitar(conversaIdAntiga)` — evento indo para uma conversa que o componente já não está mais "olhando". Pré-existente ao fix do Bug #8a (não introduzido por ele), mas como a correção do #8a mexeu exatamente nesse hook, seria uma boa oportunidade de fechar isso junto. Impacto prático baixo (o pior caso é um `PararDigitar` supérfluo emitido para um grupo SignalR do qual o cliente pode já ter saído).

#### 4. Citação de mensagem deletada aparece como "[arquivo]"
**Arquivo:** `src/ChamadosCamarj.Application/Features/Chat/Queries/ListarMensagens/ListarMensagensQueryHandler.cs:35-49`, `ChatMensagemRepository.ObterConteudosPorIdsAsync`

`Deletar()` (`ChatMensagem.cs:90-92`) zera `Conteudo` para `null`. O novo `ObterConteudosPorIdsAsync` retorna esse `Conteudo` (não `ConteudoOriginal`), então quando a mensagem citada foi deletada, `conteudoOriginal` chega `null` e cai no fallback `?? "[arquivo]"` — mostrando "[arquivo]" para uma citação de mensagem deletada, o que é enganoso (aparenta ser um anexo, não uma mensagem removida). É exatamente o mesmo comportamento já usado no preview de resposta em `MensagemInput.tsx:138` (`respostaParaMensagem.conteudo ?? '[arquivo]'`), então não é uma inconsistência introduzida por este fix — é uma lacuna pré-existente do recurso de citação como um todo, só que agora fica visível também na lista carregada (antes só aparecia no preview de quem estava respondendo). Não bloqueante, mas vale nota para uma correção futura (diferenciar `Deletada` de "sem conteúdo por ser arquivo").

---

### 🟢 Observações menores (informativo)

- **Emoji picker novo (`MensagemInput.tsx:158-172`)** não fecha ao clicar fora nem com `Escape` — só fecha ao escolher um emoji ou trocar de conversa. Funcional, mas UX menor.
- **`ChatParticipanteRemovidoNotificationHandler`** (`ChatSignalRNotificationHandlers.cs`) notifica só o grupo SignalR da conversa, ao contrário do handler de `Adicionado`, que também notifica o novo participante individualmente via `Clients.User`. Isso é assimétrico mas correto no contexto (quem foi removido não precisa ser avisado por esse evento específico — ele já vê a mensagem de sistema enquanto ainda estiver no grupo SignalR da conversa). O fato de o servidor não forçar a saída do grupo SignalR de quem foi removido (`SairConversa` depende de ação do cliente) é comportamento pré-existente do `ChatHub`, não tocado por este diff.
- A tabela de rastreabilidade em `spec.md` referencia nomes de arquivo de teste (`DefinirChatPerfilHandlerTests.cs`, `CriarGrupoHandlerTests.cs` etc.) que não existem em `tests/` — mas isso está corretamente sinalizado como "⬜ Não implementado (teste)" na própria tabela, não é uma alegação falsa. Só reforça que os ACs 41-45 (gerenciamento de membros) não têm nenhuma cobertura automatizada, apenas verificação manual — consistente com o que `spec.md`/`review-fase8.md` já admitem.

---

## Verificação dos achados de `review-fase8.md` (reivindicações vs. código)

| Bug | Reivindicação | Verificado no código | Resultado |
|---|---|---|---|
| #9 — heartbeat 500 | Fix igual ao padrão de `AdicionarReacaoAsync` (catch `PostgresException UniqueViolation`) | `ChatPresencaRepository.cs:31-54` vs `ChatMensagemRepository.cs:147-160` — padrão try/catch/detach idêntico | ✅ Confirmado (com ressalva #2 acima, janela residual) |
| #7 — reply sem citação | `ListarMensagensQueryHandler` passa a buscar em lote via `ObterConteudosPorIdsAsync`, evita N+1 | Código confere: `WHERE Id IN (...)` único, fallback `"[arquivo]"` | ✅ Confirmado (com ressalva #4, pré-existente) |
| #8b — revogação não avisa outros | `DefinirChatPerfilCommandHandler` agora publica `ChatNovaMensagemNotification` por conversa | Confirmado nas linhas 74-78 | ✅ Confirmado **só para este handler** — ver achado #1 (bloqueante) |
| #4 — emoji não funciona | Picker próprio substituindo truque de seletor nativo | Confirmado, `EMOJIS_COMPOSER` + popover | ✅ Confirmado |
| #8a — digitando não funciona | Causa raiz: `digitandoRef` nunca resetado no debounce | Confirmado, linha 40 do `MensagemInput.tsx` fecha exatamente esse gap | ✅ Confirmado (com ressalva #3, cleanup de timer) |
| #3 — resiliência de conexão | Retry manual com backoff (1s...30s) para a tentativa inicial, `status` exposto | Confirmado em `useChatSignalR.ts`, lógica de `cancelado`/`tentarConectar`/`onclose` coerente, sem race óbvia entre cleanup e retry | ✅ Confirmado |
| #5 — gerenciamento de membros | Endpoints novos + `ChatPerfilGuard.ExigirCriadorDaConversaOuAdmin` + `MembrosGrupoDialog.tsx` | Autorização correta nos 2 handlers (criador ou Admin, `403` caso contrário); edge cases (já participante → 409, não existe → 404, não é grupo → 400, usuário sem acesso → 400) todos tratados | ✅ Confirmado |
| Extra — `CriarGrupoDialog` 403 | Trocado `/api/usuarios` (Admin-only) por `/chat/presencas` | Confirmado no diff, mesmo padrão de `NovaConversaDialog` | ✅ Confirmado |

---

## Segurança — Endpoints novos de `ChatController`

| Endpoint | Checagem | Resultado |
|---|---|---|
| `GET /chat/conversas/{id}` | `ObterConversaQueryHandler` exige que o requisitante seja participante ativo (`403 ForbiddenException` caso contrário) | ✅ Correto |
| `POST /chat/grupos/{id}/participantes` | `ChatPerfilGuard.ExigirCriadorDaConversaOuAdmin(conversa.CriadoPorId, request.RequisitanteId, request.RequisitantePerfil)` — só criador do grupo específico ou `Perfil == "Admin"` (comparação por claim `perfil`, populada como `usuario.Perfil.ToString()` no JWT — consistente) | ✅ Correto, condiz com AC-42/AC-43 |
| `DELETE /chat/grupos/{id}/participantes/{usuarioId}` | Mesma guarda | ✅ Correto |
| Autenticação de base | `Program.cs:160-163` — `SetFallbackPolicy(RequireAuthenticatedUser)`, então todo endpoint sem `[AllowAnonymous]` já exige JWT válido, cobrindo AC-37 | ✅ Correto |

Não encontrei forma de um participante comum (não-criador, não-Admin) adicionar/remover alguém do grupo — a guarda é checada antes de qualquer mutação em ambos os handlers.

---

## Correção — Handlers novos

| Edge case | `AdicionarParticipanteCommandHandler` | `RemoverParticipanteCommandHandler` |
|---|---|---|
| Conversa não existe | `NotFoundException` | `NotFoundException` |
| Conversa não é grupo | `BadRequestException` | `BadRequestException` |
| Requisitante não é criador nem Admin | `ForbiddenException` (via guard) | `ForbiddenException` (via guard) |
| Usuário-alvo não existe | `NotFoundException` | N/A (usa participante já existente) |
| Usuário-alvo sem acesso ao chat (`SemAcesso`) | `BadRequestException` | N/A |
| Usuário-alvo já é participante ativo | `ConflictException` | N/A |
| Usuário-alvo foi participante e saiu (inativo) | `Reativar()` no participante existente, sem duplicar linha | N/A |
| Usuário-alvo não é participante (remover) | N/A | `NotFoundException` |
| Auditoria | `ChatHistorico.Criar(..., ParticipanteAdicionado, ...)` | `ChatHistorico.Criar(..., ParticipanteRemovido, ...)` |
| Mensagem de sistema + tempo real | `ChatMensagem.CriarSistema` + `ChatNovaMensagemNotification` + `ChatParticipanteAdicionadoNotification` (grupo + usuário novo individualmente) | `ChatMensagem.CriarSistema` + `ChatNovaMensagemNotification` + `ChatParticipanteRemovidoNotification` (grupo) |

Todos os edge cases pedidos na tarefa estão cobertos corretamente.

---

## Gate Checks (executados por mim, de forma independente)

| Check | Comando | Resultado |
|---|---|---|
| Build da solução inteira | `dotnet build` (raiz do repo) | ✅ **0 Erro(s), 0 Aviso(s)** |
| Testes unitários | `dotnet test tests/ChamadosCamarj.UnitTests/` | ✅ **216 testes, 216 aprovados, 0 falhas** — bate exatamente com o número citado em `review-fase8.md` e `spec.md` |
| Build do frontend | `npm run build` (dentro de `frontend/`) | ✅ Exit code 0. Únicos warnings: `INVALID_ANNOTATION` de `node_modules/@microsoft/signalr` (biblioteca de terceiros) e aviso de chunk size (`index-*.js` ~1MB) — ambos pré-existentes, não introduzidos por este diff |

Nenhuma divergência entre o que `review-fase8.md` reporta como resultado de gate checks e o que reproduzi de forma independente.

---

## Veredito Final

**BLOQUEANTE.** O lote corrige corretamente todos os 8 defeitos documentados em `review-fase8.md` — verifiquei cada um lendo o código, não apenas aceitando a alegação — e os três gate checks passam limpos com números idênticos aos relatados. Isso por si só seria "Aprovado".

Mas a sessão também introduziu, sem passar por spec/design/aviso de mudança de contrato, um segundo caminho de escrita para `ChatPerfil` (via `AtualizarUsuarioPerfilCommand`/`UsuarioFormDialog.tsx`) que **não replica** a auditoria, a mensagem de sistema nem a notificação em tempo real que o próprio Bug #8b acabou de corrigir no caminho original (`DefinirChatPerfilCommand`). Um Admin usando a forma mais óbvia de editar um usuário (o dialog "Editar usuário", que agora tem um campo "Chat" bem visível) revoga acesso ao chat de um jeito que viola AC-03 e AC-35 silenciosamente, sem qualquer teste ou gate check capaz de pegar isso — porque o comportamento esperado (auditoria + notificação) nunca foi o que esse handler faz.

Recomendação: resolver o achado #1 antes de promover `develop` para `main`. Os achados #2-#4 podem ficar para uma iteração seguinte sem risco de segurança/auditoria; os itens 🟢 são informativos.
