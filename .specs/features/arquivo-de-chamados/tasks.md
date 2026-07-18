# Tasks — Arquivo de Chamados

> Ver `design.md` para arquitetura completa. Criado em 2026-07-17.

---

### ARQ-T01 — Backend: `Finalizados` + filtro de período em `ListarChamadosQuery` ✅ Concluída (2026-07-17)

**O quê:** Adicionar `Finalizados` (bool?), `DataInicio` (DateTime?), `DataFim` (DateTime?) ao `ListarChamadosQuery`; traduzir `Finalizados=true` pra `[Resolvido, Fechado, Cancelado]` no Handler; adicionar `statusEntre`/`dataInicio`/`dataFim` ao `IChamadoRepository.ListarAsync`/`ChamadoRepository.ListarAsync` com os filtros `Where` correspondentes (por `DataCriacao`, nunca `DataConclusao`).

**Onde:** `Application/Features/Chamados/Queries/ListarChamadosQuery.cs`, `ListarChamadosQueryHandler.cs`, `Domain/Interfaces/IChamadoRepository.cs`, `Infrastructure/Repositories/ChamadoRepository.cs`.

**Depende de:** nada.

**Reaproveita:** o padrão de filtro condicional (`if (x.HasValue) query = query.Where(...)`) já usado nos outros parâmetros da mesma query.

**Pronto quando:** `Finalizados=true` retorna só chamados com os 3 status finalizados; combinado com `Status=Cancelado` retorna só cancelados; `DataInicio`/`DataFim` filtram por `DataCriacao`.

**Testes:** unit tests do handler (Finalizados true/false/null, combinação com Status, DataInicio/DataFim isolados e combinados) — mock do repositório, mesmo padrão de `ListarChamadosQueryHandlerTests` se já existir, senão seguir o estilo de `ObterDistribuicaoQueryHandlerTests.cs`.

**Gate:** `dotnet build` + `dotnet test` limpos.

---

### ARQ-T02 — Backend: validação de período em `ListarChamadosQueryValidator` ✅ Concluída (2026-07-17)

**O quê:** Adicionar regra: se `DataInicio` e `DataFim` forem ambos informados, `DataFim` deve ser `>= DataInicio`.

**Onde:** `Application/Features/Chamados/Validators/ListarChamadosQueryValidator.cs` (já existe, criado no débito técnico D-03).

**Depende de:** ARQ-T01 (precisa dos campos existirem no Query).

**Pronto quando:** `DataFim < DataInicio` retorna erro de validação claro, não um resultado vazio silencioso.

**Testes:** 2 casos novos no validator test existente.

**Gate:** `dotnet test` limpo.

---

### ARQ-T03 — Frontend: `FiltroChamados` ganha prioridade, período e `statusOptions` ✅ Concluída (2026-07-17)

**O quê:** Adicionar `prioridade?`, `dataInicio?`, `dataFim?` ao `FiltroChamadosValue`; `Select` de prioridade (Baixa/Média/Alta/Urgente); dois `<input type="date">` pra período; novo prop `statusOptions?: StatusChamado[]` (default: os 5 atuais) pra restringir as opções do `Select` de Status.

**Onde:** `frontend/src/features/chamados/components/FiltroChamados.tsx`.

**Depende de:** nada (componente já existe, só estende).

**Reaproveita:** o padrão de `Select`/`Input` já usado nos outros filtros do mesmo componente.

**Pronto quando:** `ChamadosListPage` (que usa `FiltroChamados` sem passar `statusOptions`) continua funcionando com os 5 status; o novo uso (Arquivo) consegue restringir pra só os 3 finalizados.

**Testes:** verificação manual (não há testes de componente isolado neste projeto, ver `TESTING.md`).

**Gate:** `npm run build` limpo.

---

### ARQ-T04 — Frontend: `api.ts`/`useChamados` repassam os novos filtros ✅ Concluída (2026-07-17)

**O quê:** Adicionar `finalizados?: boolean`, `dataInicio?: string`, `dataFim?: string`, `prioridade?: PrioridadeChamado` (se ainda não repassado) ao `ListarChamadosFiltros` em `frontend/src/features/chamados/api.ts`.

**Onde:** `frontend/src/features/chamados/api.ts`.

**Depende de:** ARQ-T01 (endpoint precisa aceitar os parâmetros).

**Pronto quando:** os novos filtros chegam na query string da chamada `GET /chamados`.

**Gate:** `npm run build` limpo.

---

### ARQ-T05 — Frontend: `ArquivoChamadosPage.tsx` (nova tela) ✅ Concluída (2026-07-17)

**O quê:** Nova página em `frontend/src/features/chamados/ArquivoChamadosPage.tsx`, cópia estrutural de `ChamadosListPage.tsx` (paginação, `FiltroChamados`, `ChamadoCard`, RBAC idêntico Admin/Atendente/Solicitante), com `finalizados: true` fixo no `useChamados()`, `statusOptions` restrito aos 3 finalizados, título "Arquivo de Chamados", estado vazio específico ("Nenhum chamado finalizado ainda", sem botão de abrir chamado).

**Onde:** `frontend/src/features/chamados/ArquivoChamadosPage.tsx` (novo).

**Depende de:** ARQ-T03, ARQ-T04.

**Reaproveita:** `ChamadosListPage.tsx` como referência estrutural direta (não extrair abstração comum agora — ver design.md).

**Pronto quando:** lista só chamados Resolvido/Fechado/Cancelado, paginado, com o mesmo RBAC de "Meus Chamados", e o clique no card abre o Detalhe do Chamado normal.

**Testes:** verificação manual (ver Execute — resolver/fechar/cancelar chamados de teste reais no Supabase e confirmar que aparecem/somem das telas certas).

**Gate:** `npm run build` limpo + verificação manual.

---

### ARQ-T06 — Frontend: rota + item de menu ✅ Concluída (2026-07-17)

**O quê:** Rota `/chamados/arquivo` no router (`App.tsx`), dentro do `ProtectedRoute`, acessível a todos os perfis. Item "Arquivo" na sidebar (`AppLayout.tsx`), visível pra todos os perfis logados (RBAC soft igual "Meus Chamados" — nenhuma restrição de visibilidade por perfil).

**Onde:** `frontend/src/App.tsx`, `frontend/src/layouts/AppLayout.tsx`.

**Depende de:** ARQ-T05.

**Pronto quando:** o link aparece na sidebar pros 3 perfis e navega pra `/chamados/arquivo`.

**Gate:** `npm run build` limpo + verificação manual.

---

## Ordem de execução

ARQ-T01 → ARQ-T02 (backend, sequencial) e ARQ-T03 → ARQ-T04 → ARQ-T05 → ARQ-T06 (frontend, sequencial, depende do backend a partir de T04). Backend e frontend podem rodar em paralelo até o ponto em que o frontend precisa dos parâmetros novos do endpoint (T04).

## Critério de aceite final (do spec.md, reafirmado)

- Resolver/fechar/cancelar 3 chamados de teste reais → confirmam que somem do Kanban/Fila e aparecem no Arquivo
- Um chamado `Aberto`/`EmAndamento` nunca aparece no Arquivo em nenhuma combinação de filtro
- Admin filtra por mês + prioridade Urgente e vê só os urgentes fechados naquele mês
- Atendente (Fábio) só vê os próprios chamados finalizados no Arquivo
- Nenhuma linha de `DELETE` foi adicionada
