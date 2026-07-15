# 🏗️ Arquitetura do Sistema

## Clean Architecture (4 camadas)

```
┌─────────────────────────────────────────┐
│         WebApi (Controllers)            │
├─────────────────────────────────────────┤
│       Application (CQRS + MediatR)      │
├─────────────────────────────────────────┤
│       Infrastructure (EF, Email)        │
├─────────────────────────────────────────┤
│            Domain (Entidades)           │
└─────────────────────────────────────────┘
```

## Diagrama de Dependências

- **WebApi** → Infrastructure
- **Infrastructure** → Application
- **Application** → Domain
- **Domain** → (nenhuma)

## Fluxo de uma Requisição

```
Cliente → Controller → Command/Query → Handler → Repository → DB
                                ↓
                         ValidationBehaviour
                         (FluentValidation)
```

## Tempo real — SignalR (Fase 5)

`ChamadosHub` notifica todos os clientes conectados a cada mudança relevante — disparado dentro dos CommandHandlers (criação, mudança de status, comentário, reatribuição, alteração de prioridade). Eventos: `ChamadoCriado`, `StatusAlterado`, `ComentarioAdicionado`, `MetricasAtualizadas`. Frontend consome via `SignalRProvider` + hook `useSignalR`, usado no Kanban, Dashboard e Fila de Atendimento.

## Auditoria — HistoricoEntrada (Fase 6)

Toda mutação de um `Chamado` (Abrir, Atribuir, Reatribuir, AlterarPrioridade, Resolver, Fechar, Cancelar) gera uma entrada de `HistoricoEntrada` (entidade Domain, enum `AcaoHistorico`, acessada via `IHistoricoRepository`), integrada diretamente nos CommandHandlers. É a fonte de dados do Relatório Mensal (Fase 7) e da timeline exibida no detalhe do chamado. Ver [[📋 Histórico de Chamados]].

## Dashboard e Relatórios

`DashboardController` expõe métricas em tempo real (situação atual dos chamados). `GET /api/relatorios/mensal` (Fase 7) agrega dados históricos via `HistoricoEntrada` para um mês fechado — são fontes de dados e propósitos diferentes: dashboard é "foto do momento", relatório é "janela de tempo fechada".

## Tecnologias

Ver `.specs/codebase/STACK.md` para a stack atual e `.specs/codebase/ARCHITECTURE.md` para detalhes de implementação (registros DI, dependências entre projetos). `docs/SPEC.md` é o documento original (snapshot histórico).

## Padrões Utilizados

- **CQRS** — Separação de Commands e Queries
- **MediatR** — Barramento de mensagens
- **Repository Pattern** — Abstração de dados
- **FluentValidation** — Validação declarativa
- **Spec-Driven Development** — [[⚙️ SDD — Spec-Driven Development]]
