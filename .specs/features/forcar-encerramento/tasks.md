# Forçar Encerramento — Tasks

**Design**: `.specs/features/forcar-encerramento/design.md`
**Status**: Done (2026-07-19) — T1 a T9 completas, verificadas manualmente via curl contra o Supabase real

---

## Execution Plan

### Phase 1: Foundation (Parallel OK)

```
T1 [P] ┐
T2 [P] ┼──→ T4
T3 [P] ┘
```

### Phase 2: Application → Web (Sequential)

```
T4 → T5
```

### Phase 3: Frontend (Sequential)

```
T5 → T6 → T7 → T8
```

### Phase 4: Fechamento (Sequential)

```
T8 → T9
```

---

## Task Breakdown

### T1: Adicionar `AcaoHistorico.EncerramentoForcado` [P]

**What**: Novo valor no enum, ao final da lista (não renumerar os existentes)
**Where**: `src/ChamadosCamarj.Domain/Enums/AcaoHistorico.cs`
**Depends on**: None
**Reuses**: Enum existente, `HasConversion<string>()` já configurado (sem migration)
**Requirement**: FORC-04

**Tools**: Nenhum MCP/skill necessário — mudança trivial em enum existente

**Done when**:
- [ ] `EncerramentoForcado = 10` adicionado ao enum
- [ ] Build limpo

**Tests**: none (literal de enum, sem comportamento próprio)
**Gate**: `dotnet build`

---

### T2: `Chamado.ForcarEncerramento()` [P]

**What**: Método de domínio que fecha o chamado de qualquer status não-final, preservando `DataConclusao` se já existir
**Where**: `src/ChamadosCamarj.Domain/Entities/Chamado.cs`
**Depends on**: None
**Reuses**: Mesmo formato de guard de `Fechar()`/`Cancelar()`/`AlterarPrioridade()`
**Requirement**: FORC-01, FORC-05, FORC-06

**Tools**: Nenhum MCP/skill necessário

**Done when**:
- [ ] Lança `InvalidOperationException` se `Status` já for `Fechado` ou `Cancelado`
- [ ] Em qualquer outro status, seta `Status = Fechado`
- [ ] `DataConclusao` recebe `UtcNow` só se ainda for `null`; se já setada (veio de um `Resolver()` anterior), é preservada
- [ ] `DataAtualizacao` atualizado
- [ ] Testes unitários cobrindo: de `Aberto` (sem `DataConclusao` prévia), de `EmAndamento`, de `Resolvido` (preserva `DataConclusao` original), rejeição a partir de `Fechado`/`Cancelado`
- [ ] Gate check passa: `dotnet test --no-build`

**Tests**: unit (Domain)
**Gate**: `dotnet test --no-build`

---

### T3: `ForcarEncerramentoChamadoCommand` + Validator [P]

**What**: Record do command e validator com as regras de motivo (obrigatório, 10-500 caracteres)
**Where**: `src/ChamadosCamarj.Application/Features/Chamados/Commands/ForcarEncerramentoChamadoCommand.cs` + `src/ChamadosCamarj.Application/Features/Chamados/Validators/ForcarEncerramentoChamadoCommandValidator.cs`
**Depends on**: None
**Reuses**: Formato de `FecharChamadoCommand`/`ReatribuirChamadoCommandValidator`
**Requirement**: FORC-02

**Tools**: Nenhum MCP/skill necessário

**Done when**:
- [ ] `record ForcarEncerramentoChamadoCommand(Guid Id, string Motivo, Guid? UsuarioId = null, string UsuarioNome = "Sistema", string? PerfilRequisitante = null) : IRequest`
- [ ] Validator: `Id` NotEmpty; `Motivo` NotEmpty + `MinimumLength(10)` + `MaximumLength(500)`, com mensagens no padrão dos outros validators
- [ ] Testes unitários do validator: motivo vazio, motivo com 9 caracteres (falha), motivo com 10 (passa), motivo com 501 (falha)
- [ ] Gate check passa: `dotnet test --no-build`

