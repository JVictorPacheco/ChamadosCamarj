# STATE — Memória do Projeto

> Atualizado em: 2026-06-24

---

## 📍 Onde estamos

**Fase 2.5 concluída** — todos os concerns (C-01 a C-10) resolvidos, mais 3 bugs encontrados em teste manual e corrigidos (PR #6 mergeada em `develop`): categoria inexistente sem validação, transições de status sem guard, e `DbUpdateConcurrencyException` ao comentar. 57 testes unitários passando (55 + 2 novos da T2 abaixo). API roda em PostgreSQL real via Supabase.

**Fase 3 — Frontend (Portal do Solicitante) em Execute, T1-T12 concluídas.** Backend API-01 pronto (T1-T3, branch `feature/fase3-bloco1-comentarios-api`, PR pendente de abertura manual). Frontend: scaffold, tema dark + sidebar (decisão de 2026-06-23 a partir de referência visual da Camarj), tipos/cliente HTTP/auth mockada, `ProfileSelector`, `AppLayout` (sidebar) e rotas reais já navegáveis ponta a ponta (`/login` → `/chamados` → placeholders das páginas reais). Tudo na branch `feature/fase3-bloco2-frontend-foundation`, PR pendente de abertura manual (`gh` CLI indisponível no ambiente). Próximo: T13/T14 (camada de dados).

---

## ✅ Decisões tomadas

| Decisão | Detalhe |
|---------|---------|
| Banco dev e prod | PostgreSQL via Supabase — mesma instância para os dois ambientes |
| Conexão Supabase | **Session pooler** (`aws-1-us-east-2.pooler.supabase.com:5432`), não "Direct connection" (IPv6-only, falha em redes sem IPv6) nem "Transaction pooler" (incompatível com prepared statements do EF Core) |
| Senha do banco | `dotnet user-secrets` local (dev) — nunca em `appsettings.json` |
| Auth | Azure AD (Microsoft Entra ID) — implementar na Fase 3/6 |
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
| Auth mockada Fase 3 | Seletor de perfil (Admin/Atendente/Solicitante) salvo em localStorage — sem Azure AD real ainda. Troca futura isolada no contexto de autenticação |
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

1. Continuar Execute da Fase 3: T13/T14 (camada de dados — `features/chamados/api.ts` + hooks TanStack Query) em diante
2. Restilizar `ProfileSelector` como tela de "login" de fato (logo Camarj + título + layout), mantendo a lógica mockada — usuário vai mandar o arquivo do logo quando puder; usar placeholder/wordmark até lá
3. Abrir os 2 PRs pendentes pra `develop` (`feature/fase3-bloco1-comentarios-api`, `feature/fase3-bloco2-frontend-foundation`) — `gh` CLI não disponível neste ambiente

---

## 💡 Ideias adiadas (deferred)

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
