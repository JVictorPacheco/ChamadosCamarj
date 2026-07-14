# Spec — Relatório Mensal (Fase 7 antecipada)

> Status: PLANEJADO
> Criado em: 2026-07-14
> Decisões de negócio capturadas via discuss em 2026-07-14 (ver seção Contexto)

---

## Problem Statement

O usuário (dono do produto/gestor) precisa apresentar, todo fim de mês, um resumo do andamento dos chamados pra superintendência da Camarj. O Dashboard operacional (Fase 5) mostra números em tempo real, mas não serve pra esse fim — o que é preciso é um **documento de período fechado** (o mês inteiro), com totais e quebras que contam a história do mês, exportável pra anexar em e-mail/apresentação.

## Contexto (decisões de negócio)

| Decisão | Escolha |
|---|---|
| Período | Mês calendário fechado (dia 1 ao último dia do mês) — sem seletor de range livre por ora |
| Métricas adicionais | Por categoria, por atendente, cumprimento de SLA, comparação com o mês anterior — todas incluídas |
| Exportação | PDF e CSV |
| Acesso | Admin (visão total) e Atendente (visão restrita aos próprios números) |

---

## Goals

- [ ] Gestor consegue ver, em menos de 1 minuto, o resumo de qualquer mês fechado (atual ou anterior)
- [ ] Números batem exatamente com os chamados reais do período (zero discrepância com o banco)
- [ ] Relatório pode ser exportado em PDF (pronto pra anexar) e CSV (pra planilha)

## Out of Scope

| Item | Motivo |
|---|---|
| Período livre (range de datas arbitrário) | Decidido: só mês calendário fechado por ora. Pode virar pedido futuro |
| Envio automático por e-mail à superintendência | Não pedido — export manual (PDF/CSV) é suficiente por ora |
| Alertas de SLA em tempo real | Já existe conceitualmente no Dashboard operacional (Fase 5); este relatório só reporta o cumprimento *passado*, não alerta |
| Edição/anotação do relatório | É um relatório gerado, não um documento editável |
| Comparação com qualquer mês arbitrário (só o anterior) | Escopo fixado no mês imediatamente anterior; comparações mais ricas ficam pra depois se pedido |

---

## User Stories

### P1: Ver relatório do mês na tela ⭐ MVP

**User Story**: Como Admin, quero ver um resumo do mês fechado (atual ou um mês anterior) com os totais e quebras principais, pra entender rapidamente como foi o desempenho antes de repassar pra superintendência.

**Why P1**: É o núcleo da feature — sem os números corretos na tela, exportação não tem valor.

**Acceptance Criteria**:

1. WHEN o Admin acessa a tela de Relatório Mensal THEN o sistema SHALL mostrar, por padrão, o mês corrente (do dia 1 até hoje, se o mês ainda não fechou) ou o mês anterior completo, com opção de trocar pra qualquer mês anterior disponível
2. WHEN um mês é selecionado THEN o sistema SHALL mostrar: total de chamados abertos no período, total resolvidos, total cancelados, tempo médio de resolução, quebra por categoria, quebra por atendente, percentual de cumprimento de SLA (resolvidos dentro do prazo vs estourados)
3. WHEN o mês selecionado tem um mês anterior com dados THEN o sistema SHALL mostrar a variação percentual de cada métrica principal em relação ao mês anterior (ex: "32 abertos (+14% vs jun)")
4. WHEN o mês selecionado não tem nenhum chamado THEN o sistema SHALL mostrar um estado vazio claro, não uma tela quebrada ou zerada sem contexto
5. WHEN o Admin acessa THEN o sistema SHALL mostrar os números de **todos** os atendentes e categorias (visão gerencial completa)

**Independent Test**: Selecionar um mês com chamados conhecidos e conferir que os totais batem com uma contagem manual via API/banco.

---

### P1: Ver relatório do mês como Atendente (visão restrita) ⭐ MVP

**User Story**: Como Atendente, quero ver meu próprio resumo mensal (chamados que resolvi, tempo médio, SLA cumprido), pra acompanhar meu desempenho.

**Why P1**: Faz parte da decisão de acesso (Admin + Atendente) — sem isso, a tela não funciona corretamente pra esse perfil.

**Acceptance Criteria**:

