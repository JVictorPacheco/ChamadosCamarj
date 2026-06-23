# Fase 3 — Portal do Solicitante Tasks

**Design**: `.specs/features/frontend-portal-solicitante/design.md`
**Status**: Draft

---

## Nota sobre verificação visual

Não há ferramenta de automação de browser (Playwright/etc.) disponível neste ambiente. Os gates abaixo cobrem **build/compilação** e, quando possível, **chamadas reais à API via curl**. Eles não provam que a tela "está bonita" ou que um clique funciona de verdade — isso fica pra você verificar abrindo `http://localhost:5173` no navegador depois de cada fase. Vou avisar exatamente quando isso for necessário.

Testes automatizados de frontend: **nenhum nesta fase** (decisão do usuário) — verificação manual no navegador.

---

## Gate Check Commands

| Gate | Comando | Quando usar |
|------|---------|-------------|
| `dotnet-build` | `dotnet build` (raiz do repo) | Tarefas de backend |
| `dotnet-test` | `dotnet test` (raiz do repo) | Tarefas de backend com handler/lógica de domínio |
| `fe-build` | `npm run build` (dentro de `frontend/`) | Toda tarefa de frontend — pega erros de TS/import |
| `fe-dev-boot` | `npm run dev` + `curl -s -o /dev/null -w "%{http_code}" http://localhost:5173` | Confirma que o dev server sobe sem crash (não confirma renderização) |

---

## Execution Plan

### Phase 1: Backend — API-01 (Sequential)

```
T1 → T2 → T3
```

### Phase 2: Frontend Foundation (Sequential)

```
T4 → T5 → T6
```

### Phase 3: Infra paralela (após Phase 2)

```
T6 ──┬→ T7 → T8
     └→ T9
```

### Phase 4: Shell de autenticação e layout (após T9)

```
T9 ──┬→ T10 ─┐
     └→ T11 ─┴→ T12
```

### Phase 5: Camada de dados de Chamados (após T3 e T8)

```
T3, T8 → T13 → T14
```

### Phase 6: Componentes compartilhados (após T5; T18 após T14)

```
T5 ──┬→ T15 → T16
     └→ T17
T14 → T18
```

### Phase 7: Páginas (Parallel OK — após dependências)

```
        ┌→ T19 ─┐
T14,T11,T12 ──┼→ T20 ─┼→ T22
(T16,T17,T18) └→ T21 ─┘
```

### Phase 8: Documentação (Sequential, último)

```
T22 → T23
```

---

## Task Breakdown

### T1: Adicionar `ObterComentariosPorChamadoAsync` ao repositório

**What**: Novo método no `IChamadoRepository` + implementação em `ChamadoRepository` que retorna os comentários de um chamado ordenados por data de criação
**Where**: `src/ChamadosCamarj.Domain/Interfaces/IChamadoRepository.cs`, `src/ChamadosCamarj.Infrastructure/Repositories/ChamadoRepository.cs`
**Depends on**: None
**Reuses**: Padrão de `ObterPorIdAsync` (AsNoTracking) já existente no mesmo arquivo
**Requirement**: API-01

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Método assina `Task<IEnumerable<Comentario>> ObterComentariosPorChamadoAsync(Guid chamadoId, CancellationToken cancellationToken = default)`
- [ ] Implementação usa `_context.Set<Comentario>().Where(c => c.ChamadoId == chamadoId).OrderBy(c => c.DataCriacao).AsNoTracking()`
- [ ] Gate check passa: `dotnet build`

**Tests**: none (repositórios não têm teste dedicado neste codebase, confirmado em `TESTING.md` e na suíte atual)
**Gate**: build

---

### T2: Criar `ComentarioResponse` + `ListarComentariosQuery` + Handler

