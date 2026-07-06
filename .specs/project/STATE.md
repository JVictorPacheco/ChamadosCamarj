# STATE — Memória do Projeto

> Atualizado em: 2026-07-01

---

## 📍 Onde estamos

**Fase 5 concluída** — Kanban + Dashboard + SignalR + Fila de Atendimento + Ações de Atendente (Assumir/Resolver/Fechar/Cancelar na UI). Branch `feature/fase-5-kanban-dashboard` mergeada em `develop` e `main` (2026-06-30). Bugs corrigidos pós-merge: botão Assumir na Fila (Link aninhado causava navegação conflitante), "Meus Chamados" diferenciado por perfil.

**Verificado manualmente pelo usuário** (2026-07-01): fluxo completo funcionando — Assumir na Fila, Resolver/Fechar/Cancelar no Detalhe, Kanban drag & drop, Dashboard com métricas.

**Próximo:** Fase 6 — Admin completo (Reatribuição entre atendentes, Log de histórico/auditoria, Comentários internos) + Google Workspace auth.

---

## ✅ Decisões tomadas

| Decisão | Detalhe |
|---------|---------|
| Banco dev e prod | PostgreSQL via Supabase — mesma instância para os dois ambientes |
| Conexão Supabase | **Session pooler** (`aws-1-us-east-2.pooler.supabase.com:5432`), não "Direct connection" (IPv6-only) nem "Transaction pooler" (incompatível com prepared statements do EF Core) |
| Senha do banco | `dotnet user-secrets` local (dev) — nunca em `appsettings.json` |
| Auth | **Google Workspace (Sign in with Google)** — corrigido em 2026-06-25, não é Azure AD/Microsoft. Mockada na Fase 3-5, real na Fase 6. Contas são **por setor** (ex: autorizacao@camarj.com.br) — perfil (Admin/Atendente/Solicitante) derivado de mapeamento conta→perfil no backend |
| Anexos | Supabase Storage — implementar na Fase 4 |
| Email | MailKit IMAP — suporte@camarj.com.br / ti@camarj.com.br |
| Frontend | React 19 + TS + Vite + TailwindCSS v4 + Shadcn/ui |
| Seed | 5 categorias fixas com GUIDs determinísticos |
| Atendentes | Victor (Admin) + Fábio (Atendente) |
| SLA | Urgente 8h, Alta 24h, Média 16h, Baixa 48h |
| Notificações | SignalR (real-time, Fase 5 ✅) + Push navegador + Desktop (Electron/Tauri futuro) |
| Mobile | Web primeiro, mobile no futuro |
| Docs | Obsidian (docs/obsidian/) |
| Auth mockada | Seletor de perfil (Admin/Atendente/Solicitante) salvo em localStorage — sem Google real ainda. Campo `id` no Perfil mock para identificação do responsável |
| Localização do frontend | `/frontend` na raiz do repo, ao lado de `src/`, `tests/` e `docs/` |
| Filtragem "Meus Chamados" | Admin=todos os chamados, Atendente=chamados onde é responsável (`responsavelId`), Solicitante=chamados que abriu (`solicitanteEmail`) — decidido em 2026-07-01 |
| Log de histórico | Entidade `HistoricoEntrada` para auditoria completa do fluxo de cada chamado — planejado para Fase 6 |
| Reatribuição Admin | Admin pode mover chamado entre atendentes mesmo em `EmAndamento` via endpoint `/reatribuir` separado do `/atribuir` — planejado para Fase 6 |

---

## 🔴 Blockers ativos

Nenhum.

---

## 📌 Pendências (não bloqueantes)

| Pendência | Detalhe |
|-----------|---------|
| Hospedagem em produção | Onde a API vai rodar (VM, Docker, Azure App Service etc.) e como a connection string será injetada |
| Fase 4 | Email + Storage ainda não implementados — aguardando priorização |

---

## 📋 TODOs (ordenados por prioridade)

1. Implementar Reatribuição Admin — backend (`Chamado.Reatribuir()` + `ReatribuirChamadoCommand` + endpoint) + frontend (botão + select de atendente no detalhe)
2. Implementar Log de Histórico — entidade `HistoricoEntrada`, registrar em cada transição de status/atribuição
3. Comentários internos — visíveis só para Admin/Atendente (campo `Tipo` já existe no `Comentario`)
4. Alterar prioridade — Admin pode alterar prioridade de qualquer chamado
5. Login Google Workspace real (Fase 6) — substituir seletor mockado