**Tests**: unit (Validators)
**Gate**: `dotnet test --no-build`

---

### T4: `ForcarEncerramentoChamadoCommandHandler`

**What**: Orquestra guard de Admin, aplica `Chamado.ForcarEncerramento()`, persiste, audita e notifica
**Where**: `src/ChamadosCamarj.Application/Features/Chamados/Commands/ForcarEncerramentoChamadoCommandHandler.cs`
**Depends on**: T1, T2, T3
**Reuses**: `PerfilRequisitanteGuard.ExigirAdmin`, estrutura de `ReatribuirChamadoCommandHandler` (busca → aplica → `HistoricoEntrada.Criar` → `AdicionarAsync` → `StatusAlteradoNotification`)
**Requirement**: FORC-01, FORC-03, FORC-04, FORC-05

**Tools**: Nenhum MCP/skill necessário

**Done when**:
- [ ] `PerfilRequisitanteGuard.ExigirAdmin(request.PerfilRequisitante)` chamado antes de tocar no agregado
- [ ] `NotFoundException` se o chamado não existir
- [ ] `chamado.ForcarEncerramento()` chamado e persistido via `AtualizarAsync`
- [ ] `HistoricoEntrada.Criar(chamado.Id, request.UsuarioNome, request.UsuarioId, AcaoHistorico.EncerramentoForcado, detalheAnterior: <status anterior>.ToString(), detalheNovo: request.Motivo)`, adicionada via `IHistoricoRepository.AdicionarAsync`
- [ ] `StatusAlteradoNotification` publicada, mesmo padrão dos outros handlers
- [ ] Testes unitários (repositório mockado): sucesso a partir de Aberto/EmAndamento/Resolvido; `ForbiddenException` se perfil não-Admin; `InvalidOperationException`/rejeição se já Fechado/Cancelado propaga; histórico registrado com o motivo correto
- [ ] Gate check passa: `dotnet test --no-build`

**Tests**: unit (Application Handlers, mock de repositório)
**Gate**: `dotnet test --no-build`

**Commit**: `feat(chamados): adiciona forçar encerramento (Admin) com auditoria`

---

### T5: Endpoint `PATCH /api/chamados/{id}/forcar-encerramento`

**What**: DTO de request + endpoint no controller, extraindo identidade do `ICurrentUserService`
**Where**: `src/ChamadosCamarj.Application/Features/Chamados/DTOs/ForcarEncerramentoRequest.cs` (novo) + `src/ChamadosCamarj.WebApi/Controllers/ChamadosController.cs` (modificado)
**Depends on**: T4
**Reuses**: Padrão dos endpoints `Fechar`/`Cancelar`/`Reatribuir` já existentes no mesmo controller
**Requirement**: FORC-01, FORC-03

**Tools**: Nenhum MCP/skill necessário

**Done when**:
- [ ] `record ForcarEncerramentoRequest(string Motivo)`
- [ ] `[HttpPatch("{id:guid}/forcar-encerramento")]` monta o command com `_currentUser.UsuarioId`, `_currentUser.Nome`, `_currentUser.Perfil`
- [ ] `ProducesResponseType` para 204/400/403/404, no padrão dos outros endpoints de ação
- [ ] Suite completa de testes ainda passa (nenhuma regressão)
- [ ] Gate check passa: `dotnet test --no-build`

**Tests**: none (projeto não tem testes de Controller — nenhum dos outros endpoints de ação tem; comportamento já coberto pelo teste do Handler em T4)
**Gate**: `dotnet test --no-build`

**Commit**: `feat(chamados): expõe endpoint de forçar encerramento`

---

### T6: Hook `useForcarEncerramentoChamado`

