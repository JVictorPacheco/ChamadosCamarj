# 🗺️ Roadmap — Sistema de Chamados

> Última atualização: 2026-07-01

## Fases do Desenvolvimento

### ✅ Fase 0 — Setup (CONCLUÍDA)
- [x] SPEC finalizado
- [x] Decisões tomadas
- [x] Obsidian estruturado
- [x] Setup da solução .NET 9 (4 projetos)
- [x] Setup do repositório GitHub
- [x] Docker Compose (PostgreSQL local — descontinuado após migração pro Supabase)

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
- [x] 59 testes unitários passando
- [x] 3 bugs de teste manual corrigidos (categoria inexistente, transições de status sem guard, DbUpdateConcurrencyException)
- [x] Middleware global de tratamento de erro

### ✅ Fase 3 — Frontend: Portal do Solicitante (CONCLUÍDA)
- [x] API-01: endpoint `GET /chamados/{id}/comentarios`
- [x] Setup React 19 + Vite + TS + TailwindCSS v4 + Shadcn/ui em `/frontend`
- [x] Seletor de perfil mockado (Admin/Atendente/Solicitante — sem [[🔐 Google Workspace]] real ainda)
- [x] Abertura de chamado (portal)
- [x] Lista de chamados com filtros + paginação
- [x] Detalhe do chamado (com comentários)
- [x] Comentários públicos
- [x] Teste E2E (Playwright) — `e2e/fluxo-completo.spec.ts`

### ✅ Fase 5 — Kanban + Dashboard + Ações de Atendente (CONCLUÍDA)
- [x] Kanban com drag & drop (dnd-kit) entre colunas de status
- [x] Dashboard com métricas: total por status, alertas de SLA, chamados recentes
- [x] Notificações SignalR em tempo real
- [x] Fila de Atendimento (chamados Abertos ordenados por prioridade)
- [x] Botão Assumir na Fila de Atendimento
- [x] Ações no Detalhe: Assumir, Resolver, Fechar, Cancelar (por perfil + status)
- [x] "Meus Chamados" diferenciado por perfil (Admin=todos, Atendente=responsavelId, Solicitante=email)
- [x] Bug fix: Link aninhado no card da Fila eliminado

### 📧 Fase 4 — Integração Email + Storage
- [ ] EmailReceiverService (IMAP/MailKit — suporte@camarj.com.br / ti@camarj.com.br)
- [ ] Parsing de email → Chamado automático
- [ ] Resposta automática
- [ ] Anexos via [[📦 Supabase Storage]]

### 🔐 Fase 6 — Admin Completo + Log + Google Workspace

> ⚠️ **Corrigido em 2026-06-25:** Camarj usa **Google Workspace** (Gmail corporativo), não Azure AD.

- [ ] **Reatribuição Admin** — mover chamado entre atendentes (qualquer status não-final)
- [ ] **Log de histórico** — ver [[📋 Histórico de Chamados]] para detalhes
- [ ] **Comentários internos** — visíveis só para Admin/Atendente
- [ ] **Alterar prioridade** — Admin pode alterar de qualquer chamado
- [ ] **Forçar encerramento** — Admin pode fechar/cancelar sem seguir o fluxo normal
- [ ] Login real via [[🔐 Google Workspace]] (substitui o seletor mockado)
- [ ] Mapeamento conta→perfil no backend
- [ ] RBAC real (baseado em claims do token Google)
- [ ] Admin: gerenciar categorias, usuários e configurações

### 📈 Fase 7 — Relatórios + SLA
- [ ] Relatórios por período/categoria/atendente
- [ ] SLA tracking com alertas de vencimento
- [ ] Exportação CSV/PDF
- [ ] Dashboard de carga por atendente

---

> **Progresso atual:** ✅ Fases 0-3 e 5 concluídas → 🔐 Fase 6 (Admin + Log + Auth) como próximo passo.
