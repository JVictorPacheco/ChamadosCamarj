# Roadmap — ChamadosCamarj

> Última atualização: 2026-06-18 (mapeamento real do código)

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
- [x] C-09: Criar projeto de testes (Domain unit tests) — 48 testes passando

## 🎨 Fase 3 — Frontend Básico

- [ ] Setup React + Vite + TailwindCSS + Shadcn/ui
- [ ] Login com Azure AD
- [ ] Lista de chamados com filtros
- [ ] Detalhe do chamado
- [ ] Abertura de chamado (portal)
- [ ] Comentários públicos

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

## 🔐 Fase 6 — Azure AD + Admin completo

- [ ] Login corporativo completo com perfis
- [ ] RBAC: Admin, Atendente, Solicitante
- [ ] Admin: gerenciar categorias, usuários, configs

## 📈 Fase 7 — Relatórios + SLA

- [ ] Relatórios por período/categoria/atendente
- [ ] SLA tracking com alertas de vencimento
- [ ] Exportação CSV/PDF
