# AGENTS.md — ChamadosCamarj

## Stack
- **Backend:** .NET 9 + Clean Architecture + CQRS (MediatR)
- **Frontend:** React 19 + TS + Vite + TailwindCSS v4 + Shadcn/ui
- **Banco:** PostgreSQL (Supabase)
- **Auth:** Email e senha via ASP.NET Core Identity (PasswordHasher)
- **Anexos:** Supabase Storage
- **Testes:** xUnit (backend) + Playwright (frontend E2E)

## Estrutura
```
.specs/         → SDD: project/ (STATE, ROADMAP, PROJECT), codebase/ (arquitetura), features/ (spec)
src/
├── Domain/     → Entidades, Enums, Interfaces
├── Application/ → CQRS: Commands/Queries/Handlers/Validators/DTOs
├── Infrastructure/ → EF Core, Repositories, Serviços externos
└── WebApi/     → Controllers, Program.cs, Middleware
frontend/
└── src/features/ → Feature folders (api.ts, hooks/, components/, *Page.tsx)
tests/
└── ChamadosCamarj.UnitTests/
```

## Como rodar
```bash
# Backend
cd src/ChamadosCamarj.WebApi && dotnet run   # API em :5000, Scalar em /scalar
# Testes
dotnet test tests/ChamadosCamarj.UnitTests/
# Frontend (outro terminal)
cd frontend && npm run dev                     # :5173
npm run build                                  # gate check TS + Vite
```

## Agentes orquestrados (todos Go)
- `spec` → specs SDD (Kimi K3)
- `build-backend` → C#, .NET, EF Core (GLM-5.2)
- `build-frontend` → React, TS, Tailwind (Kimi K2.7 Code)
- `review` → code review (Grok 4.5)
- `explorar` → explorar código (DeepSeek V4 Flash)

Alternar com Tab entre agents. Usar @review e @explorar.

## MCPs disponíveis
- `shadcn` → consultar componentes shadcn/ui (props, variantes, exemplos)
- `context7` → buscar documentação de bibliotecas (React, .NET, etc.)
- `dotnet-context` → analisar a solução .NET com Roslyn (DbContexts, entidades, migrations, DI)

## SDD — Sempre seguir
1. **Constitution** (STATE.md): perguntas sem resposta não viram suposições; spec antes do código; mudança de contrato sinalizada antes de aplicar
2. **Specify → Design → Tasks → Execute** (auto-sized por complexidade)
3. **Gate checks** obrigatórios: `dotnet test` + `npm run build` antes de finalizar

## Git Flow
- `main` — produção (sempre estável)
- `develop` — integração (branch ativa)
- `feature/*` — branches para cada grupo de trabalho (criar de `develop`, mergear em `develop`)
- `release/*` — preparação de release (criar de `develop`, mergear em `main` + `develop`)
- `hotfix/*` — correção urgente em produção (criar de `main`, mergear em `main` + `develop`)
- Nunca commitar direto em `main`. Ao abrir PR, base: `develop`, não `main`.
- Commits atômicos: um commit por funcionalidade/bug, com mensagem descritiva.
