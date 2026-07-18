# Spec — Arquivo de Chamados (Encerrados)

> Status: IMPLEMENTADA (ARQ-T01 a ARQ-T06 completas, 2026-07-17) — aguardando validação do usuário e commit
> Criado em: 2026-07-16
> Pedido feito pelo usuário durante teste manual da aplicação (branch `develop`, sessão de 2026-07-16)

---

## Problem Statement

Hoje, chamados `Resolvido`, `Fechado` e `Cancelado` continuam misturados nas mesmas telas operacionais (Kanban, Fila de Atendimento, "Meus Chamados") junto com os chamados ativos. À medida que o volume cresce, isso polui a visão do dia a dia. O usuário pediu, ao testar a aplicação, uma forma de "remover" chamados finalizados da vista — mas apagar dados de um sistema de auditoria contradiz o próprio motivo de existir do `HistoricoEntrada` (Fase 6) e do Relatório Mensal (Fase 7), que dependem desses registros permanecerem intactos. A solução é uma **tela separada** ("Arquivo de Chamados") que tira o finalizado do fluxo ativo sem apagar nada do banco.

## Contexto (decisões de negócio)

| Decisão | Escolha |
|---|---|
| Exclusão de dados | **Não.** Nenhum chamado é apagado — contraria o objetivo de auditoria/compliance já estabelecido em `HistoricoEntrada` e Relatório Mensal |
| Abordagem | Tela nova e separada, tipo "arquivo", listando só os chamados finalizados (Resolvido, Fechado, Cancelado) |
| Escopo | "Algo simples" — reaproveitar o máximo possível do que já existe (endpoint de listagem, componente de filtro, card de chamado) |
| Filtros pedidos | Por data e por prioridade, "e tudo mais" — interpretado como: também reaproveitar os filtros já existentes (status entre os finalizados, categoria, busca textual) |

---

## Goals

- [ ] Usuário consegue ver todos os chamados já finalizados (Resolvido/Fechado/Cancelado) numa tela própria, sem os ativos misturados
- [ ] Kanban, Fila de Atendimento e "Meus Chamados" continuam mostrando só o que está ativo — nenhuma mudança de comportamento nessas telas
- [ ] Consegue filtrar por período (data), prioridade, status (dentre os finalizados), categoria e busca textual
- [ ] Nenhum dado novo é armazenado — a tela é só uma visão filtrada sobre os chamados que já existem

## Out of Scope

| Item | Motivo |
|---|---|
| Exclusão/purge definitivo de chamados | Decidido: nunca apagar — quebraria auditoria e os números já fechados do Relatório Mensal |
| Reabertura de chamado a partir dessa tela | `Chamado.Reabrir()` já existe no Domain; a tela de Arquivo só lista e linka pro Detalhe do Chamado, onde as ações por perfil já existem — não duplicar lógica de ação aqui |
| Exportação (CSV/PDF) desta lista | Não pedido; Relatório Mensal já cobre exportação de números agregados. Puxar aqui só se virar pedido futuro |
| "Forçar encerramento" (Admin) | Item pendente da Fase 6, spec separada (`.specs/features/fase-6-admin-log/spec.md`), não faz parte deste escopo |

---

## User Stories

### P1: Ver chamados finalizados numa tela separada ⭐ MVP

**User Story**: Como Admin/Atendente, quero ver os chamados já Resolvidos, Fechados ou Cancelados numa lista própria, separada do Kanban/Fila, pra consultar o que já foi concluído sem isso poluir minha visão do dia a dia.

**Why P1**: É o núcleo do pedido — sem a lista separada, nada mais nessa spec tem valor.

**Acceptance Criteria**:

1. WHEN o usuário acessa "Arquivo de Chamados" THEN o sistema SHALL listar somente chamados com status `Resolvido`, `Fechado` ou `Cancelado`, paginados (reaproveitando `GET /api/chamados` e o padrão de paginação já usado em `ChamadosListPage`)
2. WHEN não há nenhum chamado finalizado THEN o sistema SHALL mostrar um estado vazio claro, não uma lista quebrada
3. WHEN o usuário clica num chamado da lista THEN o sistema SHALL abrir o Detalhe do Chamado já existente (`ChamadoDetailPage`) — sem tela de detalhe própria
4. WHEN um Atendente acessa THEN o sistema SHALL mostrar só os chamados onde ele foi responsável (mesmo critério de "Meus Chamados" hoje: `responsavelId`); WHEN um Solicitante acessa THEN o sistema SHALL mostrar só os chamados que ele abriu (`solicitanteEmail`) — mesmo padrão de RBAC "soft" já usado no resto do app (não é dado novo mais sensível, já visível em Kanban/"Meus Chamados" hoje)

