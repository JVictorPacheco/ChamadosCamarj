# Autorização de Chamados no Servidor — Especificação

> **SDD:** implementando
> **Status:** `Pendente`
> **Branch:** `feature/autorizacao-chamados`
> **Criada em:** 2026-09-29
> **Atualizada em:** 2026-09-29

---

## 1. Problema

**Situação atual:**
As regras de quem vê e quem faz o quê nos chamados (matriz em `docs/obsidian/00 Visão/Perfis e
Permissões.md`) são aplicadas quase só pelas telas. O sistema em si aceita qualquer pedido de
qualquer usuário logado. Quem abre as ferramentas do navegador consegue:
- ver **todos os chamados da empresa** na lista, basta não informar o próprio e-mail;
- ver detalhe, anexos e histórico de **qualquer chamado**, mesmo quando a tela diz "não pertence
  ao seu perfil" (os dados já chegaram ao navegador);
- **resolver, fechar, assumir, reatribuir ou mudar a prioridade** de qualquer chamado sendo
  Solicitante;
- abrir um chamado **em nome de outra pessoa**.

Além disso, há três incoerências de regra:
- quem é Solicitante e está num grupo vê na lista os chamados dos colegas do grupo, mas ao clicar
  recebe "não pertence ao seu perfil";
- a spec `grupos-equipes` e o Obsidian dizem que Solicitante vê só os que abriu, mas a regra
  decidida em 2026-08-01 (e confirmada pelo usuário em 2026-09-29) é que o grupo enxerga os
  chamados do grupo;
- um Admin que pertence a um grupo passa a ver só os chamados do grupo, e não todos.

**Impacto:**
Chamados podem conter dados sensíveis de outras áreas (reembolso, RH, financeiro). Qualquer
colaborador logado pode ler ou alterar chamados que não são dele. E o caso de uso que motivou os
grupos, **cobrir as férias de um colega da mesma equipe**, funciona pela metade.

**Solução esperada:**
O sistema passa a aplicar, ele mesmo, as regras de visibilidade e de ações por perfil e por
grupo, de forma igual em todas as telas, independente do que o navegador enviar.

---

## 2. Fora de Escopo

- Criar ou mudar perfis, grupos ou a tela de cadastro de grupos.
- Chat Corporativo (tem controle de acesso próprio).
- Relatório Mensal: o conteúdo e as quebras não mudam. Só passa a ser protegido no servidor
  (AC-21, incluído em 2026-10-01).
- Chamados abertos por e-mail (funcionalidade ainda não implementada).
- Mudar o que cada perfil pode fazer na matriz do Obsidian. Esta feature **aplica** a matriz,
  com um único acréscimo: o que o Solicitante pode fazer nos chamados do grupo (AC-07).
- Registrar em log as tentativas de acesso negado.

---

## 3. User Stories

| ID | User Story |
|----|------------|
| US-01 | Como **Solicitante**, quero que ninguém de fora veja ou altere os chamados que abri, para que as informações que coloco neles fiquem protegidas. |
| US-02 | Como **Solicitante de um grupo** (ex.: Reembolso), quero ver e acompanhar os chamados sob responsabilidade dos colegas do meu grupo, para que eu consiga cobrir quando alguém sai de férias. |
| US-03 | Como **Atendente**, quero ver só os chamados da fila, os meus e os da minha equipe, para que chamados de outros setores fiquem restritos a quem os trata. |
| US-04 | Como **Admin**, quero ver e gerenciar todos os chamados mesmo estando num grupo, para que eu continue administrando o sistema inteiro. |
| US-05 | Como **gestor da CAMARJ**, quero que as ações sobre um chamado sejam permitidas só a quem tem esse direito, para que o histórico e os status sejam confiáveis. |

---

## 4. Critérios de Aceitação

**Termos usados:**
- **Chamado do grupo:** chamado **aberto por** um membro do mesmo grupo do usuário **ou** cujo
  **responsável** é membro desse grupo. *(Revisado em 2026-10-01: a versão aprovada em 2026-09-29
  só considerava o responsável, o que não atendia o caso de uso. O analista de Reembolso que cobre
  as férias de um colega precisa ver os chamados que o colega **abriu**, e nesses o responsável é
  da equipe que atende, não do grupo Reembolso.)*
