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

### 🔧 Fase 2.5 — Correções (PENDENTE — antes do Frontend)
> Concerns identificados no mapeamento do código. Ver [[⚠️ Concerns]].
- [ ] Filtros de ListarChamados no banco (não em memória)
- [ ] CategoriasController usar MediatR
- [ ] DatabaseSeeder substituir seed inline de Program.cs
- [ ] Validators para Atribuir e Comentar
- [ ] Commands + Endpoints para Fechar e Cancelar
- [ ] Corrigir migration (ComentarioId em Anexos)
- [ ] Criar projeto de testes unitários (Domain)

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

> **Progresso atual:** ✅ Fase 2 concluída → 🔧 Fase 2.5 (correções antes do Frontend)
