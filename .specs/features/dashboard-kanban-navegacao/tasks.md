# Tasks: Dashboard Clicável & Kanban com Navegação

**Status:** Todas as tarefas concluídas
**Data:** 2026-08-07

---

## Mudança 1: Dashboard clicável

### Backend

#### T1 — Adicionar `CategoriaId` ao retorno de `ContarPorCategoriaAsync` ✅
- **Arquivo:** `src/ChamadosCamarj.Domain/Interfaces/CategoriaContagem.cs`
- **Descrição:** Criar record `CategoriaContagem(string Nome, Guid? CategoriaId, int Quantidade)` com o campo `CategoriaId`.
- **Status:** Concluído. O record já existe com `Guid? CategoriaId`.

#### T2 — Atualizar assinatura do repositório ✅
- **Arquivo:** `src/ChamadosCamarj.Domain/Interfaces/IChamadoRepository.cs`
- **Descrição:** Alterar `ContarPorCategoriaAsync` de `Dictionary<string, int>` para `List<CategoriaContagem>`.
- **Status:** Concluído. A interface já retorna `Task<List<CategoriaContagem>>`.

#### T3 — Atualizar implementação do repositório ✅
- **Arquivo:** `src/ChamadosCamarj.Infrastructure/Repositories/ChamadoRepository.cs`
- **Descrição:** A consulta EF Core agrupa por `CategoriaId` e `CategoriaNome`, projetando `CategoriaContagem` com `Guid? CategoriaId`.
- **Status:** Concluído. Linha 304-312: agrupa por `{ CategoriaId, CategoriaNome }` e projeta `new CategoriaContagem(...)`.

#### T4 — Atualizar DTO de resposta ✅
- **Arquivo:** `src/ChamadosCamarj.Application/Features/Dashboard/DTOs/DashboardMetricsResponse.cs`
- **Descrição:** `PorCategoriaItem` deve incluir `Guid? CategoriaId`.
- **Status:** Concluído. `PorCategoriaItem(string CategoriaNome, Guid? CategoriaId, int Quantidade)`.

#### T5 — Atualizar handler de métricas ✅
- **Arquivo:** `src/ChamadosCamarj.Application/Features/Dashboard/Queries/ObterMetricasQueryHandler.cs`
- **Descrição:** Mapear `CategoriaContagem` → `PorCategoriaItem` preservando `CategoriaId`.
- **Status:** Concluído. Linha 34: `porCategoria.Select(c => new PorCategoriaItem(c.Nome, c.CategoriaId, c.Quantidade))`.

### Frontend

#### T6 — Tipagem do Dashboard no frontend ✅
- **Arquivo:** `frontend/src/types/dashboard.ts`
- **Descrição:** Interface `DashboardMetrics.porCategoria` deve incluir `categoriaId: string | null`.
- **Status:** Concluído. O campo `categoriaId` já está presente.

#### T7 — Adicionar `onSliceClick` ao DonutChart ✅
- **Arquivo:** `frontend/src/components/charts/DonutChart.tsx`
- **Descrição:** Prop opcional `onSliceClick?: (label: string) => void`. O `Pie` do Recharts chama `onSliceClick(data[index].label)` no `onClick`.
- **Status:** Concluído. Linha 26 e 43: `onSliceClick` prop + handler no `Pie`.

#### T8 — Adicionar `onBarClick` ao CategoriaChart ✅
- **Arquivo:** `frontend/src/features/dashboard/CategoriaChart.tsx`
- **Descrição:** Prop opcional `onBarClick?: (item: CategoriaData) => void`. O `Bar` do Recharts chama `onBarClick(data[index])` no `onClick`.
- **Status:** Concluído. Linha 11 e 26: `onBarClick` prop + handler no `Bar`.

#### T9 — Adicionar `onClick` ao DashboardKpi ✅
- **Arquivo:** `frontend/src/features/dashboard/DashboardKpi.tsx`
- **Descrição:** Prop opcional `onClick?: () => void`. Quando presente, o `Card` ganha `cursor-pointer` e `hover:bg-accent/50`.
- **Status:** Concluído. Linha 8 e 22: `onClick` prop + classes condicionais.

