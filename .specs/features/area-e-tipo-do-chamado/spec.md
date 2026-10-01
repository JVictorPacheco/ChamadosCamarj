# Área e Tipo do Chamado — Especificação

> **SDD:** spec-aprovada
> **Status:** `Pendente`
> **Branch:** `feature/area-e-tipo-do-chamado`
> **Criada em:** 2026-10-01
> **Atualizada em:** 2026-10-01
> **Depende de:** `autorizacao-chamados` (implementar depois dela)

---

## 1. Problema

**Situação atual:**
O campo "Categoria" da abertura de chamado lista **áreas da empresa** (Reembolso, Financeiro,
Comercial...), e não o tipo do pedido. Por isso o sistema não sabe distinguir um incidente de uma
dúvida ou de um pedido de melhoria. Além disso, a lista de categorias repete quase igual a lista
de grupos (equipes), e as duas são cadastradas em separado. E, a cada reinício do sistema, as
categorias padrão voltam ao nome original, desfazendo o que o Admin editou.

**Impacto:**
- Não dá para medir quanto do volume é incidente, dúvida ou melhoria, o que é a pergunta mais
  importante para priorizar o trabalho.
- O nome "Categoria" confunde quem abre o chamado.
- O Admin não consegue manter as próprias listas.

**Solução esperada:**
Separar as duas informações. **Área** é a área de onde o chamado é aberto. **Tipo** é a natureza
do pedido. As duas listas podem ser mantidas pelo Admin.

---

## 2. Fora de Escopo

- SLA diferente por tipo ou por área: o SLA continua dependendo só da prioridade.
- Encaminhamento automático para um atendente conforme o tipo.
- Mudar a regra de quem pode ver ou fazer o quê, exceto o acréscimo do AC-07.
- Abertura de chamado por e-mail.

---

## 3. User Stories

| ID | User Story |
|----|------------|
| US-01 | Como **Solicitante**, quero informar o tipo do meu chamado (incidente, dúvida...), para que ele seja tratado da forma certa. |
| US-02 | Como **Solicitante**, quero que a área do chamado já venha preenchida com a minha, para não precisar escolher toda vez. |
| US-03 | Como **membro de um grupo**, quero ver os chamados abertos para a minha área, para acompanhar tudo o que é da minha equipe. |
| US-04 | Como **Admin**, quero cadastrar e editar áreas e tipos, e que as minhas edições não se percam, para manter as listas atualizadas sem depender de TI. |
| US-05 | Como **gestor**, quero ver no Dashboard e no Relatório os chamados por tipo e por área, para entender de onde vem a demanda e de que natureza ela é. |

---

## 4. Critérios de Aceitação

### US-01 / US-02 — Abertura

- **AC-01:** Dado um usuário abrindo um chamado, quando ele vê o formulário, então há dois campos
  obrigatórios: **Área** e **Tipo**. Não existe mais o campo "Categoria".
- **AC-02:** Dado um usuário que pertence a um grupo, quando ele abre o formulário, então a Área já
  vem preenchida com a do seu grupo, e ele pode trocar se precisar.
- **AC-03:** Dado o campo Tipo, quando o formulário abre, então as opções são os tipos ativos. A lista
  inicial é: Incidente, Dúvida, Solicitação, Customização, Melhoria.
- **AC-04:** Dado o botão "Sugerir" da triagem automática, quando o usuário o usa, então o sistema
  sugere Área **e** Tipo a partir do título e da descrição (ex.: "erro", "não funciona" → Incidente;
  "como faço" → Dúvida).

### US-03 — Visibilidade pela área

- **AC-05:** Área e grupo passam a ser **a mesma lista**. Cada área é um grupo, e um grupo pode não
  ter membros (ex.: Financeiro, Super e Tendência).
- **AC-06:** Os chamados existentes ficam com a área equivalente à "categoria" que tinham. Nenhum
  chamado perde informação.
