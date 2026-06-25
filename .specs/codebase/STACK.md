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
| SignalR | planned | Notificações em tempo real (Fase 5) |

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

## Auth (Frontend, Fase 3)

Mockada — `AuthContext` com 3 perfis fixos (Admin/Atendente/Solicitante) salvos em `localStorage`, sem chamada real ao Azure AD. Login corporativo real (Microsoft Entra ID) fica pra **Fase 6** — depende de acesso ao tenant da Camarj, ainda não disponível.

## Infraestrutura

- **Dev e Prod:** PostgreSQL via Supabase (mesma instância) — conexão via Session pooler, senha em `dotnet user-secrets` (dev)
- **Storage:** Supabase Storage (S3-compatible)
- **Auth:** Azure AD (Microsoft Entra ID)
- **CI/CD:** GitHub Actions (planejado)
- **Containers:** `docker-compose.yml` existe mas não é mais usado para o banco (era PostgreSQL local antes da migração para Supabase)

## Ferramentas de dev

- Scalar (`/scalar`) — API Explorer em dev
- OpenAPI nativo .NET 9 (`/openapi/v1.json`)
- Supabase Dashboard (SQL editor) para inspecionar o banco