**What**: DTO de resposta, query e handler que chamam o método criado em T1
**Where**: `src/ChamadosCamarj.Application/Features/Chamados/DTOs/ComentarioResponse.cs`, `src/ChamadosCamarj.Application/Features/Chamados/Queries/ListarComentariosQuery.cs` (+ Handler no mesmo arquivo, seguindo convenção do projeto)
**Depends on**: T1
**Reuses**: Padrão de `ObterChamadoPorIdQuery`/Handler; mapeamento manual estilo `ChamadoMappings.cs`
**Requirement**: API-01

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] `ComentarioResponse(Guid Id, string Autor, string Conteudo, TipoComentario Tipo, DateTime DataCriacao)` criado
- [ ] `ListarComentariosQuery(Guid ChamadoId) : IRequest<IEnumerable<ComentarioResponse>>`
- [ ] Handler injeta `IChamadoRepository`, chama `ObterComentariosPorChamadoAsync`, mapeia pra `ComentarioResponse`
- [ ] Teste unitário do handler com `IChamadoRepository` mockado (padrão `ComentarioChamadoHandlerTests`/`AbrirChamadoHandlerTests`)
- [ ] Gate check passa: `dotnet test` — contagem de testes deve subir em pelo menos +1 em relação aos 55 atuais

**Tests**: unit (Application Handlers — mock do repositório, conforme `TESTING.md`)
**Gate**: full (`dotnet test`)

---

### T3: Endpoint `GET /chamados/{id}/comentarios`

**What**: Novo endpoint no `ChamadosController` que despacha `ListarComentariosQuery` via MediatR
**Where**: `src/ChamadosCamarj.WebApi/Controllers/ChamadosController.cs`
**Depends on**: T2
**Reuses**: Padrão dos outros `[HttpGet]` no mesmo controller
**Requirement**: API-01

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] `[HttpGet("{id:guid}/comentarios")]` retorna `200` com array de `ComentarioResponse`
- [ ] Gate check passa: `dotnet build` + `dotnet test`

**Tests**: none (controllers não têm teste dedicado neste codebase)
**Gate**: build + manual

**Verify**: subir a API (`dotnet run --project src/ChamadosCamarj.WebApi`), criar um chamado, comentar nele, chamar `GET /api/chamados/{id}/comentarios` via curl e confirmar que o comentário aparece no array.

---

### T4: Scaffold do projeto Vite + React + TS ✅ Done

**What**: Criar o projeto em `frontend/` com Vite + React + TypeScript, configurar alias `@` pro `src/`
**Where**: `frontend/` (novo diretório na raiz do repo)
**Depends on**: None
**Reuses**: Nenhum (greenfield)

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] `frontend/package.json`, `vite.config.ts`, `tsconfig.json` existem
- [ ] Alias `@` → `frontend/src` configurado em `vite.config.ts` e `tsconfig.json`
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T5: TailwindCSS v4 + shadcn/ui init + tema dark + componentes base ✅ Done

**What**: Instalar TailwindCSS (plugin `@tailwindcss/vite`) e inicializar shadcn/ui, aplicar os tokens de tema dark de `design.md` (seção "Tema Visual"), adicionar componentes base + `sidebar`
**Where**: `frontend/` (`components.json`, `src/index.css` — tokens de tema, `src/components/ui/*`)
**Depends on**: T4
**Reuses**: Nenhum

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] `npx shadcn@latest init` configurado (estilo, cor base, CSS variables)
- [ ] Tokens de `design.md` (Tema Visual) aplicados em `:root`/`.dark` — `--background`, `--foreground`, `--card`, `--border`, `--primary`, `--primary-foreground`, `--muted-foreground`, `--destructive`, `--chart-1..4`, `--sidebar*`
- [ ] Dark mode forçado como único tema (`<html class="dark">` ou equivalente, sem toggle)
- [ ] Componentes adicionados: `button`, `card`, `input`, `select`, `badge`, `textarea`, `label`, `alert`, `skeleton`, `sidebar` (+ dependências que o CLI instalar automaticamente: `separator`, `sheet`, `tooltip`)
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

**Verify**: `npm run dev`, abrir no navegador, confirmar visualmente que o fundo está escuro (não branco) e os componentes shadcn renderizam com a cor primária mint, não o azul/cinza padrão.

---

### T6: Instalar e configurar React Router, TanStack Query, React Hook Form

**What**: Instalar as 3 libs e configurar o shell em `main.tsx`/`App.tsx` (`QueryClientProvider` + `BrowserRouter`, sem rotas reais ainda)
**Where**: `frontend/src/main.tsx`, `frontend/src/App.tsx`
**Depends on**: T4
**Reuses**: Nenhum

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] `react-router`, `@tanstack/react-query`, `react-hook-form` instalados
- [ ] `App.tsx` envolto em `QueryClientProvider` e `BrowserRouter`
- [ ] Gate check passa: `fe-build` e `fe-dev-boot` (retorna 200)

