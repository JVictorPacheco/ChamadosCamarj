# 🗺️ Roadmap — Sistema de Chamados

> Última atualização: 2026-06-23

## Fases do Desenvolvimento

### ✅ Fase 0 — Setup (CONCLUÍDA)
- [x] SPEC finalizado
- [x] Decisões tomadas
- [x] Obsidian estruturado
- [x] Setup da solução .NET 9 (4 projetos)
- [x] Setup do repositório GitHub
- [x] Docker Compose (PostgreSQL local — descontinuado depois da migração pro Supabase)

### ✅ Fase 1 — Domain Layer (CONCLUÍDA)
- [x] Entidades: Chamado, Comentario, Categoria, Anexo
- [x] Enums: StatusChamado, PrioridadeChamado, OrigemChamado, TipoComentario
- [x] Interfaces: IChamadoRepository, ICategoriaRepository, IEmailReceiverService, IStorageService
- [x] Migration inicial + EF Core
- [x] SLA automático no construtor do Chamado

### ✅ Fase 2 — CQRS + API (CONCLUÍDA)
- [x] Commands: Abrir, Atribuir, Atualizar, Comentar, Resolver, Fechar, Cancelar
- [x] Queries: Listar (filtros + paginação no banco), ObterPorId
- [x] Validators: todos os Commands
- [x] Controllers REST (ChamadosController + CategoriasController, via MediatR)
- [x] Pipeline Behavior FluentValidation
- [x] OpenAPI + Scalar UI

### ✅ Fase 2.5 — Correções + Migração Postgres (CONCLUÍDA)
> Ver [[⚠️ Concerns]] — C-01 a C-10 todos resolvidos.
- [x] Migração SQLite → PostgreSQL via Supabase (dev e prod, mesma instância)
- [x] Filtros de ListarChamados no banco (não em memória)
- [x] CategoriasController usando MediatR
- [x] DatabaseSeeder centralizado
- [x] Validators para Atribuir e Comentar
- [x] Commands + Endpoints para Fechar e Cancelar
- [x] Migration corrigida (ComentarioId em Anexos)
- [x] Projeto de testes unitários (55 testes)
- [x] **3 bugs de teste manual corrigidos**: categoria inexistente sem validação, transições de status sem guard, `DbUpdateConcurrencyException` ao comentar
- [x] Middleware global de tratamento de erro (antes não existia — toda exceção virava 500 cru)

### 🎨 Fase 3 — Frontend: Portal do Solicitante (EM ANDAMENTO — planejamento pronto, Execute não iniciado)
> Spec-Driven completo em `.specs/features/frontend-portal-solicitante/` (spec + design + 23 tasks, todas aprovadas).
- [ ] **API-01**: endpoint `GET /chamados/{id}/comentarios` (pré-requisito de backend — falta o conteúdo dos comentários na API hoje)
- [ ] Setup React + Vite + TS + TailwindCSS + Shadcn/ui em `/frontend`
- [ ] Seletor de perfil mockado (Admin/Atendente/Solicitante — sem [[🔐 Azure AD]] real ainda)
- [ ] Abertura de chamado (portal)
- [ ] Lista de chamados com filtros
- [ ] Detalhe do chamado (com comentários)
- [ ] Comentários públicos
- [ ] Decisão pendente: teste E2E (Playwright) do fluxo principal

### 📧 Fase 4 — Integração Email + Storage
- [ ] EmailReceiverService (IMAP/MailKit)
- [ ] Parsing de email → Chamado automático
- [ ] Resposta automática
- [ ] Anexos via [[📦 Supabase Storage]]

### 📊 Fase 5 — Kanban + Dashboard
- [ ] Telas de Atendente (fila, assumir, resolver, fechar) — ficaram de fora da Fase 3 de propósito
- [ ] Kanban (status drag & drop)
- [ ] Dashboard com gráficos
- [ ] Notificações SignalR em tempo real
- [ ] Filtros avançados

### 🔐 Fase 6 — Azure AD + Admin
- [ ] Login corporativo real (substitui o mock da Fase 3)
- [ ] Perfis e permissões (RBAC)
- [ ] Admin: categorias, usuários, configs

### 📈 Fase 7 — Relatórios + SLA
- [ ] Relatórios por período/categoria/atendente
- [ ] SLA tracking com alertas
- [ ] Exportação (CSV/PDF)

---

> **Progresso atual:** ✅ Fase 2.5 concluída → 🎨 Fase 3 planejada, aguardando Execute (ver `.specs/HANDOFF.md` pra retomar exatamente de onde parou).
