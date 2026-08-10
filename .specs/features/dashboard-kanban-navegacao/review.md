# Code Review: Dashboard Clicável & Kanban com Navegação

**Data:** 2026-08-07
**Build:** `npm run build` — passou (sem erros TS/Vite)
**Veredito:** Aprovado

---

## Resumo

A feature implementa duas mudanças principais:

1. **Dashboard clicável** — cards KPI, gráfico de rosca (distribuição por situação), gráfico de barras (por categoria e por prioridade) navegam para `/chamados` com filtros aplicados via query string.
2. **Kanban com navegação** — clique no card do Kanban navega para `/chamados/{id}` (página de detalhe). Arrasto (drag-and-drop) continua funcionando via `activationConstraint: { distance: 8 }` no `PointerSensor` do dnd-kit.

A implementação segue a spec em `.specs/features/dashboard-kanban-navegacao/spec.md` e as convenções do projeto em `.specs/codebase/CONVENTIONS.md`.

---

## Arquivos analisados

### Mudança 1 — Dashboard clicável

| Arquivo | Responsabilidade |
|---|---|
| `frontend/src/features/dashboard/DashboardPage.tsx` | Orquestração: handlers de clique + props para charts/KPI. Mapeamento `STATUS_MAP` (label da rosca → `StatusChamado`). Controle de acesso por perfil. |
| `frontend/src/features/dashboard/DashboardKpi.tsx` | Card KPI com prop `onClick` opcional. KPIs sem `onClick` não são clicáveis (Tempo Médio, SLA). |
| `frontend/src/features/dashboard/CategoriaChart.tsx` | Gráfico de barras (Recharts) com prop `onBarClick`. Reutilizado para categorias e prioridades. |
| `frontend/src/components/charts/DonutChart.tsx` | Gráfico de rosca com prop `onSliceClick`. Rótulos customizados via `renderValueLabel`. |
| `frontend/src/features/dashboard/hooks.ts` | Hooks TanStack Query (`useDashboardMetrics`, `useDashboardDistribuicao`) com `staleTime: 15_000` e invalidação via SignalR. |
| `frontend/src/features/dashboard/api.ts` | Funções `obterMetricas()` e `obterDistribuicao()`. |
| `frontend/src/types/dashboard.ts` | Tipos `DashboardMetrics` e `DistribuicaoResponse` que espelham DTOs do backend. |

### Mudança 2 — Kanban com navegação

| Arquivo | Responsabilidade |
|---|---|
| `frontend/src/features/chamados/kanban/KanbanCard.tsx` | Card arrastável com `useDraggable` (dnd-kit). `div` externo com listeners/attributes de drag, `div` interno com `onClick` para `navigate(/chamados/${id})`. Opacidade 0.5 durante drag. |
| `frontend/src/features/chamados/kanban/KanbanBoard.tsx` | Board com `DndContext`, `PointerSensor({ activationConstraint: { distance: 8 } })`, optimistic update no `handleDragEnd`, rollback em caso de erro. |
| `frontend/src/features/chamados/kanban/KanbanColumn.tsx` | Coluna droppable com `useDroppable`. Destaque visual (`bg-primary/10`) quando `isOver`. |
| `frontend/src/features/chamados/kanban/useKanbanChamados.ts` | Hook TanStack Query que busca até 100 chamados para o Kanban. Invalidação via SignalR. |
| `frontend/src/features/chamados/KanbanPage.tsx` | Página do Kanban com controle de acesso (bloqueia Solicitante). |

### Suporte

| Arquivo | Responsabilidade |
|---|---|
| `frontend/src/features/chamados/ChamadosListPage.tsx` | Leitura de filtros da URL via `useSearchParams`. Validação de `status`, `prioridade`, `slaStatus` contra arrays de valores válidos. Sincronização bidirecional URL ↔ filtros. |
| `frontend/src/features/chamados/ChamadoDetailPage.tsx` | Página de detalhe (`/chamados/:id`). Destino da navegação a partir do KanbanCard. |
| `frontend/src/features/chamados/components/ChamadoCard.tsx` | Card reutilizado na listagem e no Kanban. |
| `frontend/src/features/chamados/components/FiltroChamados.tsx` | Componente de filtros com suporte a `status`, `prioridade`, `categoriaId`, `busca`, `slaStatus`, `motivoEncerramento`. |
| `frontend/src/features/chamados/hooks/useChamados.ts` | Hook TanStack Query para listagem de chamados com filtros. |
| `frontend/src/features/chamados/api.ts` | Funções de API: `listarChamados`, `alterarStatus`, `atribuirChamado`, etc. `buildQueryString` converte filtros em query string. |
| `frontend/src/types/api.ts` | Tipos compartilhados: `StatusChamado`, `PrioridadeChamado`, `ChamadoResponse`, etc. |
| `frontend/src/App.tsx` | Rotas: `/chamados`, `/chamados/:id`, `/atendimento/kanban`, `/atendimento/dashboard`. |

