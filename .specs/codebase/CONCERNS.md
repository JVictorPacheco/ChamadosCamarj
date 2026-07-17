# Concerns — Débito Técnico e Riscos

> Itens identificados no mapeamento de 2026-06-18. Todos os itens abaixo foram resolvidos em 2026-06-19.

---

## 🟡 EM ABERTO (revisão sênior de 2026-07-16, pré-commit da F5a)

> Revisão completa de backend + frontend feita antes de commitar a F5a (login mockado + admin de usuários). Os 4 itens de severidade Alta foram corrigidos na hora (ver `STATE.md` → Concluído recentemente). Os itens abaixo são Médio/Baixo — não bloquearam o commit, mas **o usuário pediu explicitamente para não esquecer de tratá-los depois.**

### Médio

- **D-01 — Mover chamado no Kanban não gera `HistoricoEntrada`.** `AlterarStatusChamadoCommand`/Handler não carregam `UsuarioId`/`UsuarioNome` (diferente de Reatribuir/AlterarPrioridade/Resolver/Fechar/Cancelar, que geram histórico). Furo de auditoria: mudar status via drag & drop no Kanban não deixa rastro de quem fez. Corrigir adicionando identidade do ator ao command, igual aos outros.
- **D-02 — Sem guarda contra auto-lockout de Admin.** `AtualizarUsuarioPerfilCommandHandler` permite um Admin desativar a própria conta ou desativar/rebaixar o último Admin ativo do sistema, travando a administração sem rota de recuperação fora do banco direto. Adicionar checagem: bloquear se for o último Admin ativo.
- **D-03 — `ListarChamadosQuery` sem Validator, paginação sem limites.** `pagina`/`tamanhoPagina` não são validados — página negativa gera erro 500 não tratado no Postgres (`OFFSET` negativo), e não há teto de `tamanhoPagina` (alguém pode pedir milhares de registros de uma vez). Criar `ListarChamadosQueryValidator` (`Pagina > 0`, `TamanhoPagina` entre 1 e 100).
- **D-04 — Mensagem de erro enganosa em `UsuariosPage` para não-Admin.** Antes do bloqueio real (implementado em 2026-07-16), um 403 do backend aparecia como "Serviço indisponível" genérico. Com o bloqueio de conteúdo real já em vigor isso fica menos crítico (não-Admin nem chega a disparar a query), mas vale conferir se `isError` ainda pode mascarar um 403 de sessão expirada/perfil trocado no meio do uso.
- **D-05 — Hook `useSignalR` recria a conexão a cada subscribe/unsubscribe.** `notify` depende de `[subscribers]` (um `Set` recriado a cada render), e o efeito que abre a `HubConnection` depende de `[notify]` — toda navegação entre telas que assinam SignalR (Kanban/Dashboard/Fila) para e reconecta a conexão. Trocar o `Set` de subscribers por `useRef` pra dar identidade estável a `notify` e a conexão abrir só uma vez.
- **D-06 — Cor clara hardcoded no Kanban quebra o tema escuro.** `KanbanColumn.tsx`: `isOver ? 'border-blue-400 bg-blue-50' : ...` — `bg-blue-50` é um azul quase branco, viola a convenção de só usar tokens do tema. Trocar por algo como `border-primary bg-primary/10`.

### Baixo

- **D-07 — `HistoricoEntrada` com construtor público** (`public HistoricoEntrada() { }`), diferente de todas as outras entidades (`private`). Cosmético — trocar por `private` e usar o factory `Criar()` já existente.
- **D-08 — `ObterDistribuicaoQueryHandler` faz 5 queries sequenciais** (uma por status) em vez de um único `GroupBy`, inconsistente com `ContarPorCategoriaAsync`/`ContarPorPrioridadeAsync` no mesmo repositório. Não é urgente (poucos status fixos), mas vale unificar.
- **D-09 — Checagem "só Admin" de `UsuariosController` está no Controller, não no Handler.** Funciona hoje, mas qualquer novo caller que invoque os Commands de `Usuarios` diretamente (fora do Controller) não herda a proteção automaticamente. Decisão consciente a se tomar (mover pra dentro do Handler ou documentar a convenção).
- **D-10 — Indentação inconsistente** em `ResolverChamadoCommandHandler.cs`, `FecharChamadoCommandHandler.cs`, `CancelarChamadoCommandHandler.cs` (linha do `RegistrarHistoricoAsync`). `dotnet format` resolve.
- **D-11 — `ProfileSelector.tsx` é arquivo morto** — sem nenhuma referência desde que `LoginPage.tsx` assumiu seu lugar (F5a). Remover.
- **D-12 — Cores de status hardcoded em `KanbanBoard.tsx`** (`bg-red-500`, `bg-yellow-500` etc.) em vez dos tokens `--chart-*`/`--status-*` já usados no Dashboard/Relatório Mensal. Funcionam visualmente, mas destoam da convenção.
- **D-13 — `UsuarioFormDialog` usa `<label>` solto pro checkbox "Ativo"** em vez do padrão `<Label htmlFor>` usado no resto do form e do projeto. Funciona (associação implícita), mas inconsistente.
- **D-14 — Mistura de `perfil!.id` (non-null assertion) com `perfil?.tipo`** nos mesmos componentes (`ChamadoDetailPage.tsx`, `FilaAtendimentoPage.tsx`). Seguro na prática (rota já garante `perfil` via `ProtectedRoute`), mas inconsistente estilisticamente.
- **D-15 — `DashboardPage` tem uma pequena race entre duas queries independentes** (`useDashboardMetrics` e `useDashboardDistribuicao`) — se uma responder antes da outra, a tela pode mostrar "Nenhum chamado no sistema" por uma fração de segundo antes do gráfico real aparecer. Gatear a seção de distribuição pelo próprio loading/erro dela.

