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

## Frontend (não iniciado)

- React + TypeScript + Vite
- TailwindCSS + Shadcn/ui

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
