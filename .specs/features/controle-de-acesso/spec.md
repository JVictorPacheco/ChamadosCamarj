# Controle de Acesso por Módulo — Especificação

> **SDD:** em-review
> **Status:** `Pendente`
> **Branch:** `feature/controle-de-acesso`
> **Criada em:** 2026-10-02
> **Atualizada em:** 2026-10-02
> **Origem:** pedido do usuário em 2026-10-02: o Admin poder dar e tirar acesso a módulos por pessoa,
> com os perfis atuando como padrão.
> **Aprovação:** spec aprovada pelo usuário em 2026-10-02, com o AC-15 conforme a proposta.

---

## 1. Problema

**Situação atual:** o que cada pessoa vê no sistema depende só do perfil:

| Módulo | Solicitante | Atendente | Admin |
|---|---|---|---|
| Abrir chamado, Meus chamados | ✅ | ✅ | ✅ |
| Arquivo | ✅ | ✅ | ✅ |
| Kanban, Fila | — | ✅ | ✅ |
| Dashboard, Relatório mensal | — | ✅ | ✅ |
| Chat | conforme acesso ao chat, ajustado pelo Admin em Usuários (sem acesso / participante / pode criar grupos) | idem | idem |
| Usuários, Tipos de chamado, Áreas e Grupos | — | — | ✅ |

Não há como, por exemplo, tirar o Relatório mensal de um Atendente que não precisa dele, nem dar o
Dashboard a um Solicitante que acompanha os números da área. O acesso ao Chat é ajustado numa tela, e
nada mais é ajustável.

**Impacto:** pessoas veem módulos de que não precisam, ou não têm módulos de que precisariam, e a
única saída é trocar o perfil inteiro.

**Solução esperada:** uma tela de **Controle de acesso**, só do Admin, onde cada pessoa herda o padrão
do perfil e o Admin liga ou desliga módulos para ela. O acesso ao Chat passa a ser ajustado nessa
mesma tela.

---

## 2. Fora de Escopo

- **Restringir Admins.** Admin tem sempre acesso total e não aparece como ajustável (decisão de
  2026-10-02).
- **Mudar o padrão de um perfil inteiro.** O ajuste é por pessoa; o padrão de cada perfil é fixo.
- **Controle de ações dentro dos módulos** (ex.: comentário interno, cancelar, anexar). Continuam
  regidas pelo perfil e pelas regras de hoje. O controle é do módulo inteiro (decisão de 2026-10-02).
- **Remover "Abrir chamado" e "Meus chamados".** São fixos para todo usuário ativo. Para tirar todo
  acesso, o Admin desativa a conta, como já existe.
- **Dar módulos de atendimento (Kanban, Fila) a Solicitante.** Quem precisa atender passa a ser
  Atendente, trocando o perfil em Usuários, como já existe.
- **Mudar quem vê qual chamado.** Os módulos mostram só os chamados que a pessoa já pode ver, pelas
  regras de visibilidade atuais.

---

## 3. User Stories

| ID | User Story |
|----|------------|
| US-01 | Como **Admin**, quero ver e ajustar os módulos de cada pessoa a partir do padrão do perfil dela, para que cada um tenha só o que precisa. |
| US-02 | Como **Admin**, quero dar a um Solicitante módulos de consulta (Dashboard, Relatório mensal), para que ele acompanhe números sem virar Atendente. |
| US-03 | Como **Admin**, quero ajustar o acesso ao Chat na mesma tela, para gerenciar tudo num lugar só. |
| US-04 | Como **pessoa que teve um módulo retirado**, quero que ele suma do meu menu na hora e que o sistema não me deixe usá-lo, para que a regra valha de verdade. |
| US-05 | Como **Admin**, quero que toda mudança de acesso fique registrada (quem mudou, de quem, o quê, quando), para auditar depois. |

---

## 4. Critérios de Aceitação

### Módulos ajustáveis e padrões

| Módulo | Padrão Solicitante | Padrão Atendente | Ajustável para Solicitante | Ajustável para Atendente |
|---|---|---|---|---|
| Arquivo | ligado | ligado | tirar | tirar |
| Kanban | — | ligado | não (só Atendente) | tirar |
| Fila | — | ligado | não (só Atendente) | tirar |
| Dashboard | desligado | ligado | **dar** ou tirar | tirar |
| Relatório mensal | desligado | ligado | **dar** ou tirar | tirar |
| Chat | sem acesso | sem acesso | sem acesso / participante / pode criar grupos | idem |

