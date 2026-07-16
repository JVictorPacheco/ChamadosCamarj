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
| [[📋 Histórico de Chamados]] | Log de auditoria do ciclo de vida *(Fase 6 ✅ implementado)* |
| [[📧 Integração Email]] | Captura automática via IMAP/Gmail *(Fase 4, não iniciada)* |
| [[🔐 Google Workspace]] | Autenticação corporativa *(Fase 6, login real pendente — T09/T15)* |
| [[📦 Supabase Storage]] | Anexos em bucket S3 *(Fase 4, não iniciada)* |
| [[🗺️ Roadmap]] | Fases do desenvolvimento |
| [[📈 Relatório Mensal]] | Relatório fechado do mês, exportação CSV/PDF *(Fase 7 ✅ implementado)* |
| [[💬 Decisões]] | Decisões tomadas com o Victor |
| [[📝 Perguntas Pendentes]] | O que ainda precisa responder |
| [[⚠️ Concerns]] | Débito técnico e riscos identificados |

---

## 👥 Equipe

- **Victor** — Admin / Desenvolvedor
- **Fábio** — Atendente

---

## 📍 Onde paramos (2026-07-15)

- ✅ **Fases 0–3 concluídas** — backend completo, frontend portal do solicitante funcionando
- ✅ **Fase 5 concluída** — Kanban, Dashboard, SignalR, Fila de Atendimento, Ações de Atendente (Assumir/Resolver/Fechar/Cancelar). Dashboard retrabalhado em 2026-07-14/15 (ver abaixo)
- 🔐 **Fase 6 quase completa** — Reatribuição Admin, Log de Histórico, Alterar Prioridade e Comentários Internos implementados e verificados (T01-T14). Falta só o login Google real (T09/T15), pausado a pedido do usuário para dar lugar à Fase 7
- ✅ **Fase 7 concluída (antecipada)** — Relatório Mensal: seletor de mês, KPIs com variação vs. mês anterior, rosca de SLA, quebra por categoria/atendente, exportação CSV/PDF
- ✅ **PR #13 mergeado em `develop`** (2026-07-15) — Fase 6 (T01-T14) + Fase 7 completas
- ⏭️ **Próximo:** retomar T09/T15 (login Google Workspace real)

### Features implementadas na Fase 5 (e retrabalho de 2026-07-14/15)
- Kanban com drag & drop (dnd-kit) entre status
- Dashboard com métricas: KPIs simplificados (Resolvidos Hoje + Tempo Médio) e rosca "Distribuição por situação" (Aguardando/Assumido/Resolvido/Encerrado/Cancelado) — substituiu o gráfico de Tendência
- Distinção entre **Resolvido** (marcado como solucionado) e **Encerrado** (confirmado e arquivado)
- Notificações SignalR em tempo real
- Fila de Atendimento (chamados Abertos por prioridade)
- Botões de ação no Detalhe: Assumir, Resolver, Fechar, Cancelar
- "Meus Chamados" diferenciado por perfil (Admin=todos, Atendente=responsável, Solicitante=seus)

### Features implementadas na Fase 6 (T01-T14)
- Reatribuição de chamado pelo Admin (`PATCH /chamados/{id}/reatribuir`)
- Alterar prioridade (`PATCH /chamados/{id}/prioridade`)
- Histórico/auditoria completo (`HistoricoEntrada`), timeline no detalhe do chamado
- Comentários internos filtrados por perfil

---

> *Última atualização: 2026-07-15 — PR #13 mergeado em `develop` (Fase 6 quase completa, falta login Google real; Fase 7 Relatório Mensal concluída). Ver [[🗺️ Roadmap]] para detalhes.*
