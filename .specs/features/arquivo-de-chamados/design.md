# Design — Arquivo de Chamados (Encerrados)

> Baseado em `spec.md` (seção "Notas técnicas") + leitura do código real (`ListarChamadosQuery`, `ChamadoRepository`, `ChamadosListPage`, `FiltroChamados`) em 2026-07-17.

---

## Decisão 1 — Backend: `Finalizados=true` (não lista de status)

Confirmando a recomendação da spec: adicionar um parâmetro booleano `Finalizados` a `ListarChamadosQuery`, em vez de mudar `Status` para aceitar lista.

**Por quê:** `Status` hoje é usado como filtro único e exato por várias telas (Kanban, Fila, "Meus Chamados") via `Enum.TryParse`. Mudar sua semântica pra lista quebraria esses usos ou exigiria uma migração cuidadosa deles. `Finalizados=true` é aditivo — ninguém que já usa a query hoje é afetado — e combina de forma natural com `Status` quando o usuário quer refinar ainda mais (ARQ-06, P2): `Finalizados=true&Status=Cancelado` simplesmente aplica os dois filtros (`Status IN (Resolvido,Fechado,Cancelado) AND Status == Cancelado`), redundante mas correto, sem lógica condicional especial no handler.

```csharp
public record ListarChamadosQuery(
    int Pagina = 1,
    int TamanhoPagina = 10,
    string? Status = null,
    string? Prioridade = null,
    Guid? ResponsavelId = null,
    Guid? CategoriaId = null,
    string? Busca = null,
    string? SolicitanteEmail = null,
    bool? Finalizados = null,       // NOVO
    DateTime? DataInicio = null,    // NOVO
    DateTime? DataFim = null        // NOVO
) : IRequest<PagedResult<ChamadoResponse>>;
```

No `ListarChamadosQueryHandler`, `Finalizados == true` vira um array fixo `[StatusChamado.Resolvido, StatusChamado.Fechado, StatusChamado.Cancelado]` passado pro repositório. No `IChamadoRepository.ListarAsync`/`ChamadoRepository.ListarAsync`, adicionar dois parâmetros novos:

```csharp
Task<(IEnumerable<Chamado> Items, int Total)> ListarAsync(
    int pagina, int tamanhoPagina,
    StatusChamado? status = null,
    PrioridadeChamado? prioridade = null,
    Guid? responsavelId = null,
    Guid? categoriaId = null,
    string? busca = null,
    string? solicitanteEmail = null,
    IEnumerable<StatusChamado>? statusEntre = null,  // NOVO — usado só quando Finalizados=true
    DateTime? dataInicio = null,                      // NOVO
    DateTime? dataFim = null,                         // NOVO
    CancellationToken cancellationToken = default);
```

Filtro no repositório (mesmo padrão dos filtros já existentes, `IQueryable` + `Where` condicional):

```csharp
if (statusEntre is not null)
    query = query.Where(c => statusEntre.Contains(c.Status));

if (dataInicio.HasValue)
    query = query.Where(c => c.DataCriacao >= dataInicio.Value);

if (dataFim.HasValue)
    query = query.Where(c => c.DataCriacao <= dataFim.Value);
```

**Decisão 2 — Filtro de data é por `DataCriacao`, ponta a ponta.** Já confirmado na spec (`Cancelar()` não seta `DataConclusao`) — `DataInicio`/`DataFim` no Query, Handler e Repositório filtram exclusivamente por `DataCriacao`. Não introduzir um segundo filtro de "data de conclusão" nesta feature.

**Validação:** `ListarChamadosQueryValidator` (já existe, criado no D-03 do débito técnico) ganha uma regra opcional: se ambos `DataInicio`/`DataFim` forem informados, `DataFim >= DataInicio` (senão, mensagem clara em vez de um resultado vazio silencioso confuso).

**Bug encontrado e corrigido em 2026-07-17 (reportado pelo usuário ao testar):** filtrar por qualquer data quebrava com 500 ("Serviço indisponível" na tela) — `System.ArgumentException: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'`. O model binding do ASP.NET Core cria `DateTime` com `Kind=Unspecified` a partir da query string, mas `DataCriacao` é `timestamp with time zone` no Postgres (só aceita UTC). Corrigido no `ListarChamadosQueryHandler`: `DataInicio` vira `DateTime.SpecifyKind(..., DateTimeKind.Utc)` na meia-noite do dia; `DataFim` vira o **fim** do dia (`23:59:59.999...` UTC), não a meia-noite — senão filtrar "hoje até hoje" não incluiria nada criado depois da meia-noite exata.

---