Abrir chamado e Meus chamados: sempre ligados. Módulos de administração: sempre só do Admin.

### US-01 — Tela de controle de acesso

- **AC-01:** Dado um Admin, quando abre "Controle de acesso" no menu de Administração, então vê a lista
  de pessoas (Solicitantes e Atendentes) com o perfil e um resumo dos módulos de cada uma, e pode
  buscar por nome ou e-mail.
- **AC-02:** Dado um Admin, quando abre uma pessoa, então vê cada módulo ajustável marcado ou
  desmarcado, indicando o que é **padrão do perfil** e o que é **ajuste** daquela pessoa.
- **AC-03:** Dado um Admin, quando desmarca um módulo ligado por padrão (ex.: Relatório mensal de um
  Atendente) e salva, então aquela pessoa perde o módulo; as demais pessoas do mesmo perfil não são
  afetadas.
- **AC-04:** Dado uma pessoa com ajustes, quando o Admin clica em "Voltar ao padrão do perfil", então
  os ajustes dela são removidos e ela volta a ter exatamente o padrão do perfil.
- **AC-05:** Dado um Atendente ou Solicitante, quando tenta abrir "Controle de acesso" (pelo menu ou
  pelo endereço), então não vê o menu e recebe "sem permissão".
- **AC-06:** Dado um Admin na tela, quando procura outro Admin, então ele aparece como "Admin — acesso
  total", sem ajuste de **módulos**. O Admin também não ajusta os próprios módulos. **O Chat dos Admins
  continua ajustável nesta tela** — inclusive o do próprio Admin, como era na tela de Usuários (decisão do
  usuário em 2026-10-05, após o review R-02).

### US-02 — Módulos de consulta para Solicitante

- **AC-07:** Dado um Solicitante, quando o Admin liga para ele o Dashboard ou o Relatório mensal, então
  o módulo aparece no menu e mostra **só os números dos chamados que ele já pode ver** (os que abriu e,
  se tiver equipe, os da equipe), nunca os do sistema todo.
- **AC-08:** Dado um Solicitante, quando o Admin abre a pessoa, então Kanban e Fila não aparecem como
  opção (só para Atendentes).

### US-03 — Chat na mesma tela

- **AC-09:** Dado um Admin, quando ajusta o acesso ao Chat de uma pessoa na tela de controle de acesso
  (sem acesso / participante / pode criar grupos), então o efeito é o mesmo de hoje: conceder e retirar
  avisam a pessoa e os participantes em tempo real, e a auditoria do chat registra a mudança.
- **AC-10:** Dado que o ajuste do Chat passa para a tela nova, quando o Admin abre o cadastro em
  Usuários, então o campo de acesso ao Chat não aparece mais lá (um lugar só), e os acessos ao Chat
  que existem hoje são mantidos sem mudança.

### US-04 — Efeito na hora

- **AC-11:** Dado uma pessoa com o sistema aberto, quando o Admin tira um módulo dela, então em poucos
  segundos o item some do menu dela sem precisar sair e entrar, e, se ela estiver na tela daquele
  módulo, é levada para "Meus chamados" com o aviso "Seu acesso a este módulo foi retirado.".
- **AC-12:** Dado uma pessoa sem um módulo, quando tenta abrir a tela pelo endereço ou chamar suas
  funções por fora da tela, então o sistema recusa com "sem permissão". Para Dashboard, Relatório
  mensal e Chat, a recusa vale também no servidor. Para Arquivo, Kanban e Fila, que só mostram de
  outra forma chamados que a pessoa já vê em "Meus chamados", a tela fica bloqueada e o menu some; o
  servidor não esconde chamados a mais por causa disso.
- **AC-13:** Dado uma pessoa a quem o Admin **deu** um módulo, quando ela está com o sistema aberto,
  então o item aparece no menu dela em poucos segundos.

### US-05 — Auditoria

- **AC-14:** Dado qualquer mudança de acesso (módulo ou Chat), quando é salva, então fica registrado
  quem mudou, de quem, qual módulo, de quê para quê e quando, e o Admin consegue ver esse registro na
  pessoa, na tela de controle de acesso.

### Mudança de perfil

- **AC-15:** Dado uma pessoa com ajustes de módulos, quando o Admin muda o perfil dela em Usuários (ex.:
  Atendente vira Admin, ou Solicitante vira Atendente), então os ajustes de módulos são apagados e ela
  passa ao padrão do novo perfil; o acesso ao Chat é mantido. A mudança fica na auditoria (decisão do
  usuário, 2026-10-02).