**Independent Test**: Resolver/fechar/cancelar 3 chamados de teste, confirmar que somem do Kanban e apareçam no Arquivo; confirmar que um chamado `Aberto`/`EmAndamento` nunca aparece lá.

---

### P1: Filtrar por período (data de abertura)

**User Story**: Como Admin, quero filtrar o Arquivo por um intervalo de datas, pra achar rapidamente chamados de um período específico.

**Why P1**: Foi pedido explicitamente ("filtro por data").

**Acceptance Criteria**:

1. WHEN o usuário define uma data de início e/ou fim THEN o sistema SHALL filtrar por `DataCriacao` (data de abertura) dentro do intervalo
2. WHEN nenhuma data é informada THEN o sistema SHALL mostrar todos os períodos (comportamento atual)

**Independent Test**: Filtrar por um mês conhecido e conferir que só aparecem chamados abertos naquele mês.

> Ver Notas Técnicas — por que o filtro é por `DataCriacao` (abertura) e não por "data de conclusão" no P1.

---

### P1: Filtrar por prioridade

**User Story**: Como Admin/Atendente, quero filtrar o Arquivo por prioridade, pra revisar por exemplo só os chamados `Urgente` que já foram concluídos.

**Why P1**: Foi pedido explicitamente ("filtro por priorização"). O backend (`ListarChamadosQuery.Prioridade`) já suporta esse filtro — só falta expor no componente de filtro do frontend.

**Acceptance Criteria**:

1. WHEN o usuário seleciona uma prioridade (Baixa/Média/Alta/Urgente) THEN o sistema SHALL filtrar a lista por `Prioridade`
2. WHEN nenhuma prioridade é selecionada THEN o sistema SHALL mostrar todas

**Independent Test**: Filtrar por "Urgente" e conferir que só aparecem chamados urgentes finalizados.

---

### P2: Filtrar por status (dentre os finalizados), categoria e busca

**User Story**: Como Admin/Atendente, quero refinar ainda mais o Arquivo por status específico (só Cancelados, por exemplo), categoria ou texto livre, reaproveitando os filtros que já existem no resto do app.

**Why P2**: Não foi pedido explicitamente, mas "e tudo mais" e o reaproveitamento do componente `FiltroChamados` (que já tem Status/Categoria/Busca) tornam isso quase gratuito de entregar junto.

**Acceptance Criteria**:

1. WHEN o usuário seleciona um status dentre `Resolvido`/`Fechado`/`Cancelado` THEN o sistema SHALL filtrar por esse status específico (dentro do universo já restrito a finalizados)
2. WHEN o usuário seleciona uma categoria ou digita um termo de busca THEN o sistema SHALL aplicar esses filtros normalmente, iguais aos já existentes em `FiltroChamados`

**Independent Test**: Combinar status=Cancelado + categoria=Financeiro e conferir resultado.

---

## Edge Cases

- WHEN um chamado é reaberto (`Reabrir()`) depois de estar em `Resolvido`/`Fechado` THEN o sistema SHALL deixar de mostrá-lo no Arquivo assim que o status mudar — a listagem é sempre pelo status **atual**, não um snapshot congelado
- WHEN o filtro de data é combinado com um status que não existia ainda naquele período (ex: `Fechado` só foi implementado na Fase 6) THEN o sistema SHALL simplesmente retornar vazio, sem erro
- WHEN o Atendente ou Solicitante não tem nenhum chamado finalizado THEN o sistema SHALL mostrar o mesmo estado vazio do caso geral, não esconder a tela inteira

---

## Requirement Traceability

