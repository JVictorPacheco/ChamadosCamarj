# 📋 SPEC — Sistema de Gestão de Chamados

> **Versão:** 1.1  
> **Status:** Em desenvolvimento (decisões tomadas)  
> **Metodologia:** [[⚙️ SDD — Spec-Driven Development]]

## Estrutura do Spec

O spec completo está em [[../SPEC.md|SPEC.md]].

### Seções principais

1. [[🏠 Home|Visão Geral]]
2. [[🏗️ Arquitetura]]
3. [[📊 Modelo de Dados]]
4. Fluxo do Chamado
5. Regras de Negócio
6. [[📧 Integração Email]]
7. [[👥 Perfis de Usuário]]
8. API Endpoints
9. Frontend
10. Tecnologias
11. [[🗺️ Roadmap]]
12. [[💬 Decisões]]
13. [[📝 Perguntas Pendentes]]

---

## Stack Tecnológica

| Camada | Tecnologia |
|--------|------------|
| Backend | .NET 9 + Clean Architecture |
| CQRS | MediatR |
| Validação | FluentValidation |
| ORM | EF Core 9 |
| BD | PostgreSQL (Supabase) |
| Frontend | React + TS + Vite + TailwindCSS + Shadcn/ui |
| Email | MailKit (IMAP) |
| Auth | [[🔐 Azure AD]] |
| Anexos | [[📦 Supabase Storage]] |
| Tempo real | SignalR |
| Cache | Redis |
| Logs | Serilog |
