# Relatório Mensal + Rosca no Dashboard — Tasks

**Design**: `.specs/features/relatorio-mensal/design.md`
**Status**: Approved

**Execução:** direta (sem sub-agentes paralelos) — a Fase 0 e a Fase 1 mexem no mesmo arquivo de interface (`IHistoricoRepository`), então rodam em sequência apesar de serem conceitualmente independentes.

**Correção feita durante o Execute (2026-07-14):** T1-T3 abaixo descrevem a Distribuição como "eventos dos últimos 7 dias" via `HistoricoEntrada` — o usuário corrigiu isso durante a implementação: a rosca é a **situação atual** (Aguardando/Assumido/Resolvido/Cancelado), não um evento de período. Isso simplificou T1-T3: nenhum método novo de repositório foi necessário, só reaproveitar `ContarPorStatusAsync` (já existia). Ver detalhes em `design.md`, seção "Adendo".

---

## Execution Plan

### Fase 0: Dashboard — Tendência (linha) → Distribuição (rosca)

```
T1 → T2 → T3 → T4 → T5 → T6
```

### Fase 1: Relatório Mensal — Backend

```
T7 → T8 → T9 → T10 → T11
```

### Fase 2: Relatório Mensal — Frontend

```
T12 → T13 → T14 → T15 → T17 → T18 → T19
              (T4 do Fase 0) ──────┘
```

### Fase 3: Verificação end-to-end

```
T20
```

---

## Task Breakdown

### T1: Adicionar `ContarPorAcaoNoPeriodoAsync` em `IHistoricoRepository`

**What**: Novo método que conta eventos de histórico por ação (`Criado`/`Resolvido`/`Cancelado`) dentro de um período, usado tanto pela Distribuição do Dashboard quanto (depois) pelo Relatório
**Where**: `src/ChamadosCamarj.Domain/Interfaces/IHistoricoRepository.cs` (assinatura) + `src/ChamadosCamarj.Infrastructure/Repositories/HistoricoRepository.cs` (implementação)
**Depends on**: None
**Reuses**: Mesmo padrão de `GroupBy`+`Select` de `ChamadoRepository.ContarPorCategoriaAsync`
**Requirement**: Adendo gráficos de rosca (Design)

**Done when**:
- [ ] `Task<Dictionary<AcaoHistorico, int>> ContarPorAcaoNoPeriodoAsync(IEnumerable<AcaoHistorico> acoes, DateTime inicio, DateTime fimExclusivo, CancellationToken)` implementado
- [ ] Filtra por `DataHora >= inicio && DataHora < fimExclusivo && acoes.Contains(Acao)`
- [ ] `dotnet build` sem erros

**Tests**: none (método de repositório/EF Core — sem cobertura de repositório neste projeto, ver TESTING.md)
**Gate**: build

---

### T2: Substituir `ObterTendenciaQuery` por `ObterDistribuicaoQuery`

**What**: Novo Query+Handler que retorna totais de Abertos/Resolvidos/Cancelados dos últimos N dias; remove o Query/Handler antigo de tendência
**Where**: `src/ChamadosCamarj.Application/Features/Dashboard/Queries/ObterDistribuicaoQuery.cs`, `ObterDistribuicaoQueryHandler.cs`, `DTOs/DistribuicaoResponse.cs` (novos); deletar `ObterTendenciaQuery.cs`, `ObterTendenciaQueryHandler.cs`, `TendenciaResponse` de `DashboardMetricsResponse.cs` (ou onde estiver)
**Depends on**: T1
**Reuses**: Estrutura de `ObterMetricasQueryHandler`
**Requirement**: Adendo gráficos de rosca

**Done when**:
- [ ] `DistribuicaoResponse(int Abertos, int Resolvidos, int Cancelados)` criado
- [ ] Handler usa `ChamadoRepository` (Abertos via `DataCriacao`, Resolvidos via `DataConclusao` — reaproveita a correção já feita) + `ContarPorAcaoNoPeriodoAsync` (T1) só para Cancelados
- [ ] Query/Handler/DTO antigos de Tendência removidos (não deixar código morto)
- [ ] `dotnet build` sem erros