**What**: Mutation hook que chama o novo endpoint e invalida cache de chamado + histórico
**Where**: `frontend/src/features/chamados/hooks/useAcoesChamado.ts` (modificado)
**Depends on**: T5
**Reuses**: Mesmo formato de `useAlterarPrioridadeChamado` (mesmo arquivo)
**Requirement**: FORC-01

**Tools**: Nenhum MCP/skill necessário

**Done when**:
- [ ] Hook chama `apiFetch` no `PATCH .../forcar-encerramento` com `{ motivo }`
- [ ] `onSuccess` invalida query do chamado e do histórico (mesmo padrão dos outros hooks de ação)
- [ ] `npm run build` limpo

**Tests**: none (projeto não tem testes unitários de frontend — decisão já registrada em TESTING.md)
**Gate**: `npm run build`

---

### T7: `ForcarEncerramentoModal.tsx`

**What**: Modal com textarea de motivo (10-500 caracteres, contador visível), erro inline, botão desabilitado até o mínimo
**Where**: `frontend/src/features/chamados/components/ForcarEncerramentoModal.tsx`
**Depends on**: T6
**Reuses**: Estrutura de `AlterarPrioridadeModal.tsx` (Dialog + hook de mutation + erro inline)
**Requirement**: FORC-02

**Tools**: Nenhum MCP/skill necessário

**Done when**:
- [ ] Textarea com contador de caracteres, mínimo 10 e máximo 500
- [ ] Botão de confirmar desabilitado enquanto o motivo não atinge o mínimo, ou durante `isPending`
- [ ] Erro do backend (`error.message`) exibido inline, mesmo padrão dos outros modais
- [ ] `npm run build` limpo

**Tests**: none
**Gate**: `npm run build`

---

### T8: Botão "Forçar Encerramento" no Detalhe do Chamado

**What**: Botão visível só para Admin e só quando `Status` não é `Fechado`/`Cancelado`, `variant="destructive"`, abre o modal de T7
**Where**: `frontend/src/features/chamados/pages/ChamadoDetailPage.tsx` (ou componente de ações equivalente já existente — localizar durante a implementação)
**Depends on**: T7
**Reuses**: Mesmo padrão condicional de exibição por perfil já usado pros outros botões de ação (Assumir/Resolver/Fechar/Cancelar)
**Requirement**: FORC-01, FORC-03, FORC-05

**Tools**: Nenhum MCP/skill necessário

**Done when**:
- [ ] Botão só renderiza se `perfil === 'Admin'`
- [ ] Botão não renderiza se `chamado.status` for `Fechado` ou `Cancelado`
- [ ] Clique abre `ForcarEncerramentoModal`
- [ ] `npm run build` limpo

**Tests**: none
**Gate**: `npm run build`

**Commit**: `feat(chamados): UI de forçar encerramento no detalhe do chamado`

---

### T9: Verificação manual + regressão completa

**What**: Rodar a suíte completa dos dois lados e validar manualmente os critérios de aceite da spec
**Where**: N/A (verificação, sem novo código)
**Depends on**: T8
**Reuses**: N/A
**Requirement**: FORC-01 a FORC-06 (todos)

**Tools**: Nenhum MCP/skill necessário

**Done when**:
- [ ] `dotnet test --no-build` — todos os testes passam (contagem igual ou maior que antes desta feature, sem exclusão silenciosa)
- [ ] `npm run build` — limpo
- [ ] Manual (Admin, navegador): forçar encerramento de um chamado `Aberto` com motivo válido → chamado vira `Fechado`, histórico mostra a entrada com o motivo
- [ ] Manual: tentar com motivo vazio/curto → erro inline, chamado não muda
- [ ] Manual: chamado com `Status = Fechado` → botão não aparece
- [ ] Manual (Atendente): botão não aparece; chamar o endpoint direto (curl/Scalar) com token de Atendente → 403
- [ ] Manual: forçar encerramento a partir de `Resolvido` (que já tinha `DataConclusao`) → `DataConclusao` não muda

**Tests**: none (verificação, não código novo)
**Gate**: full (`dotnet test --no-build` + `npm run build`)