**Tests**: none
**Gate**: build

---

### T7: `types/api.ts` — interfaces TS espelhando os DTOs [P]

**What**: Criar todas as interfaces/union types listadas em "Data Models" do `design.md`
**Where**: `frontend/src/types/api.ts`
**Depends on**: T4
**Reuses**: Contrato definido em `design.md` (espelha `ChamadoResponse`, `ComentarioResponse`, `CategoriaResponse`, `PagedResult<T>`, requests)

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Todas as interfaces do `design.md` presentes, sem erros de TS
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T8: `lib/api.ts` — cliente HTTP tipado

**What**: `apiFetch<T>` + `ApiError`, conforme design
**Where**: `frontend/src/lib/api.ts`
**Depends on**: T7
**Reuses**: Formato de erro do `ExceptionHandlingMiddleware` (backend)

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] `apiFetch` lança `ApiError` (com `status` e `errors?`) em respostas não-2xx
- [ ] Base URL lida de `import.meta.env.VITE_API_BASE_URL` com fallback `http://localhost:5000/api`
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T9: `AuthContext` + `useAuth` (mock auth) [P]

**What**: Contexto com os 3 perfis mockados fixos (tabela do `design.md`), persistido em `localStorage`
**Where**: `frontend/src/auth/AuthContext.tsx`
**Depends on**: T4
**Reuses**: Nenhum

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] `useAuth()` retorna `{ perfil, login(tipo), logout() }`
- [ ] Estado persiste entre reloads via `localStorage`
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T10: `ProfileSelector` (tela `/login`) [P]

**What**: Tela com 3 cards (Admin/Atendente/Solicitante) que chamam `login()`
**Where**: `frontend/src/auth/ProfileSelector.tsx`
**Depends on**: T9, T5
**Reuses**: `Card`, `Button` (shadcn)

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Os 3 perfis aparecem como opções clicáveis
- [ ] Clicar chama `login(tipo)` do `AuthContext`
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T11: `AppLayout` (sidebar + outlet + sair) [P]

**What**: Layout com menu lateral (shadcn `Sidebar`), item "Meus Chamados" (`/chamados`), botão de destaque "Abrir Chamado" (`/chamados/novo`), nome/perfil ativo e botão "sair" no rodapé da sidebar, `<Outlet />` na área de conteúdo
**Where**: `frontend/src/layouts/AppLayout.tsx`
**Depends on**: T9, T5
**Reuses**: `useAuth()`, React Router `<Outlet />`, shadcn `Sidebar`/`SidebarProvider`/`SidebarMenu`
**Requirement**: Decisão de design de 2026-06-23 (ver `design.md` — AppLayout)

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Sidebar fixa renderiza com item "Meus Chamados" e botão "Abrir Chamado"
- [ ] Item ativo (rota atual) destacado com `--sidebar-accent`/`--sidebar-primary`
- [ ] Mostra nome do perfil ativo no rodapé da sidebar
- [ ] Botão "sair" chama `logout()` e redireciona pra `/login`
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T12: Wiring de rotas em `App.tsx`

**What**: Registrar `/login`, e rotas protegidas `/chamados`, `/chamados/novo`, `/chamados/:id` dentro do `AppLayout`, com redirect pra `/login` se não houver perfil mockado. Páginas reais ainda não existem — usar placeholders (`<div>TODO</div>`) nesta tarefa
**Where**: `frontend/src/App.tsx`
**Depends on**: T6, T10, T11
**Reuses**: `useAuth()` pra checar sessão mockada

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Acessar `/chamados` sem perfil mockado redireciona pra `/login`
- [ ] Selecionar um perfil em `/login` redireciona pra `/chamados`
- [ ] Gate check passa: `fe-build` e `fe-dev-boot`

**Tests**: none
**Gate**: build

---

### T13: `features/chamados/api.ts` — funções de API