| Requirement ID | Story | Phase | Status |
|---|---|---|---|
| ARQ-01 | P1: Listar só finalizados, paginado | ARQ-T01, ARQ-T05 | Mapped |
| ARQ-02 | P1: Link pro Detalhe do Chamado | ARQ-T05 (reaproveita `ChamadoCard`) | Mapped |
| ARQ-03 | P1: RBAC por perfil (mesmo padrão de "Meus Chamados") | ARQ-T05 | Mapped |
| ARQ-04 | P1: Filtro por período (DataCriacao) | ARQ-T01, ARQ-T02, ARQ-T03, ARQ-T04 | Mapped |
| ARQ-05 | P1: Filtro por prioridade | ARQ-T03, ARQ-T04 | Mapped |
| ARQ-06 | P2: Filtro por status/categoria/busca | ARQ-T03, ARQ-T05 | Mapped |

**Coverage:** 6 total, 6 mapped to tasks, 0 unmapped — ver `tasks.md` para detalhamento e `design.md` para arquitetura.

---

## Notas técnicas (para a fase de Design)

- **Backend:** `ListarChamadosQuery`/`ListarChamadosQueryHandler` (`src/ChamadosCamarj.Application/Features/Chamados/Queries/`) hoje aceita só **um** valor de `Status` (match exato via `Enum.TryParse`). Pra listar "Resolvido OU Fechado OU Cancelado" de uma vez, a opção mais simples é estender `Status` pra aceitar uma lista separada por vírgula (ex: `Status=Resolvido,Fechado,Cancelado`), mudando o parse de um enum único pra uma lista de enums e o filtro do repositório de `==` pra `.Contains()`. Alternativa mais isolada: um novo parâmetro booleano (`Finalizados=true`) que o handler traduz pra esse mesmo `IN`, sem mudar a semântica do `Status` existente — evita quebrar quem já usa `Status` como filtro único (Kanban, Fila).
- **`Prioridade` já está pronto ponta a ponta no backend** (`ListarChamadosQuery.Prioridade` → `IChamadoRepository.ListarAsync`) — só falta adicionar o `Select` de prioridade no componente `FiltroChamados.tsx` (frontend), que hoje só tem Status/Categoria/Busca.
- **Filtro de data — por que `DataCriacao` (abertura) no P1, não "data de conclusão":** `Chamado.DataConclusao` é preenchido em `Resolver()`/`Fechar()`, mas **`Cancelar()` não seta `DataConclusao`** (mesma lacuna documentada na spec do Relatório Mensal, `.specs/features/relatorio-mensal/spec.md`, seção "Notas técnicas"). Filtrar por `DataConclusao` deixaria os `Cancelado` de fora de qualquer filtro de período. `DataCriacao` existe e é confiável pra 100% dos chamados, então é o filtro seguro pro MVP. Se depois quiserem filtrar por "data em que foi finalizado" especificamente, a fonte confiável é `HistoricoEntrada` (mesma solução usada no Relatório Mensal), não `DataConclusao` isolado — não reintroduzir esse bug.
- **Reaproveitar, não duplicar:** a tela pode ser uma variação de `ChamadosListPage.tsx` (mesma estrutura de paginação/card) com uma query fixa adicional de "só finalizados" e o filtro de data/prioridade acrescentados ao `FiltroChamadosValue`. Evitar criar um componente de card de chamado novo.

---

## Success Criteria

- [ ] Um chamado nunca aparece simultaneamente no Kanban/Fila (ativo) e no Arquivo (finalizado) — são mutuamente exclusivos pelo status atual
- [ ] Nenhuma linha de código de exclusão (`DELETE`) foi adicionada — a feature é 100% leitura/filtro
- [ ] Atendente e Solicitante veem só os seus, igual ao padrão já usado em "Meus Chamados"
- [ ] Filtro de prioridade funciona igual em qualquer tela que já usa `ListarChamadosQuery.Prioridade`

## Dependências

- Fase 5 ✅ (padrão de listagem/paginação de chamados)
- Fase 6 ✅ (nenhuma dependência direta, mas reforça o motivo de não apagar dados — `HistoricoEntrada`)

## Critérios de aceite

- Admin filtra por mês + prioridade Urgente e vê só os chamados urgentes fechados naquele mês
- Atendente (Fábio) só vê os próprios chamados finalizados
- Nenhum chamado ativo (`Aberto`/`EmAndamento`) aparece na tela em nenhuma combinação de filtro
- Clicar num card do Arquivo abre o Detalhe do Chamado normal, com histórico completo
