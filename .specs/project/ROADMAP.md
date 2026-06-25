# Roadmap — ChamadosCamarj

> Última atualização: 2026-06-25 (Fase 3 concluída)

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

- [x] Commands: Abrir, Atribuir, Atualizar, Comentar, Resolver
- [x] Queries: Listar (com filtros e paginação), ObterPorId
- [x] Validators: AbrirChamadoCommand, AtualizarChamadoCommand
- [x] Controllers: ChamadosController (6 endpoints), CategoriasController
- [x] Pipeline Behavior (FluentValidation)
- [x] Mapeamento manual `Chamado → ChamadoResponse`
- [x] OpenAPI + Scalar UI
- [x] Seed das 5 categorias da CAMARJ

## ✅ Fase 2.5 — Correções antes de avançar (COMPLETA)

> Identificados no mapeamento — todos resolvidos em 2026-06-19

- [x] C-01: Resolver conflito SQLite dev vs PostgreSQL migration — migrado para Supabase (session pooler) em dev e prod
- [x] C-02: Mover filtros de ListarChamados para query no banco
- [x] C-03: CategoriasController usar MediatR
- [x] C-04: Remover seed inline de Program.cs, usar DatabaseSeeder
- [x] C-05: Criar validators para Atribuir e Comentar
- [x] C-06: Criar Commands + Endpoints para Fechar e Cancelar
- [x] C-07: Corrigir migration — adicionar ComentarioId em Anexos
- [x] C-09: Criar projeto de testes (Domain unit tests) — 59 testes passando (após API-01/API-02 da Fase 3)

## ✅ Fase 3 — Frontend: Portal do Solicitante (COMPLETA)

> Spec completo em `.specs/features/frontend-portal-solicitante/spec.md`. Escopo restrito ao Solicitante — ações de Atendente (fila, assumir, resolver) ficam pra Fase 5 (Kanban). Auth mockada (seletor de perfil), Azure AD real fica pra Fase 6.

- [x] API-01: endpoint `GET /api/chamados/{id}/comentarios` (pré-requisito de backend)
- [x] API-02: filtro `solicitanteEmail` em `GET /api/chamados` (gap descoberto durante o Execute, T20)
- [x] Setup React + Vite + TailwindCSS + Shadcn/ui (`/frontend` na raiz), tema dark customizado (paleta + sidebar a partir de referência visual da Camarj)
- [x] Seletor de perfil mockado (Admin/Atendente/Solicitante) — todos os 3 veem a mesma visão de Solicitante por ora
- [x] Abertura de chamado (portal) — `AbrirChamadoPage`
- [x] Lista de chamados com filtros — `ChamadosListPage`, filtrada por `solicitanteEmail`
- [x] Detalhe do chamado (com comentários) — `ChamadoDetailPage`, 404 + trava de RBAC-lite de UI
- [x] Comentários públicos — `ComentarioList`/`ComentarioForm`
- [x] 1 teste E2E (Playwright) cobrindo o fluxo feliz completo

**Ainda não tem (fora de escopo desta fase):** ações de Atendente (resolver/fechar/cancelar na UI — backend já suporta), upload de anexos, diferenciação real de permissão por perfil, login corporativo real.

## 📧 Fase 4 — Integração Email + Storage

- [ ] `EmailReceiverService` (IMAP/MailKit)
- [ ] Parsing de e-mail → Chamado automático
- [ ] Resposta automática ao solicitante
- [ ] `StorageService` (Supabase S3)
- [ ] Upload/download de anexos

## 📊 Fase 5 — Kanban + Dashboard

- [ ] Kanban (arrastar cards entre status)
- [ ] Dashboard com gráficos (chamados por categoria/status)
- [ ] Notificações SignalR em tempo real
- [ ] Filtros avançados + busca full-text

## 🔐 Fase 6 — Login Google Workspace + Admin completo

> Corrigido em 2026-06-25: Camarj usa Google Workspace (Gmail corporativo), não Azure AD/Microsoft como assumido antes. Contas são por setor (ex: autorizacao@camarj.com.br), não por analista individual.

- [ ] Login real via "Sign in with Google" (substitui o seletor mockado)
- [ ] Mapeamento conta→perfil no backend (tabela/config de usuários por setor)
- [ ] RBAC: Admin, Atendente, Solicitante
- [ ] Admin: gerenciar categorias, usuários, configs

## 📈 Fase 7 — Relatórios + SLA

- [ ] Relatórios por período/categoria/atendente
- [ ] SLA tracking com alertas de vencimento
- [ ] Exportação CSV/PDF
