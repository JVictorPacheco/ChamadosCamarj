# Roadmap — ChamadosCamarj

> Última atualização: 2026-07-16

## ✅ Fase 0 — Setup

- [x] SPEC finalizado e Obsidian estruturado
- [x] Solução .NET 9 criada (4 projetos)
- [x] Repositório GitHub
- [x] Docker Compose (PostgreSQL local)
- [x] SQLite configurado para dev

## ✅ Fase 1 — Domain Layer (COMPLETA)

- [x] `BaseEntity` (Id Guid, DataCriacao, DataAtualizacao)
- [x] Entidades: `Chamado`, `Comentario`, `Categoria`, `Anexo`
- [x] Enums: `StatusChamado`, `PrioridadeChamado`, `OrigemChamado`, `TipoComentario`
- [x] Interfaces: `IChamadoRepository`, `ICategoriaRepository`, `IEmailReceiverService`, `IStorageService`
- [x] Lógica de negócio: Atribuir, Resolver, Fechar, Reabrir, Cancelar, AlterarPrioridade
- [x] SLA calculado automaticamente no construtor do Chamado
- [x] Migration inicial (PostgreSQL schema)

## ✅ Fase 2 — CQRS + API (COMPLETA)

- [x] Commands: Abrir, Atribuir, Atualizar, Comentar, Resolver, Fechar, Cancelar
- [x] Queries: Listar (com filtros e paginação), ObterPorId
- [x] Validators: todos os Commands
- [x] Controllers: ChamadosController (endpoints completos), CategoriasController
- [x] Pipeline Behavior (FluentValidation)
- [x] Mapeamento manual `Chamado → ChamadoResponse`
- [x] OpenAPI + Scalar UI
- [x] Seed das 5 categorias da CAMARJ

## ✅ Fase 2.5 — Correções antes de avançar (COMPLETA)

> Identificados no mapeamento — todos resolvidos em 2026-06-19

- [x] C-01: Resolver conflito SQLite dev vs PostgreSQL migration — migrado para Supabase (session pooler)
- [x] C-02: Mover filtros de ListarChamados para query no banco
- [x] C-03: CategoriasController usar MediatR
- [x] C-04: Remover seed inline de Program.cs, usar DatabaseSeeder
- [x] C-05: Criar validators para Atribuir e Comentar
- [x] C-06: Criar Commands + Endpoints para Fechar e Cancelar
- [x] C-07: Corrigir migration — adicionar ComentarioId em Anexos
- [x] C-09: Criar projeto de testes (Domain unit tests) — 59 testes passando
- [x] 3 bugs de teste manual corrigidos: categoria inexistente sem validação, transições de status sem guard, DbUpdateConcurrencyException ao comentar

## ✅ Fase 3 — Frontend: Portal do Solicitante (COMPLETA)

> Spec em `.specs/features/frontend-portal-solicitante/spec.md`. Escopo: visão do Solicitante. Auth mockada.

- [x] API-01: endpoint `GET /api/chamados/{id}/comentarios`
- [x] API-02: filtro `solicitanteEmail` em `GET /api/chamados`
- [x] Setup React 19 + Vite + TailwindCSS v4 + Shadcn/ui, tema dark customizado
- [x] Seletor de perfil mockado (Admin/Atendente/Solicitante) — Google Workspace real fica pra Fase 6
- [x] Abertura de chamado — `AbrirChamadoPage`
- [x] Lista de chamados com filtros + paginação — `ChamadosListPage`
- [x] Detalhe do chamado — `ChamadoDetailPage` (404 + RBAC-lite de UI)
- [x] Comentários públicos — `ComentarioList` / `ComentarioForm`
- [x] 1 teste E2E (Playwright) cobrindo o fluxo feliz completo

## ✅ Fase 5 — Kanban + Dashboard + Ações de Atendente (COMPLETA)

> Spec em `.specs/features/fase-5-kanban-dashboard/spec.md`. Mergeado em `develop` e `main` (2026-06-30).

