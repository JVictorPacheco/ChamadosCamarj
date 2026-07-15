# 🗺️ Roadmap — Sistema de Chamados

> Última atualização: 2026-07-15. Roadmap detalhado e sempre atualizado em `.specs/project/ROADMAP.md`.

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
> Mergeada em `develop`/`main` em 2026-06-30. Dashboard retrabalhado em 2026-07-14/15 (ver abaixo).

- [x] Kanban com drag & drop (dnd-kit) entre colunas de status
- [x] Dashboard com métricas: total por status, alertas de SLA, chamados recentes
- [x] Notificações SignalR em tempo real
- [x] Fila de Atendimento (chamados Abertos ordenados por prioridade)
- [x] Botão Assumir na Fila de Atendimento
- [x] Ações no Detalhe: Assumir, Resolver, Fechar, Cancelar (por perfil + status)
- [x] "Meus Chamados" diferenciado por perfil (Admin=todos, Atendente=responsavelId, Solicitante=email)
- [x] Bug fix: Link aninhado no card da Fila eliminado
- [x] Bug fix (2026-07-14): Dashboard não mostrava Cancelados/Resolvidos (só "hoje"); card "Abertos" agora detalha assumidos vs. em espera
- [x] Retrabalho (2026-07-14/15): gráfico de Tendência (linha, 7 dias) substituído por rosca "Distribuição por situação" (Aguardando/Assumido/Resolvido/Encerrado/Cancelado, foto do momento); KPIs simplificados pra só Resolvidos Hoje + Tempo Médio; corrigido bug de `ObterTendenciaAsync` que contava "resolvidos" pela data de criação, não de resolução

### 📧 Fase 4 — Integração Email + Storage
> Ainda não iniciada. Pode ser feita em paralelo com a Fase 6.

- [ ] EmailReceiverService (IMAP/MailKit — suporte@camarj.com.br / ti@camarj.com.br)
- [ ] Parsing de email → Chamado automático
- [ ] Resposta automática
- [ ] Anexos via [[📦 Supabase Storage]]

### 🔐 Fase 6 — Admin Completo + Log + Google Workspace (PAUSADA em T09)

> ⚠️ **Corrigido em 2026-06-25:** Camarj usa **Google Workspace** (Gmail corporativo), não Azure AD.
> **Pausada em 2026-07-14** a pedido do usuário — Fase 7 (Relatório Mensal) antecipada por ter prazo real (fechamento mensal pra superintendência). Trabalho feito em `feature/fase-6-admin-log` (ainda não mergeada em `develop`, commits pushados). T01-T08 (backend) e T10-T14 (frontend) concluídos e verificados via Playwright em 2026-07-14.

**Backend — completo:**
- [x] **Log de histórico** — entidade `HistoricoEntrada` + `IHistoricoRepository`, ver [[📋 Histórico de Chamados]]
- [x] **Reatribuição Admin** — endpoint `PATCH /chamados/{id}/reatribuir` (qualquer status não-final)
- [x] Geração de `HistoricoEntrada` integrada em todos os CommandHandlers
- [x] Endpoint `GET /chamados/{id}/historico`
- [x] **Alterar prioridade** — endpoint `PATCH /chamados/{id}/prioridade`
- [x] **Comentários internos** — filtro por perfil em `ListarComentariosQueryHandler`
- [ ] Login real via [[🔐 Google Workspace]] (T09 — endpoint `POST /auth/google`, JWT, tabela `UsuarioPerfil`) — não iniciado

**Frontend — completo (T10-T14):**
- [x] Reatribuir, Histórico (timeline), Alterar Prioridade, Comentário interno — componentes reescritos do zero em `frontend/src/features/chamados/` (uma versão anterior tinha sido commitada no caminho errado com padrões inexistentes no projeto)
- [ ] **Forçar encerramento** — Admin pode fechar/cancelar sem seguir o fluxo normal (ainda não abordado)
- [ ] Login real via Google Workspace (T15, depende de T09)
- [ ] Mapeamento conta→perfil no backend
- [ ] RBAC real (baseado em claims do token Google)
- [ ] Admin: gerenciar categorias, usuários e configurações

### 📈 Fase 7 — Relatório Mensal (CONCLUÍDA — antecipada)

> **Antecipada em 2026-07-14** na frente de T09/T15 — usuário precisa de um relatório mensal pra apresentar à superintendência todo fim de mês. Spec completo em `.specs/features/relatorio-mensal/`. Ver [[📈 Relatório Mensal]].

- [x] Seletor de mês, totais (abertos/resolvidos/cancelados + variação % vs. mês anterior)
- [x] Quebra por categoria e por atendente, cumprimento de SLA (rosca), tempo médio de resolução
- [x] Dados vindos de `HistoricoEntrada` (data real de cada evento), não do status atual do chamado
- [x] Exportação CSV (client-side) e PDF (via impressão)
- [x] RBAC: Admin vê tudo, Atendente só os próprios números, Solicitante bloqueado de verdade (não só link escondido)
- [ ] Relatórios por período livre (além do mês fechado) — fora de escopo por ora
- [ ] SLA tracking com alertas de vencimento em tempo real — fora de escopo por ora
- [ ] Dashboard de carga por atendente — fora de escopo por ora

---

> **Progresso atual:** ✅ Fases 0-3 e 5 concluídas, ✅ Fase 7 (Relatório Mensal) concluída, 🔐 Fase 6 quase completa (só falta T09/T15 — login Google real). Próximo passo: retomar T09/T15 e abrir PR de `feature/fase-6-admin-log` → `develop`.
