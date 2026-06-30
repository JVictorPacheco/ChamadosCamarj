# 📊 SPEC — Fase 5: Kanban + Dashboard + SignalR

> **Versão:** 1.0  
> **Status:** Aguardando revisão  
> **Metodologia:** SDD (Spec-Driven Development)  
> **Branch:** `feature/fase-5-kanban-dashboard`  
> **Depende de:** Fases 0–3 (concluídas)

---

## 1. VISÃO GERAL

Esta fase transforma o sistema de um simples portal de chamados em uma **ferramenta de atendimento completa**, adicionando:

- **Kanban board** com drag & drop para mover chamados entre status
- **Painel do Atendente** com fila de trabalho, ações rápidas (assumir, resolver, fechar)
- **Dashboard** com gráficos e métricas em tempo real
- **Notificações SignalR** atualizando a interface automaticamente

### Problema atual
- O atendente não tem visão da fila de chamados pendentes
- Não existe kanban para gerenciar fluxo de trabalho
- Sem dashboard, gestores não enxergam métricas
- A interface só atualiza com refresh manual (F5)

### Solução
- Kanban com colunas por status e drag & drop
- Dashboard com gráficos (Recharts)
- SignalR push pra atualizar cards, kanban e dashboard em tempo real
- Página exclusiva do atendente com fila priorizada

---

## 2. ARQUITETURA DA FASE 5

```
┌─────────────────────────────────────────────────────┐
│                   FRONTEND (React)                  │
│                                                     │
│  ┌──────────┐  ┌──────────┐  ┌──────────────────┐  │
│  │  Kanban  │  │Dashboard │  │Painel Atendente  │  │
│  │  Board   │  │  Charts  │  │(fila + ações)    │  │
│  └────┬─────┘  └────┬─────┘  └────────┬─────────┘  │
│       │             │                 │             │
│       └─────────────┼─────────────────┘             │
│                     │                               │
│              ┌──────┴──────┐                        │
│              │  SignalR    │  ← conexão WebSocket   │
│              │  Client     │     em tempo real      │
│              └──────┬──────┘                        │
└─────────────────────┼───────────────────────────────┘
                      │
┌─────────────────────┼───────────────────────────────┐
│              BACKEND (.NET 9)                        │
│                                                     │
│  ┌──────────────────┐  ┌────────────────────────┐   │
│  │  ChamadosController│  │  DashboardController  │   │
│  │  (PUT assumir,    │  │  GET /metricas         │   │
│  │   resolver, etc.) │  │  GET /tendencia        │   │
│  └────────┬─────────┘  └───────────┬────────────┘   │
│           │                        │                 │
│  ┌────────┴────────────────────────┴────────────┐   │
│  │              SignalR Hub                      │   │
│  │  ChamadosHub → ChamadoCriado, StatusAlterado, │   │
│  │                 ComentarioAdicionado           │   │
│  └───────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

---

## 3. NOVOS ENDPOINTS DA API

### 3.1 Dashboard

```
GET /api/dashboard/metricas
  → { totalAbertos, totalEmAndamento, totalResolvidosHoje, tempoMedioResolucao, porCategoria, porPrioridade }

GET /api/dashboard/tendencia?dias=7
  → [{ data, abertos, resolvidos }, ...]
```

### 3.2 Kanban (batch update)

```
PUT /api/chamados/{id}/status
  Body: { "novoStatus": "EmAndamento" }
  → Atualiza status + notifica via SignalR
