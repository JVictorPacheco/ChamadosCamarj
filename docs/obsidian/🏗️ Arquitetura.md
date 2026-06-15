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

## Tecnologias

Ver [[📋 SPEC]] para detalhes completos.

## Padrões Utilizados

- **CQRS** — Separação de Commands e Queries
- **MediatR** — Barramento de mensagens
- **Repository Pattern** — Abstração de dados
- **FluentValidation** — Validação declarativa
- **Spec-Driven Development** — [[⚙️ SDD — Spec-Driven Development]]
