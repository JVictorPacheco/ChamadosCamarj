# STATE — Memória do Projeto

> Atualizado em: 2026-06-18

---

## 📍 Onde estamos

**Fase 2 concluída** — Domain + CQRS + API REST funcionando com SQLite em dev.

O próximo passo é resolver os concerns da **Fase 2.5** antes de iniciar o Frontend (Fase 3). Os concerns mais críticos são C-01 (banco) e C-02 (performance).

---

## ✅ Decisões tomadas

| Decisão | Detalhe |
|---------|---------|
| Banco dev | SQLite temporário — mover para PostgreSQL/Supabase em prod |
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

| ID | Blocker |
|----|---------|
| C-01 | Conflito SQLite dev vs PostgreSQL migration — resolver antes de subir para Supabase |

---

## 📋 TODOs (ordenados por prioridade)

1. Corrigir `ListarChamadosQueryHandler` — filtros no banco, não em memória (C-02)
2. `CategoriasController` usar `IMediator` (C-03)
3. Usar `DatabaseSeeder.SeedAsync()` em vez de seed inline (C-04)
4. Validators para `AtribuirChamadoCommand` e `ComentarChamadoCommand` (C-05)
5. Commands + Endpoints para `Fechar` e `Cancelar` (C-06)
6. Corrigir migration — coluna `ComentarioId` em `Anexos` (C-07)
7. Criar projeto `ChamadosCamarj.UnitTests` com testes do Domain (C-09)
8. Iniciar Frontend React (Fase 3)

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
