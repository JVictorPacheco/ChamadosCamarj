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
| Dashboard | Métricas em tempo real na home (Fase 5 ✅, retrabalhado 07-14/15) + Relatório Mensal separado (Fase 7 ✅) | — |
| Mobile | Futuro (web primeiro) | — |
| Notificações | SignalR real-time (Fase 5 ✅) + Push navegador/Desktop futuro | — |
| "Meus Chamados" | Admin=todos, Atendente=responsavelId, Solicitante=solicitanteEmail | 2026-07-01 |
| Log de histórico | Entidade `HistoricoEntrada` — auditoria de cada transição do chamado — ✅ implementado (Fase 6) | 2026-07-01 |
| Reatribuição Admin | Endpoint `/reatribuir` separado, sem restrição de status (Admin move entre atendentes) — ✅ implementado (Fase 6) | 2026-07-01 |
| Auth mockada | `localStorage` com seletor de perfil — substituída pelo F5a (login por e-mail + cadastro) e depois pelo Google real (T09/T15) | 2026-06-23 |
| Ordem Fase 6 vs Fase 7 | Fase 7 (Relatório Mensal) antecipada na frente de T09/T15 — motivada por prazo real de fechamento mensal pra superintendência | 2026-07-14 |
| Relatório Mensal | Documento de período fechado (mês), não uma view "semanal" do dashboard — dashboard fica com números em tempo real, relatório é outra tela/exportação | 2026-07-14 |
| Dashboard — gráfico "Distribuição" | Rosca de situação **atual** dos chamados (Aguardando/Assumido/Resolvido/Encerrado/Cancelado), não uma janela de tempo — substituiu o gráfico de Tendência (linha, 7 dias) | 2026-07-14/15 |
| Resolvido vs. Encerrado | Passos distintos do ciclo de vida: `Resolver()` marca como solucionado, `Fechar()` confirma e arquiva (só a partir de Resolvido) — não são sinônimos em métricas/relatórios | 2026-07-14/15 |
| RBAC do Relatório Mensal | Bloqueio real (redirect) para Solicitante, diferente do RBAC "soft" (só esconde link) do resto do app — por expor dado mais sensível (desempenho por atendente) | 2026-07-14/15 |
| F5a como passo intermediário | Login mockado por e-mail + cadastro de usuários pelo Admin (`UsuarioPerfil`) implementado antes do Google real — não descartável, o T09 reaproveita a mesma tabela | 2026-07-16 |
| Nunca apagar chamados | Chamados finalizados (Resolvido/Fechado/Cancelado) ganham tela separada de leitura ([[🗺️ Roadmap|Arquivo de Chamados]]), nunca exclusão — quebraria `HistoricoEntrada`/Relatório Mensal | 2026-07-16 |
| Assinatura do JWT (T09) | Simétrica (`SymmetricSecurityKey`) em vez de assimétrica — mais simples pra um único backend emitindo e validando | 2026-07-18 |
| Expiração do token (T09) | 8-12h, sem refresh token | 2026-07-18 |
| Logout por inatividade (T09) | 20 minutos sem interação do usuário — ideia do próprio usuário ao ser questionado sobre o que fazer com sessões esquecidas abertas | 2026-07-18 |
| Login email+senha | TI informou que o Google OAuth está fora do plano CAMARJ. Login ativo substituído por **email+senha** via ASP.NET Core Identity (`PasswordHasher`). Senhas definidas pelo Admin na tela `Admin > Usuários`. Código do Google OAuth mantido dormante — spec em `.specs/features/auth-email-senha/spec.md` | 2026-07-24 |

---

## ⚠️ Decisões Corrigidas

| Decisão original | Correção | Data |
|-----------------|----------|------|
| Azure AD (Microsoft) | **Google Workspace** — Camarj usa Gmail corporativo, não Microsoft | 2026-06-25 |
| Contas por analista | **Contas por setor** (ex: autorizacao@camarj.com.br) — depois substituído pelo cadastro do Admin (F5a, tabela `UsuarioPerfil`) | 2026-06-25 |
| Fase 5 como próximo passo | Fase 5 concluída, próximo é Fase 6 | 2026-07-01 |
| Fase 6 como próximo passo | Fase 6 quase completa (T01-T14); Fase 7 (Relatório Mensal) antecipada na frente de T09/T15 | 2026-07-14 |
| Rosca do Dashboard como "eventos dos últimos 7 dias" | Corrigido para "foto do momento" (situação atual via `ContarPorStatusAsync`) — primeira tentativa usou `HistoricoEntrada`/período por engano | 2026-07-14/15 |
| T09/T15 como pendente | **Implementado por completo em 2026-07-18** — falta só o Client ID real da TI | 2026-07-18 |
| Google OAuth como login ativo | **Substituído por email+senha (2026-07-24)** — TI informou que o Client ID está fora do plano CAMARJ. Código Google OAuth mantido dormante | 2026-07-24 |
