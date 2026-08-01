# 🗺️ Roadmap — Sistema de Chamados

> Última atualização: 2026-07-21. Roadmap detalhado e sempre atualizado em `.specs/project/ROADMAP.md`.

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

### 📧 Fase 4 — Integração Email + Storage (Storage CONCLUÍDO — 2026-07-21)
> Storage de Anexos implementado e verificado de ponta a ponta. Email/IMAP segue sem data — depende de senha de app das caixas suporte@/ti@camarj.com.br.

- [ ] EmailReceiverService (IMAP/MailKit — suporte@camarj.com.br / ti@camarj.com.br)
- [ ] Parsing de email → Chamado automático
- [ ] Resposta automática
- [x] Anexos via [[📦 Supabase Storage]] — upload/listagem/download, verificado contra o Supabase real (ver nota lá sobre os 2 bugs corrigidos)

### 🔐 Fase 6 — Admin Completo + Log + Google Workspace (praticamente concluída)

> ⚠️ **Corrigido em 2026-06-25:** Camarj usa **Google Workspace** (Gmail corporativo), não Azure AD.
> **Pausada em 2026-07-14** a pedido do usuário — Fase 7 (Relatório Mensal) antecipada por ter prazo real (fechamento mensal pra superintendência). Trabalho feito em `feature/fase-6-admin-log`, **mergeado em `develop` via PR #13 em 2026-07-15**. T01-T14 concluídos e verificados. **Retomada em 2026-07-16/18:** F5a (login mockado por e-mail + cadastro de usuários) implementada como passo intermediário, depois T09/T15 (login Google real) implementado por completo em 2026-07-18. **2026-07-24:** login Google OAuth ficou dormante — TI informou que o Client ID está fora do plano CAMARJ. Auth ativa é **email+senha** (ASP.NET Core Identity/PasswordHasher).

**Backend — completo:**
- [x] **Log de histórico** — entidade `HistoricoEntrada` + `IHistoricoRepository`, ver [[📋 Histórico de Chamados]]
- [x] **Reatribuição Admin** — endpoint `PATCH /chamados/{id}/reatribuir` (qualquer status não-final)
- [x] Geração de `HistoricoEntrada` integrada em todos os CommandHandlers
- [x] Endpoint `GET /chamados/{id}/historico`
- [x] **Alterar prioridade** — endpoint `PATCH /chamados/{id}/prioridade`
- [x] **Comentários internos** — filtro por perfil em `ListarComentariosQueryHandler`
- [x] **F5a (2026-07-16):** cadastro/gestão de usuários pelo Admin — tabela `UsuarioPerfil`, `UsuariosController` (CRUD + ativar/desativar), passo intermediário não descartável rumo ao login real
- [x] **T09 — Login real via [[🔐 Google Workspace]] (2026-07-18):** `POST /auth/google` valida o token do Google, JWT próprio (simétrico, 8-12h), autenticação global por padrão (`RequireAuthenticatedUser`), `ICurrentUserService` substitui dados de ator vindos do cliente em todos os Controllers. ⚠️ **DORMENTE (2026-07-24)** — TI informou que o Client ID está fora do plano CAMARJ.
- [x] **Auth email+senha (2026-07-24):** `POST /auth/login` com ASP.NET Core Identity `PasswordHasher`, senhas definidas pelo Admin. Login Google OAuth mantido dormante. Spec em `.specs/features/auth-email-senha/spec.md`

**Frontend — completo (T10-T14 + T15):**
- [x] Reatribuir, Histórico (timeline), Alterar Prioridade, Comentário interno — componentes reescritos do zero em `frontend/src/features/chamados/` (uma versão anterior tinha sido commitada no caminho errado com padrões inexistentes no projeto)
- [x] **T15 — Login real via Google Workspace (2026-07-18):** `LoginPage` com `GoogleLogin`/`GoogleOAuthProvider`, logout automático por 20min de inatividade, redesenho visual (logo maior, tipografia serifada) — ⚠️ **DORMENTE**, substituído pela `LoginPage` de email+senha
- [x] **Forçar encerramento** (2026-07-19) — Admin fecha um chamado direto de qualquer status não-final, com motivo obrigatório auditado no histórico (`AcaoHistorico.EncerramentoForcado`). Ver `.specs/features/forcar-encerramento/`
- [ ] Admin: gerenciar categorias e configurações do sistema (usuários já implementado no F5a)

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

### 🗄️ Arquivo de Chamados (CONCLUÍDA — 2026-07-18)

> Pedido pelo usuário em 2026-07-16: chamados finalizados (Resolvido/Fechado/Cancelado) misturados com os ativos no dia a dia. Decisão explícita: **nunca apagar chamados** (quebraria auditoria/Relatório Mensal) — solução é uma tela separada de leitura filtrada. Spec → Design → Tasks → Execute completos em `.specs/features/arquivo-de-chamados/`.

- [x] Nova tela "Arquivo" (mesmo RBAC de "Meus Chamados") lista só Resolvido/Fechado/Cancelado
- [x] Filtros: status, prioridade, categoria, busca, período (`DataCriacao`)
- [x] Backend aditivo: `Finalizados=true` + `DataInicio`/`DataFim` em `ListarChamadosQuery` (não quebra Kanban/Fila)
- [x] Bug corrigido (achado pelo usuário testando): filtro de data quebrava com 500 (`DateTime Kind=Unspecified` vs. `timestamptz` do Postgres)

### 🔢 Número do Chamado (CONCLUÍDA — 2026-07-19/20)

> `Guid` interno não é referenciável em conversa/e-mail. Formato: `CAM-{número}`, sem zero-padding. Spec em `.specs/features/numero-do-chamado/`.

- [x] Coluna `Numero` gerada por sequence do Postgres, backfill cronológico dos chamados existentes
- [x] Exibido em toda tela de lista/detalhe
- [x] Busca por número no campo de busca já existente (`"42"` ou `"CAM-42"`)

### 🔒 RBAC real do Dashboard/Kanban/Fila (CONCLUÍDA — 2026-07-20)

> As 3 telas só escondiam o link da sidebar pro Solicitante. Aplicado o mesmo bloqueio real já usado no Relatório Mensal/Admin > Usuários.

---

> **Progresso atual:** ✅ Fases 0-3, 5 e 7 concluídas. ✅ Fase 6 praticamente completa — login ativo é **email+senha** (PasswordHasher), Google OAuth dormante. ✅ Arquivo de Chamados, Forçar Encerramento, Número do Chamado, RBAC real, Storage de Anexos (Fase 4 metade 1) e auth email+senha todos concluídos. Próximo passo: senha de app do IMAP (Fase 4 metade 2 — Email); decisão de hospedagem em produção também pendente.
