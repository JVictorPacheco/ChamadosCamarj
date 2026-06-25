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
│   │   │   │   │   ├── ListarChamadosQuery.cs + Handler ← filtros via IQueryable no banco, incl. `solicitanteEmail` (API-02)
│   │   │   │   │   ├── ObterChamadoPorIdQuery.cs + Handler
│   │   │   │   │   └── ListarComentariosQuery.cs + Handler ← API-01, retorna `ComentarioResponse[]` de um chamado
│   │   │   │   ├── DTOs/
│   │   │   │   │   ├── ChamadoResponse.cs
│   │   │   │   │   ├── ComentarioResponse.cs ← Id, Autor, Conteudo, Tipo, DataCriacao
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
│       │   ├── ChamadosController.cs   ← GET (+ filtro solicitanteEmail), GET/{id}, GET/{id}/comentarios, POST, PUT, PATCH atribuir/resolver/fechar/cancelar, POST comentarios
│       │   └── CategoriasController.cs ← GET via IMediator
│       ├── Properties/launchSettings.json
│       ├── appsettings.json            ← ConnectionString PostgreSQL/Supabase (sem senha)
│       ├── appsettings.Development.json
│       └── Program.cs                  ← DI, Middleware, MigrateAsync + DatabaseSeeder.SeedAsync
│
├── frontend/                         ← Fase 3 (Portal do Solicitante) — React + Vite + TS
│   ├── e2e/
│   │   └── fluxo-completo.spec.ts   ← Playwright: login mock → abrir → detalhe → comentar → listar → click no card
│   ├── playwright.config.ts
│   ├── src/
│   │   ├── App.tsx                  ← Rotas (React Router), QueryClient (retry custom p/ 4xx), providers
│   │   ├── auth/
│   │   │   ├── AuthContext.tsx      ← Auth mockada: 3 perfis fixos, persistido em localStorage
│   │   │   └── ProfileSelector.tsx  ← Tela `/login`
│   │   ├── layouts/
│   │   │   └── AppLayout.tsx        ← Sidebar (shadcn `Sidebar`) + outlet + sair
│   │   ├── lib/
│   │   │   ├── api.ts               ← `apiFetch`/`ApiError` (cliente HTTP tipado)
│   │   │   └── utils.ts
│   │   ├── types/
│   │   │   └── api.ts               ← Tipos TS espelhando os DTOs reais do backend
│   │   ├── components/ui/           ← shadcn/ui (button, card, sidebar, select, etc.)
│   │   └── features/chamados/
│   │       ├── api.ts                       ← 6 funções (listarChamados, obterChamado, abrirChamado, listarComentarios, comentar, listarCategorias)
│   │       ├── hooks/                       ← useChamados, useChamado, useComentarios, useCategorias, useAbrirChamado, useComentar
│   │       ├── components/                  ← StatusBadge, PrioridadeBadge, SlaBadge, ChamadoCard, FiltroChamados, ComentarioList, ComentarioForm
│   │       ├── AbrirChamadoPage.tsx         ← `/chamados/novo`
│   │       ├── ChamadosListPage.tsx         ← `/chamados`
│   │       └── ChamadoDetailPage.tsx        ← `/chamados/:id`
│   └── package.json
│
├── docker-compose.yml               ← PostgreSQL local (não usado desde a migração para Supabase)
├── ChamadosCamarj.sln
└── README.md
```

## Notas sobre o que está faltando

- Frontend React — Fase 3 (Portal do Solicitante) **completa**. Ações de Atendente (Kanban, resolver/fechar/cancelar na UI), upload de anexos, e Admin ficam pras Fases 4-6
- `IEmailReceiverService` e `IStorageService` — interfaces existem, sem implementação (Fase 4)
- Decisão de hospedagem em produção e injeção da connection string lá — pendente, não bloqueia o Frontend