#### T10 — Orquestrar navegação no DashboardPage ✅
- **Arquivo:** `frontend/src/features/dashboard/DashboardPage.tsx`
- **Descrição:** Criar handlers `handleStatusClick`, `handleCategoriaClick`, `handlePrioridadeClick` usando `useNavigate`. Passar para `DonutChart`, `CategoriaChart` e `DashboardKpi`.
- **Status:** Concluído. Linhas 25-38 (handlers), 79 (KPI onClick), 117 (DonutChart onSliceClick), 128 e 139 (CategoriaChart onBarClick).

#### T11 — Migrar ChamadosListPage para useSearchParams ✅
- **Arquivo:** `frontend/src/features/chamados/ChamadosListPage.tsx`
- **Descrição:** Substituir `useState` local por `useSearchParams`. Função `parseFiltrosFromParams` extrai `status`, `prioridade`, `categoriaId`, `busca`, `slaStatus` da URL. `handleFiltrosChange` atualiza a URL com `setSearchParams`.
- **Status:** Concluído. Linha 33: `const [searchParams, setSearchParams] = useSearchParams()`. Linhas 15-29: `parseFiltrosFromParams`. Linhas 53-61: `handleFiltrosChange`.

---

## Mudança 2: Kanban com navegação

### Frontend

#### T12 — Adicionar navegação ao KanbanCard ✅
- **Arquivo:** `frontend/src/features/chamados/kanban/KanbanCard.tsx`
- **Descrição:** Importar `useNavigate`, adicionar `onClick` no div interno com `navigate(`/chamados/${chamado.id}`)`.
- **Status:** Concluído. Linhas 1 e 7: `useNavigate`. Linha 23: `onClick={() => navigate(...)}`.

#### T13 — Verificar activationConstraint no KanbanBoard ✅
- **Arquivo:** `frontend/src/features/chamados/kanban/KanbanBoard.tsx`
- **Descrição:** Confirmar que `PointerSensor` usa `activationConstraint: { distance: 8 }` para distinguir clique de arraste.
- **Status:** Concluído. Linha 28: `activationConstraint: { distance: 8 }`.

---

## Gate checks

| Check | Comando | Status |
|---|---|---|
| Backend build | `dotnet build src/ChamadosCamarj.WebApi/` | ✅ |
| Backend tests | `dotnet test tests/ChamadosCamarj.UnitTests/` | ✅ |
| Frontend build | `npm run build` (em `frontend/`) | ✅ |
| TypeScript | Verificado via `npm run build` | ✅ |

---

## Arquivos modificados (resumo)

### Backend (5 arquivos)
1. `src/ChamadosCamarj.Domain/Interfaces/CategoriaContagem.cs` — record com `Guid? CategoriaId`
2. `src/ChamadosCamarj.Domain/Interfaces/IChamadoRepository.cs` — assinatura `List<CategoriaContagem>`
3. `src/ChamadosCamarj.Infrastructure/Repositories/ChamadoRepository.cs` — query com `CategoriaId`
4. `src/ChamadosCamarj.Application/Features/Dashboard/DTOs/DashboardMetricsResponse.cs` — `PorCategoriaItem` com `CategoriaId`
5. `src/ChamadosCamarj.Application/Features/Dashboard/Queries/ObterMetricasQueryHandler.cs` — mapeamento com `CategoriaId`

### Frontend (7 arquivos)
6. `frontend/src/types/dashboard.ts` — interface com `categoriaId`
7. `frontend/src/components/charts/DonutChart.tsx` — prop `onSliceClick`
8. `frontend/src/features/dashboard/CategoriaChart.tsx` — prop `onBarClick`
9. `frontend/src/features/dashboard/DashboardKpi.tsx` — prop `onClick`
10. `frontend/src/features/dashboard/DashboardPage.tsx` — handlers de navegação
11. `frontend/src/features/chamados/ChamadosListPage.tsx` — `useSearchParams`
12. `frontend/src/features/chamados/kanban/KanbanCard.tsx` — `useNavigate` + `onClick`