**What**: As 6 funções listadas no design (`listarChamados`, `obterChamado`, `abrirChamado`, `listarComentarios`, `comentar`, `listarCategorias`)
**Where**: `frontend/src/features/chamados/api.ts`
**Depends on**: T8, T3
**Reuses**: `apiFetch` (T8), endpoints reais do backend (incluindo o novo de T3)

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] As 6 funções implementadas e tipadas
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

**Verify**: com a API rodando (`dotnet run`) e o frontend em dev, chamar `listarCategorias()` num componente temporário e confirmar no console que retorna as 5 categorias reais.

---

### T14: Hooks do TanStack Query

**What**: `useChamados`, `useChamado`, `useComentarios`, `useCategorias`, `useAbrirChamado` (mutation), `useComentar` (mutation)
**Where**: `frontend/src/features/chamados/hooks/`
**Depends on**: T13
**Reuses**: Funções de T13, `queryClient.invalidateQueries`

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Cada hook usa a `queryKey` definida no `design.md`
- [ ] Mutations invalidam a query correspondente no sucesso
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T15: `StatusBadge` + `PrioridadeBadge` + `SlaBadge` [P]

**What**: 3 componentes pequenos de exibição, cada um mapeando um enum/condição pra uma cor de badge (shadcn `Badge`)
**Where**: `frontend/src/features/chamados/components/StatusBadge.tsx`, `PrioridadeBadge.tsx`, `SlaBadge.tsx`
**Depends on**: T5
**Reuses**: `Badge` (shadcn)
**Requirement**: FE-06 (SlaBadge)

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] `SlaBadge` mostra "Atrasado" quando `dataLimite` no passado e status não-terminal; senão mostra prazo restante
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T16: `ChamadoCard`

**What**: Card usado na listagem (título, badges, categoria, data)
**Where**: `frontend/src/features/chamados/components/ChamadoCard.tsx`
**Depends on**: T15
**Reuses**: `StatusBadge`, `PrioridadeBadge`, `SlaBadge`, `Card` (shadcn)

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Recebe um `ChamadoResponse` via prop e renderiza os dados principais
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T17: `FiltroChamados` [P]

**What**: Selects de status/categoria + campo de busca, controlado, emite `onChange` com os filtros
**Where**: `frontend/src/features/chamados/components/FiltroChamados.tsx`
**Depends on**: T5
**Reuses**: `Select`, `Input` (shadcn)

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Componente controlado, expõe `{status, categoriaId, busca}` pro pai
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T18: `ComentarioList` + `ComentarioForm`

**What**: Timeline de comentários públicos (lê de `useComentarios`, filtra `tipo === "Publico"`) + form de novo comentário (usa `useComentar`)
**Where**: `frontend/src/features/chamados/components/ComentarioList.tsx`, `ComentarioForm.tsx`
**Depends on**: T14
**Reuses**: `useComentarios`, `useComentar`, `Textarea`/`Button` (shadcn)
**Requirement**: FE-05

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Lista mostra só comentários com `tipo === "Publico"`, ordenados por data
- [ ] Form bloqueia envio com conteúdo vazio (client-side) e mantém o texto digitado se a API retornar erro
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T19: `AbrirChamadoPage` [P]

**What**: Página `/chamados/novo` com formulário (React Hook Form) usando `useAbrirChamado` e `useCategorias`
**Where**: `frontend/src/features/chamados/AbrirChamadoPage.tsx`
**Depends on**: T14, T11, T12
**Reuses**: `useAbrirChamado`, `useCategorias`, `useAuth` (preenche solicitanteNome/Email automaticamente), componentes shadcn de form
**Requirement**: FE-02

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Envio com sucesso redireciona pro detalhe do chamado criado
- [ ] Erros 400 da API aparecem ao lado do campo certo (via `setError`)
- [ ] `solicitanteNome`/`solicitanteEmail` vêm do perfil mockado ativo, sem campo editável
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T20: `ChamadosListPage` [P]

**What**: Página `/chamados` — lista filtrada pelo e-mail do perfil ativo, com `FiltroChamados`, `ChamadoCard` e paginação
**Where**: `frontend/src/features/chamados/ChamadosListPage.tsx`
**Depends on**: T14, T16, T17, T11, T12
**Reuses**: `useChamados`, `ChamadoCard`, `FiltroChamados`
**Requirement**: FE-03

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Lista só chamados do `solicitanteEmail` do perfil ativo
- [ ] Estado vazio mostra call-to-action pra abrir chamado
- [ ] Paginação usa `temProxima`/`temAnterior` de `PagedResult`
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T21: `ChamadoDetailPage` [P]

