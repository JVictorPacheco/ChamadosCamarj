# Editar Chamado — Especificação

> **SDD:** spec-aprovada
> **Status:** `Pendente`
> **Branch:** `feature/editar-chamado`
> **Criada em:** 2026-10-02
> **Atualizada em:** 2026-10-02
> **Origem:** ROADMAP "✏️ Editar Chamado" e AC-11 adiado de `correcoes-pre-deploy`. Regras definidas
> pelo usuário em 2026-10-02.
> **Aprovação:** spec aprovada pelo usuário em 2026-10-02.

---

## 1. Problema

**Situação atual:**
- Não existe na tela nenhuma forma de corrigir o título ou a descrição de um chamado. Quem escreveu
  errado precisa complementar por comentário, e o título errado continua na lista, no Kanban e nos
  relatórios.
- A ação de editar existe só por trás da tela, com uma regra que não serve ao negócio: qualquer
  Atendente que vê o chamado pode editar, quem abriu não pode, e é possível editar até chamado
  encerrado.
- Quando alguém edita, nada fica registrado no histórico. Não dá para saber o que estava escrito
  antes nem quem mudou.

**Impacto:** pedidos mal descritos atrasam o atendimento, e alterações sem rastro enfraquecem a
auditoria do chamado.

**Solução esperada:** um botão "Editar" no detalhe do chamado abre um modal para corrigir título e
descrição, liberado só para quem deve editar e com registro do antes e depois no histórico.

---

## 2. Fora de Escopo

- Editar área, tipo, prioridade, responsável ou status pelo modal. Essas mudanças continuam nas ações
  que já existem, com as permissões de hoje.
- Editar comentários ou anexos.
- Notificar alguém quando o chamado é editado. Só o histórico registra (decisão de 2026-10-02).
- Editar chamado encerrado (Resolvido, Fechado ou Cancelado), inclusive pelo Admin. Para isso, é
  preciso reabrir o chamado antes.

---

## 3. User Stories

| ID | User Story |
|----|------------|
| US-01 | Como **quem abriu o chamado** (qualquer perfil), quero corrigir título e descrição enquanto ninguém assumiu, para que o atendimento comece com o pedido certo. |
| US-02 | Como **Atendente responsável** pelo chamado, quero ajustar título e descrição do que eu assumi, para que o registro reflita o que de fato está sendo tratado. |
| US-03 | Como **Admin**, quero editar título e descrição de qualquer chamado em aberto, para corrigir registros sem depender de quem abriu. |
| US-04 | Como **qualquer pessoa que vê o chamado**, quero ver no histórico quem editou, quando, e o texto de antes e de depois, para que nenhuma alteração passe despercebida. |
| US-05 | Como **quem está editando**, quero não perder o que digitei se outra pessoa alterar o chamado ao mesmo tempo, para conferir e salvar de novo sem reescrever tudo. |

---

## 4. Critérios de Aceitação

### Regra de quem pode editar (vale para a tela e para o servidor)

A pessoa pode editar o chamado quando ele **não está encerrado** (Resolvido, Fechado ou Cancelado)
**e** pelo menos uma destas condições vale:
- ela é **Admin**;
- ela é o **responsável atual** do chamado (assumiu ou recebeu por reatribuição);
- ela **abriu** o chamado **e ninguém o assumiu ainda** (está sem responsável).

### US-01 — Quem abriu edita enquanto ninguém assumiu

- **AC-01:** Dado um chamado Aberto, sem responsável, aberto por um Solicitante, quando esse
  Solicitante abre o detalhe, então vê o botão "Editar" e consegue salvar novo título e descrição.
- **AC-02:** Dado o mesmo chamado depois que um Atendente o assumiu, quando o Solicitante que o abriu
  abre o detalhe, então o botão "Editar" não aparece. Se ele tentar mesmo assim (por fora da tela), o
  sistema recusa com "Você não tem permissão para realizar esta ação neste chamado."
- **AC-03:** Dado um chamado aberto por um **Atendente** para ele mesmo e ainda sem responsável,
  quando esse Atendente abre o detalhe, então pode editar, como qualquer pessoa que abriu.
- **AC-04:** Dado um Solicitante do mesmo grupo que vê o chamado do colega, quando abre o detalhe,
  então não vê o botão "Editar". Só quem abriu edita.

### US-02 — Responsável edita o que assumiu

- **AC-05:** Dado um chamado Em andamento cujo responsável é o Atendente A, quando A abre o detalhe,
  então vê "Editar" e consegue salvar.
- **AC-06:** Dado o mesmo chamado, quando outro Atendente B, que também vê o chamado (por exemplo,
  da mesma equipe), abre o detalhe, então não vê "Editar". Se tentar por fora da tela, o sistema
  recusa.
- **AC-07:** Dado um chamado aberto pelo Atendente A que ele mesmo assumiu, quando A abre o detalhe,
  então continua podendo editar, agora como responsável.
- **AC-08:** Dado um chamado reatribuído pelo Admin do Atendente A para o Atendente B, quando cada um
  abre o detalhe, então só B (o responsável atual) pode editar.

### US-03 — Admin edita todos os chamados em aberto

- **AC-09:** Dado qualquer chamado não encerrado, com ou sem responsável, quando um Admin (com ou
  sem equipe) abre o detalhe, então vê "Editar" e consegue salvar.

### Chamado encerrado

- **AC-10:** Dado um chamado Resolvido, Fechado ou Cancelado, quando qualquer pessoa, inclusive o
  Admin, abre o detalhe, então não vê "Editar". Uma tentativa por fora da tela é recusada com "Não é
  possível editar um chamado encerrado. Reabra o chamado para editá-lo."