- [x] Kanban com drag & drop (dnd-kit) entre colunas de status
- [x] Dashboard com métricas: total por status, alertas de SLA, chamados recentes
- [x] Notificações SignalR em tempo real (criação, mudança de status, comentários)
- [x] Fila de Atendimento — lista de chamados Abertos ordenados por prioridade
- [x] Botão Assumir na Fila de Atendimento
- [x] Ações no Detalhe do Chamado: Assumir, Resolver, Fechar, Cancelar (por perfil + status)
- [x] RBAC de UI: atendentes/admin veem ações corretas, solicitante só vê o que é seu
- [x] "Meus Chamados" diferenciado por perfil (Admin=todos, Atendente=responsavelId, Solicitante=solicitanteEmail)
- [x] Bug fix: Link aninhado no card da Fila eliminado
- [x] Bug fix (2026-07-14): Dashboard não mostrava Cancelados/Resolvidos (só "hoje"); card "Abertos" agora detalha assumidos vs em espera
- [x] Retrabalho (2026-07-14/15): gráfico de Tendência (linha, 7 dias) substituído por rosca "Distribuição por situação" (Aguardando/Assumido/Resolvido/Encerrado/Cancelado, foto do momento); KPIs simplificados pra só Resolvidos Hoje + Tempo Médio, já que a rosca cobre o resto

## 📧 Fase 4 — Integração Email + Storage

> Ainda não iniciada. Pode ser feita em paralelo com Fase 6.

- [ ] `EmailReceiverService` (IMAP/MailKit — suporte@camarj.com.br / ti@camarj.com.br)
- [ ] Parsing de e-mail → Chamado automático
- [ ] Resposta automática ao solicitante
- [ ] `StorageService` (Supabase Storage S3)
- [ ] Upload/download de anexos no portal

## 🔐 Fase 6 — Admin Completo + Log + Google Workspace (PAUSADA em T09)

> **Pausada em 2026-07-14** a pedido do usuário — Fase 7 (Relatório Mensal) antecipada por ter prazo real (fechamento mensal pra superintendência). Retomar T09/T15 depois.

> Corrigido em 2026-06-25: Camarj usa Google Workspace (Gmail corporativo), não Azure AD.
> Planejado em 2026-07-01: features de Admin e auditoria.
> Spec em `.specs/features/fase-6-admin-log/spec.md`.
> Trabalho feito em `feature/fase-6-admin-log`, **mergeada em `develop` via PR #13 em 2026-07-15**. T01-T14 concluídos e verificados via Playwright.

**Backend — completo (T01-T08), incluindo ator real na auditoria:**
- [x] T01 `HistoricoEntrada` + enum `AcaoHistorico` + `IHistoricoRepository`
- [x] T02 `Chamado.Reatribuir()` na domain entity
- [x] T03/T04 `ReatribuirChamadoCommand` + endpoint `PATCH /chamados/{id}/reatribuir`
- [x] T05 Geração de `HistoricoEntrada` integrada em todos os CommandHandlers (Abrir, Atribuir, Resolver, Fechar, Cancelar, Reatribuir, AlterarPrioridade) — `UsuarioId`/`UsuarioNome` agora vêm do `AuthContext` mockado do frontend em vez de "Sistema" fixo (ver Aprendizados)
- [x] T06 `ListarHistoricoQuery` + endpoint `GET /chamados/{id}/historico`
- [x] T07 endpoint `PATCH /chamados/{id}/prioridade`
- [x] T08 Filtro de comentários internos por perfil em `ListarComentariosQueryHandler` (endpoint corrigido pra repassar `perfilUsuario`)
- [x] **F5a (decidido em 2026-07-15, IMPLEMENTADO e MERGEADO em `develop` em 2026-07-16):** Login mockado por e-mail + cadastro de usuários (Admin) — tabela `UsuarioPerfil`, `UsuariosController` (CRUD), tela `Admin > Usuários` com bloqueio real de RBAC, `LoginPage` substitui `ProfileSelector`. T09a-T09e completas, testadas contra o Supabase real, revisadas por um code review sênior (4 bugs Altos corrigidos antes do commit), validadas pelo usuário e pushadas (commits `76ce0d1`/`a0747a7`). Não é descartável: a tabela `UsuarioPerfil` é reaproveitada sem mudança pelo T09 real. 15 itens de débito técnico (Médio/Baixo) da revisão ficaram documentados em `.specs/codebase/CONCERNS.md`, ainda pendentes
- [ ] T09 (F5b) Login Google Workspace real (endpoint `POST /auth/google`, JWT) — depende de F5a. Quando entrar, trocar `UsuarioId`/`UsuarioNome` client-supplied por claims do JWT
- [ ] Documento pra TI com pré-requisitos de infra (OAuth Client ID, domínio autorizado, redirect URIs) — pedido pelo usuário em 2026-07-15, ainda não escrito

