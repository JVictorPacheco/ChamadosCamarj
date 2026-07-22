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
| [[📧 Integração Email]] | Captura automática via IMAP/Gmail *(Fase 4 metade 2, não iniciada — falta senha de app do IMAP)* |
| [[🔐 Google Workspace]] | Autenticação corporativa *(T09/T15 ✅ implementado — falta só o Client ID da TI)* |
| [[📦 Supabase Storage]] | Anexos em bucket S3 *(Fase 4 metade 1 ✅ implementado e verificado de ponta a ponta)* |
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

## 📍 Onde paramos (2026-07-21)

- ✅ **Fases 0–3, 5 e 7 concluídas** — backend completo, frontend funcionando, Kanban/Dashboard/SignalR/Fila, Relatório Mensal
- ✅ **Fase 6 praticamente completa** — Reatribuição Admin, Log de Histórico, Alterar Prioridade, Comentários Internos, F5a e T09/T15 (login Google real) implementados. Falta só o **Client ID real da TI** pro login funcionar de ponta a ponta
- ✅ **Arquivo de Chamados concluído** (2026-07-18) — tela separada pra chamados finalizados
- ✅ **Forçar Encerramento concluído** (2026-07-19) — Admin fecha chamado direto de qualquer status não-final, motivo obrigatório auditado
- ✅ **Número do Chamado concluído** (2026-07-19/20) — `CAM-{número}` sequencial, backfill dos existentes, busca por número no campo já existente
- ✅ **RBAC real do Dashboard/Kanban/Fila concluído** (2026-07-20) — bloqueio de verdade pro Solicitante, mesmo padrão do Relatório Mensal
- ✅ **Storage de Anexos concluído** (2026-07-21) — Fase 4 metade 1, upload/listagem/download via Supabase Storage, verificado de ponta a ponta contra o Supabase real
- ✅ **Débito técnico da revisão sênior resolvido** (2026-07-17) — 15 itens de `CONCERNS.md` corrigidos
- ⏭️ **Próximo:** aguardar Client ID da TI e senha de app do IMAP (Fase 4 metade 2 — Email); sem ordem confirmada além disso. Decisão de hospedagem em produção também pendente

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

> *Última atualização: 2026-07-21 — Storage de Anexos concluído e verificado de ponta a ponta (Fase 4 metade 1); Forçar Encerramento, Número do Chamado e RBAC real do Dashboard/Kanban/Fila também concluídos desde a última atualização. Falta o Client ID da TI (login Google) e a senha de app do IMAP (Fase 4 metade 2 — Email). Ver [[🗺️ Roadmap]] para detalhes.*