**What**: Página `/chamados/:id` — todos os dados do chamado + `ComentarioList`/`ComentarioForm`
**Where**: `frontend/src/features/chamados/ChamadoDetailPage.tsx`
**Depends on**: T14, T18, T11, T12, T15
**Reuses**: `useChamado`, `ComentarioList`, `ComentarioForm`, badges de T15
**Requirement**: FE-04

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] 404 mostra página de "não encontrado" com link de volta
- [ ] Bloqueia visualmente (mensagem) se `solicitanteEmail` do chamado != perfil ativo — conforme nota do spec (trava só de UI)
- [ ] Gate check passa: `fe-build`

**Tests**: none
**Gate**: build

---

### T22: Integração final — conectar páginas reais nas rotas

**What**: Substituir os placeholders de T12 pelas páginas reais (T19, T20, T21), adicionar navegação (botão "Abrir chamado" na lista, click no card → detalhe)
**Where**: `frontend/src/App.tsx`
**Depends on**: T19, T20, T21

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Fluxo completo navegável: `/login` → escolher Solicitante → `/chamados` (vazio) → abrir chamado → `/chamados/novo` → enviar → redireciona pro detalhe → comentar → ver na timeline
- [ ] Gate check passa: `fe-build` e `fe-dev-boot`

**Tests**: none
**Gate**: build

**Verify (manual, sua parte)**: abrir `http://localhost:5173` no navegador e percorrer o fluxo completo acima de ponta a ponta.

---

### T23: Atualizar documentação (`.specs/` + `STATE.md`)

**What**: Atualizar `STRUCTURE.md` (nova pasta `frontend/`), `TESTING.md` (seção frontend: "sem testes automatizados, verificação manual"), `STACK.md` (libs do frontend confirmadas), `ROADMAP.md` (Fase 3 concluída) e `STATE.md`
**Where**: `.specs/codebase/STRUCTURE.md`, `.specs/codebase/TESTING.md`, `.specs/codebase/STACK.md`, `.specs/project/ROADMAP.md`, `.specs/project/STATE.md`
**Depends on**: T22

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Todos os 5 arquivos refletem o estado real pós-Fase 3
- [ ] `STATE.md` aponta pra Fase 4 (Email + Storage) como próximo passo

**Tests**: none
**Gate**: none (revisão de conteúdo)

**Commit**: `docs(specs): atualizar documentação pós Fase 3`

---

## Parallel Execution Map

```
Phase 1 (Sequential):     T1 → T2 → T3
Phase 2 (Sequential):     T4 → T5 → T6
Phase 3 (Parallel):       T6 done → { T7 → T8 } [P]  e  T9 [P]
Phase 4:                  T9 done → { T10 [P], T11 [P] } → T12
Phase 5 (Sequential):     T3, T8 done → T13 → T14
Phase 6 (Parallel):       T5 done → { T15 → T16, T17 } [P]   e   T14 done → T18
Phase 7 (Parallel):       T14,T11,T12 (+T16,T17,T18,T15) done → { T19 [P], T20 [P], T21 [P] }
Phase 8 (Sequential):     T19,T20,T21 done → T22 → T23
```

---

## Task Granularity Check

| Task | Scope | Status |
|------|-------|--------|
| T1 | 1 função (interface+impl) | ✅ Granular |
| T2 | 1 DTO + 1 query + 1 handler (1 feature CQRS, padrão do projeto) | ✅ Granular |
| T3 | 1 endpoint | ✅ Granular |
| T4 | 1 scaffold | ✅ Granular |
| T5 | 1 setup (tailwind+shadcn, 9 componentes gerados via CLI, não escritos à mão) | ✅ Granular |
| T6 | 3 libs, 1 shell de configuração | ✅ Granular |
| T7 | 1 arquivo de tipos | ✅ Granular |
| T8 | 1 função + 1 classe de erro | ✅ Granular |
| T9 | 1 contexto + 1 hook | ✅ Granular |
| T10 | 1 componente | ✅ Granular |
| T11 | 1 componente | ✅ Granular |
| T12 | 1 arquivo (rotas) | ✅ Granular |
| T13 | 6 funções (1 arquivo, API client da feature) | ✅ Granular |
| T14 | 6 hooks (1 arquivo/pasta, todos sobre as funções de T13) | ✅ Granular |
| T15 | 3 componentes pequenos cohesivos (badges) | ✅ Granular |
| T16 | 1 componente | ✅ Granular |
| T17 | 1 componente | ✅ Granular |
| T18 | 2 componentes acoplados (lista+form do mesmo conceito) | ✅ Granular |
| T19 | 1 página | ✅ Granular |
| T20 | 1 página | ✅ Granular |
| T21 | 1 página | ✅ Granular |
| T22 | 1 arquivo (wiring) | ✅ Granular |
| T23 | 5 arquivos de doc | ✅ Granular |