- **AC-11:** Dado um chamado encerrado que foi **reaberto** (fica Em andamento e sem responsável),
  quando quem o abriu, ou um Admin, abre o detalhe, então volta a poder editar pelas regras acima.

### O modal

- **AC-12:** Dado quem pode editar, quando clica em "Editar", então abre um modal com título e
  descrição preenchidos com o texto atual, e os botões "Salvar" e "Cancelar".
- **AC-13:** Dado o modal aberto, quando a pessoa deixa título ou descrição em branco, ou passa do
  limite de tamanho já usado na abertura do chamado, então o "Salvar" mostra o erro no campo e nada é
  gravado.
- **AC-14:** Dado o modal aberto, quando a pessoa clica em "Cancelar" ou fecha o modal, então nada é
  gravado, e na próxima vez o modal abre de novo com o texto atual do chamado.
- **AC-15:** Dado que a pessoa salvou, quando a gravação termina, então o modal fecha e o detalhe, a
  lista e o Kanban mostram o título e a descrição novos.
- **AC-16:** Dado o modal aberto, quando a pessoa salva **sem mudar nada**, então o modal fecha sem
  gravar e sem criar entrada no histórico.

### US-04 — Histórico

- **AC-17:** Dado que alguém salvou uma edição, quando qualquer pessoa que vê o chamado abre o
  histórico, então aparece uma entrada "Chamado editado" com quem editou, quando, e o que mudou:
  título anterior e novo e/ou descrição anterior e nova (só o que de fato mudou).
- **AC-18:** Dado que só a descrição mudou, quando o histórico é exibido, então a entrada mostra só a
  descrição (anterior e nova), sem repetir o título.

### US-05 — Edição simultânea (AC-11 de `correcoes-pre-deploy`)

- **AC-19:** Dado que A abriu o modal e, depois disso, B alterou o chamado (qualquer ação), quando A
  clica em "Salvar", então a gravação é recusada com "Outra pessoa alterou este chamado. Os dados
  foram atualizados; confira e refaça a ação.", **o modal continua aberto com o texto que A
  digitou**, e o detalhe por trás é atualizado.
- **AC-20:** Dado o caso do AC-19, quando A confere e clica em "Salvar" de novo, então a gravação é
  feita, desde que A continue podendo editar (por exemplo, se nesse meio tempo B assumiu o chamado
  de um Solicitante, a nova tentativa do Solicitante é recusada por permissão).
- **AC-21:** Dado que B só comentou ou anexou um arquivo enquanto A editava, quando A salva, então a
  gravação não é recusada (comentário e anexo não contam como alteração, como hoje).

### Critérios Transversais

- **AC-22:** A regra de quem pode editar vale no servidor (não só esconder o botão). Quem não vê o
  chamado continua recebendo "não encontrado", como hoje.
- **AC-23:** Erros retornam no formato `{ message: "..." }` em português.
- **AC-24:** `dotnet test` passa sem falhas, com teste automatizado para cada regra de permissão
  (AC-01 a AC-11) e para o histórico (AC-16 a AC-18).
- **AC-25:** `npm run build` passa sem erros, e os testes E2E cobrem pelo menos: editar como quem
  abriu, botão ausente para quem não pode, e o conflito mantendo o texto no modal (AC-19).

---

## 5. Rastreabilidade

| Critério | Arquivo de Teste | Método de Teste | Status |
|----------|-----------------|-----------------|--------|
| AC-01..AC-25 | a definir no design/tasks | — | ⬜ Pendente |

---

## 6. Decisões (2026-10-02, com o usuário)

| Decisão | Alternativa considerada | Motivo da escolha |
|---------|------------------------|-------------------|
| Modal no detalhe, não tela nova | Tela de edição | Correção pontual, sem sair do chamado |
| Quem abriu edita **enquanto ninguém assumiu** (vale para qualquer perfil) | Só o Solicitante; ou o Atendente só o que assumiu, mesmo que tenha aberto | Quem abriu conhece o pedido; depois de assumido, o pedido não muda por baixo de quem atende |
| Responsável atual edita o que assumiu; outros Atendentes não | Qualquer Atendente que vê (regra de hoje) | Evita que colegas mudem o chamado de quem está atendendo |
| Admin edita todos os não encerrados | Admin edita inclusive encerrados | O registro encerrado fica preservado; para mudar, reabre |
| Encerrado não se edita; reaberto volta às regras | Bloquear para sempre | Reabrir é o caminho natural para corrigir |
| Só título e descrição | Incluir área | Área muda quem vê o chamado; tem ação e regra próprias |
| Histórico com antes e depois, sem notificação | Avisar o responsável em tempo real | Rastreabilidade sem ruído |
| Conflito mantém o texto digitado no modal | Fechar o modal | Não perder o que a pessoa escreveu |

---

## 7. Dependências

- Depende de: `correcoes-pre-deploy` (versão do chamado e checagem de conflito) e
  `autorizacao-chamados` (regra de visibilidade e matriz de permissões), ambas em `main`.
- Muda a regra da ação "Editar" definida em `autorizacao-chamados` (AC-20 daquela spec: hoje
  "Solicitante não edita; Atendente que vê edita").

---

## 8. Gate Checks

- [ ] `dotnet build` — 0 erros
- [ ] `dotnet test` — X testes, 0 falhas
- [ ] `npm run build` — 0 erros
- [ ] E2E Playwright — passando
- [ ] ACs verificados por testes automatizados ou ao vivo
- [ ] Obsidian atualizado (regra 6): Acompanhamento do Chamado, Perfis e Permissões
- [ ] `spec.md` com status final; `STATE.md` e `ROADMAP.md` atualizados
- [ ] PR aberto com base `develop`
