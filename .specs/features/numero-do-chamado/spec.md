# Spec — Número do Chamado

> Status: IMPLEMENTADA (2026-07-19) — migration aplicada e verificada contra o Supabase real
> Criado em: 2026-07-19
> Pedido pelo usuário: chamados hoje só têm o `Guid` interno como identificador — sem número curto pra referenciar em conversa, e-mail ou telefone.

## Problem Statement

`Chamado.Id` é um `Guid` — impossível de referenciar de cabeça numa ligação ou num e-mail ("o chamado a1b2c3d4-..."). Sistemas de chamado reais (Jira, Zendesk) sempre têm um número curto e sequencial, separado do identificador técnico interno.

## Decisões confirmadas com o usuário (2026-07-19)

| Decisão | Escolha |
|---|---|
| Formato de exibição | `CAM-{número}`, **sem zero-padding** (`CAM-1`, `CAM-42`, `CAM-1500`) — cresce naturalmente, sem parecer "inflado" pro volume real da empresa |
| Geração | Sequencial, gerado no momento da criação do chamado |
| Identificador técnico (`Guid Id`) | Não muda — `Numero` é só um campo novo, adicional, só pra exibição/referência humana |

## Out of Scope (nesta primeira versão — "o simples")

| Item | Motivo |
|---|---|
| Buscar/filtrar chamados por número | Não pedido agora; adicionar depois se sentir falta |
| Reiniciar a numeração por ano (`CAM-2026-42`) | Formato mais simples escolhido explicitamente pelo usuário |
| Expor o número na integração de e-mail (Fase 4) | Fase 4 ainda não implementada |

## User Stories

### P1: Todo chamado (novo e já existente) tem um número sequencial ⭐ MVP

**User Story**: Como qualquer perfil, quero ver um número curto (`CAM-42`) em vez do Guid, pra referenciar o chamado facilmente em conversa.

**Acceptance Criteria**:
1. WHEN um chamado novo é aberto THEN o sistema SHALL atribuir automaticamente o próximo número sequencial disponível
2. WHEN dois chamados são abertos ao mesmo tempo (concorrência) THEN o sistema SHALL garantir que nenhum dos dois recebe o mesmo número (gerado pelo banco via sequence, não pela aplicação)
3. WHEN a migration roda THEN todo chamado **já existente** SHALL receber um número, atribuído em ordem cronológica de `DataCriacao`
4. WHEN o número é exibido (lista, card, Kanban, Detalhe) THEN o sistema SHALL mostrar no formato `CAM-{número}`, sem zero-padding

**Independent Test**: Rodar a migration num banco com chamados existentes, confirmar que todos ganham número em ordem cronológica sem duplicar; abrir um chamado novo e confirmar que o número continua a sequência (não reinicia do 1).

## Requirement Traceability

| Requirement ID | Story | Status |
|---|---|---|
| NUM-01 | Sequence do Postgres gera o número, não a aplicação | Verified |
| NUM-02 | Migration faz backfill cronológico dos chamados existentes | Verified |
| NUM-03 | Exibição em `CAM-{número}` sem padding, na lista/card/Kanban/Detalhe | Verified |

**Coverage:** 3 total, 3 verificados contra o Supabase real.

## Verificação (2026-07-19)

- Migration `AddNumeroChamado` aplicada com sucesso no restart da API (auto-apply já existente no `Program.cs`)
- **Backfill**: 37 chamados existentes, todos com `Numero` único (1 a 37), em ordem cronológica exata (`dataCriacao` crescente bate 1:1 com `numero` crescente)
- **Sequência nova**: chamado criado depois da migration recebeu `numero: 38` — continua exatamente de onde o backfill parou, sem reiniciar
- 197 testes de backend continuam passando (nenhum teste novo dedicado — `Numero` é gerado pelo banco, sem lógica de aplicação pra unitário cobrir; a correção real foi verificada via curl contra dados reais)
- `npm run build` limpo — `CAM-{numero}` aparece em `ChamadoCard` (cobre Lista, Arquivo, Fila e Kanban, que reaproveita o mesmo card) e no cabeçalho do `ChamadoDetailPage`