### Critérios Transversais

- **AC-16:** Todos os usuários existentes continuam com exatamente os mesmos módulos que têm hoje
  depois do deploy (todos começam no padrão do perfil; os acessos ao Chat são mantidos).
- **AC-17:** Erros retornam no formato `{ message: "..." }` em português.
- **AC-18:** `dotnet test` passa sem falhas, com teste automatizado para cada regra de acesso
  (AC-05 a AC-08, AC-12, AC-15) e para a manutenção dos acessos atuais (AC-16).
- **AC-19:** `npm run build` passa sem erros, e os testes E2E cobrem: ajustar um módulo, o menu da
  pessoa mudando e a tela bloqueada.

---

## 5. Rastreabilidade

| Critério | Arquivo de Teste | Método de Teste | Status |
|----------|-----------------|-----------------|--------|
| AC-02..AC-08, AC-16 | `ModulosDeAcessoTests`, `UsuarioPerfilTests` + banco conferido | `Padroes_SaoIguaisAoMenuDeAntesDaFeature`, `Solicitante_NuncaTemKanbanNemFila_*` | ✅ |
| AC-01..AC-06, AC-09, AC-14 | `AcessosCommandsTests` + E2E + ao vivo | `Salvar_*`, `VoltarAoPadrao_*`, `Listar_AdminComoAcessoTotal` | ✅ |
| AC-07, AC-12 | `ObterDistribuicaoQueryHandlerTests`, `ObterRelatorioMensalQueryHandlerTests` + ao vivo | `Handle_SolicitanteComModuloDashboard_*`, `Handle_AtendenteSemModuloDashboard_*`, `Handle_ComIdsVisiveis_*` | ✅ |
| AC-10, AC-15 | `AtualizarUsuarioPerfilHandlerTests` + ao vivo | `Handle_QuandoPerfilMuda_*`, `Handle_ChatPerfilNulo_*` | ✅ |
| AC-11, AC-13 | `e2e/controle-de-acesso.spec.ts`, `AcessoSignalRNotificationHandlersTests` | menu muda na hora em outro navegador | ✅ |
| AC-17..AC-19 | gates + `impacto.md` | 506 testes, E2E 19/19 | ✅ |

---

## 6. Decisões (2026-10-02, com o usuário)

| Decisão | Alternativa considerada | Motivo da escolha |
|---------|------------------------|-------------------|
| Admin tem sempre acesso total, não ajustável | Admin restringe Admin com travas | Evita Admin trancando Admin; quem não deve ter tudo não deve ser Admin |
| Ajuste por pessoa; padrão do perfil fixo | Também editar o padrão do perfil | Mais simples; exceções são por pessoa |
| Controle do módulo inteiro | Também ações dentro dos módulos | Bem menos regras para testar e manter |
| Solicitante pode receber só módulos de consulta; quem precisa atender vira Atendente | Qualquer módulo; ou só remover | Módulos de atendimento sem as ações de atendente não fazem sentido |
| Abrir chamado e Meus chamados fixos | Tudo removível | Para tirar tudo, desativa-se a conta |
| Chat unificado na tela nova, com os 3 níveis | Manter separado em Usuários | Um lugar só para o Admin |
| Mudança vale na hora | No próximo login | Regra precisa valer de verdade |
| Chat dos Admins ajustável na tela nova (2026-10-05, review R-02) | Chat automático para Admin | Mantém o que existia na tela de Usuários; 3 Admins hoje estão sem Chat por escolha |

---

## 7. Dependências

- Depende de: `autorizacao-chamados` (visibilidade e permissões no servidor), `chat-corporativo`
  (acesso ao Chat e auditoria), `logout-inatividade`/autenticação (o que vai no login).
- Toca: menu (`AppLayout`), rotas do frontend, Dashboard, Relatório, Chat, tela de Usuários.

---

## 8. Gate Checks

- [ ] `dotnet build` — 0 erros
- [ ] `dotnet test` — X testes, 0 falhas
- [ ] `npm run build` — 0 erros
- [ ] E2E Playwright — passando
- [ ] `/analise-cod` sem 🔴 em aberto
- [ ] ACs verificados por testes automatizados ou ao vivo
- [ ] Obsidian atualizado (regra 6): Perfis e Permissões, Administração, Chat Corporativo; ADR novo
- [ ] `spec.md` com status final; `STATE.md` e `ROADMAP.md` atualizados
- [ ] PR aberto com base `develop`
