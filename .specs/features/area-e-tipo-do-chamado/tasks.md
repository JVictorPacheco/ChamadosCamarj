# Área e Tipo do Chamado — Tasks

> **Branch:** `feature/area-e-tipo-do-chamado` (implementada num git worktree em `Projects/ChamadosCamarj-area`)
> **Spec:** `spec.md` (AC-01 a AC-16) | **Design:** `design.md`
> **Ferramenta:** Claude Code (Opus 5.5) — spec, design, tasks e implementação na sessão principal; review por sub-agente independente
> **Gate checks:** `dotnet build` + `dotnet test tests/ChamadosCamarj.UnitTests/` + `npm --prefix frontend run build`

## Backend
- [x] **T01.** Domain: `TipoChamado`; `Chamado` com `AreaId`/`TipoId` (opcionais no banco) e `CategoriaId` legado opcional; `ReclassificarTipo`; `AcaoChamado.ReclassificarTipo` (Atendente/Admin); `AcaoHistorico.TipoReclassificado` *(AC-01, AC-11)*
- [x] **T02.** Tipos: repositório, CRUD (`/api/tipos`, gerado a partir de Categorias), configuração EF *(AC-08)*
- [x] **T03.** Categorias saem da API (controller, feature, repositório); entidade e tabela ficam (compatibilidade) *(AC-09, AC-15)*
- [x] **T04.** Abertura exige área (grupo ativo) e tipo (ativo); listagem filtra por `areaId`/`tipoId`; visibilidade inclui `AreaId == grupo` *(AC-01, AC-07, AC-14)*
- [x] **T05.** Dashboard e Relatório por área e por tipo; triagem sugere área e tipo *(AC-04, AC-12, AC-13)*
- [x] **T06.** Login devolve `grupoId` *(AC-02)*
- [x] **T07.** Migration `AddAreaETipoChamado` compatível com a versão anterior + `AreaETipoPadrao` (tipos fixos e SQL de preenchimento idempotente, também no seeder); seeder não sobrescreve mais grupos/tipos *(AC-05, AC-06, AC-10, AC-11)*
- [x] **T08.** Testes ajustados (390 verdes)

## Frontend
- [x] **T09.** Abertura com Área (padrão = grupo do usuário) e Tipo; "Sugerir área e tipo" *(AC-01..AC-04)*
- [x] **T10.** Detalhe mostra Área e Tipo; Atendente/Admin reclassificam o tipo *(AC-11)*
- [x] **T11.** Filtros, cartão, Dashboard e Relatório (+ exportação) por área e tipo *(AC-12..AC-14)*
- [x] **T12.** Admin: "Tipos de chamado" (`/admin/tipos`) no lugar de Categorias; "Áreas e Grupos" *(AC-08, AC-09, AC-15)*

## Validação
- [x] **T13.** Aplicar a migration no Supabase (aprovada em 2026-10-02) e conferir: chamados por categoria antes = por área depois; nenhum chamado sem área/tipo
- [x] **T14.** Verificação ao vivo (API + tela) com dados de teste, apagados no final — 21 checagens; 1 falha real corrigida (nomes na resposta da abertura)
- [x] **T15.** Review independente (1 bloqueante e 7 atenção — corrigidos ou registrados, ver review.md)
- [x] **T16.** Gates finais (401/401, build ok)

## Pendências conhecidas
- Testes E2E do Playwright (`frontend/e2e/*.spec.ts`) ainda usam "Categoria" — não fazem parte dos gates; atualizar numa próxima rodada.
- Limpeza pós-deploy (outra feature): `AreaId`/`TipoId` obrigatórios; remover `Categorias`/`CategoriaId`.
