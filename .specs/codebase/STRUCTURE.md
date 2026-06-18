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
│   │   │   │   │   └── ResolverChamadoCommand.cs + Handler
│   │   │   │   ├── Queries/
│   │   │   │   │   ├── ListarChamadosQuery.cs + Handler
│   │   │   │   │   └── ObterChamadoPorIdQuery.cs + Handler
│   │   │   │   ├── DTOs/
│   │   │   │   │   ├── ChamadoResponse.cs
│   │   │   │   │   ├── AbrirChamadoRequest.cs
│   │   │   │   │   └── AtualizarChamadoRequest.cs
│   │   │   │   └── Validators/
│   │   │   │       ├── AbrirChamadoCommandValidator.cs
│   │   │   │       └── AtualizarChamadoCommandValidator.cs
│   │   │   └── Categorias/
│   │   │       ├── DTOs/CategoriaResponse.cs
│   │   │       └── Queries/ListarCategoriasQuery.cs + Handler
│   │   └── Mappings/
│   │       └── ChamadoMappings.cs   ← Extension: Chamado → ChamadoResponse
│   │
│   ├── ChamadosCamarj.Infrastructure/
│   │   ├── Data/
│   │   │   ├── Configurations/      ← Fluent API configs (EF Core)
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── DatabaseSeeder.cs    ← NÃO UTILIZADO (seed está em Program.cs)
│   │   ├── Migrations/
│   │   │   └── 20260614000000_InitialCreate.cs ← Schema PostgreSQL (conflita com SQLite dev)
│   │   └── Repositories/
│   │       ├── ChamadoRepository.cs
│   │       └── CategoriaRepository.cs
│   │
│   └── ChamadosCamarj.WebApi/
│       ├── Controllers/
│       │   ├── ChamadosController.cs   ← GET, POST, PUT, PATCH atribuir/resolver, POST comentarios
│       │   └── CategoriasController.cs ← GET (bypassa MediatR — injeta repo direto)
│       ├── Properties/launchSettings.json
│       ├── appsettings.json            ← ConnectionString SQLite
│       ├── appsettings.Development.json
│       ├── chamadoscamarj.db           ← Banco SQLite local (dev)
│       └── Program.cs                  ← DI, Middleware, Seed inline
│
├── docker-compose.yml               ← PostgreSQL local (porta 5432)
├── ChamadosCamarj.sln
└── README.md
```

## Notas sobre o que está faltando

- `/tests/` — mencionado no README mas não existe ainda
- Frontend React — não iniciado
- `IEmailReceiverService` e `IStorageService` — interfaces existem, sem implementação
- Commands para `Fechar` e `Cancelar` — métodos existem no Domain, sem Command/Endpoint
