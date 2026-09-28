# Stack Tecnológico

## Runtime

- **.NET 9** (SDK)
- **C# 13** (implicit usings, file-scoped namespaces, records, collection expressions)

## Pacotes principais (Backend)

| Pacote | Versão | Uso |
|--------|--------|-----|
| MediatR | latest | CQRS — dispatch de Commands/Queries |
| FluentValidation | latest | Validação declarativa via Pipeline Behavior |
| Microsoft.EntityFrameworkCore | 9.x | ORM |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.x | Dev e Produção (PostgreSQL/Supabase) |
| Scalar.AspNetCore | latest | UI de documentação OpenAPI |
| MailKit | planned | IMAP — captura de e-mails (Fase 4) |
| Serilog | planned | Logging estruturado (Fase futura) |
| SignalR | ✅ Implementado (Fase 5) | Notificações em tempo real |
| EF Core Concurrency | ✅ Implementado | `IsConcurrencyToken()` — locking otimista via `DataAtualizacao` |
| Auto-triagem | ✅ Implementado | `KeywordTriagemService` — sugestão de categoria/grupo por palavras-chave |
| Idempotência | ✅ Implementado | `[Idempotent]` filter + `Idempotency-Key` header |

## Pacotes principais (Frontend — `frontend/`, Fase 3 completa)

| Pacote | Versão | Uso |
|--------|--------|-----|
| React | 19.x | UI |
| Vite | 8.x | Build/dev server |
| TypeScript | 6.x (`tsc -b`) | Tipagem, gate de build |
| TailwindCSS | v4 (via `@tailwindcss/vite`) | Estilos utilitários |
| shadcn/ui | — | Componentes (Radix por baixo), tema dark customizado (paleta Camarj) |
| React Router | 8.x | Roteamento client-side (`BrowserRouter`) |
| TanStack Query | v5 | Data fetching/cache; `retry` customizado (não tenta de novo em 4xx) |
| React Hook Form | 7.x | Formulários (`AbrirChamadoPage`) |
| @playwright/test | 1.61.x | Teste E2E (`frontend/e2e/`) |

## Auth (Frontend)

Email e senha via ASP.NET Core Identity PasswordHasher (login, cadastro, redefinir senha, reset por e-mail). Login Google Workspace implementado mas dormant (descontinuado pela TI).

## Infraestrutura

- **Dev e Prod:** PostgreSQL via Supabase (mesma instância) — conexão via Session pooler, senha em `dotnet user-secrets` (dev)
- **Storage:** Supabase Storage (S3-compatible)
- **CI/CD:** nenhum pipeline no repo (workflow do Azure removido em 2026-09-28 — falhava em todo push)
- **Frontend hosting:** Cloudflare Pages (grátis, build a partir da `main`) — `https://chamados.okurumin.com.br`
- **Backend hosting:** Cloudflare Tunnel (API exposta em `https://chamados.okurumin.com.br/api`, deploy manual) — ver `docs/DEPLOY-CLOUDFLARE.md`
- **Containers:** nenhum (Dockerfile/docker-compose removidos em 2026-09-28 — não eram usados desde a migração para Supabase)

## Ferramentas de dev

- Scalar (`/scalar`) — API Explorer em dev
- OpenAPI nativo .NET 9 (`/openapi/v1.json`)
- Supabase Dashboard (SQL editor) para inspecionar o banco