1. WHEN um Atendente acessa a tela de Relatório Mensal THEN o sistema SHALL mostrar apenas os chamados onde ele é/foi o responsável (mesmo critério de "Meus Chamados" já usado no resto do app)
2. WHEN um Atendente acessa THEN o sistema SHALL **não** mostrar a quebra "por atendente" (não faz sentido comparar colegas nessa visão) nem os totais de outros atendentes
3. WHEN um Solicitante tenta acessar a URL do relatório THEN o sistema SHALL bloquear o acesso (RBAC de UI, mesmo padrão já usado no Dashboard/Kanban)

**Independent Test**: Logar como Fábio (Atendente), ver que os números batem só com os chamados dele; logar como Ana (Solicitante) e confirmar que a rota é bloqueada.

---

### P2: Exportar em PDF

**User Story**: Como Admin, quero exportar o relatório do mês em PDF, pra anexar num e-mail ou apresentação pra superintendência.

**Why P2**: É o objetivo final declarado pelo usuário, mas depende do P1 estar correto primeiro — construído em cima da tela, não em paralelo.

**Acceptance Criteria**:

1. WHEN o Admin clica em "Exportar PDF" THEN o sistema SHALL gerar um arquivo PDF com todas as métricas visíveis na tela naquele momento (mesmo mês selecionado)
2. WHEN o PDF é gerado THEN o sistema SHALL incluir cabeçalho com o nome do mês/ano e data de geração
3. WHEN a exportação falha (ex: erro de rede) THEN o sistema SHALL avisar o usuário, sem travar a tela

**Independent Test**: Gerar o PDF de um mês conhecido e abrir o arquivo, conferindo que os números batem com a tela.

---

### P3: Exportar em CSV

**User Story**: Como Admin, quero exportar os dados em CSV, pra jogar numa planilha própria se precisar de outro corte.

**Why P3**: Complementar ao PDF — útil, mas não bloqueia a entrega do valor principal (apresentar à superintendência).

**Acceptance Criteria**:

1. WHEN o Admin clica em "Exportar CSV" THEN o sistema SHALL gerar um arquivo com uma linha por métrica/quebra (categoria, atendente, etc.), legível em Excel/Sheets

**Independent Test**: Abrir o CSV exportado no Excel/Google Sheets e conferir que os números batem com a tela.

---

## Edge Cases

- WHEN o mês selecionado é o mês corrente (ainda não terminou) THEN o sistema SHALL deixar claro que os dados são parciais (ex: "até hoje, dia 14")
- WHEN não existe mês anterior com dados (ex: primeiro mês de uso do sistema) THEN o sistema SHALL omitir a comparação, não mostrar "+∞%" ou erro
- WHEN um chamado foi cancelado THEN o sistema SHALL contá-lo em "Cancelados", nunca em "Resolvidos" nem no cálculo de SLA
- WHEN um chamado foi reaberto (`Reabrir()`) depois de resolvido dentro do período THEN o sistema SHALL refletir o estado mais recente ao fim do período (não contar como "resolvido" duas vezes nem gerar inconsistência)
- WHEN um Atendente não teve nenhum chamado no mês THEN o sistema SHALL mostrar zerado, não ocultar a seção

---

### P1: Integridade dos dados (não-negociável) ⭐ MVP

**User Story**: Como Admin, preciso que os números do relatório (e do Dashboard que o alimenta) reflitam exatamente a data real de cada evento — quando o chamado foi de fato aberto, resolvido ou cancelado — nunca um status "atual" que possa mascarar o que aconteceu dentro do mês.

**Why P1**: Dito explicitamente pelo usuário como requisito crítico — um relatório pra superintendência com número errado é pior do que não ter relatório. Isso não é feature nova, é a condição de aceite de tudo o que já está descrito acima.

**Acceptance Criteria**:

1. WHEN um chamado é aberto, resolvido, fechado, cancelado ou reaberto THEN o sistema SHALL usar a data real de cada evento (`DataCriacao`, `DataConclusao`, ou o registro correspondente em `HistoricoEntrada`) para decidir em qual mês ele conta — nunca o status atual isolado
2. WHEN um chamado muda de estado depois que o mês fechou (ex: resolvido em julho, fechado em agosto) THEN o sistema SHALL manter o mês original do evento intacto nos relatórios já gerados daquele mês — o relatório de julho não muda retroativamente por causa de uma ação em agosto
3. WHEN os números do Dashboard operacional e os números do Relatório Mensal são comparados para o mesmo recorte (ex: "resolvidos este mês até hoje") THEN o sistema SHALL mostrar valores consistentes entre as duas telas — mesma fonte de verdade, sem cálculo duplicado ou divergente
4. WHEN qualquer métrica for implementada THEN o critério de contagem (qual campo de data, qual condição de status) SHALL ser documentado no Design e conferido manualmente contra o banco antes do Execute ser considerado concluído

