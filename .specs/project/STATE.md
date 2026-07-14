# STATE — Memória do Projeto

> Atualizado em: 2026-07-14

---

## 📍 Onde estamos

**Fase 5 concluída** — Kanban + Dashboard + SignalR + Fila de Atendimento + Ações de Atendente. Mergeada em `develop`/`main` (2026-06-30). Dashboard corrigido nesta sessão (ver abaixo).

**Fase 6 — T01-T08 e T10-T14 concluídos e verificados nesta sessão (2026-07-13/14)**, branch `feature/fase-6-admin-log` (checkout local, ainda não mergeada em `develop`). Faltam T09/T15 (login Google Workspace) — **pausados a pedido do usuário** pra adiantar o relatório mensal (Fase 7), que tem prazo real (fechamento mensal pra superintendência). Retomar T09/T15 depois da Fase 7.

**Fase 7 (Relatórios) antecipada em 2026-07-14** — motivo: usuário precisa apresentar um relatório mensal de andamento dos chamados pra superintendência todo fim de mês. Ainda em fase de Specify (não iniciado o Design/Tasks/Execute).

**Dashboard (Fase 5) corrigido em 2026-07-14:** não mostrava total de Cancelados nem Resolvidos (só "resolvidos hoje"), e "Abertos" não deixava claro quantos já tinham sido assumidos vs quantos aguardavam. Agora mostra "Abertos no momento" (Aberto+EmAndamento, com subtexto "X assumidos · Y em espera"), "Resolvidos" (com "Hoje: N"), "Cancelados" e "Tempo Médio". Backend reaproveitou `ContarPorStatusAsync` (já genérico) pra Resolvido/Cancelado — nenhum método novo no repositório.

O que foi corrigido/concluído nesta sessão:
- Frontend da Fase 6 (T10-T14) estava commitado no lugar errado (`src/ChamadosCamarj.Web/...`) e não-funcional (axios, toast, componentes shadcn não instalados, tema claro) — **reescrito do zero** em `frontend/src/features/chamados/`, seguindo os padrões reais do projeto. Arquivos órfãos apagados.
- Componentes shadcn que faltavam (`dialog`, `checkbox`, `radio-group`) instalados via CLI.
- `ComentarioForm`/`ComentarioList` estendidos pra suportar comentário interno (toggle + exibição condicional por perfil, já que `ComentarioList` antes só mostrava `Publico`).
- Bug de backend: endpoint `GET /comentarios` não repassava `perfilUsuario` pra query — filtro de interno nunca disparava. Corrigido.
- Bug de infra: migration `AddHistoricoEntrada` estava incompleta (sem `.Designer.cs`, sem snapshot atualizado) — travava o startup da API com "pending model changes". Regenerada via `dotnet ef migrations add` e sincronizada com o Postgres local (Docker) que já tinha a tabela criada manualmente numa sessão anterior.
- Bug de dados: histórico gravava `"Sistema"` fixo em todas as ações (Reatribuir, AlterarPrioridade, Resolver, Fechar, Cancelar nunca recebiam quem estava agindo). Adicionado `UsuarioId`/`UsuarioNome` nesses 5 commands/endpoints, e os hooks do frontend (`useAcoesChamado.ts`) agora enviam `perfil.id`/`perfil.nome` do `AuthContext` mockado automaticamente. **Importante:** esse campo é confiável só porque é mock — quando o login Google (T09) entrar, o backend precisa extrair o usuário via claims do JWT, nunca aceitar esse campo vindo do cliente em produção.
- Verificado ponta a ponta via Playwright ad-hoc: reatribuir, alterar prioridade, histórico (com usuário real, não mais "Sistema"), comentário interno visível só pra Admin/Atendente.
- Seção "Frontend" adicionada em `.specs/codebase/CONVENTIONS.md` (não existia — provável causa raiz do arquivo no lugar errado).

**Também descoberto em 2026-07-13:** os documentos de estado (`STATE.md`/`ROADMAP.md`/`HANDOFF.md`) na branch `develop` estavam desatualizados — diziam "próximo é Fase 4" quando na verdade Fase 5 e boa parte da Fase 6 já tinham sido feitas em branches não mergeadas.

**Próximo:** Specify do Relatório Mensal (Fase 7) — período fechado, totais, comparação com mês anterior, exportação. Depois: retomar T09/T15 (Google Workspace auth). Fase 4 (Email/Storage) segue sem data.

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

---

## 📋 TODOs (ordenados por prioridade)

1. Specify do Relatório Mensal (Fase 7): definir período, métricas (totais por status/categoria/atendente, comparação com mês anterior), formato de exportação (PDF/CSV), quem acessa (provavelmente só Admin)
2. Depois do relatório: retomar T09/T15 (Google Workspace auth)
3. Quando T09 entrar: trocar `UsuarioId`/`UsuarioNome` (hoje enviados pelo cliente, aceitáveis só por não haver auth real) por extração via claims do JWT no backend
4. Criar PR de `feature/fase-6-admin-log` → `develop` (T01-T14 completos e verificados)
5. "Forçar encerramento" (Admin fechar/cancelar fora do fluxo normal) — item da spec da Fase 6 ainda não abordado

## ✅ Concluído recentemente

- Dashboard corrigido: totais de Cancelados/Resolvidos adicionados, "Abertos" detalhado em assumidos/em espera — 2026-07-14
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
