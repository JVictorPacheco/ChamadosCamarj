# ChamadosCamarj

Sistema de gestão de chamados corporativos da CAMARJ.

## Stack

- **Backend:** .NET 9 + Clean Architecture + CQRS (MediatR)
- **Frontend:** React + TypeScript + Vite + TailwindCSS + Shadcn/ui
- **Banco:** PostgreSQL (Supabase) — dev e prod na mesma instância
- **Auth:** Email e senha (PasswordHasher ASP.NET Core Identity)
- **Email:** MailKit (IMAP) — Fase 4, ainda não implementado
- **Anexos:** Supabase Storage (S3) — implementado

## Estrutura

```
src/
├── ChamadosCamarj.Domain/         # Entidades, Enums, Interfaces
├── ChamadosCamarj.Application/     # Commands, Queries, Validators, DTOs
├── ChamadosCamarj.Infrastructure/  # EF Core, Repositories, Email
├── ChamadosCamarj.WebApi/          # Controllers, Program.cs
frontend/                           # React 19 + TS + Vite + TailwindCSS v4 + Shadcn/ui
├── src/
│   ├── features/                   # Telas e componentes por domínio (chamados, dashboard, kanban...)
│   └── ...
docs/
├── DEPLOY-CLOUDFLARE.md            # Guia de deploy (Cloudflare Pages + Tunnel)
├── GUIA-ORQUESTRACAO-SDD.md        # Passo a passo de orquestração multi-IA
├── obsidian/                       # Documentação funcional (vault Obsidian) — o sistema como ele é, foco em negócio
└── arquivo/                        # Material histórico (spec original, handoff antigo) — não é mantido
.specs/                             # Documentação viva (Spec-Driven Development) — fonte da verdade do estado atual
├── project/                        # PROJECT.md, ROADMAP.md, STATE.md
├── codebase/                       # ARCHITECTURE.md, STACK.md, STRUCTURE.md, CONVENTIONS.md...
└── features/                       # spec/design/tasks por feature
tests/
└── ChamadosCamarj.UnitTests/
```

## Como rodar

1. Pré-requisitos: .NET 9 SDK, Node.js, acesso ao projeto Supabase (`oxiqutweuejvopofbkoy`).
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
5. Rode o frontend:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
   Acesse `http://localhost:5173`. O login é por e-mail e senha (usuários cadastrados pelo Admin).

> Dev e produção apontam para o **mesmo banco Supabase** — qualquer requisição feita localmente grava dados reais.

> Para o estado atual do projeto (fases concluídas, decisões, pendências), veja `.specs/project/STATE.md`.

## Deploy em Produção

| Peça | Onde | URL |
|------|------|-----|
| Frontend | Cloudflare Pages (grátis) | `https://chamados.okurumin.com.br` |
| Backend | Cloudflare Tunnel (grátis) | `https://chamados.okurumin.com.br/api` |
| Banco | Supabase (grátis) | `aws-1-us-east-2.pooler.supabase.com` |

- **Frontend:** build do Cloudflare Pages a partir da `main`; a URL da API vem de `frontend/.env.production` (`VITE_API_BASE_URL`).
- **Backend:** API .NET rodando localmente e exposta via Cloudflare Tunnel — deploy manual.

> Custo total: **R$ 0,00** (tudo free tier). Guia completo em `docs/DEPLOY-CLOUDFLARE.md`.
