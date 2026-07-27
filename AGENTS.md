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
- `spec` → specs SDD (DeepSeek V4 Flash)
- `build-backend` → C#, .NET, EF Core (Kimi K2.7 Code)
- `build-frontend` → React, TS, Tailwind (Kimi K2.7 Code)
- `review` → code review (Kimi K2.7 Code)
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

## Branch
- `develop` é a branch ativa. Nunca commitar direto em `main`.
- Ao abrir PR, conferir base: `develop`, não `main`.
