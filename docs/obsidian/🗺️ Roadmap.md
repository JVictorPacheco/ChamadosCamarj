# 🗺️ Roadmap — Sistema de Chamados

> Última atualização: 2026-06-18

## Fases do Desenvolvimento

### ✅ Fase 0 — Setup (CONCLUÍDA)
- [x] SPEC finalizado
- [x] Decisões tomadas
- [x] Obsidian estruturado
- [x] Setup da solução .NET 9 (4 projetos)
- [x] Setup do repositório GitHub
- [x] Docker Compose (PostgreSQL local)

### ✅ Fase 1 — Domain Layer (CONCLUÍDA)
- [x] Entidades: Chamado, Comentario, Categoria, Anexo
- [x] Enums: StatusChamado, PrioridadeChamado, OrigemChamado, TipoComentario
- [x] Interfaces: IChamadoRepository, ICategoriaRepository, IEmailReceiverService, IStorageService
- [x] Migration inicial + EF Core
- [x] SLA automático no construtor do Chamado

### ✅ Fase 2 — CQRS + API (CONCLUÍDA)
- [x] Commands: Abrir, Atribuir, Atualizar, Comentar, Resolver
- [x] Queries: Listar (filtros + paginação), ObterPorId
- [x] Validators: AbrirChamadoCommand, AtualizarChamadoCommand
- [x] Controllers REST (ChamadosController + CategoriasController)
- [x] Pipeline Behavior FluentValidation
- [x] OpenAPI + Scalar UI

### 🔧 Fase 2.5 — Correções (EM ANDAMENTO)
> Ver [[⚠️ Concerns]] para status de cada item.

**Bloco 1 — ✅ Merged (PR #1)**
- [x] DatabaseSeeder substituir seed inline de Program.cs (C-04)
- [x] CategoriasController usar MediatR (C-03)
- [x] Validators para Atribuir e Comentar (C-05)
- [x] Commands + Endpoints para Fechar e Cancelar (C-06)
- [x] Fix: DTOs de request para body binding (System.Text.Json + MediatR)

**Bloco 2 — 🔃 PR aberto (PR #2)**
- [x] Filtros e paginação no banco via IQueryable (C-02)
- [x] PagedResult<T> com total, totalPaginas, temProxima/Anterior
- [x] Testado: GET /api/chamados com filtros, paginação e busca

**Bloco 3 — ⏳ Próximo**
- [ ] Criar projeto ChamadosCamarj.UnitTests (C-09)
- [ ] Testes do Domain: SLA, transições de status, validações

**Bloco 4 — ⏳ Pendente (decisão necessária)**
- [ ] Resolver SQLite dev vs PostgreSQL migration (C-01)
- [ ] Corrigir ComentarioId na migration de Anexos (C-07)

### 🎨 Fase 3 — Frontend Básico
- [ ] Login com [[🔐 Azure AD]]
- [ ] Lista de chamados com filtros
- [ ] Detalhe do chamado
- [ ] Abertura de chamado (portal)

### 📧 Fase 4 — Integração Email + Storage
- [ ] EmailReceiverService (IMAP/MailKit)
- [ ] Parsing de email → Chamado automático
- [ ] Resposta automática
- [ ] Anexos via [[📦 Supabase Storage]]

### 📊 Fase 5 — Kanban + Dashboard
- [ ] Kanban (status drag & drop)
- [ ] Dashboard com gráficos
- [ ] Notificações SignalR em tempo real
- [ ] Filtros avançados

### 🔐 Fase 6 — Azure AD + Admin
- [ ] Login corporativo completo
- [ ] Perfis e permissões (RBAC)
- [ ] Admin: categorias, usuários, configs

### 📈 Fase 7 — Relatórios + SLA
- [ ] Relatórios por período/categoria/atendente
- [ ] SLA tracking com alertas
- [ ] Exportação (CSV/PDF)

---

> **Progresso atual:** 🔧 Fase 2.5 — Bloco 2 com PR aberto. Próximo: Bloco 3 (testes unitários).
