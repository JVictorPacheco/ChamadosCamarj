# Roadmap — ChamadosCamarj

> Última atualização: 2026-07-01

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

## 📧 Fase 4 — Integração Email + Storage

> Ainda não iniciada. Pode ser feita em paralelo com Fase 6.

- [ ] `EmailReceiverService` (IMAP/MailKit — suporte@camarj.com.br / ti@camarj.com.br)
- [ ] Parsing de e-mail → Chamado automático
- [ ] Resposta automática ao solicitante
- [ ] `StorageService` (Supabase Storage S3)
- [ ] Upload/download de anexos no portal

## 🔐 Fase 6 — Admin Completo + Log + Google Workspace

> Corrigido em 2026-06-25: Camarj usa Google Workspace (Gmail corporativo), não Azure AD.
> Planejado em 2026-07-01: features de Admin e auditoria.
> Spec em `.specs/features/fase-6-admin-log/spec.md`.

- [ ] **Reatribuição Admin** — mover chamado entre atendentes (qualquer status não-final)
- [ ] **Log de histórico** — entidade `HistoricoEntrada` auditando cada transição de status, atribuição, comentário
- [ ] **Comentários internos** — visíveis só para Admin/Atendente (campo `Tipo` já existe no `Comentario`)
- [ ] **Alterar prioridade** — Admin pode alterar prioridade de qualquer chamado
- [ ] **Forçar encerramento** — Admin pode fechar/cancelar sem seguir o fluxo normal
- [ ] **Login real via Google Workspace** — substitui o seletor mockado
- [ ] Mapeamento conta→perfil no backend (tabela de usuários por setor)
- [ ] RBAC real (baseado em claims do token Google)
- [ ] Admin: gerenciar categorias, usuários e configurações do sistema

## 📈 Fase 7 — Relatórios + SLA

- [ ] Relatórios por período/categoria/atendente
- [ ] SLA tracking com alertas de vencimento
- [ ] Exportação CSV/PDF
- [ ] Dashboard de carga por atendente
