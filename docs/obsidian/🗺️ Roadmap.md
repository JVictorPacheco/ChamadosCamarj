# 🗺️ Roadmap — Sistema de Chamados

## Fases do Desenvolvimento

### 🔧 Fase 0 — Setup (Atual)
- [x] SPEC finalizado
- [x] Decisões tomadas
- [x] Obsidian estruturado
- [ ] Setup da solução .NET 9
- [ ] Setup do repositório GitHub
- [ ] Conexão com Supabase

### 📦 Fase 1 — Domain Layer
- [ ] Entidades: Chamado, Comentario, Categoria, Anexo
- [ ] Enums: StatusChamado, Prioridade
- [ ] Interfaces: IChamadoRepository
- [ ] Migration inicial + EF Core

### ⚡ Fase 2 — CQRS + API
- [ ] Commands: Abrir, Atribuir, Finalizar, Comentar
- [ ] Queries: Listar, ObterPorId
- [ ] Validators: FluentValidation
- [ ] Controllers REST

### 🎨 Fase 3 — Frontend Básico
- [ ] Login com [[🔐 Azure AD]]
- [ ] Lista de chamados com filtros
- [ ] Detalhe do chamado
- [ ] Abertura de chamado (portal)

### 📧 Fase 4 — Integração Email
- [ ] EmailReceiverService (IMAP)
- [ ] Parsing de email → Chamado
- [ ] Resposta automática
- [ ] Anexos via [[📦 Supabase Storage]]

### 📊 Fase 5 — Kanban + Dashboard
- [ ] Kanban (status drag & drop)
- [ ] Dashboard com gráficos
- [ ] Notificações SignalR em tempo real
- [ ] Filtros avançados

### 🔐 Fase 6 — Azure AD + Admin
- [ ] Login corporativo completo
- [ ] Perfis e permissões
- [ ] Admin: categorias, usuários, configs

### 📈 Fase 7 — Relatórios + SLA
- [ ] Relatórios por período/categoria/atendente
- [ ] SLA tracking
- [ ] Exportação (CSV/PDF)

---

> **Progresso:** 🔧 Fase 0 — Setup inicial
