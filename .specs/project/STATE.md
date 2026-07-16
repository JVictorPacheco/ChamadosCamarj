# STATE — Memória do Projeto

> Atualizado em: 2026-07-15

---

## 📍 Onde estamos

**Fase 5 concluída** — Kanban + Dashboard + SignalR + Fila de Atendimento + Ações de Atendente. Mergeada em `develop`/`main` (2026-06-30). Dashboard bastante retrabalhado nesta sessão (ver abaixo).

**Fase 6 — T01-T08 e T10-T14 concluídos, verificados e MERGEADOS em `develop`** via PR #13 ("Feature/fase 6 admin log"), mergeado em 2026-07-15T03:05Z. Faltam T09/T15 (login Google Workspace) — **pausados a pedido do usuário** pra adiantar o relatório mensal (Fase 7). **Retomar T09/T15 é o próximo passo.**

**Fase 7 (Relatório Mensal) CONCLUÍDA, verificada e também já em `develop`** (mesmo PR #13). Specify → Design → Tasks → Execute completos (`.specs/features/relatorio-mensal/`). Endpoint `GET /api/relatorios/mensal`, página `/atendimento/relatorio-mensal` com seletor de mês, KPIs com variação % vs mês anterior, rosca de SLA, quebra por categoria e por atendente, exportação CSV e PDF (via impressão). RBAC: Admin vê tudo, Atendente só os próprios números, Solicitante bloqueado (bloqueio de verdade, não só link escondido — ver Aprendizados).

**IMPORTANTE pra retomar:** o checkout local ficou na branch `feature/fase-6-admin-log` (já mergeada, pode ser deletada/arquivada). Ao voltar, faça checkout em `develop` e dê `git pull` antes de continuar — é lá que está tudo agora.

**Dashboard (Fase 5) retrabalhado em 2026-07-14/15:**
- Cards de KPI simplificados: só "Resolvidos Hoje" e "Tempo Médio" (o resto virou redundante com a rosca)
- Gráfico de "Tendência" (linha, 7 dias) substituído por rosca "Distribuição por situação": Aguardando/Assumido/Resolvido/Encerrado/Cancelado — é a situação **atual** dos chamados, não uma janela de tempo (decisão do usuário, corrigida depois de uma primeira tentativa errada baseada em `HistoricoEntrada`/período — ver Aprendizados)
- Distingue **Resolvido** (`Chamado.Resolver()`, marcado como solucionado) de **Encerrado** (`Chamado.Fechar()`, confirmado e arquivado — só possível a partir de Resolvido) — antes só existia "Resolvido"
- Endpoint `GET /api/dashboard/tendencia` virou `GET /api/dashboard/distribuicao` (sem parâmetro `dias`, é uma foto do momento)

**Também descoberto em 2026-07-13:** os documentos de estado (`STATE.md`/`ROADMAP.md`/`HANDOFF.md`) na branch `develop` estavam desatualizados — diziam "próximo é Fase 4" quando na verdade Fase 5 e boa parte da Fase 6 já tinham sido feitas em branches não mergeadas.

**Próximo:** retomar T09/T15 (Google Workspace auth) — a partir da `develop`, não mais da branch de feature (já mergeada). Fase 4 (Email/Storage) segue sem data.

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
| Ordem Fase 6 vs Fase 7 | Fase 7 (Relatório Mensal) antecipada na frente de T09/T15 (Google auth) — decisão de 2026-07-14, motivada por prazo real de fechamento mensal pra superintendência |
| Relatório mensal | É um documento de período fechado (mês), não uma view "semanal" do dashboard operacional — dashboard fica com números em tempo real, relatório é outra tela/exportação (Fase 7) |

---

## 🔴 Blockers ativos

Nenhum.

---

## 📌 Pendências (não bloqueantes)

| Pendência | Detalhe |
|-----------|---------|
| Hospedagem em produção | Onde a API vai rodar (VM, Docker, Azure App Service etc.) e como a connection string será injetada |
| Fase 4 | Email + Storage ainda não implementados — aguardando priorização |
| **⚠️ Reconectar ao Supabase e revalidar Fase 6/Fase 7** | Durante a sessão de 2026-07-14/15, o `user-secrets` local ficou temporariamente apontando pro Postgres local (Docker, `chamados-postgres`) em vez do Supabase — toda a verificação de Reatribuir/Histórico/Alterar Prioridade/Comentário Interno/Relatório Mensal rodou contra o banco local. Confirmado com o usuário: era só temporário, Supabase continua sendo o banco real. **Antes de considerar essas features validadas em definitivo:** restaurar `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."` com a connection string do Supabase (ver README) e rodar a API — se aparecer erro de "pending model changes" no startup, é sinal de que a migration `AddHistoricoEntrada` precisa do mesmo tratamento que recebeu no banco local (ver Aprendizados: migration incompleta/tabela já existente sem registro em `__EFMigrationsHistory`) |

---

## 📋 TODOs (ordenados por prioridade)

1. Ao retomar: `git checkout develop && git pull` (a branch de feature já foi mergeada, não usar mais)
2. Retomar T09/T15 (Google Workspace auth)
3. Quando T09 entrar: trocar `UsuarioId`/`UsuarioNome` (hoje enviados pelo cliente, aceitáveis só por não haver auth real) por extração via claims do JWT no backend
4. "Forçar encerramento" (Admin fechar/cancelar fora do fluxo normal) — item da spec da Fase 6 ainda não abordado
5. Revisar se as outras telas com soft-RBAC (Dashboard/Kanban/Fila — só escondem o link, sem bloqueio de rota) precisam do mesmo tratamento que o Relatório Mensal recebeu, ou se ficam assim até o login real (T09) trazer autenticação de verdade

## ✅ Concluído recentemente

- **PR #13 mergeado em `develop`** (2026-07-15T03:05Z) — Fase 6 completa (T01-T14) + Fase 7 (Relatório Mensal) inteira, 42 commits
- Fase 7 (Relatório Mensal) completa: backend (endpoint + agregação via HistoricoEntrada + SLA + comparação mês anterior) e frontend (página + exportação CSV/PDF), RBAC com bloqueio real pro Solicitante — 2026-07-14/15
- Dashboard: rosca "Distribuição por situação" substitui gráfico de Tendência; KPIs simplificados; distinção Resolvido vs Encerrado — 2026-07-14/15
- Fase 6 T01-T14 completos e verificados via Playwright (reatribuir, alterar prioridade, histórico com usuário real, comentário interno) — 2026-07-14
- Backend completo da Fase 6 (T01-T08): `HistoricoEntrada`, `ReatribuirChamadoCommand`, `AlterarPrioridadeChamadoCommand`, endpoints correspondentes, filtro de comentário interno — branch `feature/fase-6-admin-log`, até 2026-07-12
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
- STATE.md/ROADMAP.md são só tão confiáveis quanto a branch em que foram lidos — uma branch de feature pode ficar muito à frente da `develop` sem que ninguém perceba, porque as docs só são atualizadas na branch onde o trabalho acontece. Ao retomar um projeto depois de um tempo, checar `git log`/branches remotas antes de confiar cegamente no STATE.md da branch atual
- Arquivos gerados numa sessão anterior podem acabar no caminho errado (ex: dentro de `src/<ProjName>.Web/` em vez de `frontend/`) e usando convenções de um stack genérico (axios, toast, shadcn não instalado) em vez das reais do projeto — sempre conferir se um componente "pronto" está de fato no diretório certo e compila contra as libs realmente instaladas antes de assumir que uma tarefa está concluída
- Uma migration EF Core só é válida com os 3 artefatos em sincronia: arquivo `.cs` (Up/Down), `.Designer.cs` e `ApplicationDbContextModelSnapshot.cs`. Se só o primeiro existe, `dotnet ef migrations list` nem reconhece a migration, e o app trava no startup com "pending model changes" — mesmo a tabela já existindo fisicamente no banco (criada manualmente numa sessão anterior). Sinal de alerta: `git log` de um arquivo de migration mostra só 1 commit em vez dos 3 arquivos de costume
- Sem autenticação real, comandos que alteram estado (Reatribuir, AlterarPrioridade, Resolver, Fechar, Cancelar) não têm de onde tirar "quem está fazendo isso" — se o command só recebe `Guid Id`, o handler não tem escolha a não ser hardcodar um valor fixo ("Sistema") no histórico/auditoria. Ao adicionar auditoria numa feature, checar se o command carrega identidade de ator, não só o dado da ação em si
- `ObterTendenciaAsync` (Dashboard, Fase 5) tinha um bug sutil de data: contava "resolvidos" usando a data de **criação** do chamado, não a de resolução (`DataConclusao`) — um chamado aberto num dia e resolvido em outro aparecia "resolvido" no dia errado. Corrigido junto com a Fase 7 por estar diretamente ligado ao requisito de dados verdadeiros do usuário
- Nem toda "métrica de gráfico" deve virar "métrica de período" só porque outra parte do sistema (o Relatório Mensal, nesse caso) trabalha com período — perguntar explicitamente se o gráfico é uma foto do momento (snapshot) ou uma janela de tempo (eventos) antes de desenhar a query. Uma primeira tentativa da rosca do Dashboard assumiu "eventos dos últimos 7 dias" via `HistoricoEntrada` quando o usuário só queria a situação atual (`ContarPorStatusAsync`, já existente) — retrabalho evitável se a pergunta tivesse sido feita antes de implementar
- `Resolvido` e `Encerrado/Fechado` são passos distintos do ciclo de vida do chamado (`Resolver()` marca como solucionado; `Fechar()` confirma e arquiva, só a partir de `Resolvido`) — não tratar como sinônimos em métricas/relatórios
- RBAC de UI neste projeto é "soft" por padrão (só esconde o link da sidebar, não bloqueia a rota) — aceitável pra telas com dado já visível em outro lugar (Dashboard, Kanban), mas uma tela nova que expõe dado mais sensível (ex: desempenho individual por atendente no Relatório Mensal) pode precisar de bloqueio de verdade (redirect/alerta), mesmo destoando do padrão das telas mais antigas — avaliar caso a caso, não copiar o padrão automaticamente
- EF Core: dá pra fazer `JOIN` direto em LINQ (`from x in a join y in b on x.FkId equals y.Id select ...`) contra outro `DbSet` do mesmo `DbContext` sem precisar de navigation property configurada na entidade — útil quando a entidade (ex: `HistoricoEntrada`) foi desenhada sem relacionamento de navegação pro que ela referencia
- Sempre conferir `dotnet user-secrets list` no início de uma sessão antes de rodar/testar a API — o `user-secrets` local pode estar apontando pro banco errado (ex: Postgres local via Docker em vez do Supabase real) por causa de troubleshooting de uma sessão anterior, e isso não aparece em lugar nenhum do código/git (user-secrets não é versionado). Toda "verificação com dados reais" feita nessas condições precisa ser refeita contra o banco real antes de dar como definitiva