**Independent Test**: Pegar 3-5 chamados com histórico conhecido (datas de abertura/resolução/cancelamento anotadas manualmente) e conferir, um por um, que aparecem no mês certo do relatório e no Dashboard sem divergência.

---

## Requirement Traceability

| Requirement ID | Story | Phase | Status |
|---|---|---|---|
| REL-01 | P1: Ver relatório (Admin) — seletor de mês + totais básicos | Design | Pending |
| REL-02 | P1: Ver relatório (Admin) — quebra por categoria | Design | Pending |
| REL-03 | P1: Ver relatório (Admin) — quebra por atendente | Design | Pending |
| REL-04 | P1: Ver relatório (Admin) — cumprimento de SLA | Design | Pending |
| REL-05 | P1: Ver relatório (Admin) — comparação com mês anterior | Design | Pending |
| REL-06 | P1: Ver relatório (Atendente) — visão restrita | Design | Pending |
| REL-07 | P1: RBAC — bloquear Solicitante | Design | Pending |
| REL-08 | P2: Exportar PDF | Design | Pending |
| REL-09 | P3: Exportar CSV | Design | Pending |
| REL-10 | P1: Integridade dos dados (não-negociável) | Design | Pending |

**Coverage:** 10 total, 0 mapped to tasks, 10 unmapped ⚠️ (aguardando Design)

---

## Notas técnicas (para a fase de Design)

- `Chamado.DataConclusao` é preenchido em `Resolver()`/`AlterarStatus(Resolvido)` e permanece preenchido em `Fechado` — ou seja, "Resolvidos no período" deve filtrar por `DataConclusao` dentro do range, não pelo `Status` atual (um chamado resolvido em julho e fechado em agosto ainda conta como "resolvido em julho")
- `Cancelar()` não seta `DataConclusao` — não há campo dedicado de "data de cancelamento" hoje; a Fase 6 introduziu `HistoricoEntrada` (com `AcaoHistorico.Cancelado` + `DataHora`), que pode ser a fonte mais confiável pra saber *quando* algo foi cancelado, em vez de `DataAtualizacao`
- SLA cumprido/estourado = comparar `DataConclusao` com `DataLimite`, só para chamados com `DataConclusao` preenchido dentro do período
- Reaproveitar o padrão de `ObterMetricasQueryHandler`/`ContarPorStatusAsync` do Dashboard (Fase 5) onde fizer sentido, mas este relatório precisa de filtragem por período — os métodos atuais do repositório não filtram por data, isso é novo
- **REL-10 é o requisito mais crítico da feature** — o Design precisa decidir explicitamente, por métrica, qual campo de data/fonte de verdade é usado (não deixar implícito), e o Execute precisa validar cada uma contra dados reais do banco antes de dar como concluída

---

## Success Criteria

- [ ] Todo número do relatório bate 1:1 com uma contagem manual feita direto no banco/API, para pelo menos 2 meses diferentes testados
- [ ] Dashboard operacional e Relatório Mensal nunca divergem quando comparados no mesmo recorte de tempo
- [ ] Um relatório de mês já fechado não muda de valor depois que ações acontecem em meses seguintes
- [ ] Admin consegue gerar e exportar (PDF e CSV) o relatório de um mês em menos de 1 minuto
- [ ] Atendente vê corretamente só os próprios números, nunca os de colegas

---

## Dependências

- Fase 5 ✅ (Dashboard — padrão de referência)
- Fase 6 ✅ T01-T08 (HistoricoEntrada, pode ser fonte de dados pra "quando" um evento aconteceu)

## Critérios de aceite

- Admin consegue ver o relatório de julho/2026 e os totais batem com os chamados reais no banco
- Atendente (Fábio) vê só os próprios números, sem a quebra por atendente
- Solicitante não consegue acessar a tela
- PDF exportado tem os mesmos números da tela
- CSV exportado abre corretamente no Excel/Sheets
