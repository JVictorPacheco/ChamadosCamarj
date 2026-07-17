# Concerns — Débito Técnico e Riscos

> Itens identificados no mapeamento de 2026-06-18. Todos os itens abaixo foram resolvidos em 2026-06-19.

---

## ✅ RESOLVIDOS (2026-07-17)

> Itens de backend (D-01, D-02, D-03, D-07, D-08, D-09, D-10) e frontend (D-04, D-05, D-06, D-11 a D-15) da revisão sênior de 2026-07-16, corrigidos em conjunto.

### Backend

- **D-01 — Mover chamado no Kanban não gera `HistoricoEntrada`.** `AlterarStatusChamadoCommand` e `AlterarStatusRequest` ganharam `UsuarioId`/`UsuarioNome` (mesmo padrão de `ReatribuirChamadoCommand`/`AlterarPrioridadeChamadoCommand`). `AlterarStatusChamadoCommandHandler` agora injeta `IHistoricoRepository` e chama `RegistrarHistoricoAsync` a cada transição, guardando o status anterior e o novo em `DetalheAnterior`/`DetalheNovo`. `ChamadosController.AlterarStatus` repassa `request.UsuarioId`/`request.UsuarioNome` pro command. **Decisão de design:** como `AlterarStatus` pode transicionar para qualquer status (inclusive `EmAndamento`/`Aberto`, que não têm um `AcaoHistorico` dedicado), foi criado um valor genérico novo no enum — `AcaoHistorico.StatusAlterado = 9` — em vez de reaproveitar `Resolvido`/`Fechado`/`Cancelado` (que ficam reservados para quando a ação vem dos commands dedicados `Resolver`/`Fechar`/`Cancelar`). Isso deixa claro no histórico se a mudança veio do Kanban (drag & drop genérico) ou de uma ação de negócio explícita. **Confirmado com o usuário em 2026-07-17:** manter o enum separado (`StatusAlterado`). **Correção adicional (2026-07-17):** o backend ficou pronto mas o frontend do Kanban (`KanbanBoard.tsx`) ainda não enviava `usuarioId`/`usuarioNome` — os campos foram deixados opcionais no backend (default `"Sistema"`) pra não quebrar em runtime, mas o drag & drop continuaria gravando "Sistema" em vez do usuário real. Completado: `frontend/src/features/chamados/api.ts` (`alterarStatus` ganhou os 2 parâmetros) e `KanbanBoard.tsx` (usa `useAuth()` pra pegar `perfil.id`/`perfil.nome` e passar pro `alterarStatus`).
- **D-02 — Sem guarda contra auto-lockout de Admin.** `AtualizarUsuarioPerfilCommandHandler` agora verifica, antes de desativar ou trocar o `Perfil` de um usuário: se o alvo é Admin ativo e a operação o tornaria não-Admin e/ou inativo, conta quantos `UsuarioPerfil` ativos com `Perfil == Admin` existem via `ListarAsync` (filtrado em memória, sem novo método de repositório). Se for o único, lança `ConflictException` ("Não é possível desativar/rebaixar o último Admin ativo do sistema."). Usuários já inativos não disparam a checagem (evita around-trip desnecessário no `ListarAsync`).
- **D-03 — `ListarChamadosQuery` sem Validator, paginação sem limites.** Criado `ListarChamadosQueryValidator` (`Features/Chamados/Validators/`) com `Pagina > 0` e `TamanhoPagina` entre 1 e 100 (`InclusiveBetween`). **Confirmado:** o pipeline `ValidationBehaviour` já valida Queries, não só Commands — a prova é que `ObterRelatorioMensalQueryValidator` já existia e funcionava antes desta tarefa (MediatR não distingue Command de Query no pipeline, e `AddValidatorsFromAssembly` registra qualquer `AbstractValidator<T>` do assembly). Não foi necessário nenhum ajuste em `Program.cs`.
- **D-07 — `HistoricoEntrada` com construtor público.** Trocado para `private HistoricoEntrada() { }`, igual às demais entidades. O factory estático `Criar()` (membro da própria classe) continua funcionando sem alteração.
- **D-08 — `ObterDistribuicaoQueryHandler` faz 5 queries sequenciais.** Adicionado `IChamadoRepository.ContarPorStatusAgrupadoAsync()`, que faz um único `GroupBy(c => c.Status)` (mesmo padrão de `ContarPorCategoriaAsync`/`ContarPorPrioridadeAsync`). O handler agora chama esse método uma vez e monta o `DistribuicaoResponse` via `Dictionary.GetValueOrDefault` por status.
- **D-09 — Checagem "só Admin" de `UsuariosController` está no Controller, não no Handler.** Movida para dentro dos Handlers (`CriarUsuarioPerfilCommandHandler`, `AtualizarUsuarioPerfilCommandHandler`, `ListarUsuariosPerfilQueryHandler`), cada um agora com campo `PerfilRequisitante` (string?) no Command/Query, preenchido pelo Controller a partir do query param `perfilRequisitante` que já existia. Criada `ForbiddenException` (`Common/Exceptions/`, mesmo padrão minimalista de `ConflictException`) mapeada para HTTP 403 em `ExceptionHandlingMiddleware`. A checagem em si foi centralizada num helper novo, `Common/Authorization/PerfilRequisitanteGuard.ExigirAdmin(...)`, usado pelos 3 Handlers em vez de duplicar a comparação de string em cada um. `UsuariosController` não faz mais a checagem — só repassa `perfilRequisitante` pro Command/Query (via `command with { PerfilRequisitante = ... }` no Criar/Atualizar, e no construtor no Listar) e removeu os helpers `EhAdmin`/`Proibido`, que ficaram sem uso. **Confirmado com o usuário em 2026-07-17:** manter o `PerfilRequisitanteGuard` compartilhado.
- **D-10 — Indentação inconsistente.** Corrigida manualmente a indentação da linha `await _historicoRepository.RegistrarHistoricoAsync(...)` em `ResolverChamadoCommandHandler.cs`, `FecharChamadoCommandHandler.cs` e `CancelarChamadoCommandHandler.cs` (estava com 16 espaços em vez de 8). Não foi rodado `dotnet format` na solução inteira para evitar reformatação em massa de arquivos não relacionados.

