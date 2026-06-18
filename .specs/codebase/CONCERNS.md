# Concerns — Débito Técnico e Riscos

> Itens identificados no mapeamento de 2026-06-18. Ordenados por impacto.

---

## 🔴 CRÍTICO

### C-01 — Conflito SQLite dev vs PostgreSQL migration

**Problema:** `appsettings.json` usa SQLite (`chamadoscamarj.db`) e `Program.cs` registra `UseSqlite`. Mas a migration `20260614000000_InitialCreate.cs` usa tipos PostgreSQL nativos (`uuid`, `character varying`, `timestamp with time zone`). Essa migration **não roda no SQLite**.

**Situação atual:** O app usa `EnsureCreated()` em dev, que ignora as migrations e cria o schema diretamente do modelo. Funciona em dev, mas a migration está desatualizada para PostgreSQL.

**Risco:** Na hora de migrar para Supabase/PostgreSQL, as migrations precisarão ser recriadas ou a configuração dual precisará ser feita corretamente.

**Solução recomendada:** Usar `IDesignTimeDbContextFactory` para gerar migrations PostgreSQL, e manter SQLite apenas como `UseSqlite` em dev sem migrations. Ou usar PostgreSQL desde o início via Docker.

---

### C-02 — Filtros de ListarChamados em memória (N+1 risco)

**Problema:** `ListarChamadosQueryHandler` chama `ObterTodosAsync()` que carrega **todos os chamados** do banco (com Includes de Categoria, Comentarios e Anexos), depois filtra e pagina em memória.

**Impacto:** Com volume de chamados, isso vira um problema de performance grave.

**Solução recomendada:** Passar os filtros para o repositório e construir a query `IQueryable<Chamado>` com predicados antes de executar.

---

## 🟡 MÉDIO

### C-03 — CategoriasController bypassa CQRS

**Problema:** `CategoriasController` injeta `ICategoriaRepository` diretamente em vez de usar `IMediator`. Existe `ListarCategoriasQuery` no Application mas não é usada.

**Impacto:** Inconsistência arquitetural — quebra o padrão CQRS adotado.

**Solução:** Mudar o controller para usar `IMediator.Send(new ListarCategoriasQuery())`.

---

### C-04 — DatabaseSeeder não é chamado (código morto)

**Problema:** `DatabaseSeeder.cs` tem um método `SeedAsync()` bem estruturado, mas o seed real é feito inline e de forma síncrona em `Program.cs`.

**Impacto:** Confusão sobre qual é o mecanismo de seed real.

**Solução:** Remover o inline de `Program.cs` e chamar `await DatabaseSeeder.SeedAsync(db)`.

---

### C-05 — Validators ausentes em 3 Commands

**Problema:** `AtribuirChamadoCommand`, `ComentarChamadoCommand` e `ResolverChamadoCommand` não têm validators FluentValidation. Qualquer input inválido chega até o Handler sem validação.

**Solução:** Criar `AtribuirChamadoCommandValidator`, `ComentarChamadoCommandValidator`.

---

### C-06 — Fechar e Cancelar sem Command/Endpoint

**Problema:** `Chamado.Fechar()` e `Chamado.Cancelar()` existem no Domain, mas não há:
- `FecharChamadoCommand` + Handler
- `CancelarChamadoCommand` + Handler
- Endpoints `PATCH /{id}/fechar` e `PATCH /{id}/cancelar`

**Impacto:** O ciclo de vida completo do chamado não está exposto na API.

---

### C-07 — ComentarioId ausente na Migration de Anexos

**Problema:** `Anexo.cs` tem `ComentarioId (Guid?)` e FK de navegação para `Comentario`, mas a Migration não criou essa coluna nem o FK `FK_Anexos_Comentarios_ComentarioId`.

**Impacto:** Ao migrar para PostgreSQL, o schema vai divergir do modelo.

---

## 🟢 BAIXO

### C-08 — Seed com IDs fixos hardcoded em Program.cs

**Problema:** O seed inline usa `Guid.Parse("a1b2c3d4-...")` hardcoded para as categorias, enquanto `DatabaseSeeder.cs` não usa IDs fixos (deixa o `NewGuid()` do BaseEntity).

**Impacto:** Inconsistência entre os dois mecanismos de seed.

---

### C-09 — Sem testes

**Problema:** O README menciona `tests/ChamadosCamarj.UnitTests/` mas o diretório não existe.

**Impacto:** Sem cobertura de testes para o Domain (que tem lógica de negócio relevante: SLA, transições de estado).

---

### C-10 — `db.Database.EnsureCreated()` em dev

**Problema:** `EnsureCreated()` não aplica migrations — cria o schema do zero. Se o schema mudar, o banco local pode ficar stale sem aviso.

**Solução:** Em dev, usar `db.Database.Migrate()` ou deletar o `.db` manualmente ao mudar o schema.
