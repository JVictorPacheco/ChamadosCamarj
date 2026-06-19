# STATE — Memória do Projeto

> Atualizado em: 2026-06-19

---

## 📍 Onde estamos

**Fase 2.5 concluída** — todos os concerns (C-01 a C-09) resolvidos. API roda em PostgreSQL real via Supabase (pooler, sessão), migrations aplicadas com `MigrateAsync()`, seed centralizado, validators completos, ciclo de vida do chamado (Fechar/Cancelar) exposto, 48 testes unitários passando.

Próximo passo: **Fase 3 — Frontend React**.

---

## ✅ Decisões tomadas

| Decisão | Detalhe |
|---------|---------|
| Banco dev e prod | PostgreSQL via Supabase — mesma instância para os dois ambientes |
| Conexão Supabase | **Session pooler** (`aws-1-us-east-2.pooler.supabase.com:5432`), não "Direct connection" (IPv6-only, falha em redes sem IPv6) nem "Transaction pooler" (incompatível com prepared statements do EF Core) |
| Senha do banco | `dotnet user-secrets` local (dev) — nunca em `appsettings.json` |
| Auth | Azure AD (Microsoft Entra ID) — implementar na Fase 3/6 |
| Anexos | Supabase Storage — implementar na Fase 4 |
| Email | MailKit IMAP — suporte@camarj.com.br / ti@camarj.com.br |
| Frontend | React + TS + Vite + TailwindCSS + Shadcn/ui |
| Seed | 5 categorias fixas com GUIDs determinísticos |
| Atendentes | Victor (Admin) + Fábio (Atendente) |
| SLA | Urgente 8h, Alta 24h, Média 16h, Baixa 48h |
| Notificações | Push navegador + Desktop (Electron/Tauri futuro) |
| Mobile | Web primeiro, mobile no futuro |
| Docs | Obsidian (docs/obsidian/) |

---

## 🔴 Blockers ativos

Nenhum.

---

## 📌 Pendências (não bloqueantes)

| Pendência | Detalhe |
|-----------|---------|
| Hospedagem em produção | Onde a API vai rodar (VM, Docker, Azure App Service etc.) e como a connection string será injetada lá — decidir antes do deploy, não bloqueia o Frontend |

---

## 📋 TODOs (ordenados por prioridade)

1. Iniciar Frontend React (Fase 3)

---

## 💡 Ideias adiadas (deferred)

- **Reembolso workflow:** Possível integração com sistema financeiro no futuro
- **App mobile:** Web primeiro, avaliar PWA ou React Native depois
- **Electron/Tauri:** Para notificações desktop — após o web estar estável
- **Redis:** Cache planejado no SPEC mas sem prioridade definida
- **Serilog:** Logging estruturado — adicionar antes de ir para produção

---

## 🎓 Aprendizados

- `EnsureCreated()` não aplica migrations — bom para dev rápido, perigoso para mudanças de schema
- `ObterTodosAsync()` + filtro em memória é um padrão a evitar desde o início
- `CategoriasController` foi uma exceção ao padrão CQRS — deve ser corrigido
