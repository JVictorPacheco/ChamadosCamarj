# 💬 Decisões Tomadas

## Decisões Finais ✅

| Decisão | Resposta | Data |
|---------|----------|------|
| Nome do Sistema | **ChamadosCamarj** | — |
| Empresa | CAMARJ | — |
| Autenticação | [[🔐 Google Workspace]] *(corrigido — não é Azure AD)* | 2026-06-25 |
| Banco | PostgreSQL via Supabase (Session pooler, porta 5432) | — |
| Frontend | React 19 + TS + Vite + TailwindCSS v4 + Shadcn/ui | — |
| Metodologia | [[⚙️ SDD — Spec-Driven Development]] | — |
| Documentação | Obsidian | — |
| Email suporte | suporte@camarj.com.br / ti@camarj.com.br | — |
| Atendentes mock | Victor (Admin) + Fábio (Atendente) | — |
| Categorias | Autorização, Atendimento, Super e Tendência, Reembolso, Financeiro | — |
| SLA Baixo | 48h | — |
| SLA Médio | 16h | — |
| SLA Alto | 24h | — |
| SLA Urgente | 8h | — |
| Anexos | [[📦 Supabase Storage]] (bucket S3) | — |
| Dashboard | Métricas na home (Fase 5 ✅) + relatórios avançados (Fase 7) | — |
| Mobile | Futuro (web primeiro) | — |
| Notificações | SignalR real-time (Fase 5 ✅) + Push navegador/Desktop futuro | — |
| "Meus Chamados" | Admin=todos, Atendente=responsavelId, Solicitante=solicitanteEmail | 2026-07-01 |
| Log de histórico | Entidade `HistoricoEntrada` — auditoria de cada transição do chamado | 2026-07-01 |
| Reatribuição Admin | Endpoint `/reatribuir` separado, sem restrição de status (Admin move entre atendentes) | 2026-07-01 |
| Auth mockada | `localStorage` com seletor de perfil — substituído na Fase 6 pelo Google real | 2026-06-23 |

---

## ⚠️ Decisões Corrigidas

| Decisão original | Correção | Data |
|-----------------|----------|------|
| Azure AD (Microsoft) | **Google Workspace** — Camarj usa Gmail corporativo, não Microsoft | 2026-06-25 |
| Contas por analista | **Contas por setor** (ex: autorizacao@camarj.com.br) — perfil derivado de mapeamento conta→perfil | 2026-06-25 |
| Fase 5 como próximo passo | Fase 5 concluída, próximo é Fase 6 | 2026-07-01 |
