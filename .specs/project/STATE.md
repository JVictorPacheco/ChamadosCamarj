# STATE — Memória do Projeto

> Atualizado em: 2026-06-18 (fim da sessão)

---

## 📍 Onde estamos

**Fase 2.5 em andamento.** Bloco 1 merged, Bloco 2 com PR aberto aguardando merge.

Ao retornar: fazer merge do PR #2 (Bloco 2) e iniciar o **Bloco 3 — testes unitários**.

---

## ✅ Concluído nesta sessão (2026-06-18)

### Bloco 1 — PR #1 — Merged ✅
- C-04: Seed centralizado no `DatabaseSeeder.SeedAsync()`
- C-03: `CategoriasController` via `IMediator` + `ListarCategoriasQuery`
- C-05: Validators para `AtribuirChamadoCommand` e `ComentarChamadoCommand`
- C-06: `FecharChamadoCommand` + `CancelarChamadoCommand` + endpoints
- Fix: DTOs de request para body binding (`AbrirChamadoRequest`, `ComentarChamadoRequest`)

### Bloco 2 — PR #2 — Aberto 🔃
- C-02: Filtros e paginação movidos para o banco via `IQueryable`
- `PagedResult<T>`: novo DTO com `Items`, `Total`, `TotalPaginas`, `TemProxima`, `TemAnterior`
- `IChamadoRepository.ListarAsync()`: novo método com filtros tipados
- Testado: GET /api/chamados com status, prioridade, categoriaId, busca e paginação

### Infraestrutura Git criada nesta sessão
- `.specs/` criado com 10 documentos de mapeamento brownfield
- `docs/obsidian/` atualizado (Roadmap, Concerns, Home)
- `.gitignore` atualizado (*.db, workspace.json)
- Branch padrão de trabalho: `feature/*` → `develop` → PR → merge

---

## 🔴 Blockers ativos

| ID | Blocker |
|----|---------|
| C-01 | SQLite dev vs PostgreSQL migration — resolver antes de ir para Supabase |

---

## 📋 Próximos passos (ordem)

1. **Agora:** Fazer merge do PR #2 (Bloco 2)
2. **Bloco 3:** Criar `ChamadosCamarj.UnitTests` — testes do Domain
   - `ChamadoTests`: SLA por prioridade, transições de status, Cancelar só de Aberto/EmAndamento
   - `CategoriaTests`: Ativar/Desativar
   - Validators: AbrirChamadoCommandValidator
3. **Bloco 4:** Decisão sobre banco (SQLite dev vs PostgreSQL) + corrigir migration (C-07)
4. **Fase 3:** Frontend React

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
| Git flow | feature/* → develop (PR) → main (release) |
| Body binding | DTOs de request separados dos Commands MediatR |

---

## 💡 Ideias adiadas (deferred)

- App mobile (PWA ou React Native) — após web estável
- Electron/Tauri para notificações desktop
- Redis cache
- Serilog logging estruturado — antes de ir para produção