## ✅ Concluído recentemente

- Fase 5 (`feature/fase-5-kanban-dashboard`) mergeada em `develop` e `main` (2026-06-30) — Kanban, Dashboard, SignalR, Fila de Atendimento
- Ações de Atendente implementadas na UI: botões Assumir/Resolver/Fechar/Cancelar no detalhe do chamado + Assumir na Fila de Atendimento
- Bug fix: botão Assumir na Fila (Link aninhado causava navegação conflitante — eliminado, card e botões agora são elementos independentes)
- "Meus Chamados" diferenciado por perfil (Admin vê todos, Atendente vê os que é responsável, Solicitante vê os que abriu)
- AuthContext: campo `id` adicionado ao Perfil mock para identificação do responsável nas requisições de atribuição
- PRs #9, #10, #11 mergeados em `develop` (2026-06-25) + merge direto do restante — Fase 3 100% em `develop`

---

## 💡 Ideias adiadas (deferred)

- **Login real via Google (Fase 6, registrado em 2026-06-25):** "Sign in with Google" no lugar do seletor mockado. Camarj usa Google Workspace, não Azure AD. Contas são por setor — perfil precisa ser derivado de mapeamento conta→perfil no backend
- **Dashboard de carga por atendente:** Ver quantos chamados `EmAndamento` cada atendente tem — útil para balancear a reatribuição
- **Atribuição automática:** Round-robin ao assumir, ou sugestão baseada em carga atual
- **Alertas de SLA:** Notificar quando SLA está próximo de vencer
- **Reembolso workflow:** Possível integração com sistema financeiro no futuro
- **App mobile:** Web primeiro, avaliar PWA ou React Native depois
- **Electron/Tauri:** Para notificações desktop — após o web estar estável
- **Redis:** Cache planejado no SPEC mas sem prioridade definida
- **Serilog:** Logging estruturado — adicionar antes de ir para produção
- **Tipografia da referência visual (2026-06-23):** títulos em serifa + labels em mono caixa-alta, vistos em `Exemplo_Imagem_Camarj_Chamado.jpeg`

---

## 🎓 Aprendizados

- `EnsureCreated()` não aplica migrations — bom para dev rápido, perigoso para mudanças de schema
- `ObterTodosAsync()` + filtro em memória é um padrão a evitar desde o início
- `CategoriasController` foi uma exceção ao padrão CQRS — deve ser corrigido
- EF Core `Update()` num grafo carregado com `AsNoTracking()` marca entidades filhas com Guid client-gerado como `Modified` em vez de `Added` — gera `DbUpdateConcurrencyException` ao tentar UPDATE numa linha que não existe. Pra adicionar uma entidade filha nova, inserir direto via `DbSet.AddAsync()`, nunca reenviar o grafo do pai inteiro
- Nenhuma transição de status do `Chamado` tinha guard — sempre validar o `Status` atual antes de mudar de estado em entidades com ciclo de vida
- Sem middleware de tratamento de erro, toda exceção (incluindo `ValidationException` do FluentValidation) virava uma página 500 crua — middleware global de exceção é essencial mesmo em APIs pequenas
- Supabase: "Direct connection" é IPv6-only (falha em rede sem IPv6); "Transaction pooler" não suporta prepared statements do EF Core; usar "Session pooler"
- Gaps de filtro descobertos só no Execute (Fase 3): `ListarChamadosQuery` não tinha filtro por `solicitanteEmail`, apesar de existir um requisito explícito — revisar queries de listagem contra os requisitos de UI antes de assumir que os filtros existentes bastam
- TanStack Query: o `retry` default (3x com backoff) se aplica a QUALQUER erro, incluindo 4xx — um 404 real demorava vários segundos pra aparecer na UI. Configurar `retry` customizado no `QueryClient`
- Branches criadas a partir de `develop` ANTES de um PR anterior ser mergeado não herdam commits desse PR — decisões de design/spec registradas só numa branch precisam ser replicadas manualmente
- Link aninhado (`<a>` dentro de `<a>`) é HTML inválido e causa comportamento imprevisível nos eventos de clique — nunca envolver botões de ação em um `<Link>` pai; usar `useNavigate()` programaticamente no elemento clicável do card
- `localStorage` persiste entre sessões — em auth mockada, o perfil anterior fica salvo; sempre confirmar qual perfil está ativo no footer da sidebar antes de testar