**Tests**: none (handler simples, delega toda a lógica pro repositório — mesmo critério já aplicado ao `ObterMetricasQueryHandler`, que também não tem teste dedicado)
**Gate**: build

---

### T3: Atualizar `DashboardController` — rota `/distribuicao`

**What**: Trocar o endpoint `GET /api/dashboard/tendencia` por `GET /api/dashboard/distribuicao?dias=7`
**Where**: `src/ChamadosCamarj.WebApi/Controllers/DashboardController.cs`
**Depends on**: T2
**Reuses**: Estrutura do endpoint `/metricas` já existente

**Done when**:
- [ ] Endpoint antigo `/tendencia` removido, novo `/distribuicao` criado
- [ ] `curl http://localhost:5000/api/dashboard/distribuicao?dias=7` retorna `{ abertos, resolvidos, cancelados }` com números reais
- [ ] `dotnet build` sem erros

**Tests**: none
**Gate**: build + verificação manual via curl

---

### T4: Criar componente `DonutChart` compartilhado

**What**: Componente Recharts genérico de rosca (`PieChart`+`Pie` com `innerRadius`), reaproveitado pela Distribuição do Dashboard e pelo SLA do Relatório
**Where**: `frontend/src/components/charts/DonutChart.tsx`
**Depends on**: None (pode rodar em paralelo com T1-T3, é frontend)
**Reuses**: Mesmo padrão de import/estrutura de `CategoriaChart.tsx`

**Done when**:
- [ ] `DonutChart({ data: { label: string; value: number; color: string }[] })` renderiza rosca com `ResponsiveContainer`
- [ ] Mostra `Tooltip` e `Legend`
- [ ] `npm run build` sem erros de tipo

**Tests**: none (decisão do projeto: sem teste de componente isolado — ver TESTING.md)
**Gate**: build

---

### T5: Atualizar tipos e hooks do Dashboard no frontend

**What**: Substituir `TendenciaItem`/`TendenciaResponse`/`useDashboardTendencia` por `DistribuicaoResponse`/`useDashboardDistribuicao`; remover `TendenciaChart.tsx`
**Where**: `frontend/src/types/dashboard.ts`, `frontend/src/features/dashboard/hooks.ts`, deletar `frontend/src/features/dashboard/TendenciaChart.tsx`
**Depends on**: T3 (precisa do endpoint novo pra apontar), T4 (usa o `DonutChart`)
**Reuses**: `apiFetch`, padrão de hook de `useDashboardMetrics`

**Done when**:
- [ ] Tipo `DistribuicaoResponse` espelha o DTO do backend
- [ ] `useDashboardDistribuicao(dias)` busca via `apiFetch('/dashboard/distribuicao?dias=...')`
- [ ] Código morto (tipos/hooks/componente de tendência) removido
- [ ] `npm run build` sem erros

**Tests**: none
**Gate**: build

---

### T6: Atualizar `DashboardPage` — seção "Distribuição (últimos 7 dias)"

**What**: Trocar a seção "Tendência (7 dias)" (`TendenciaChart`) pela nova rosca (`DonutChart` alimentado por `useDashboardDistribuicao`)
**Where**: `frontend/src/features/dashboard/DashboardPage.tsx`
**Depends on**: T5
**Reuses**: `DonutChart` (T4)

**Done when**:
- [ ] Seção renderiza rosca com 3 fatias (Abertos vermelho, Resolvidos verde, Cancelados uma terceira cor)
- [ ] `npm run build` sem erros
- [ ] Verificação manual no navegador: números da rosca batem com os dados reais (checar via curl na API)

**Tests**: none (verificação manual, per TESTING.md)
**Gate**: build + verificação manual no navegador

