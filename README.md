# ChamadosCamarj

Sistema de gestão de chamados corporativos da CAMARJ.

## Stack

- **Backend:** .NET 9 + Clean Architecture + CQRS (MediatR)
- **Frontend:** React + TypeScript + Vite + TailwindCSS + Shadcn/ui
- **Banco:** PostgreSQL (Supabase)
- **Auth:** Azure AD
- **Email:** MailKit (IMAP)
- **Anexos:** Supabase Storage (S3)

## Estrutura

```
src/
├── ChamadosCamarj.Domain/         # Entidades, Enums, Interfaces
├── ChamadosCamarj.Application/     # Commands, Queries, Validators, DTOs
├── ChamadosCamarj.Infrastructure/  # EF Core, Repositories, Email
├── ChamadosCamarj.WebApi/          # Controllers, Program.cs
docs/
├── SPEC.md                         # Spec-Driven Development
└── obsidian/                       # Notas para Obsidian
tests/
└── ChamadosCamarj.UnitTests/
```

## Como rodar

1. Pré-requisitos: .NET 9 SDK, acesso ao projeto Supabase (`oxiqutweuejvopofbkoy`).
2. Configure a connection string do banco via `user-secrets` (nunca em `appsettings.json`):
   ```bash
   cd src/ChamadosCamarj.WebApi
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=aws-1-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.oxiqutweuejvopofbkoy;Password=<peça a senha pro Victor>;SSL Mode=Require;Trust Server Certificate=true"
   ```
   > Use o **Session pooler** do Supabase (porta 5432). A "Direct connection" só resolve via IPv6 e o "Transaction pooler" não suporta os prepared statements do EF Core.
3. Rode a API:
   ```bash
   dotnet run --project src/ChamadosCamarj.WebApi
   ```
   As migrations e o seed das categorias rodam automaticamente na primeira execução.
4. Acesse `http://localhost:5000/scalar` (ambiente Development) para testar os endpoints.

> Dev e produção apontam para o **mesmo banco Supabase** — qualquer requisição feita localmente grava dados reais.