**Frontend — completo (T10-T14), reescrito e verificado em 2026-07-14:**
- [x] T10-T14 (Reatribuir, Histórico, Alterar Prioridade, Comentário interno) — os componentes originais tinham sido commitados no caminho errado (`src/ChamadosCamarj.Web/...`) usando padrões inexistentes no projeto (axios, toast, shadcn não instalado, tema claro). Reescritos do zero em `frontend/src/features/chamados/`, seguindo os padrões reais (`apiFetch`, erro inline, shadcn via CLI, tema dark). `ComentarioForm`/`ComentarioList` estendidos em vez de duplicados.
- [ ] **Forçar encerramento** — Admin pode fechar/cancelar sem seguir o fluxo normal (ainda não abordado)
- [ ] **Login real via Google Workspace** — substitui a `LoginPage` mockada do F5a (T15, depende de T09)
- [x] ~~Mapeamento conta→perfil no backend~~ → entra pelo F5a (tabela `UsuarioPerfil`), antes do T09 real
- [ ] RBAC real (baseado em claims do token Google)
- [ ] Admin: gerenciar categorias, usuários e configurações do sistema

## 📈 Fase 7 — Relatório Mensal (CONCLUÍDA — antecipada)

> **Antecipada em 2026-07-14** na frente de T09/T15 — usuário precisa de um relatório mensal de andamento dos chamados pra apresentar à superintendência todo fim de mês. Spec completo em `.specs/features/relatorio-mensal/` (spec → design → tasks → execute). **Mergeada em `develop` via PR #13 em 2026-07-15.**

- [x] **Relatório mensal**: seletor de mês, totais (abertos/resolvidos/cancelados + variação % vs mês anterior), quebra por categoria e por atendente, cumprimento de SLA (rosca), tempo médio de resolução
- [x] Dados vindos de `HistoricoEntrada` (data real de cada evento — REL-10), não do status atual do chamado
- [x] Exportação CSV (client-side) e PDF (via impressão, `@media print` dedicado) — sem bibliotecas novas
- [x] RBAC: Admin vê tudo, Atendente só os próprios números (sem quebra por atendente), Solicitante bloqueado de verdade (não só link escondido)
- [x] Bug corrigido de quebra: Dashboard tinha `ObterTendenciaAsync` contando "resolvidos" pela data de criação, não de resolução
- [ ] Relatórios por período livre (granularidade além do mês fechado) — fora de escopo por ora
- [ ] SLA tracking com alertas de vencimento (tempo real — diferente do relatório, que é histórico)
- [ ] Exportação CSV/PDF
- [ ] Dashboard de carga por atendente

**Retrabalho de visualização de dados (2026-07-16):** Dashboard e Relatório Mensal revisados como um review de dev sênior — cores de gráfico migradas de hex fixo pra tokens do tema (`--chart-1..5`, `--status-good/critical` em `frontend/src/index.css`, validados com a skill `dataviz`), bug de cor cinza-puro corrigido no tema claro, labels diretos nas fatias da rosca (o hover não existe no PDF exportado), cor de sinal (verde/vermelho) na variação % do Relatório Mensal. Detalhes em `.specs/HANDOFF.md`.

## 🗄️ Fase 8 — Arquivo de Chamados (SPEC PRONTA — não iniciada)

> Pedido pelo usuário em 2026-07-16 ao testar a aplicação: chamados finalizados (Resolvido/Fechado/Cancelado) misturados com os ativos nas telas do dia a dia. Decisão explícita: **nunca apagar chamados** (quebraria `HistoricoEntrada`/Relatório Mensal) — solução é uma tela separada de leitura filtrada. Spec completa em `.specs/features/arquivo-de-chamados/spec.md`.

- [ ] Listar só chamados finalizados, paginado, reaproveitando `GET /api/chamados`
- [ ] Filtro por período (`DataCriacao`)
- [ ] Filtro por prioridade (backend já suporta, falta UI)
- [ ] Filtro por status/categoria/busca (reaproveitar `FiltroChamados.tsx`)
- [ ] RBAC igual ao padrão de "Meus Chamados" (Admin=todos, Atendente=responsavelId, Solicitante=solicitanteEmail)