---

## Checklist de Critérios de Aceite

### Mudança 1 — Dashboard clicável

- [x] Clicar numa fatia da rosca navega para `/chamados?status={mapeado}` com o filtro aplicado
- [x] Clicar numa barra de categoria navega para `/chamados?categoriaId={id}`
- [x] Barra sem `categoriaId` (null) não é clicável
- [x] Clicar numa barra de prioridade navega para `/chamados?prioridade={valor}`
- [x] Clicar no KPI "Resolvidos Hoje" navega para `/chamados?status=Resolvido`
- [x] KPI "Tempo Médio" e "SLA" não são clicáveis
- [x] A lista de chamados reflete os filtros da URL ao carregar
- [x] Alterar filtros na lista atualiza a URL

### Mudança 2 — Kanban com navegação

- [x] Clicar num card do Kanban navega para `/chamados/{id}`
- [x] Arrastar um card entre colunas funciona (drag and drop)
- [x] Clique simples não dispara drag (`activationConstraint distance=8`)
- [x] Card em drag mostra opacidade reduzida (0.5) e indicação visual

---

## Análise detalhada

### Segurança

| Item | Status | Detalhe |
|---|---|---|
| Validação de `status`, `prioridade`, `slaStatus` na URL | ✅ | `ChamadosListPage.tsx:23-28` valida contra arrays de valores conhecidos. Parâmetros inválidos são silenciosamente ignorados. |
| Validação de `categoriaId` na URL | ⚠️ | `categoriaId` é lido diretamente da URL sem validação de formato GUID. Um valor inválido é enviado à API como string crua. A API deve lidar com isso (provavelmente retorna 400 ou lista vazia), mas seria desejável validar no frontend. |
| `over.id as StatusChamado` no Kanban | ✅ | `KanbanBoard.tsx:48` faz cast do `over.id` para `StatusChamado`. Os droppable IDs são definidos como literais `StatusChamado` em `KanbanColumn.tsx:13`, então o cast é seguro. |
| Controle de acesso por perfil | ✅ | Dashboard (`DashboardPage.tsx:40`) e Kanban (`KanbanPage.tsx:12`) bloqueiam Solicitante. Detail Page (`ChamadoDetailPage.tsx:296`) verifica `solicitanteEmail` para Solicitantes. |
| Injeção via query string | ✅ | `categoriaId`, `status`, `prioridade` são passados como valores de query string para o endpoint `GET /api/chamados`, que é seguro (leitura). |

### Correção

| Item | Status | Detalhe |
|---|---|---|
| Mapeamento de labels da rosca → status da API | ✅ | `DashboardPage.tsx:11-17` `STATUS_MAP`: Aguardando→Aberto, Assumido→EmAndamento, Resolvido→Resolvido, Encerrado→Fechado, Cancelado→Cancelado. Confere com a spec. |
| `handleCategoriaClick` com `categoriaId` null | ✅ | `DashboardPage.tsx:31` só navega se `item.categoriaId` for truthy. Atende AC. |
| `handlePrioridadeClick` com `categoriaNome` | ✅ | `DashboardPage.tsx:37` usa `item.categoriaNome` que, no contexto do gráfico de prioridades, contém o valor da prioridade (o array é remapeado na linha 138). |
| Fluxo de navegação Kanban → Detalhe | ✅ | `KanbanCard.tsx:23` `onClick={() => navigate(`/chamados/${chamado.id}`)}` no div interno. O div externo tem os handlers de drag. |
| Distinção clique vs drag | ✅ | `KanbanBoard.tsx:28` `PointerSensor({ activationConstraint: { distance: 8 } })` exige 8px de movimento para ativar o drag. Cliques sem arrasto disparam apenas o `onClick`. |
| Optimistic update + rollback | ✅ | `KanbanBoard.tsx:54-56` atualiza cache antes da chamada API. `KanbanBoard.tsx:62` reverte com `invalidateQueries` no catch. |
| Drag para fora do Kanban | ✅ | `KanbanBoard.tsx:45` `if (!over) return` — early return para drops fora de qualquer droppable. |
| Drag para mesma coluna | ✅ | `KanbanBoard.tsx:51` `if (chamado.status === novoStatus) return` — evita chamada de API desnecessária. |
| Sincronização URL ↔ filtros | ✅ | `ChamadosListPage.tsx:53-61` `handleFiltrosChange` cria `new URLSearchParams` e chama `setSearchParams(params, { replace: true })`. `ChamadosListPage.tsx:15-29` `parseFiltrosFromParams` lê os filtros da URL. |
| Paginação preserva filtros | ✅ | `ChamadosListPage.tsx:63-68` `setPagina` copia `searchParams` existentes, altera apenas o parâmetro `pagina`. |
| `fecharChamado` e `cancelarChamado` com motivo | ✅ | `ChamadoDetailPage.tsx:71-72` passam `motivo`, `motivoOutro` e `observacao` corretamente para as mutations. |