**Commit**: `feat(dashboard): trocar Tendência (linha) por Distribuição (rosca)`

---

### T7: Criar `EventoRelatorioItem` + `ObterEventosParaRelatorioAsync`

**What**: Projeção + método de repositório que traz, num JOIN só, os eventos (`Criado`/`Resolvido`/`Cancelado`) do período com os dados do chamado necessários (categoria, atendente, prazo, conclusão)
**Where**: `src/ChamadosCamarj.Domain/Interfaces/IHistoricoRepository.cs` (assinatura), `src/ChamadosCamarj.Infrastructure/Repositories/HistoricoRepository.cs` (implementação), `EventoRelatorioItem` como record em `src/ChamadosCamarj.Domain/` (projeção compartilhada Domain, já que a interface do repositório vive lá)
**Depends on**: T6 concluído (mesmo arquivo `IHistoricoRepository.cs` que T1 editou — evita conflito de edição simultânea)
**Reuses**: Padrão de `JOIN`/`Select` direto de `ChamadoRepository.ContarPorCategoriaAsync`
**Requirement**: REL-01 a REL-05, REL-10

**Done when**:
- [ ] `ObterEventosParaRelatorioAsync(DateTime inicio, DateTime fimExclusivo, CancellationToken): Task<List<EventoRelatorioItem>>` implementado
- [ ] `JOIN` com `Chamados`/`Categorias` traz `CategoriaNome`, `ResponsavelId`, `ResponsavelNome`, `DataConclusao`, `DataLimite` no mesmo round-trip
- [ ] `dotnet build` sem erros

**Tests**: none (repositório/EF Core, ver TESTING.md)
**Gate**: build

---

### T8: Criar DTOs de `RelatorioMensalResponse`

**What**: Todos os records de resposta descritos no Design (`RelatorioMensalResponse`, `SlaResponse`, `PorCategoriaItem` próprio da feature, `PorAtendenteItem`, `ComparacaoMesAnteriorResponse`)
**Where**: `src/ChamadosCamarj.Application/Features/Relatorios/DTOs/`
**Depends on**: T7
**Reuses**: Nenhum DTO existente reaproveitado diretamente — `PorCategoriaItem` é recriado aqui (evita acoplar `Relatorios` em `Dashboard.DTOs`)
**Requirement**: REL-01 a REL-05

**Done when**:
- [ ] Todos os records do Design criados exatamente como especificado
- [ ] `dotnet build` sem erros

**Tests**: none (só records de dados)
**Gate**: build

---

### T9: Criar `ObterRelatorioMensalQuery` + Validator

**What**: Query record + validator (ano/mês dentro de um intervalo razoável — não aceitar mês > 12, ano fora de um range plausível)
**Where**: `src/ChamadosCamarj.Application/Features/Relatorios/Queries/ObterRelatorioMensalQuery.cs`, `Validators/ObterRelatorioMensalQueryValidator.cs`
**Depends on**: T8
**Reuses**: Padrão de `AlterarPrioridadeChamadoCommandValidator` (FluentValidation, Fase 6)
**Requirement**: Edge case da spec (ano/mês inválido → 400)

**Done when**:
- [ ] `ObterRelatorioMensalQuery(int Ano, int Mes, Guid? ResponsavelId = null)` criado
- [ ] Validator rejeita `Mes` fora de 1-12 e `Ano` implausível (ex: < 2020 ou > ano corrente + 1)
- [ ] `dotnet build` sem erros

**Tests**: unit (validators têm teste dedicado no projeto — ver `Fase6ValidatorsTests.cs` como referência)
**Gate**: quick (`dotnet test --no-build`)

---

### T10: Implementar `ObterRelatorioMensalQueryHandler`

