# Spec: Dashboard Clicável & Kanban com Navegação

**Status:** Implementado
**Data:** 2026-08-07

---

## Mudança 1: Dashboard clicável (click-through para lista filtrada)

### Contexto

O Dashboard (`/atendimento/dashboard`) exibe métricas agregadas — cards KPI, rosca de distribuição por status, gráfico de barras por categoria e por prioridade — mas nenhum elemento era clicável. O gestor visualizava os números sem conseguir acessar os chamados subjacentes.

### Comportamento esperado

| Elemento | Ação | Navegação |
|---|---|---|
| Fatia da rosca "Distribuição por situação" | Clique | `/chamados?status={StatusChamado}` |
| Barra do gráfico "Por Categoria" | Clique | `/chamados?categoriaId={Guid}` |
| Barra do gráfico "Por Prioridade" | Clique | `/chamados?prioridade={PrioridadeChamado}` |
| Card KPI "Resolvidos Hoje" | Clique | `/chamados?status=Resolvido` |

Mapeamento dos labels da rosca para status da API:

| Label da rosca | StatusChamado |
|---|---|
| Aguardando | Aberto |
| Assumido | EmAndamento |
| Resolvido | Resolvido |
| Encerrado | Fechado |
| Cancelado | Cancelado |

Card KPI "Tempo Médio" e "SLA (mês)" não são clicáveis — são métricas informativas sem vínculo direto com uma lista filtrável.

### Contrato de API

#### GET /api/dashboard/metricas → `DashboardMetricsResponse`

```csharp
public record DashboardMetricsResponse(
    int TotalResolvidosHoje,
    double? TempoMedioResolucaoHoras,
    List<PorCategoriaItem> PorCategoria,
    List<PorPrioridadeItem> PorPrioridade,
    SlaComplianceItem? SlaCompliance
);

public record PorCategoriaItem(string CategoriaNome, Guid? CategoriaId, int Quantidade);
public record PorPrioridadeItem(string Prioridade, int Quantidade);
```

- `PorCategoriaItem.CategoriaId` é usado como parâmetro `categoriaId` na navegação.
- Quando `CategoriaId` é `null` (categoria "Sem categoria"), a barra não é clicável.

#### GET /api/dashboard/distribuicao → `DistribuicaoResponse`

```csharp
public record DistribuicaoResponse(int Aguardando, int Assumido, int Resolvido, int Encerrado, int Cancelado);
```

Os campos são mapeados para status da API via `STATUS_MAP` no frontend.

#### GET /api/chamados?status=&categoriaId=&prioridade=

Endpoint de listagem já aceita filtros via query string. A `ChamadosListPage` lê esses parâmetros via `useSearchParams` e os aplica ao componente `FiltroChamados`.

### Dependência 1: Backend — `categoriaId` nos dados de `porCategoria`

**Resolvido.** O repositório `ChamadoRepository.ContarPorCategoriaAsync()` retorna `List<CategoriaContagem>` onde `CategoriaContagem` já inclui `Guid? CategoriaId`. O handler mapeia para `PorCategoriaItem` com o ID preservado.

### Dependência 2: Frontend — `ChamadosListPage` com `useSearchParams`

**Resolvido.** `ChamadosListPage` usa `useSearchParams` do React Router v7. Os filtros `status`, `prioridade`, `categoriaId`, `busca`, `slaStatus` são lidos da URL e sincronizados bidirecionalmente.

### Implementação (frontend)

**Arquivos envolvidos:**

| Arquivo | Responsabilidade |
|---|---|
| `features/dashboard/DashboardPage.tsx` | Orquestração: handlers de clique + props para charts/KPI |
| `features/dashboard/DashboardKpi.tsx` | Card KPI com prop `onClick` opcional |
| `features/dashboard/CategoriaChart.tsx` | Gráfico de barras com prop `onBarClick` |
| `components/charts/DonutChart.tsx` | Gráfico de rosca com prop `onSliceClick` |
| `features/chamados/ChamadosListPage.tsx` | Leitura de filtros da URL via `useSearchParams` |
| `types/dashboard.ts` | Interface `DashboardMetrics` com `categoriaId` |

**Fluxo de navegação:**

```
DashboardPage
  ├── handleStatusClick(label) → STATUS_MAP[label] → navigate(`/chamados?status=X`)
  ├── handleCategoriaClick(item) → navigate(`/chamados?categoriaId=${item.categoriaId}`)
  ├── handlePrioridadeClick(item) → navigate(`/chamados?prioridade=${item.categoriaNome}`)
  └── KPI onClick → navigate('/chamados?status=Resolvido')

ChamadosListPage
  └── useSearchParams() → parseFiltrosFromParams() → aplica filtros
```

---

## Mudança 2: Kanban com navegação para detalhe do chamado

### Contexto

O Kanban (`/atendimento/kanban`) permite arrastar cards entre colunas via drag-and-drop (dnd-kit), mas clicar num card para abrir o detalhe do chamado não funcionava. O atendente precisava sair do Kanban e navegar manualmente.

### Comportamento esperado

- **Clique** no card do Kanban → navega para `/chamados/{id}` (página de detalhe).
- **Arrasto** (drag) do card → continua funcionando (alteração de status via `alterarStatus`).
- A distinção entre clique e arrasto é garantida pelo `activationConstraint: { distance: 8 }` no `PointerSensor` do `DndContext`, que exige 8px de movimento antes de ativar o drag.

### Implementação (frontend)

**Arquivos envolvidos:**

| Arquivo | Responsabilidade |
|---|---|
| `features/chamados/kanban/KanbanCard.tsx` | Card arrastável com `onClick` para navegação |
| `features/chamados/kanban/KanbanBoard.tsx` | Board com `DndContext` e `activationConstraint` |
| `features/chamados/ChamadoDetailPage.tsx` | Página de detalhe (`/chamados/:id`) |

**Fluxo:**

```
KanbanCard
  ├── useDraggable({ id: chamado.id }) → listeners, attributes no wrapper div
  ├── useNavigate() → onClick no div interno → navigate(`/chamados/${chamado.id}`)
  └── Drag vs Click: activationConstraint distance=8 distingue as intenções

KanbanBoard
  └── DndContext sensors={[PointerSensor({ activationConstraint: { distance: 8 } })]}
```

O wrapper externo (`div ref={setNodeRef} {...listeners} {...attributes}`) recebe os handlers de drag. O `div` interno contém o `onClick` com `useNavigate`. Como o `PointerSensor` só ativa o drag após 8px de movimento, um clique simples (sem arrasto) dispara apenas o `onClick`.

---

## Verificação

### Critérios de aceite — Mudança 1

- [x] Clicar numa fatia da rosca navega para `/chamados?status={mapeado}` com o filtro aplicado
- [x] Clicar numa barra de categoria navega para `/chamados?categoriaId={id}`
- [x] Barra sem `categoriaId` (null) não é clicável
- [x] Clicar numa barra de prioridade navega para `/chamados?prioridade={valor}`
- [x] Clicar no KPI "Resolvidos Hoje" navega para `/chamados?status=Resolvido`
- [x] KPI "Tempo Médio" e "SLA" não são clicáveis
- [x] A lista de chamados reflete os filtros da URL ao carregar
- [x] Alterar filtros na lista atualiza a URL

### Critérios de aceite — Mudança 2

- [x] Clicar num card do Kanban navega para `/chamados/{id}`
- [x] Arrastar um card entre colunas funciona (drag and drop)
- [x] Clique simples não dispara drag (activationConstraint distance=8)
- [x] Card em drag mostra opacidade reduzida (0.5) e indicação visual