- **AC-07:** Dado um membro do grupo X, quando ele lista ou abre chamados, então, além do que já pode
  ver pela feature `autorizacao-chamados`, também vê os chamados cuja **área é X**. As mesmas
  permissões de ação valem: o Solicitante vê, comenta e anexa, mas não cancela o que não abriu.

### US-04 — Administração

- **AC-08:** Dado o Admin, quando ele acessa a tela de cadastro, então consegue criar, renomear,
  desativar e reativar **Tipos**. Um tipo desativado some da abertura, mas continua aparecendo nos
  chamados antigos.
- **AC-09:** Dado o Admin, quando ele cria ou edita uma **Área**, então isso acontece na tela de
  Grupos, que passa a se chamar "Áreas e Grupos". Não existe mais tela separada de "Categorias".
- **AC-10:** Dado que o Admin renomeou ou desativou uma área ou um tipo, quando o sistema reinicia,
  então a edição dele permanece. As listas iniciais só são criadas se não existirem.
- **AC-11:** Dado um chamado aberto antes desta mudança, quando ele é exibido, então o Tipo aparece
  como **"Não classificado"**. O Atendente ou o Admin pode reclassificar o tipo no detalhe do chamado.

### US-05 — Indicadores

- **AC-12:** Dado o Dashboard, quando ele carrega, então mostra chamados **por tipo** e **por área**
  (o gráfico "por categoria" vira "por área").
- **AC-13:** Dado o Relatório Mensal e a exportação dele, quando gerados, então trazem as quebras por
  tipo e por área.
- **AC-14:** Dado o filtro da lista de chamados, quando o usuário filtra, então pode filtrar por
  Área e por Tipo.

### Critérios Transversais

- **AC-15:** Em todas as telas, a palavra "Categoria" deixa de aparecer para os usuários.
- **AC-16:** `dotnet test` e `npm run build` passam sem falhas.

---

## 5. Rastreabilidade

| Critério | Arquivo de Teste | Método de Teste | Status |
|----------|-----------------|-----------------|--------|
| AC-01 a AC-16 | a definir no `tasks.md` | — | ⬜ Pendente |

---

## 6. Decisões

| Decisão | Alternativa considerada | Quem decidiu / quando |
|---------|------------------------|-------------------|
| O campo se chama **Área** | Departamento; Área de abertura do chamado | Usuário, 2026-10-01 |
| Tipos iniciais: Incidente, Dúvida, Solicitação, Customização, Melhoria, configuráveis pelo Admin | Lista fixa | Usuário, 2026-10-01 |
| Área = de onde o chamado é aberto; grupo = quem enxerga os chamados daquela área. As duas usam **a mesma lista** | Duas listas separadas (Categoria e Grupo) | Usuário explicou a diferença e deixou a recomendação comigo, 2026-10-01. Unificar evita manter duas listas quase iguais e faz "Reembolso" significar a mesma coisa nos dois lugares. |
| Área vem preenchida com o grupo de quem abre, mas pode ser trocada | Área travada no grupo do usuário | Usuário, 2026-10-01 |
| Chamados antigos: Tipo "Não classificado", reclassificável pelo Atendente ou pelo Admin | Obrigar a reclassificar tudo | Usuário, 2026-10-01 |

---

## 7. Dependências

- Depende de: `autorizacao-chamados` (a regra de visibilidade fica concentrada lá; o AC-07 desta
  feature só acrescenta um critério).
- Afeta: `grupos-equipes`, `relatorio-mensal`, `fase-5-kanban-dashboard` (gráficos), triagem
  automática, Obsidian (`Abertura de Chamados`, `Grupos e Equipes`, `Perfis e Permissões`,
  `Fila, Kanban e Dashboard`, `Relatório Mensal`).

---

## 8. Gate Checks

- [ ] `dotnet build` — 0 erros
- [ ] `dotnet test` — X testes, 0 falhas
- [ ] `npm run build` — 0 erros
- [ ] ACs verificados
- [ ] `STATE.md`, `ROADMAP.md` e Obsidian atualizados
- [ ] PR aberto com base `develop`