## Decisão 2 — Frontend: nova página reaproveitando `ChamadosListPage`

Em vez de duplicar `ChamadosListPage.tsx`, extrair a lógica comum e criar `ArquivoChamadosPage.tsx` como uma variação fina que:
- Sempre passa `finalizados: true` pro `useChamados()`
- Reaproveita `FiltroChamados`, `ChamadoCard`, a paginação e o RBAC (Admin=todos, Atendente=`responsavelId`, Solicitante=`solicitanteEmail`) exatamente como em `ChamadosListPage.tsx`
- Título fixo: "Arquivo de Chamados"
- Estado vazio com texto específico ("Nenhum chamado finalizado ainda", sem o botão "Abrir chamado" que só faz sentido na lista de ativos)

**Não extrair um hook/componente genérico agora** — os dois componentes (`ChamadosListPage`/`ArquivoChamadosPage`) são pequenos o bastante (~80 linhas) que duplicar a estrutura de paginação/RBAC é mais simples de ler do que introduzir uma abstração nova pra 2 usos. Reavaliar se surgir um 3º caso de uso.

### `FiltroChamados.tsx` — extensão

1. **Novo prop `statusOptions?: StatusChamado[]`** (default: os 5 atuais) — permite ao `ArquivoChamadosPage` restringir o `Select` de Status só a `Resolvido`/`Fechado`/`Cancelado` (ARQ-06), sem duplicar o componente.
2. **Novo campo de prioridade** no `FiltroChamadosValue` (`prioridade?: PrioridadeChamado`) + `Select` no componente (ARQ-05) — o backend já suporta isso ponta a ponta (`ListarChamadosFiltros.prioridade` já existe em `api.ts`), só faltava a UI. Esse `Select` passa a existir pra **ambas** as telas (`ChamadosListPage` também ganha o filtro, não só o Arquivo) — não há motivo pra restringir só ao Arquivo, e a spec do Relatório Mensal já usa prioridade como filtro em outro lugar do app.
3. **Novos campos de período** (`dataInicio?: string`, `dataFim?: string`, formato `yyyy-mm-dd` de `<input type="date">`) — ARQ-04. **Revisão de UX em 2026-07-17:** inicialmente os campos apareciam nas duas telas por reaproveitamento direto do componente, mas sem legenda visível (só `aria-label`), o que deixava confuso o que representavam. Corrigido: `FiltroChamados` ganhou um prop `mostrarPeriodo?: boolean` (default `false`) que controla se os campos de data renderizam, cada um agora com um `Label` visível ("De"/"Até"). Só `ArquivoChamadosPage` passa `mostrarPeriodo` — filtrar por data de abertura entre chamados *ativos* (`ChamadosListPage`) é um caso de uso raro comparado a revisar um período de chamados já finalizados, que foi o pedido original da spec.

### Rota e navegação

- Nova rota `/chamados/arquivo`, dentro do `ProtectedRoute` (mesmo padrão de `/chamados`), acessível a **todos os perfis** (não é Admin-only — ARQ-03 usa RBAC "soft" igual "Meus Chamados", não bloqueio de perfil).
- Item de menu "Arquivo" na sidebar (`AppLayout.tsx`), ao lado de "Meus Chamados"/Kanban, visível pra todos os perfis logados.

---

## Requirement Traceability (atualizado)

| Requirement ID | Story | Componente | Status |
|---|---|---|---|
| ARQ-01 | Listar só finalizados, paginado | `ListarChamadosQuery.Finalizados` + `ArquivoChamadosPage` | → Tasks |
| ARQ-02 | Link pro Detalhe do Chamado | Reaproveita `<Link to={`/chamados/${id}`}>` do `ChamadoCard` | → Tasks |
| ARQ-03 | RBAC por perfil (mesmo padrão de "Meus Chamados") | Lógica de `filtrosQuery` copiada de `ChamadosListPage` | → Tasks |
| ARQ-04 | Filtro por período (DataCriacao) | `DataInicio`/`DataFim` em Query/Repo + inputs em `FiltroChamados` | → Tasks |
| ARQ-05 | Filtro por prioridade | `Select` de prioridade em `FiltroChamados` (backend já pronto) | → Tasks |
| ARQ-06 | Filtro por status/categoria/busca | `statusOptions` restrito + reaproveita Categoria/Busca existentes | → Tasks |

---

## Fora de escopo (reafirmado do spec.md)

- Nenhum `DELETE` — 100% leitura/filtro.
- Sem exportação CSV/PDF nesta tela.
- Sem ação de reabrir chamado a partir daqui (só link pro Detalhe, onde as ações já existem).
