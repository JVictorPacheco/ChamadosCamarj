# 📈 Relatório Mensal

> ✅ **Implementado e verificado (Fase 7, antecipada, 2026-07-14/15).** Spec completo em `.specs/features/relatorio-mensal/` (spec → design → tasks → execute).

## O que é

Documento de **período fechado** (mês calendário) com o resumo do andamento dos chamados, pensado para o Admin apresentar à superintendência da Camarj todo fim de mês. Diferente do [[🏗️ Arquitetura|Dashboard]] operacional (Fase 5), que mostra a situação **atual** em tempo real, o Relatório Mensal é uma "foto histórica" de um mês específico — os números de um mês já fechado não mudam com ações futuras.

## Por que existe

- O Dashboard (tempo real) não serve para prestação de contas mensal — não há como "voltar no tempo" nele
- Antecipado na frente do login Google real (T09/T15) por ter prazo de negócio real: fechamento mensal para a superintendência

## Onde fica

- **Página:** `/atendimento/relatorio-mensal`
- **Endpoint:** `GET /api/relatorios/mensal?ano={int}&mes={int}&responsavelId={guid?}`

## O que mostra

- Seletor de mês (mês corrente ou qualquer mês anterior com dados)
- Totais: abertos, resolvidos, cancelados — com variação % vs. mês anterior
- Quebra por categoria e por atendente (só para Admin)
- Cumprimento de SLA (rosca: dentro do prazo vs. estourado)
- Tempo médio de resolução

## Fonte dos dados

Usa **`HistoricoEntrada`** (data real de cada evento — `AcaoHistorico` + `DataHora`), não o status atual do chamado. Isso garante que um chamado resolvido em julho e fechado em agosto continue contando como "resolvido em julho" — ver [[📋 Histórico de Chamados]] e REL-10 na spec (integridade dos dados, requisito não-negociável).

## RBAC — bloqueio real, diferente do resto do app

| Perfil | O que vê |
|--------|----------|
| Admin | Tudo — todos os atendentes e categorias |
| Atendente | Só os próprios números (sem quebra por atendente) |
| Solicitante | **Bloqueado de verdade** (redirect), não só link escondido |

> O resto do app (Dashboard, Kanban, Fila) usa RBAC "soft" — só esconde o link da sidebar, sem bloquear a rota. O Relatório Mensal recebeu bloqueio real por expor dado mais sensível (desempenho individual por atendente). Ver [[👥 Perfis de Usuário]] e [[💬 Decisões]].

## Exportação

- **CSV** — client-side, uma linha por métrica/quebra
- **PDF** — via impressão do navegador (`@media print` dedicado), sem bibliotecas novas

## Relação com outros documentos

- [[🗺️ Roadmap]] — Fase 7 (concluída, antecipada)
- [[📋 Histórico de Chamados]] — fonte de dados via `HistoricoEntrada`
- [[👥 Perfis de Usuário]] — RBAC por perfil
- [[💬 Decisões]] — decisões de 2026-07-14/15 (ordem Fase 6 vs. 7, relatório como período fechado)