- **Fila:** chamados ainda sem responsável.
- **Pode ver o chamado:** o chamado aparece nas listas, abre no detalhe e libera anexos,
  histórico e comentários (os internos seguem a regra do AC-10).
- **Recusa:** o pedido é negado com a mensagem "Você não tem permissão para acessar este chamado."
  (ou para realizar a ação), e **nenhum dado do chamado é enviado**.

### US-01 / US-02 — Solicitante

- **AC-01:** Dado um Solicitante **sem grupo**, quando ele lista chamados, então vê só os que
  abriu, **mesmo que o pedido informe outro e-mail ou não informe nenhum**.
- **AC-02:** Dado um Solicitante **com grupo**, quando ele lista chamados, então vê os que abriu
  mais os chamados do grupo, e nenhum outro.
- **AC-03:** Dado um Solicitante, quando ele pede o detalhe, os anexos, o link de download de um
  anexo, o histórico ou os comentários de um chamado que não pode ver (AC-01/AC-02), então recebe
  a recusa.
- **AC-04:** Dado um Solicitante com grupo, quando ele abre o detalhe de um chamado do grupo,
  então a tela mostra o chamado normalmente, sem a mensagem "não pertence ao seu perfil".
- **AC-05:** Dado um Solicitante, quando ele tenta assumir, resolver, encerrar, reabrir, reatribuir,
  mudar a prioridade, mudar o status ou forçar o encerramento de **qualquer** chamado, então recebe
  a recusa e o chamado não muda.
- **AC-06:** Dado um Solicitante, quando ele tenta cancelar um chamado, então só consegue se foi
  **ele quem o abriu** e o chamado estiver Aberto ou Em andamento. Chamado do grupo que não abriu
  → recusa.
- **AC-07:** Dado um Solicitante com grupo, quando ele comenta (comentário público) ou anexa um
  arquivo num chamado do grupo, então a ação é aceita, como no chamado que ele mesmo abriu.

### US-03 — Atendente

- **AC-08:** Dado um Atendente (com ou sem grupo), quando ele lista chamados ou abre qualquer um
  deles (detalhe, anexos, histórico, comentários), então só tem acesso à fila, aos chamados em que
  é o responsável, aos chamados do grupo (se tiver grupo) e aos que ele mesmo abriu. Qualquer outro
  → recusa.
- **AC-09:** Dado um Atendente, quando ele tenta reatribuir, mudar a prioridade ou forçar o
  encerramento, então recebe a recusa (ações exclusivas do Admin). Assumir, resolver, encerrar,
  cancelar e reabrir continuam permitidos nos chamados que ele pode ver (AC-08).

### Comentários internos

- **AC-10:** Dado um Solicitante, quando ele lista comentários, então nunca recebe os internos; e
  quando tenta criar um comentário interno, recebe a recusa. Atendente e Admin continuam vendo e
  criando comentários internos nos chamados que podem ver.

### US-04 — Admin

- **AC-11:** Dado um Admin, **com ou sem grupo**, quando ele lista, abre ou age sobre chamados,
  então tem acesso a todos, como hoje.

### US-05 — Abertura e consistência entre telas

- **AC-12:** Dado qualquer usuário, quando ele abre um chamado, então o solicitante registrado é
  **sempre o usuário logado** (nome e e-mail), independente do que o pedido informar.
- **AC-13:** Dado qualquer perfil, quando ele usa Meus Chamados, Arquivo, Fila, Kanban, Dashboard
  ou a busca por número, então só aparecem chamados que ele pode ver (AC-01/02/08/11), e os
  números do Dashboard contam só esses chamados.
- **AC-14:** Dado qualquer usuário usando as telas normalmente, quando ele faz o que a sua
  permissão já permitia antes desta feature, então nada muda para ele. O que muda é só o que era
  indevido.

### Incluídos na fase de design (aprovados pelo usuário em 2026-10-01)