---

## ✅ RESOLVIDOS

### C-01 — Conflito SQLite dev vs PostgreSQL migration (RESOLVIDO)

**Problema:** `appsettings.json` usava SQLite (`chamadoscamarj.db`) e `Program.cs` registrava `UseSqlite`. Mas a migration `20260614000000_InitialCreate.cs` usava tipos PostgreSQL nativos (`uuid`, `character varying`, `timestamp with time zone`). Essa migration não rodava no SQLite.

**Solução aplicada:** Dev e prod agora usam o mesmo PostgreSQL via Supabase. `Program.cs` mudou de `UseSqlite` + `EnsureCreated()` para `UseNpgsql` + `MigrateAsync()`. Migration recriada (`20260619130320_InitialCreate`) com tipos PostgreSQL corretos, já incluindo a FK do C-07.

**Detalhe de conexão importante:** a aba "Direct connection" do Supabase só resolve via IPv6 e falha em redes sem IPv6. A aba "Transaction pooler" (porta 6543) não suporta bem prepared statements do EF Core. A conexão que funciona é o **Session pooler** (porta 5432, host `aws-N-<região>.pooler.supabase.com`, usuário `postgres.<project_ref>`). Senha fica em `dotnet user-secrets` em dev, nunca em `appsettings.json`.

---

### C-02 — Filtros de ListarChamados em memória (N+1 risco) (RESOLVIDO)

**Problema:** `ListarChamadosQueryHandler` chama `ObterTodosAsync()` que carrega **todos os chamados** do banco (com Includes de Categoria, Comentarios e Anexos), depois filtra e pagina em memória.

**Impacto:** Com volume de chamados, isso vira um problema de performance grave.

**Solução recomendada:** Passar os filtros para o repositório e construir a query `IQueryable<Chamado>` com predicados antes de executar.

---

## ✅ Demais itens (RESOLVIDOS)

### C-03 — CategoriasController bypassa CQRS (RESOLVIDO)

**Problema:** `CategoriasController` injeta `ICategoriaRepository` diretamente em vez de usar `IMediator`. Existe `ListarCategoriasQuery` no Application mas não é usada.

**Impacto:** Inconsistência arquitetural — quebra o padrão CQRS adotado.

**Solução:** Mudar o controller para usar `IMediator.Send(new ListarCategoriasQuery())`.

---

### C-04 — DatabaseSeeder não é chamado (código morto) (RESOLVIDO)

**Problema:** `DatabaseSeeder.cs` tem um método `SeedAsync()` bem estruturado, mas o seed real é feito inline e de forma síncrona em `Program.cs`.

**Impacto:** Confusão sobre qual é o mecanismo de seed real.

**Solução:** Remover o inline de `Program.cs` e chamar `await DatabaseSeeder.SeedAsync(db)`.

---

### C-05 — Validators ausentes em 3 Commands (RESOLVIDO)

**Problema:** `AtribuirChamadoCommand`, `ComentarChamadoCommand` e `ResolverChamadoCommand` não têm validators FluentValidation. Qualquer input inválido chega até o Handler sem validação.

**Solução:** Criar `AtribuirChamadoCommandValidator`, `ComentarChamadoCommandValidator`.

---

### C-06 — Fechar e Cancelar sem Command/Endpoint (RESOLVIDO)

**Problema:** `Chamado.Fechar()` e `Chamado.Cancelar()` existem no Domain, mas não há:
- `FecharChamadoCommand` + Handler
- `CancelarChamadoCommand` + Handler
- Endpoints `PATCH /{id}/fechar` e `PATCH /{id}/cancelar`

**Impacto:** O ciclo de vida completo do chamado não está exposto na API.

---

### C-07 — ComentarioId ausente na Migration de Anexos (RESOLVIDO)

**Problema:** `Anexo.cs` tem `ComentarioId (Guid?)` e FK de navegação para `Comentario`, mas a Migration não criou essa coluna nem o FK `FK_Anexos_Comentarios_ComentarioId`.

**Impacto:** Ao migrar para PostgreSQL, o schema vai divergir do modelo.

---

## 🟢 BAIXO (parcialmente resolvidos)

### C-08 — Seed com IDs fixos hardcoded em Program.cs (RESOLVIDO — seed inline removido)

**Problema:** O seed inline usa `Guid.Parse("a1b2c3d4-...")` hardcoded para as categorias, enquanto `DatabaseSeeder.cs` não usa IDs fixos (deixa o `NewGuid()` do BaseEntity).

**Impacto:** Inconsistência entre os dois mecanismos de seed.

---

### C-09 — Sem testes (RESOLVIDO)

**Problema:** O README menciona `tests/ChamadosCamarj.UnitTests/` mas o diretório não existe.

**Solução aplicada:** Projeto `ChamadosCamarj.UnitTests` criado, 48 testes cobrindo Domain e Application passando.

---

### C-10 — `db.Database.EnsureCreated()` em dev (RESOLVIDO)

**Problema:** `EnsureCreated()` não aplica migrations — cria o schema do zero. Se o schema mudar, o banco local pode ficar stale sem aviso.

**Solução aplicada:** Substituído por `db.Database.MigrateAsync()` junto com a migração para PostgreSQL (C-01).
