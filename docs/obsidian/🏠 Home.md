# 🏠 Sistema de Chamados CAMARJ

Bem-vindo ao mapa completo do projeto!

## 🎯 Visão Geral

Sistema interno para **gestão de chamados corporativos** da CAMARJ. Colaboradores abrem chamados via portal ou email — atendentes gerenciam via Kanban, fila e dashboard.

## 📚 Índice

| Nota | Descrição |
|------|-----------|
| [[📋 SPEC]] | Documento completo do Spec-Driven Development |
| [[🏗️ Arquitetura]] | Clean Architecture + Stack |
| [[📊 Modelo de Dados]] | Entidades, Enums, Relacionamentos |
| [[👥 Perfis de Usuário]] | Admin, Atendente, Solicitante — permissões e fluxos |
| [[📋 Histórico de Chamados]] | Log de auditoria do ciclo de vida *(Fase 6)* |
| [[📧 Integração Email]] | Captura automática via IMAP/Gmail *(Fase 4)* |
| [[🔐 Google Workspace]] | Autenticação corporativa *(Fase 6)* |
| [[📦 Supabase Storage]] | Anexos em bucket S3 *(Fase 4)* |
| [[🗺️ Roadmap]] | Fases do desenvolvimento |
| [[💬 Decisões]] | Decisões tomadas com o Victor |
| [[📝 Perguntas Pendentes]] | O que ainda precisa responder |
| [[⚠️ Concerns]] | Débito técnico e riscos identificados |

---

## 👥 Equipe

- **Victor** — Admin / Desenvolvedor
- **Fábio** — Atendente

---

## 📍 Onde paramos (2026-07-01)

- ✅ **Fases 0–3 concluídas** — backend completo, frontend portal do solicitante funcionando
- ✅ **Fase 5 concluída** — Kanban, Dashboard, SignalR, Fila de Atendimento, Ações de Atendente (Assumir/Resolver/Fechar/Cancelar)
- ⏭️ **Próximo:** Fase 6 — Reatribuição Admin, Log de Histórico, Comentários Internos, Google Workspace Auth

### Features implementadas na Fase 5
- Kanban com drag & drop (dnd-kit) entre status
- Dashboard com métricas (totais por status, alertas SLA, recentes)
- Notificações SignalR em tempo real
- Fila de Atendimento (chamados Abertos por prioridade)
- Botões de ação no Detalhe: Assumir, Resolver, Fechar, Cancelar
- "Meus Chamados" diferenciado por perfil (Admin=todos, Atendente=responsável, Solicitante=seus)

---

> *Última atualização: 2026-07-01 — Fase 5 concluída, Fase 6 planejada (Admin completo + Log + Google Workspace).*
