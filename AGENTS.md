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

## Agentes orquestrados — depende de qual ferramenta está rodando

Este projeto é trabalhado tanto pelo **opencode** quanto pelo **Claude Code**. Cada um lê seu
próprio arquivo de instrução (`AGENTS.md` aqui, `CLAUDE.md` na raiz) mas o papel de cada agente é o
mesmo — só muda o mecanismo de invocação.

### No opencode (todos Go, modelos diferentes por aba)
- `spec` → specs SDD (Kimi K3)
- `build-backend` → C#, .NET, EF Core (GLM-5.2)
- `build-frontend` → React, TS, Tailwind (Kimi K2.7 Code)
- `review` → code review (Grok 4.5)
- `explorar` → explorar código (DeepSeek V4 Flash)

Alternar com Tab entre agents. Usar @review e @explorar.

### No Claude Code
Não existem abas com modelos diferentes — é uma sessão única (Claude Sonnet/Opus) fazendo `spec` +
`build-backend` + `build-frontend` diretamente, sem trocar de agente. Os papéis equivalentes:

- **`spec`/`build-*`** → o próprio agente principal da sessão, sem sub-agente — é a forma padrão
  de trabalhar.
- **`review`** → ao terminar uma implementação não-trivial (mais de uma correção, mudança de
  contrato, ou qualquer coisa que vá pra `main`), abrir um **sub-agente novo, sem o contexto da
  sessão principal**, pra revisar o diff de forma independente — mesmo espírito do Grok revisando o
  Sonnet no opencode. Só o sub-agente não carrega o viés/contexto de quem implementou.
- **`explorar`** → busca de código dentro da própria sessão principal (não precisa de sub-agente
  separado; a sessão principal já tem as ferramentas de busca).

Registrar no `tasks.md`/`review.md` da feature qual ferramenta e qual papel foi usado em cada etapa,
igual já é feito pro opencode (ex: cabeçalho do `tasks.md` de `chat-corporativo` registra "Claude
Code (Sonnet 4.6 — spec; Opus 4.8 — backend e review; Sonnet 4.6 — frontend)").

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