```

---

## 4. SIGNALR HUB — ChamadosHub

### Eventos enviados pelo servidor:

| Evento | Quando dispara | Payload |
|--------|---------------|---------|
| `ChamadoCriado` | Novo chamado aberto | `ChamadoResponse` |
| `StatusAlterado` | Status mudou (drag/assumir/resolver) | `{ id, novoStatus, dataAtualizacao }` |
| `ComentarioAdicionado` | Novo comentário | `{ chamadoId, comentario }` |
| `MetricasAtualizadas` | Métricas do dashboard mudaram | `DashboardMetrics` |

---

## 5. FRONTEND — NOVAS TELAS

### 5.1 Kanban Board (`/atendimento/kanban`)

```
┌─────────────────────────────────────────────────────┐
│  [Aberto]        [Em Andamento]      [Resolvido]    │
│  ┌─────────┐     ┌─────────┐        ┌─────────┐    │
│  │ Card 1  │     │ Card 4  │        │ Card 7  │    │
│  │ Card 2  │     │ Card 5  │        └─────────┘    │
│  │ Card 3  │     └─────────┘                        │
│  └─────────┘                                        │
│  ┌─────────┐                                        │
│  │ Card 6  │  ← drag & drop entre colunas           │
│  └─────────┘                                        │
└─────────────────────────────────────────────────────┘
```

- Colunas: Aberto, Em Andamento, Resolvido, Fechado, Cancelado
- Drag & drop com `@dnd-kit/core`
- Ao soltar → PUT `/chamados/{id}/status` → SignalR notifica todos
- Cards compactos: título + prioridade + SLA + solicitante

### 5.2 Dashboard (`/atendimento/dashboard`)

```
┌─────────────────────────────────────────────────────┐
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐ │
│  │ 12       │ │ 5        │ │ 8        │ │ 4.2h   │ │
│  │ Abertos  │ │ Em And.  │ │ Resolv.  │ │ T. Médio│ │
│  └──────────┘ └──────────┘ └──────────┘ └────────┘ │
│                                                     │
│  ┌─────────────────────┐ ┌─────────────────────────┐│
│  │ Gráfico: Tendência  │ │ Gráfico: Por Categoria  ││
│  │ (linha 7 dias)      │ │ (barras)                ││
│  └─────────────────────┘ └─────────────────────────┘│
└─────────────────────────────────────────────────────┘
```

- 4 cards de métricas no topo (KPI)
- Gráfico de tendência (linha) — abertos vs resolvidos nos últimos 7 dias
- Gráfico por categoria (barras)
- Atualização em tempo real via SignalR (`MetricasAtualizadas`)

### 5.3 Painel do Atendente (`/atendimento/fila`)

- Fila de chamados pendentes (Aberto, ordenado por prioridade + SLA)
- Ações rápidas: Assumir, Resolver, Fechar
- Ao assumir → PUT `assumir` → card some da fila, aparece no kanban
- Integração com SignalR pra atualizar em tempo real

### 5.4 Navegação (Sidebar)

Adicionar ao `AppLayout` (quando perfil = Atendente/Admin):

```
[+] Abrir Chamado
─────────────────
📋 Meus Chamados
📊 Kanban          ← NOVO
📈 Dashboard       ← NOVO  
📥 Fila            ← NOVO
─────────────────
👤 Victor (Atendente)
[Sair]
```

---

## 6. COMPONENTES NOVOS

| Componente | Descrição | Reutiliza? |
|-----------|-----------|------------|
| `KanbanBoard` | Board completo com colunas | Novo |
| `KanbanColumn` | Coluna (status) com cards | Novo |
| `KanbanCard` | Card arrastável (wrapper do ChamadoCard) | Reutiliza ChamadoCard |
| `DashboardKpi` | Card de métrica (número grande + label) | Novo |
| `TendenciaChart` | Gráfico de linha (Recharts) | Novo |
| `CategoriaChart` | Gráfico de barras (Recharts) | Novo |
| `FilaAtendimento` | Lista de chamados pendentes com ações | Novo |
| `SignalRProvider` | Contexto de conexão SignalR | Novo |

---

## 7. DEPENDÊNCIAS NOVAS

### Backend
- `Microsoft.AspNetCore.SignalR` (já incluso no ASP.NET Core)

### Frontend
```json
{
  "@microsoft/signalr": "^9.0.0",
  "@dnd-kit/core": "^6.1.0",
  "@dnd-kit/sortable": "^8.0.0",
  "recharts": "^2.12.0"
}
```

---

## 8. REGRAS DE NEGÓCIO

1. **Drag & drop:** Só atendente/admin pode mover cards no kanban
2. **Transições válidas:** Aberto → EmAndamento → Resolvido → Fechado. Cancelado a partir de qualquer status exceto Fechado
3. **Assumir:** Ao clicar "Assumir", o chamado fica atribuído ao atendente logado
4. **Notificação:** Toda mudança de status notifica TODOS os clientes conectados via SignalR
5. **Dashboard:** Métricas recalculadas a cada evento (criação, status change, resolução)
6. **Fila:** Ordenada por prioridade (Urgente > Alta > Média > Baixa) e depois por SLA mais próximo

---

## 9. TASKS (T01–T09)

| ID | Task | Camada |
|----|------|--------|
| **T01** | Criar `ChamadosHub` (SignalR) + registrar no `Program.cs` | Backend |
| **T02** | Criar endpoint `PUT /chamados/{id}/status` + Command | Backend |
| **T03** | Criar `DashboardController` + Queries de métricas | Backend |
| **T04** | Disparar eventos SignalR nos Handlers existentes | Backend |
| **T05** | Instalar deps frontend (signalr, dnd-kit, recharts) | Frontend |
| **T06** | Criar `SignalRProvider` + hook `useSignalR` | Frontend |
| **T07** | Criar Kanban Board (`/atendimento/kanban`) com drag & drop | Frontend |
| **T08** | Criar Dashboard (`/atendimento/dashboard`) com gráficos | Frontend |
| **T09** | Criar Painel do Atendente (`/atendimento/fila`) + atualizar sidebar | Frontend |

---

## 10. VERIFICAÇÃO (como testar)

- [ ] Arrastar card no kanban → status atualiza no backend + outros clientes veem
- [ ] Abrir chamado em uma aba → aparece no kanban de outra aba sem refresh
- [ ] Dashboard atualiza métricas em tempo real
- [ ] Assumir chamado → card some da fila, aparece no kanban
- [ ] Teste E2E Playwright: fluxo completo do atendente

---

> **Próximo passo:** Revisão do Victor → Aprovação → Implementar T01–T09 em sequência.
