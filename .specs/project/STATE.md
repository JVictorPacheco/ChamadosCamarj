# STATE — Memória do Projeto

> Atualizado em: 2026-06-25

---

## 📍 Onde estamos

**Fase 2.5 concluída** — todos os concerns (C-01 a C-10) resolvidos, mais 3 bugs encontrados em teste manual e corrigidos (PR #6 mergeada em `develop`): categoria inexistente sem validação, transições de status sem guard, e `DbUpdateConcurrencyException` ao comentar. API roda em PostgreSQL real via Supabase.

**Fase 3 — Frontend (Portal do Solicitante) CONCLUÍDA (T1-T23).** Backend: API-01 (endpoint de comentários) + API-02 (filtro `solicitanteEmail`, gap descoberto durante o Execute) — branch `feature/fase3-bloco1-comentarios-api`, PR aberto. Frontend completo: scaffold, tema dark + sidebar (decisão de 2026-06-23 a partir de referência visual da Camarj), auth mockada, 3 páginas reais (abrir/listar/detalhe chamado), comentários públicos, 1 teste E2E (Playwright) — branch `feature/fase3-bloco2-frontend-foundation`, PR aberto. 59 testes unitários de backend passando.

**Verificado manualmente pelo usuário** (2026-06-24/25): fluxo completo funcionando no navegador, incluindo clique no card da lista → detalhe.

**Próximo:** Fase 4 — Integração Email + Storage (`EmailReceiverService`, parsing de e-mail → chamado, `StorageService`/Supabase, upload de anexos).

---

## ✅ Decisões tomadas

| Decisão | Detalhe |
|---------|---------|
| Banco dev e prod | PostgreSQL via Supabase — mesma instância para os dois ambientes |
| Conexão Supabase | **Session pooler** (`aws-1-us-east-2.pooler.supabase.com:5432`), não "Direct connection" (IPv6-only, falha em redes sem IPv6) nem "Transaction pooler" (incompatível com prepared statements do EF Core) |
| Senha do banco | `dotnet user-secrets` local (dev) — nunca em `appsettings.json` |
| Auth | **Google Workspace (Sign in with Google)** — corrigido em 2026-06-25, não é Azure AD/Microsoft como assumido antes. Mockada na Fase 3, real na Fase 6. Contas são **por setor**, não por analista individual (ex: autorizacao@camarj.com.br) — perfil (Admin/Atendente/Solicitante) é derivado da conta logada, precisa de mapeamento conta→perfil no backend |
| Anexos | Supabase Storage — implementar na Fase 4 |
| Email | MailKit IMAP — suporte@camarj.com.br / ti@camarj.com.br |
| Frontend | React + TS + Vite + TailwindCSS + Shadcn/ui |
| Seed | 5 categorias fixas com GUIDs determinísticos |
| Atendentes | Victor (Admin) + Fábio (Atendente) |
| SLA | Urgente 8h, Alta 24h, Média 16h, Baixa 48h |
| Notificações | Push navegador + Desktop (Electron/Tauri futuro) |
| Mobile | Web primeiro, mobile no futuro |
| Docs | Obsidian (docs/obsidian/) |
| Escopo Fase 3 | Só visão do Solicitante (abrir, listar, detalhe, comentar). Ações de Atendente ficam pra quando o Kanban (Fase 5) for feito |
| Auth mockada Fase 3 | Seletor de perfil (Admin/Atendente/Solicitante) salvo em localStorage — sem login Google real ainda. Troca futura isolada no contexto de autenticação |
| Localização do frontend | `/frontend` na raiz do repo, ao lado de `src/`, `tests/` e `docs/` |

---

## 🔴 Blockers ativos

Nenhum.

---

## 📌 Pendências (não bloqueantes)

| Pendência | Detalhe |
|-----------|---------|
| Hospedagem em produção | Onde a API vai rodar (VM, Docker, Azure App Service etc.) e como a connection string será injetada lá — decidir antes do deploy, não bloqueia o Frontend |

---

## 📋 TODOs (ordenados por prioridade)

1. Iniciar Fase 4 (Email + Storage) — Specify/Design antes do Execute

## ✅ Concluído recentemente

- PRs #9, #10, #11 mergeados em `develop` (2026-06-25) + merge direto do restante (API-02, T22, T23) — Fase 3 100% em `develop`
- `ProfileSelector` restilizada como tela de login real: logo da Camarj (`frontend/src/assets/logo-camarj.png`, fundo transparente, combina bem com o tema dark) + título "Portal de Chamados" + subtítulo — lógica mockada inalterada (branch `feature/fase3-login-branding`)

---

## 💡 Ideias adiadas (deferred)

- **Login real via Google (Fase 6, registrado em 2026-06-25):** "Sign in with Google" no lugar do seletor mockado. Camarj usa Google Workspace, não Azure AD (correção da decisão anterior). Contas são por setor (ex: autorizacao@camarj.com.br), não por analista — perfil (Admin/Atendente/Solicitante) precisa ser derivado de um mapeamento conta→perfil no backend (provavelmente uma tabela/config de usuários, ligando com a ideia de "Admin: gerenciar usuários" já prevista na Fase 6). Decisão do usuário: manter mock por agora, implementar isso quando chegar a Fase 6, na ordem original do roadmap
- **Reembolso workflow:** Possível integração com sistema financeiro no futuro
- **App mobile:** Web primeiro, avaliar PWA ou React Native depois
- **Electron/Tauri:** Para notificações desktop — após o web estar estável
- **Redis:** Cache planejado no SPEC mas sem prioridade definida
- **Serilog:** Logging estruturado — adicionar antes de ir para produção
- **Tipografia da referência visual (2026-06-23):** títulos em serifa + labels em mono caixa-alta, vistos em `Exemplo_Imagem_Camarj_Chamado.jpeg`. Cores e sidebar já adotados no design da Fase 3; tipografia ficou fora de escopo por ora

---

## 🎓 Aprendizados

- `EnsureCreated()` não aplica migrations — bom para dev rápido, perigoso para mudanças de schema
- `ObterTodosAsync()` + filtro em memória é um padrão a evitar desde o início
- `CategoriasController` foi uma exceção ao padrão CQRS — deve ser corrigido
- EF Core `Update()` num grafo carregado com `AsNoTracking()` marca entidades filhas com Guid client-gerado como `Modified` em vez de `Added` — gera `DbUpdateConcurrencyException` ao tentar UPDATE numa linha que não existe. Pra adicionar uma entidade filha nova, inserir direto via `DbSet.AddAsync()`, nunca reenviar o grafo do pai inteiro
- Nenhuma transição de status do `Chamado` tinha guard — sempre validar o `Status` atual antes de mudar de estado em entidades com ciclo de vida
- Sem middleware de tratamento de erro, toda exceção (incluindo `ValidationException` do FluentValidation) virava uma página 500 crua — middleware global de exceção é essencial mesmo em APIs pequenas
- Supabase: "Direct connection" é IPv6-only (falha em rede sem IPv6); "Transaction pooler" não suporta prepared statements do EF Core; usar "Session pooler"
- Gaps de filtro descobertos só no Execute (Fase 3): `ListarChamadosQuery` não tinha filtro por `solicitanteEmail`, apesar de existir um requisito explícito pra isso (FE-03) e até um método órfão (`ObterPorSolicitanteAsync`, não-paginado, nunca conectado a endpoint) sugerindo que a intenção sempre existiu — revisar queries de listagem contra os requisitos de UI antes de assumir que os filtros existentes bastam
- TanStack Query: o `retry` default (3x com backoff) se aplica a QUALQUER erro, incluindo 4xx — um 404 real demorava vários segundos pra aparecer na UI. Configurar `retry` customizado no `QueryClient` pra não tentar de novo em erros 4xx (só erros de rede/5xx valem retry)
- Branches criadas a partir de `develop` ANTES de um PR anterior ser mergeado não herdam commits desse PR — uma decisão de design/spec registrada só numa branch (ex: decisão do teste E2E, feita na branch de backend) precisa ser replicada manualmente se outra branch de trabalho (frontend) for criada a partir de `develop` nesse meio-tempo, senão a doc diverge silenciosamente