**Resultado real (2026-07-19):**
- `dotnet test --no-build`: 195/195 (eram 177 antes da feature — 18 testes novos: 5 domínio, 6 validator, 6 handler, 1 handler adicional de preservação de `DataConclusao`)
- `npm run build`: limpo nos dois lados
- Verificação manual **via curl direto contra a API + Supabase real** (tokens JWT mintados localmente com a mesma `Auth:JwtSigningKey` do ambiente, claims idênticos ao `AutenticarGoogleCommandHandler` — sem depender do Client ID real da TI, que ainda está pendente): fechar de `Aberto` com motivo válido → 204, status `Fechado`, `DataConclusao` preenchida, histórico com `EncerramentoForcado` e o motivo certo; motivo vazio/curto → 400; Atendente → 403; repetir num chamado já `Fechado` → 400
- **Verificação visual no navegador (clicar o botão de fato) não foi feita** — mesmo bloqueio já conhecido do projeto: login real do Google ainda depende do Client ID da TI, e sem login não dá pra abrir a tela autenticada. A lógica condicional do botão (`isAdmin && !statusFinal`) e a integração do modal foram conferidas por leitura de código + `tsc` (type-safe), não por clique real. **Pendência a validar visualmente assim que o Client ID chegar.**
- **Bug real encontrado e corrigido durante esta verificação, fora do escopo original:** `ICurrentUserService.UsuarioId` sempre retornava `Guid.Empty` — o ASP.NET Core remapeia claims JWT de nome curto (`sub`) para as URIs longas de `ClaimTypes` por padrão, e o código lia pelo nome curto. Afetava **todo** `HistoricoEntrada.UsuarioId` gravado desde que o login Google real (T09/F5b) entrou em produção — Reatribuir, AlterarPrioridade, Fechar, Cancelar, Atribuir, e agora também Forçar Encerramento. Corrigido com `options.MapInboundClaims = false;` em `Program.cs` (confirmado sem efeito colateral: só 3 leituras de claim em todo o projeto, só o `Sub` era afetado). Rebuild + suíte completa (195 testes) confirmados depois da correção.
- Dois chamados de teste (`[TESTE E2E] Forcar Encerramento` e `[TESTE E2E 2] Forcar Encerramento pos-fix`) ficaram gravados no Supabase real — o app não tem (por decisão de produto) nenhum jeito de apagar chamados. Seguem lá, marcados no título, até o usuário decidir removê-los manualmente via acesso direto ao banco.

**Revisão sênior feita antes do commit (a pedido do usuário), 1 achado Médio corrigido na hora:**
- 🟡 **Médio:** `ForcarEncerramentoChamadoCommandValidator.Motivo` validava `MinimumLength(10)` sobre a string crua — um motivo tipo `"ok" + 8 espaços` batia o mínimo sem ser uma justificativa real (exatamente o que a FORC-02 queria evitar). Primeiro `MinimumLength` do projeto inteiro, então não era um padrão pré-existente sendo replicado, era um gap novo. **Corrigido**: validador agora checa `motivo.Trim().Length >= 10`; Handler grava `request.Motivo.Trim()` no `DetalheNovo` (nunca com espaço nas pontas); frontend também manda `motivo.trim()` (defesa em profundidade). 2 testes novos travando o caso (validator + handler). 197 testes passando após a correção.
- 🔵 **Baixo** (mesma causa do Médio acima, resolvido junto): frontend mandava o motivo sem `trim()`.
- **Nota, não é achado novo:** `Reatribuir`/`AlterarPrioridade`/`Fechar`/`Cancelar` seguem sem guard real de Admin no backend (RBAC "soft", pendência já registrada em `STATE.md` item 6) — Forçar Encerramento é o único desses PATCHs com guard de verdade, decisão intencional do design, não corrigida aqui por estar fora de escopo.

