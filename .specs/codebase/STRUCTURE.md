# Estrutura de Arquivos

```
ChamadosCamarj/
├── .specs/                          ← Documentação estruturada (este diretório)
│   ├── project/
│   │   ├── PROJECT.md               ← Visão, objetivos, decisões
│   │   ├── ROADMAP.md               ← Fases e progresso real
│   │   └── STATE.md                 ← Memória: decisões, blockers, deferred
│   └── codebase/
│       ├── STACK.md                 ← Tecnologias e pacotes
│       ├── ARCHITECTURE.md          ← Padrões e fluxo de dados
│       ├── CONVENTIONS.md           ← Convenções de código
│       ├── STRUCTURE.md             ← Este arquivo
│       ├── TESTING.md               ← Estratégia de testes
│       ├── INTEGRATIONS.md          ← Integrações externas
│       └── CONCERNS.md              ← Débito técnico e riscos
│
├── docs/
│   ├── SPEC.md                      ← Spec raiz (referência original)
│   └── obsidian/                    ← Vault Obsidian com notas do projeto
│
├── src/
│   ├── ChamadosCamarj.Domain/
│   │   ├── Common/
│   │   │   └── BaseEntity.cs        ← Id (Guid), DataCriacao, DataAtualizacao
│   │   ├── Entities/
│   │   │   ├── Chamado.cs           ← Entidade principal (rich domain model)
│   │   │   ├── Comentario.cs        ← Comentário público ou interno
│   │   │   ├── Categoria.cs         ← Categoria do chamado
│   │   │   └── Anexo.cs             ← Arquivo anexado (storage path)
│   │   ├── Enums/
│   │   │   ├── StatusChamado.cs     ← Aberto, EmAndamento, Resolvido, Fechado, Cancelado
│   │   │   ├── PrioridadeChamado.cs ← Baixa, Media, Alta, Urgente
│   │   │   ├── OrigemChamado.cs     ← Portal, Email, API
│   │   │   └── TipoComentario.cs   ← Publico, Interno
│   │   └── Interfaces/
│   │       ├── IChamadoRepository.cs
│   │       ├── ICategoriaRepository.cs
│   │       ├── IEmailReceiverService.cs ← Planejado (Fase 4)
│   │       └── IStorageService.cs       ← Planejado (Fase 4)
│   │
│   ├── ChamadosCamarj.Application/
│   │   ├── Common/
│   │   │   ├── Behaviours/
│   │   │   │   └── ValidationBehaviour.cs ← Pipeline MediatR
│   │   │   └── Exceptions/
│   │   ├── Features/
│   │   │   ├── Chamados/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── AbrirChamadoCommand.cs + Handler
│   │   │   │   │   ├── AtribuirChamadoCommand.cs + Handler
│   │   │   │   │   ├── AtualizarChamadoCommand.cs + Handler
│   │   │   │   │   ├── ComentarChamadoCommand.cs + Handler
│   │   │   │   │   ├── ResolverChamadoCommand.cs + Handler
│   │   │   │   │   ├── FecharChamadoCommand.cs + Handler
│   │   │   │   │   └── CancelarChamadoCommand.cs + Handler
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── ListarChamadosQuery.cs + Handler ← filtros via IQueryable no banco
│   │   │   │   │   └── ObterChamadoPorIdQuery.cs + Handler
│   │   │   │   ├── DTOs/
│   │   │   │   │   ├── ChamadoResponse.cs
│   │   │   │   │   ├── AbrirChamadoRequest.cs
│   │   │   │   │   └── AtualizarChamadoRequest.cs
│   │   │   │   └── Validators/
│   │   │   │       ├── AbrirChamadoCommandValidator.cs
│   │   │   │       ├── AtualizarChamadoCommandValidator.cs
│   │   │   │       ├── AtribuirChamadoCommandValidator.cs
│   │   │   │       └── ComentarChamadoCommandValidator.cs
│   │   │   └── Categorias/
│   │   │       ├── DTOs/CategoriaResponse.cs
│   │   │       └── Queries/ListarCategoriasQuery.cs + Handler ← usado via MediatR no controller
│   │   └── Mappings/
│   │       └── ChamadoMappings.cs   ← Extension: Chamado → ChamadoResponse
│   │
│   ├── ChamadosCamarj.Infrastructure/
│   │   ├── Data/
│   │   │   ├── Configurations/      ← Fluent API configs (EF Core)
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── DatabaseSeeder.cs    ← chamado por Program.cs (SeedAsync)
│   │   ├── Migrations/
│   │   │   └── 20260619130320_InitialCreate.cs ← Schema PostgreSQL, inclui FK ComentarioId em Anexos
│   │   └── Repositories/
│   │       ├── ChamadoRepository.cs
│   │       └── CategoriaRepository.cs
│   │
│   └── ChamadosCamarj.WebApi/
│       ├── Controllers/
│       │   ├── ChamadosController.cs   ← GET, POST, PUT, PATCH atribuir/resolver/fechar/cancelar, POST comentarios
│       │   └── CategoriasController.cs ← GET via IMediator
│       ├── Properties/launchSettings.json
│       ├── appsettings.json            ← ConnectionString PostgreSQL/Supabase (sem senha)
│       ├── appsettings.Development.json
│       └── Program.cs                  ← DI, Middleware, MigrateAsync + DatabaseSeeder.SeedAsync
│
├── docker-compose.yml               ← PostgreSQL local (não usado desde a migração para Supabase)
├── ChamadosCamarj.sln
└── README.md
```

## Notas sobre o que está faltando

- Frontend React — não iniciado (Fase 3)
- `IEmailReceiverService` e `IStorageService` — interfaces existem, sem implementação (Fase 4)
- Decisão de hospedagem em produção e injeção da connection string lá — pendente, não bloqueia o Frontend