**What**: A lógica de agregação central — busca eventos do mês pedido e do anterior, filtra por `ResponsavelId` se informado, calcula totais/quebras/SLA/comparação
**Where**: `src/ChamadosCamarj.Application/Features/Relatorios/Queries/ObterRelatorioMensalQueryHandler.cs`
**Depends on**: T9
**Reuses**: `IHistoricoRepository.ObterEventosParaRelatorioAsync` (T7)
**Requirement**: REL-01, REL-02, REL-03, REL-04, REL-05, REL-06, REL-10

**Done when**:
- [ ] Calcula `inicio`/`fimExclusivo` do mês pedido corretamente (inclui mês corrente parcial)
- [ ] Quando `ResponsavelId` informado: filtra eventos pro responsável, `PorAtendente = null` (REL-06)
- [ ] SLA: só considera eventos `Resolvido` com `DataConclusao` preenchida, compara com `DataLimite`
- [ ] Comparação com mês anterior: `null` se mês anterior não tiver nenhum evento (edge case da spec)
- [ ] Teste unitário cobre: mês com dados, mês vazio, filtro por `ResponsavelId`, SLA cumprido vs estourado, ausência de mês anterior
- [ ] `dotnet test --no-build` — todos os testes passam (contagem anotada no commit)

**Tests**: unit (mock de `IHistoricoRepository`, mesmo padrão de `ListarComentariosQueryHandlerTests`)
**Gate**: quick

---

### T11: Criar `RelatoriosController`

**What**: Endpoint HTTP `GET /api/relatorios/mensal`
**Where**: `src/ChamadosCamarj.WebApi/Controllers/RelatoriosController.cs`
**Depends on**: T10
**Reuses**: Estrutura de `DashboardController` (thin controller + `IMediator`)
**Requirement**: REL-01, REL-06, REL-07

**Done when**:
- [ ] `GET /api/relatorios/mensal?ano={int}&mes={int}&responsavelId={guid?}` retorna `RelatorioMensalResponse`
- [ ] `curl` num mês com chamados reais retorna números batendo com uma contagem manual (Success Criteria da spec)
- [ ] `dotnet build` sem erros

**Tests**: none (controller thin, sem lógica própria)
**Gate**: build + verificação manual via curl

**Commit**: `feat(relatorios): endpoint GET /api/relatorios/mensal`

---

### T12: Criar tipos do Relatório no frontend

**What**: `RelatorioMensalResponse` e DTOs relacionados espelhando o backend
**Where**: `frontend/src/types/relatorio.ts`
**Depends on**: T11 (precisa do shape final do DTO)
**Reuses**: Padrão de `types/dashboard.ts`

**Done when**:
- [ ] Tipos batem exatamente com os records do backend (T8)
- [ ] `npm run build` sem erros

**Tests**: none
**Gate**: build

---

### T13: Criar `api.ts` e hook do Relatório Mensal

**What**: `obterRelatorioMensal(ano, mes, responsavelId?)` + `useRelatorioMensal`
**Where**: `frontend/src/features/relatorio-mensal/api.ts`, `hooks/useRelatorioMensal.ts`
**Depends on**: T12
**Reuses**: `apiFetch`, padrão de `useDashboardMetrics`

**Done when**:
- [ ] Hook busca via TanStack Query, `queryKey: ['relatorio-mensal', ano, mes, responsavelId]`
- [ ] `npm run build` sem erros

**Tests**: none
**Gate**: build

---

### T14: Criar `SeletorMes`

**What**: Componente de navegação entre meses (anterior/próximo), trava no mês corrente
**Where**: `frontend/src/features/relatorio-mensal/components/SeletorMes.tsx`
**Depends on**: None (pode rodar em paralelo com T12/T13)
**Reuses**: `Button` existente

**Done when**:
- [ ] Botão "próximo" desabilitado quando já está no mês corrente
- [ ] `npm run build` sem erros

**Tests**: none
**Gate**: build

---

### T15: Criar `exportar.ts` (CSV + impressão)

