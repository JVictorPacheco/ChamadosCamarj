---
tipo: decisão
status: vigente
decisao: aceita
data: 2026-10-02
atualizado: 2026-10-02
tags: [decisão, chamado, equipes]
---

# ADR-008 — Área do chamado é a equipe

## Contexto
O campo "Categoria" da abertura listava **áreas da empresa** (Reembolso, Financeiro...), e não o
tipo do pedido. Por isso não dava para saber quanto do volume era incidente, dúvida ou melhoria.
Além disso, a lista de categorias repetia quase igual a lista de equipes, cadastrada em separado.

## Decisão
- O chamado passa a ter **Área** (de onde é aberto) e **Tipo** (natureza do pedido).
- **A lista de áreas é a lista de equipes.** Quem é da equipe Reembolso enxerga os chamados com área
  Reembolso. Ver [[Perfis e Permissões]].
- Tipos: Incidente, Dúvida, Solicitação, Customização e Melhoria, mantidos pelo Admin.
- Os chamados existentes ficaram com a área equivalente à antiga categoria e o tipo
  "Não classificado". Nenhuma informação foi apagada.

## Consequências
- ✅ Indicadores por tipo e por área no Dashboard e no Relatório.
- ✅ Uma lista só para manter; "Reembolso" significa a mesma coisa em todo lugar.
- ✅ Visibilidade natural: a equipe vê os chamados da sua área.
- ⚠️ Os chamados antigos precisam ter o tipo corrigido aos poucos (o Atendente faz no detalhe).
- ⚠️ A tabela antiga de categorias ainda existe no banco, sem uso; será removida numa limpeza
  futura, depois de validada a mudança em produção.

Ver [[Abertura de Chamados]] e [[Grupos e Equipes]].