### Frontend

- **D-04 — Mensagem de erro enganosa em `UsuariosPage` para não-Admin.** Investigado, sem alteração de código: o guard `if (!isAdmin) return ...` em `UsuariosPage.tsx` roda em todo render, antes do bloco que exibe `isError`. Como o app usa login mockado (sem token/expiração — `AuthContext.perfil` só muda via `login()`/`logout()` explícitos, sem listener de storage entre abas), não existe caminho em que `isAdmin` vire `false` sem o componente re-renderizar e cair no early return primeiro. Ou seja, não há hoje um cenário prático em que a mensagem genérica mascare um 403 de permissão — o bloqueio de conteúdo já cobre isso. Nenhum código alterado para este item.
- **D-05 — Hook `useSignalR` recria a conexão a cada subscribe/unsubscribe.** `frontend/src/hooks/useSignalR.tsx`: trocado o `useState<Set<...>>` de subscribers por `useRef<Set<...>>`, dando identidade estável a `notify` (agora com deps `[]`). O `useEffect` que abre a `HubConnection` continua com `[notify]` nas deps, mas como `notify` nunca muda de referência, o efeito roda só uma vez (montagem do provider).
- **D-06 — Cor clara hardcoded no Kanban quebra o tema escuro.** `frontend/src/features/chamados/kanban/KanbanColumn.tsx`: `border-blue-400 bg-blue-50` → `border-primary bg-primary/10`.
- **D-11 — `ProfileSelector.tsx` é arquivo morto.** Confirmado via grep (zero referências fora do próprio arquivo) e removido `frontend/src/auth/ProfileSelector.tsx`.
- **D-12 — Cores de status hardcoded em `KanbanBoard.tsx`.** Remapeado para tokens do tema: Aberto → `bg-chart-1`, Em Andamento → `bg-chart-3`, Resolvido → `bg-[var(--status-good)]`, Fechado → `bg-chart-5`, Cancelado → `bg-[var(--status-critical)]`. Decisão do agente: como o Kanban mostra as 5 colunas lado a lado, optou-se por manter as 5 cores visualmente distintas (em vez de repetir `status-good` em Resolvido e Fechado), usando o token semântico só onde há sentido de bom/ruim claro (Resolvido/Cancelado) e tokens neutros `--chart-*` nos demais.
- **D-13 — `UsuarioFormDialog` usa `<label>` solto pro checkbox "Ativo".** `frontend/src/features/admin/components/UsuarioFormDialog.tsx`: trocado por `<Label htmlFor="usuario-ativo">` + `id="usuario-ativo"` no `Checkbox`, no padrão do `ComentarioForm.tsx`.
- **D-14 — Mistura de `perfil!.id` com `perfil?.tipo`.** Confirmado no router (`App.tsx` → `ProtectedRoute`) que a rota garante `perfil` não-nulo. Ainda assim, o padrão dominante no restante do projeto (14+ ocorrências em `ChamadosListPage`, `RelatorioMensalPage`, `ComentarioForm`, `ComentarioList`, `AppLayout`, e no próprio `useAtorAtual()` de `useAcoesChamado.ts`) é optional chaining com fallback (`perfil?.id ?? ''`), não non-null assertion. `ChamadoDetailPage.tsx` e `FilaAtendimentoPage.tsx` foram padronizados para esse estilo (`perfil?.id ?? ''` / `perfil?.nome ?? ''`), eliminando as 4 ocorrências de `perfil!.`.
- **D-15 — `DashboardPage` tem race entre duas queries independentes.** `frontend/src/features/dashboard/DashboardPage.tsx`: seção de distribuição agora gateada por `isPending`/`isError` próprios de `useDashboardDistribuicao` (com mensagem de carregamento e erro dedicadas), em vez de depender só do `isPending` de `useDashboardMetrics`.

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
