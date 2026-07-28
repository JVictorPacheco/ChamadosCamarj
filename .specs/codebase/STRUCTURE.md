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
│   │   │   ├── Anexo.cs             ← Arquivo anexado (storage path)
│   │   │   ├── HistoricoEntrada.cs  ← Registro de ações no chamado
│   │   │   ├── UsuarioPerfil.cs     ← Usuário com perfil e senha
│   │   │   └── Grupo.cs             ← Grupos/Equipes (planejado)
│   │   ├── Enums/
│   │   │   ├── StatusChamado.cs     ← Aberto, EmAndamento, Resolvido, Fechado, Cancelado
│   │   │   ├── PrioridadeChamado.cs ← Baixa, Media, Alta, Urgente
│   │   │   ├── OrigemChamado.cs     ← Portal, Email, API
│   │   │   ├── TipoComentario.cs   ← Publico, Interno
│   │   │   ├── Perfil.cs            ← Admin, Atendente, Solicitante
│   │   │   └── AcaoHistorico.cs     ← Ações registradas no histórico
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
│   │   │   ├── IJwtTokenService.cs
│   │   │   ├── JwtTokenService.cs
│   │   │   ├── ResetTokenHelper.cs
│   │   │   ├── IEmailSender.cs
│   │   │   ├── ICurrentUserService.cs
│   │   │   ├── AuthSettings.cs
│   │   │   ├── Exceptions/
│   │   │   └── Authorization/
│   │   │       └── PerfilRequisitanteGuard.cs
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
│   │   │   ├── Categorias/
│   │   │   │   ├── DTOs/CategoriaResponse.cs
│   │   │   │   └── Queries/ListarCategoriasQuery.cs + Handler ← usado via MediatR no controller
│   │   │   ├── Auth/                   ← Commands: Login, EsqueciSenha, ResetarSenha
│   │   │   ├── Usuarios/               ← CRUD Commands/Queries
│   │   │   ├── Relatorios/
│   │   │   └── Dashboard/
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
│   │   ├── Repositories/
│   │   │   ├── ChamadoRepository.cs
│   │   │   ├── CategoriaRepository.cs
│   │   │   ├── HistoricoRepository.cs
│   │   │   └── UsuarioPerfilRepository.cs
│   │   └── Services/
│   │       ├── SmtpEmailSender.cs
│   │       ├── SupabaseStorageService.cs
│   │       ├── NullStorageService.cs
│   │       └── GoogleTokenValidator.cs
│   │
│   └── ChamadosCamarj.WebApi/
│       ├── Controllers/
│       │   ├── ChamadosController.cs   ← GET (+ filtro solicitanteEmail), GET/{id}, GET/{id}/comentarios, POST, PUT, PATCH atribuir/resolver/fechar/cancelar, POST comentarios
│       │   ├── CategoriasController.cs ← GET via IMediator
│       │   ├── AuthController.cs       ← Login, cadastro, esqueci-senha, resetar-senha
│       │   ├── UsuariosController.cs   ← CRUD de usuários
│       │   └── RelatoriosController.cs
│       ├── Services/
│       │   └── CurrentUserService.cs   ← Extrai claims JWT do HttpContext
│       ├── Hubs/
│       │   └── ChamadosHub.cs          ← SignalR para notificações em tempo real
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
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
│   │   ├── App.tsx                  ← Rotas (React Router), QueryClient, providers (Auth, Theme, Query, SignalR)
│   │   ├── auth/
│   │   │   ├── AuthContext.tsx      ← AuthContext com login email/senha, persistido em localStorage
│   │   │   ├── LoginPage.tsx        ← Tela de login com email e senha
│   │   │   ├── ResetarSenhaPage.tsx ← Tela de reset de senha
│   │   │   └── api.ts               ← Funções de auth (login, esqueciSenha, resetarSenha)
│   │   ├── layouts/
│   │   │   └── AppLayout.tsx        ← Sidebar (shadcn `Sidebar`) + outlet + sair
│   │   ├── hooks/
│   │   │   ├── useTheme.tsx         ← ThemeProvider, useTheme: toggle entre claro/escuro
│   │   │   └── useSignalR.tsx       ← Conexão SignalR para notificações em tempo real
│   │   ├── lib/
│   │   │   ├── api.ts               ← `apiFetch`/`ApiError` (cliente HTTP tipado)
│   │   │   └── utils.ts
│   │   ├── types/
│   │   │   └── api.ts               ← Tipos TS espelhando os DTOs reais do backend
│   │   ├── components/
│   │   │   ├── ui/                  ← shadcn/ui (button, card, sidebar, select, tooltip, etc.)
│   │   │   └── PasswordInput.tsx    ← Input de senha com forwardRef + toggle de visibilidade
│   │   └── features/
│   │       ├── chamados/
│   │       │   ├── api.ts
│   │       │   ├── hooks/
│   │       │   ├── components/
│   │       │   └── *Page.tsx
│   │       ├── admin/
│   │       ├── dashboard/
│   │       └── relatorio-mensal/
│   └── package.json
│
├── docker-compose.yml               ← PostgreSQL local (não usado desde a migração para Supabase)
├── ChamadosCamarj.sln
└── README.md
```

## Notas sobre o estado atual

- Fase 5-8 completas, auth email-senha implementada, deploy Azure + Cloudflare Pages configurado. Pendências: Grupos/Equipes (planejado, sem spec).