**Reteste manual via curl após as correções do Médio/Baixo (2026-07-19), contra a API + Supabase real:** motivo `"ok" + 8 espaços` → 400 (antes passava); motivo válido com espaços nas pontas → 204 e histórico grava `'Motivo real com espacos irrelevantes nas pontas.'` sem nenhum espaço sobrando. Terceiro chamado de teste (`[TESTE E2E 3] Validacao pos-fix trim`) ficou gravado no banco real, junto dos outros dois.

---

## Parallel Execution Map

```
Phase 1 (Parallel):
  T1 [P]
  T2 [P]  } sem dependência entre si
  T3 [P]

Phase 2 (Sequential):
  T1, T2, T3 completos, então:
    T4 ──→ T5

Phase 3 (Sequential — frontend depende do contrato do endpoint):
  T5 completo, então:
    T6 ──→ T7 ──→ T8

Phase 4 (Sequential):
  T8 completo, então:
    T9
```

---

## Task Granularity Check

| Task | Scope | Status |
|---|---|---|
| T1: Enum novo | 1 arquivo, 1 valor | ✅ Granular |
| T2: Método de domínio | 1 método, 1 arquivo | ✅ Granular |
| T3: Command + Validator | 2 arquivos coesos (um valida o outro) | ✅ OK (coeso) |
| T4: Handler | 1 handler, 1 arquivo | ✅ Granular |
| T5: DTO + endpoint | 2 arquivos coesos (DTO só existe pro endpoint) | ✅ OK (coeso) |
| T6: Hook | 1 hook, 1 arquivo (modificado) | ✅ Granular |
| T7: Modal | 1 componente | ✅ Granular |
| T8: Botão + wiring | 1 mudança condicional, 1 arquivo | ✅ Granular |
| T9: Verificação | N/A (não é código) | ✅ Granular |

---

## Diagram-Definition Cross-Check

| Task | Depends On (corpo) | Diagrama mostra | Status |
|---|---|---|---|
| T1 | None | Sem seta de entrada | ✅ Match |
| T2 | None | Sem seta de entrada | ✅ Match |
| T3 | None | Sem seta de entrada | ✅ Match |
| T4 | T1, T2, T3 | Setas de T1, T2, T3 → T4 | ✅ Match |
| T5 | T4 | T4 → T5 | ✅ Match |
| T6 | T5 | T5 → T6 | ✅ Match |
| T7 | T6 | T6 → T7 | ✅ Match |
| T8 | T7 | T7 → T8 | ✅ Match |
| T9 | T8 | T8 → T9 | ✅ Match |

---

## Test Co-location Validation

| Task | Camada criada/modificada | Matriz exige | Task diz | Status |
|---|---|---|---|---|
| T1 | Enum (Domain) | Nenhum requisito p/ literal de enum | none | ✅ OK |
| T2 | Domain (`Chamado`) | unit | unit | ✅ OK |
| T3 | Validators | unit | unit | ✅ OK |
| T4 | Application Handler | unit (integração leve/mock) | unit | ✅ OK |
| T5 | Controller (WebApi) | Nenhum requisito — nenhum outro Controller tem teste próprio no projeto | none | ✅ OK |
| T6 | Frontend hook | Nenhum — decisão do projeto é sem testes unitários de frontend | none | ✅ OK |
| T7 | Frontend componente | Nenhum (idem) | none | ✅ OK |
| T8 | Frontend (wiring) | Nenhum (idem) | none | ✅ OK |
| T9 | N/A (verificação) | N/A | none, gate full | ✅ OK |

---

## Ferramentas

Nenhuma biblioteca nova, nenhum padrão desconhecido — feature inteira reaproveita CQRS/MediatR, FluentValidation, `PerfilRequisitanteGuard`, Shadcn/Dialog e os hooks já existentes. Nenhum MCP (Context7/Microsoft Learn) necessário; nenhuma skill externa. Vou implementar diretamente (sem sub-agentes) dado o tamanho pequeno de cada task.