**What**: `exportarCsv(relatorio)` e `imprimirRelatorio()` conforme Design
**Where**: `frontend/src/features/relatorio-mensal/exportar.ts`
**Depends on**: T12
**Reuses**: Nenhuma lib nova (Design: `Blob` nativo + `window.print()`)
**Requirement**: REL-08, REL-09

**Done when**:
- [ ] `exportarCsv` gera um `Blob` com linhas legíveis (categoria, atendente, totais) e dispara download
- [ ] `imprimirRelatorio` chama `window.print()`
- [ ] `npm run build` sem erros

**Tests**: none
**Gate**: build

---

### T17: Criar `RelatorioMensalPage`

**What**: Página principal — integra `SeletorMes`, KPIs (`DashboardKpi` reaproveitado), `CategoriaChart` reaproveitado pra "por categoria", tabela simples "por atendente", `DonutChart` (T4) pro SLA, comparação com mês anterior, botões de exportação, RBAC (Admin vê tudo, Atendente só o próprio, `responsavelId` vindo do `perfil` autenticado)
**Where**: `frontend/src/features/relatorio-mensal/RelatorioMensalPage.tsx`
**Depends on**: T4, T13, T14, T15
**Reuses**: `DashboardKpi`, `CategoriaChart`, `DonutChart`, `useAuth()`
**Requirement**: REL-01 a REL-09

**Done when**:
- [ ] Admin vê `PorAtendente` e todos os números; Atendente não vê a seção `PorAtendente` e só vê os próprios dados (passa `perfil.id` como `responsavelId` quando `perfil.tipo === 'Atendente'`)
- [ ] Estado vazio claro quando o mês não tem chamados
- [ ] Indica quando o mês é parcial (mês corrente, "até hoje")
- [ ] `npm run build` sem erros

**Tests**: none (verificação manual, per TESTING.md)
**Gate**: build + verificação manual no navegador

---

### T18: Adicionar folha de estilo `@media print`

**What**: CSS que esconde a sidebar e ajusta o layout da página do relatório pra impressão/PDF
**Where**: `frontend/src/index.css` (bloco `@media print`) ou CSS module da página
**Depends on**: T17
**Reuses**: Nenhum

**Done when**:
- [ ] `window.print()` (Ctrl+P) na página do relatório mostra só o conteúdo do relatório, sem sidebar
- [ ] `npm run build` sem erros

**Tests**: none
**Gate**: build + verificação manual (abrir print preview no navegador)

---

### T19: Wire de rota e navegação

**What**: Adicionar `/atendimento/relatorio-mensal` no `App.tsx` e o link "Relatório Mensal" no `AppLayout.tsx`
**Where**: `frontend/src/App.tsx`, `frontend/src/layouts/AppLayout.tsx`
**Depends on**: T17
**Reuses**: Mesmo gate `perfil.tipo !== 'Solicitante'` já usado por Kanban/Dashboard/Fila
**Requirement**: REL-07

**Done when**:
- [ ] Link aparece na sidebar pra Admin e Atendente, some pra Solicitante
- [ ] Acessar a URL direto como Solicitante não mostra o conteúdo (RBAC de UI)
- [ ] `npm run build` sem erros

**Tests**: none
**Gate**: build + verificação manual (3 perfis)

**Commit**: `feat(relatorios): página e navegação do Relatório Mensal`

---

### T20: Verificação end-to-end

**What**: Rodar a API e o frontend, testar manualmente (ou via Playwright ad-hoc, sem commitar o spec) os critérios de aceite da spec: números batendo com o banco pra 2 meses diferentes, Admin vs Atendente vs Solicitante, exportação CSV abrindo corretamente, impressão mostrando só o conteúdo, e o Dashboard com a rosca nova
**Where**: N/A (verificação, não código)
**Depends on**: T6, T19
**Requirement**: Success Criteria da spec (todos)

