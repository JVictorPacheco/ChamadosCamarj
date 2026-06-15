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