### Edge Cases

| Cenário | Status | Detalhe |
|---|---|---|
| Dashboard sem dados (métricas vazias) | ✅ | `DashboardPage.tsx:119-121`: "Nenhum chamado no sistema" quando `totalDistribuicao === 0`. `DashboardPage.tsx:130`: "Nenhum chamado ativo" quando `porCategoria.length === 0`. |
| Dashboard com erro de API | ✅ | `DashboardPage.tsx:65-69`: `<Alert variant="destructive">` com mensagem "Serviço indisponível". Exibido separadamente para métricas e distribuição. |
| Kanban sem chamados | ✅ | `KanbanColumn.tsx:33`: "Nenhum chamado" em colunas vazias. |
| Kanban com mais de 100 chamados | ⚠️ | `useKanbanChamados.ts:15`: `tamanhoPagina: 100` fixo. Chamados além do limite não aparecem. Sem paginação/infinite scroll. |
| URL com parâmetros inválidos | ✅ | `ChamadosListPage.tsx:23-28`: status/prioridade/slaStatus inválidos são ignorados (não incluídos no filtro). `busca` é passado como string (seguro). |
| `categoriaId` não-GUID na URL | ⚠️ | Passado diretamente à API sem validação. A API deve tratá-lo, mas o frontend poderia validar formato UUID. |
| Solicitante acessando Dashboard/Kanban | ✅ | Bloqueado com `<Alert variant="destructive">` + link "Voltar para a lista". |
| Card Kanban sem `id` válido | ✅ | Impossível — `chamado.id` vem do backend como GUID. |
| Concorrência: drag no Kanban + ação no detalhe | ✅ | Optimistic update no Kanban atualiza o cache local. SignalR notifica outros clients. O detalhe não tem subscription SignalR (comportamento existente, não introduzido por esta feature). |

### Performance

| Item | Status | Detalhe |
|---|---|---|
| `staleTime` nas queries de dashboard | ✅ | 15 segundos (`hooks.ts:13,33`). Razoável para dados quase-real-time com invalidação SignalR. |
| `staleTime` nas queries do Kanban | ✅ | 10 segundos (`useKanbanChamados.ts:18`). Razoável para dados que mudam com drag-and-drop. |
| `staleTime` padrão do QueryClient | ✅ | 30 segundos (`App.tsx:33`). Aplica-se a `useChamados` quando não definido explicitamente. |
| Optimistic update no Kanban | ✅ | Atualização imediata do cache seguida de chamada API. Rollback apenas em caso de erro. |
| Invalidação SignalR no Kanban sem filtro | ⚠️ | `useKanbanChamados.ts:23`: invalida em **qualquer** evento SignalR. Os hooks do dashboard filtram por `event.type`. Impacto: refetches desnecessários do Kanban quando métricas do dashboard atualizam. |
| Re-renderizações de gráficos | ✅ | `useCallback` nos handlers do dashboard evita recriação de funções passadas como props. Charts Recharts re-renderizam apenas quando `data` ou `onClick` mudam. |
| `ChamadoCard` sem memoização | ✅ | Segue a convenção do projeto de evitar `React.memo`/`useMemo` desnecessários. |
| Tamanho do bundle | ✅ | Build de produção: ~1MB JS (gzip: ~303KB). Sem regressão desta feature. Apenas warning de chunk size (pré-existente). |

### Conformidade com CONVENTIONS.md