- **AC-19:** Dado qualquer usuário conectado, quando alguém comenta, abre um chamado ou um chamado
  entra em alerta de SLA, então o aviso em tempo real que ele recebe **não traz** o texto do
  comentário nem o título do chamado. Só identifica o chamado; o conteúdo é buscado de novo
  respeitando as regras desta spec. Assim um Solicitante nunca recebe comentário interno nem dados
  de chamado que não pode ver.
- **AC-20:** Dado um Solicitante, quando ele tenta editar o título ou a descrição de um chamado
  (inclusive um que abriu), então recebe a recusa. Atendente e Admin podem, nos chamados que
  conseguem ver. O Solicitante complementa por comentário.
- **AC-21:** Dado o Relatório Mensal, quando um Solicitante o pede, então recebe a recusa; quando um
  Atendente o pede, então recebe só os próprios números, qualquer que seja o atendente informado
  no pedido. O Admin continua vendo o relatório completo.

### Critérios Transversais

- **AC-15:** Recusas retornam no formato `{ message: "..." }` em português, sem nenhum dado do
  chamado.
- **AC-16:** Pedir um chamado que existe mas o usuário não pode ver não pode revelar que ele
  existe (a resposta não distingue "sem permissão" de "não encontrado").
  **Aprovado pelo usuário em 2026-10-01:** tratar os dois como "não encontrado" no detalhe e nos sub-recursos; as ações
  respondem "sem permissão".
- **AC-17:** `dotnet test` passa sem falhas, com testes cobrindo cada perfil (Solicitante sem
  grupo, Solicitante com grupo, Atendente sem grupo, Atendente com grupo, Admin com grupo).
- **AC-18:** `npm run build` passa sem erros.

---

## 5. Rastreabilidade

> Preencher após a implementação.

| Critério | Arquivo de Teste | Método de Teste | Status |
|----------|-----------------|-----------------|--------|
| AC-01 a AC-18 | a definir no `tasks.md` | — | ⬜ Pendente |

---

## 6. Decisões

| Decisão | Alternativa considerada | Quem decidiu / quando |
|---------|------------------------|-------------------|
| Solicitante com grupo vê os chamados do grupo (confirma a regra de 2026-08-01) | Só os que abriu (spec `grupos-equipes`, T6) | Usuário, 2026-09-29. Caso de uso: cobrir as férias de um colega da equipe. |
| "Chamado do grupo" = aberto por membro do grupo **ou** responsável membro do grupo | Só o responsável (versão de 2026-09-29) | Revisado em 2026-10-01, a partir da explicação do usuário: o grupo existe para que os colegas vejam os chamados que **abriram**. A feature seguinte (`area-e-tipo-do-chamado`) acrescenta "área do chamado = grupo do usuário". |
| Nos chamados do grupo, o Solicitante vê, comenta e anexa, mas não cancela | Igual ao dono; só visualizar | Usuário, 2026-09-29 |
| Escopo inclui as ações, não só a leitura | Só leitura agora | Usuário, 2026-09-29 |
| Atendente acessa só o que vê na lista (fila + seus + grupo + os que abriu) | Qualquer chamado pelo detalhe | Usuário, 2026-09-29. Inclusão de "os que abriu" aprovada em 2026-10-01. |
| Solicitante da abertura = sempre o usuário logado | Admin abrir em nome de outro | Usuário, 2026-09-29 |

---

## 7. Dependências

- Depende de: `grupos-equipes` (grupos e vínculo usuário → grupo), `auth-email-senha` (perfil e
  grupo do usuário logado).
- Corrige: spec `grupos-equipes` (item 5 e T6 contradizem a regra vigente), notas do Obsidian
  `Perfis e Permissões` e `Grupos e Equipes`.

---

## 8. Gate Checks

- [ ] `dotnet build` — 0 erros
- [ ] `dotnet test` — X testes, 0 falhas
- [ ] `npm run build` — 0 erros
- [ ] ACs verificados por testes automatizados ou manualmente
- [ ] `spec.md` atualizada com status final
- [ ] `STATE.md`, `ROADMAP.md` e Obsidian atualizados
- [ ] PR aberto com base `develop`
