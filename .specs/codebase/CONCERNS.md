# Concerns — Débito Técnico e Riscos

> Itens identificados no mapeamento de 2026-06-18. Todos os itens abaixo foram resolvidos em 2026-06-19.

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