| Regra | Status | Detalhe |
|---|---|---|
| Function components com `export` nomeado | ✅ | Nenhum `export default` nos componentes da feature (exceto `App`). |
| Erro inline com `useState<string \| null>` + `<Alert variant="destructive">` | ✅ | Todos os estados de erro seguem este padrão. |
| Sem toast library | ✅ | Nenhum uso de `useToast`/Sonner. |
| Estados de loading: `isPending` (não `isLoading`) | ✅ | Todos os hooks TanStack Query usam `isPending` (v5). |
| `useCallback` apenas quando necessário | ✅ | Usado nos handlers do dashboard (passados como props para charts) e nos handlers do `ChamadosListPage` (dependências de `useEffect`/memo). |
| Shadcn/ui de `@/components/ui/` | ✅ | Todos os imports de componentes UI seguem este padrão. |
| API em `features/{feature}/api.ts` | ✅ | `dashboard/api.ts`, `chamados/api.ts`. |
| Hooks em `features/{feature}/hooks/` | ✅ | `dashboard/hooks.ts`, `chamados/hooks/useChamados.ts`, `chamados/kanban/useKanbanChamados.ts`. |
| Tipos em `types/api.ts` e `types/dashboard.ts` | ✅ | Type unions de string (não `enum` do TS), interfaces para DTOs. |
| Tailwind com tokens de tema | ✅ | `bg-card`, `text-muted-foreground`, `border-border`, `var(--chart-1)`, `var(--status-good)`, etc. Sem cores hardcoded. |
| `queryKey` hierárquico | ✅ | `['dashboard', 'metricas']`, `['chamados', 'kanban']`, `['chamados', filtros]`. |
| `buildQueryString` filtra `undefined` | ✅ | `api.ts:35` `if (valor !== undefined)`. |

---

## Pontos de Atenção (não bloqueantes)

### 1. `categoriaId` sem validação de formato GUID
**Arquivo:** `frontend/src/features/chamados/ChamadosListPage.tsx:18`
**Descrição:** O parâmetro `categoriaId` da URL é passado diretamente à API sem validação de formato UUID. Um valor como `?categoriaId=lixo` geraria uma chamada de API com parâmetro inválido.
**Sugestão:** Adicionar validação de GUID antes de incluir no filtro:
```tsx
const categoriaId = searchParams.get('categoriaId')
// Validar formato UUID antes de usar
const isValidGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(categoriaId ?? '')
```

### 2. Kanban limitado a 100 chamados
**Arquivo:** `frontend/src/features/chamados/kanban/useKanbanChamados.ts:15`
**Descrição:** O hook busca `tamanhoPagina: 100` fixo. Se houver mais de 100 chamados ativos, os excedentes não aparecem no Kanban.
**Sugestão:** Aumentar o limite ou implementar paginação/infinite scroll no Kanban, ou adicionar um indicador visual de que há mais chamados não exibidos.

### 3. Invalidação SignalR sem filtro de evento no Kanban
**Arquivo:** `frontend/src/features/chamados/kanban/useKanbanChamados.ts:23-25`
**Descrição:** O callback de subscribe invalida o cache do Kanban em **qualquer** evento SignalR, inclusive `MetricasAtualizadas` (que não afeta o Kanban). Compare com `hooks.ts:17-20` que filtra por `event.type === 'MetricasAtualizadas'`.
**Sugestão:** Filtrar por tipos de evento relevantes (ex: `StatusAlterado`, `ChamadoAtualizado`).

### 4. Nomenclatura confusa em `handlePrioridadeClick`
**Arquivo:** `frontend/src/features/dashboard/DashboardPage.tsx:36-38`
**Descrição:** O handler usa o tipo `{ categoriaNome: string; ... }` herdado de `CategoriaData`, mas `categoriaNome` contém o valor da prioridade neste contexto. Funcionalmente correto, mas o nome do campo é enganoso.
**Sugestão:** Renomear o parâmetro ou criar um tipo específico para prioridade.

### 5. `buildQueryString` não filtra `null`
**Arquivo:** `frontend/src/features/chamados/api.ts:35`
**Descrição:** A função `buildQueryString` filtra apenas `valor !== undefined`, mas não `valor !== null`. Se um filtro chegar como `null` (ex: de um formulário ou estado inicial), seria enviado como `?categoriaId=null`.
**Sugestão:** Alterar a condição para `if (valor !== undefined && valor !== null)`.

---

## Gate Checks

| Check | Resultado |
|---|---|
| `npm run build` (frontend) | ✅ Passou — `tsc -b && vite build` sem erros |
| `dotnet test` (backend) | Não executado (feature é exclusivamente frontend) |

⚠️ Warnings no build são de `@microsoft/signalr` (lib de terceiros) e chunk size (pré-existente), não introduzidos por esta feature.

---

## Veredito Final

**Aprovado.** A implementação atende todos os critérios de aceite da spec, está em conformidade com `CONVENTIONS.md`, e os gate checks passam. Os 5 pontos de atenção listados são melhorias incrementais (não bloqueantes) que podem ser tratadas em iterações futuras. Nenhum problema de segurança, correção ou performance foi identificado como crítico.
