---
tipo: decisão
status: vigente
decisao: aceita
data: 2026-10-01
atualizado: 2026-10-01
tags: [decisão, segurança, permissões]
---

# ADR-007 — Permissões aplicadas no servidor

## Contexto
As regras de quem vê e quem faz o quê nos chamados eram aplicadas **pelas telas**: botões
escondidos, listas filtradas pelo navegador. O sistema em si aceitava qualquer pedido de qualquer
usuário logado. Bastava usar as ferramentas do navegador para ver todos os chamados da empresa,
resolver ou cancelar chamados de outras pessoas ou abrir chamados em nome de outra pessoa. Avisos
em tempo real também levavam o texto de comentários, inclusive internos, para todos os conectados.

## Decisão
O **sistema** decide, em todo pedido, o que cada pessoa pode ver e fazer, usando só a identidade
de quem está logado. Nada que venha do navegador amplia esse acesso.
- **Uma regra só** de visibilidade, usada igual por listas, detalhe, busca, Kanban, Fila e Dashboard.
- Chamado que a pessoa não pode ver responde **"não encontrado"**, para não revelar que existe.
  Chamado que ela vê, mas ação que não pode fazer, responde **"acesso negado"**.
- Avisos em tempo real dizem só **qual** chamado mudou, sem conteúdo nem título; a tela busca os
  dados de novo, já respeitando as permissões.

## Consequências
- ✅ Dados de um setor deixam de ser acessíveis a qualquer colaborador logado.
- ✅ Histórico e status confiáveis: só quem tem o direito altera um chamado.
- ✅ Regra num lugar só: uma tela nova não precisa reimplementar permissões.
- ⚠️ O Atendente sem equipe deixou de ver todos os chamados no Kanban; passou a ver só o que
  lhe cabe (fila, os seus e os que abriu).

Ver [[Perfis e Permissões]] e [[Grupos e Equipes]].