---

## Diagram-Definition Cross-Check

| Task | Depends On (corpo da tarefa) | Diagrama mostra | Status |
|------|-------------------------------|------------------|--------|
| T1 | None | Início da Phase 1 | ✅ Match |
| T2 | T1 | T1 → T2 | ✅ Match |
| T3 | T2 | T2 → T3 | ✅ Match |
| T4 | None | Início da Phase 2 | ✅ Match |
| T5 | T4 | T4 → T5 | ✅ Match |
| T6 | T4 | T4 → T5 → T6 (sequencial, T6 após T5 no diagrama da Phase 2) | ✅ Match |
| T7 | T4 | T6 ──→ T7 (Phase 3) | ✅ Match |
| T8 | T7 | T7 → T8 | ✅ Match |
| T9 | T4 | T6 ──→ T9 (Phase 3, paralelo a T7/T8) | ✅ Match |
| T10 | T9, T5 | T9 → T10 | ✅ Match |
| T11 | T9, T5 | T9 → T11 | ✅ Match |
| T12 | T6, T10, T11 | T10, T11 → T12 | ✅ Match |
| T13 | T8, T3 | T3, T8 → T13 (Phase 5) | ✅ Match |
| T14 | T13 | T13 → T14 | ✅ Match |
| T15 | T5 | T5 → T15 (Phase 6) | ✅ Match |
| T16 | T15 | T15 → T16 | ✅ Match |
| T17 | T5 | T5 → T17 | ✅ Match |
| T18 | T14 | T14 → T18 | ✅ Match |
| T19 | T14, T11, T12 | Phase 7 entrada | ✅ Match |
| T20 | T14, T16, T17, T11, T12 | Phase 7 entrada | ✅ Match |
| T21 | T14, T18, T11, T12, T15 | Phase 7 entrada | ✅ Match |
| T22 | T19, T20, T21 | Phase 8 entrada | ✅ Match |
| T23 | T22 | T22 → T23 | ✅ Match |

**Nota**: T19/T20/T21 são `[P]` entre si (não dependem uma da outra), mas cada uma tem dependências individuais diferentes (T20 depende de mais componentes que T19). Isso é consistente — paralelismo exige só que não dependam *entre si*, não que tenham o mesmo conjunto de pré-requisitos.

---

## Test Co-location Validation

| Task | Camada criada/modificada | Matriz exige | Tarefa diz | Status |
|------|---------------------------|----------------|------------|--------|
| T1 | Infrastructure/Repository | none (sem teste dedicado, confirmado em TESTING.md) | none | ✅ OK |
| T2 | Application/Handler | unit (mock do repositório) | unit | ✅ OK |
| T3 | WebApi/Controller | none (sem teste dedicado) | none | ✅ OK |
| T4–T23 | Frontend (todas as camadas) | none (decisão do usuário nesta fase) | none | ✅ OK |

---

## Pergunta antes de executar

Pra cada tarefa, não há MCP ou Skill especializado necessário além das ferramentas padrão (Read/Write/Edit/Bash) — não detectei MCP de browser automation neste ambiente, então a verificação visual final (T22) depende de você abrir o navegador. As skills `run` e `verify` (disponíveis no seu ambiente) podem ajudar a subir e checar a API/dev server nas verificações intermediárias.

Confirma que posso seguir assim, ou prefere que eu use alguma ferramenta específica (ex: alguma extensão/MCP de browser que você tenha) pra essas verificações visuais?
