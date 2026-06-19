# Arquitetura

## Padrão: Clean Architecture + CQRS

```
┌─────────────────────────────────────────────┐
│   WebApi (Controllers, Program.cs)          │  ← Entrada HTTP
├─────────────────────────────────────────────┤
│   Application (Commands, Queries, DTOs,     │  ← Regras de aplicação
│               Validators, Mappings)         │
├─────────────────────────────────────────────┤
│   Infrastructure (EF Core, Repositories,    │  ← Acesso a dados
│                  Migrations, Seeder)        │
├─────────────────────────────────────────────┤
│   Domain (Entities, Enums, Interfaces,      │  ← Núcleo — sem dependências externas
│           Common/BaseEntity)               │
└─────────────────────────────────────────────┘
```

## Fluxo de uma requisição

```
HTTP Request
    → Controller
        → IMediator.Send(Command/Query)
            → ValidationBehaviour (FluentValidation pipeline)
                → CommandHandler / QueryHandler
                    → IRepository
                        → ApplicationDbContext (EF Core)
                            → PostgreSQL via Supabase (dev e prod)
```

## CQRS com MediatR

- **Commands:** mutam estado — retornam `ChamadoResponse` ou `Unit`
- **Queries:** leitura — retornam DTOs
- **Pipeline Behavior:** `ValidationBehaviour<TRequest, TResponse>` intercepta todos os requests e valida via FluentValidation

## Repository Pattern

- Interfaces em `Domain/Interfaces/`
- Implementações em `Infrastructure/Repositories/`
- Injetados via DI em `Program.cs`

## Dependências entre projetos

```
WebApi → Infrastructure → Application → Domain
                    ↘ Application ↗
```

## Registros DI (Program.cs)

```csharp
AddMediatR(...)          // registra handlers do assembly Application
AddValidatorsFromAssembly // registra validators do assembly Application
AddScoped<IChamadoRepository, ChamadoRepository>
AddScoped<ICategoriaRepository, CategoriaRepository>
AddDbContext<ApplicationDbContext>(UseNpgsql)
```