**Done when**:
- [ ] Números do relatório de pelo menos 2 meses conferidos manualmente contra o banco/API
- [ ] Atendente vê só os próprios números; Admin vê tudo; Solicitante bloqueado
- [ ] CSV exportado abre corretamente
- [ ] Rosca do Dashboard e rosca de SLA do Relatório renderizam com dados reais
- [ ] `dotnet test` (todos) e `npm run build` passam

**Tests**: manual + ad-hoc Playwright (não commitado)
**Gate**: full

---

## Parallel Execution Map

```
Fase 0 (Dashboard, sequencial por causa do IHistoricoRepository compartilhado):
  T1 → T2 → T3 → T5 → T6
  T4 [P] (frontend, roda em paralelo com T1-T3)

Fase 1 (Backend Relatório, começa só depois de T6 por causa do IHistoricoRepository):
  T7 → T8 → T9 → T10 → T11

Fase 2 (Frontend Relatório):
  T11 completo, então:
    T12 → T13 ─┐
    T14 [P] ───┼──→ T17 → T18 → T19
    T15 ───────┘
    (T4 já pronto da Fase 0)

Fase 3:
  T6, T19 completos, então:
    T20
```

---

## Task Granularity Check

| Task | Scope | Status |
|---|---|---|
| T1-T20 | 1 arquivo/componente/endpoint por task (T17 é a maior, mas é "1 página" cotesa — componentes que ela integra já foram criados em tasks anteriores) | ✅ Granular |

## Diagram-Definition Cross-Check

| Task | Depends On (body) | Diagrama mostra | Status |
|---|---|---|---|
| T1 | None | Início da Fase 0 | ✅ |
| T2 | T1 | T1→T2 | ✅ |
| T3 | T2 | T2→T3 | ✅ |
| T4 | None | `[P]` paralelo à Fase 0 | ✅ |
| T5 | T3, T4 | T3→T5, T4→T5 (implícito, T4 pronto antes da Fase 2) | ✅ |
| T6 | T5 | T5→T6 | ✅ |
| T7 | T6 (mesmo arquivo que T1) | Fase 1 começa após T6 | ✅ |
| T8 | T7 | T7→T8 | ✅ |
| T9 | T8 | T8→T9 | ✅ |
| T10 | T9 | T9→T10 | ✅ |
| T11 | T10 | T10→T11 | ✅ |
| T12 | T11 | T11→T12 | ✅ |
| T13 | T12 | T12→T13 | ✅ |
| T14 | None | `[P]` | ✅ |
| T15 | T12 | T12→T15 | ✅ |
| T17 | T4, T13, T14, T15 | Convergência antes de T17 | ✅ |
| T18 | T17 | T17→T18 | ✅ |
| T19 | T17 | T17→T19 | ✅ |
| T20 | T6, T19 | Fase 3 após T6 e T19 | ✅ |

## Test Co-location Validation

| Task | Camada criada/modificada | Matriz exige | Task diz | Status |
|---|---|---|---|---|
| T1, T7 | Repositório (EF Core) | none (sem cobertura de repo neste projeto) | none | ✅ |
| T2, T3, T11 | Handler/Controller thin | none (mesmo padrão de `ObterMetricasQueryHandler`) | none | ✅ |
| T9 | Validator | unit (padrão `Fase6ValidatorsTests`) | unit | ✅ |
| T10 | Handler com lógica de agregação | unit (mock de repositório, padrão dos outros handlers) | unit | ✅ |
| T4-T6, T12-T19 | Frontend (componentes/páginas/tipos) | none — decisão do projeto (TESTING.md): sem teste unitário/componente, só verificação manual | none / manual | ✅ |
| T20 | Verificação | manual + Playwright ad-hoc (mesmo padrão usado na Fase 6 desta sessão) | manual | ✅ |

---

## Ferramentas

Nenhuma MCP ou skill externa necessária — tudo reaproveita padrões já presentes no código (EF Core, MediatR, FluentValidation, TanStack Query, Recharts). Execução direta, sem sub-agentes.
