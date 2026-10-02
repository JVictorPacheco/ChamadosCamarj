# Correções Pré-Deploy — Especificação

> **SDD:** fechada
> **Status:** `Concluída`
> **Branch:** `feature/correcoes-pre-deploy`
> **Criada em:** 2026-10-02
> **Atualizada em:** 2026-10-02
> **Origem:** pendências gerais do STATE.md (consolidadas em 2026-10-02) e achados R-02/R-03 do
> review de `correcoes-acesso-chamados`. Pacote escolhido pelo usuário em 2026-10-02 para entrar
> no mesmo deploy das 4 features já em `main`.
> **Aprovação:** spec aprovada pelo usuário em 2026-10-02, incluindo AC-18 e AC-23.

---

## 1. Problema

**Situação atual:**
- **Alerta de prazo (SLA):** todo Atendente recebe o aviso de "próximo do prazo" e "prazo
  estourado" de **todos** os chamados, inclusive dos que ele não pode ver. Com isso ele fica sabendo
  o número e a situação de prazo de chamados de outras equipes.
- **Anexo em comentário:** é possível ligar um anexo a um comentário de **outro** chamado. O
  sistema não confere se o comentário pertence ao chamado.
- **Edição simultânea:** se duas pessoas estão com o mesmo chamado aberto e as duas alteram, quem
  salva por último apaga a alteração da outra sem nenhum aviso. Exemplo: A abre o chamado às
  10:00, B corrige o título às 10:01, A salva a descrição às 10:02 com o título antigo, e a
  correção de B se perde.
- **Arrumação técnica:**
  - os testes automáticos de ponta a ponta ainda usam o campo "Categoria", que deixou de existir
    na tela, e por isso falham;
  - o projeto de testes usa versões diferentes de uma biblioteca de banco, o que gera aviso na
    compilação;
  - o ícone da aba do navegador pesa 218 KB, quando poderia pesar poucos KB.
- **Processo:** as regras do projeto e o guia de orquestração descrevem só o fluxo de trabalho do
  OpenCode, e não o fluxo `/sdd` do Claude Code, que é o usado nas últimas features.

**Impacto:** vazamento de informação entre equipes (alerta), dado inconsistente (anexo), perda
silenciosa de trabalho (edição simultânea), testes que não protegem contra regressão e documentação
de processo desatualizada.

**Solução esperada:** fechar essas pendências antes do deploy, para que tudo suba junto.

---

## 2. Fora de Escopo

- Unificar as duas pastas de migrations. Mexe em migrations já aplicadas no banco de produção, e o
  risco não compensa agora.
- Limpeza de Categorias (tornar Área/Tipo obrigatórios, remover Categoria). Essa feature acontece
  depois do deploy e da reclassificação dos chamados antigos.
- SLA em horas úteis: é decisão de negócio, ainda em aberto.
- Mudar **quem** pode ver cada chamado. Esta feature só aplica a regra de visibilidade que já
  existe.
- Conflito em comentários e anexos: adicionar comentário ou anexo nunca é recusado por edição
  simultânea (decisão de 2026-10-02).
- Remover o fluxo do OpenCode da documentação. Ele continua descrito, ao lado do `/sdd`.

---

## 3. User Stories

| ID | User Story |
|----|------------|
| US-01 | Como **Atendente**, quero receber alertas de prazo só dos chamados que posso ver, para que eu não seja avisado (nem fique sabendo) de chamados de outras equipes. |
| US-02 | Como **Admin**, quero que o sistema recuse um anexo ligado a um comentário de outro chamado, para que anexos nunca apareçam no lugar errado. |
| US-03 | Como **Atendente** ou **Admin**, quero ser avisado quando outra pessoa alterou o chamado enquanto eu o via, para que eu não apague a alteração dela sem saber. |
| US-04 | Como **Admin** (dono do sistema), quero testes de ponta a ponta que funcionem com a tela atual, uma compilação sem avisos e um ícone leve, para que o sistema seja verificável e carregue rápido. |
| US-05 | Como **Admin** (dono do processo), quero que as regras e o guia de orquestração descrevam também o `/sdd`, para que qualquer ferramenta siga o processo que de fato usamos. |

---

## 4. Critérios de Aceitação

### US-01 — Alerta de prazo só para quem vê o chamado

- **AC-01:** Dado um chamado que fica próximo do prazo ou atrasado, quando o sistema dispara o
  alerta, então o alerta chega a cada Atendente que **pode ver** aquele chamado pela mesma regra da
  lista de chamados: sem responsável, atribuído a ele, ou do grupo dele.
- **AC-02:** Dado um Atendente que **não pode ver** o chamado (por exemplo, um chamado de outro
  grupo atribuído a outra pessoa), quando o alerta desse chamado é disparado, então ele não recebe
  nada.
- **AC-03:** Dado um Admin, com ou sem grupo, quando qualquer chamado gera alerta, então o Admin
  recebe o alerta, como hoje.
- **AC-04:** Dado um Solicitante, quando qualquer chamado gera alerta, então ele não recebe nada,
  como hoje.
- **AC-05:** Dado um chamado que já gerou o alerta de "próximo do prazo", quando o sistema verifica
  de novo e o chamado continua na mesma situação, então ninguém recebe o mesmo alerta outra vez,
  como hoje.

### US-02 — Anexo ligado só a comentário do mesmo chamado

- **AC-06:** Dado um chamado A e um comentário que pertence ao chamado B, quando alguém envia um
  anexo para o chamado A ligado a esse comentário, então o sistema recusa com a mensagem "O
  comentário informado não pertence a este chamado." e nenhum arquivo fica guardado.
- **AC-07:** Dado um comentário que não existe, quando alguém envia um anexo ligado a ele, então o
  sistema recusa da mesma forma.
- **AC-08:** Dado um comentário do próprio chamado, quando alguém envia um anexo ligado a ele, ou
  envia um anexo sem comentário, então o anexo é aceito como hoje.

### US-03 — Aviso de edição simultânea

- **AC-09:** Dado que a pessoa A abriu um chamado e, depois disso, a pessoa B alterou esse chamado,
  quando A tenta qualquer uma destas ações: editar título/descrição, mudar status, mudar prioridade,
  assumir, reatribuir, resolver, encerrar, cancelar, reabrir, forçar encerramento ou reclassificar o
  tipo, então a ação de A é recusada e a alteração de B é mantida.
- **AC-10:** Dado o caso do AC-09, quando a ação de A é recusada, então a tela mostra "Outra
  pessoa alterou este chamado. Os dados foram atualizados; confira e refaça a ação." e o chamado é
  recarregado com os dados atuais.
- **AC-11 — ADIADO (decisão do usuário em 2026-10-02):** Dado o caso do AC-09 na edição de
  título/descrição, quando a ação é recusada, então o formulário continua aberto com o texto que A
  digitou, para A conferir e salvar de novo. *Não se aplica nesta feature:* hoje não existe tela de
  edição (a ação só existe na API, onde o AC-09 já a protege). O AC-11 entra na futura feature
  **"Editar chamado"**, registrada no ROADMAP com estas regras definidas pelo usuário:
  - será um **modal** no detalhe do chamado (não uma tela nova);
  - **Solicitante** edita só os chamados que **ele abriu**;
  - **Atendente** edita só os chamados que **ele assumiu** (é o responsável);
  - **Admin** edita **todos**.
  Atenção: a regra de hoje na API é outra (qualquer Atendente que vê o chamado pode editar; o
  Solicitante não pode). A mudança da regra fica para essa futura feature.
- **AC-12:** Dado que ninguém alterou o chamado desde que A o abriu, quando A faz qualquer uma das
  ações do AC-09, então ela funciona como hoje, inclusive várias ações seguidas de A sem recarregar
  a página manualmente.
- **AC-13:** Dado que B só adicionou um comentário ou um anexo ao chamado, quando A faz uma das
  ações do AC-09, então a ação de A **não** é recusada.
- **AC-14:** Dado que A move um chamado no Kanban (mudança de status) e outra pessoa alterou esse
  chamado antes, quando a mudança é recusada, então o cartão volta à coluna em que estava e a mesma
  mensagem do AC-10 aparece.

### US-04 — Arrumação técnica

- **AC-15:** Dado os testes de ponta a ponta, quando rodam contra a versão atual, então nenhum deles
  usa "Categoria", a abertura de chamado preenche Área e Tipo, e todos passam.
- **AC-16:** Dado que os testes de ponta a ponta criam dados reais (o banco de desenvolvimento é o
  de produção), quando rodam, então todo chamado criado leva `[TESTE-E2E]` no título, e esses dados
  só são apagados depois do OK do usuário (decisão de 2026-10-02).
- **AC-17:** Dado o projeto de testes, quando compila, então não aparece aviso de conflito de
  versão da biblioteca de banco.
- **AC-18:** Dado o ícone da aba, quando a página carrega, então ele tem a mesma aparência de hoje
  e pesa no máximo 20 KB.

### US-05 — Processo

- **AC-19:** Dado a regra 4 das Regras de Processo (STATE.md) e o guia de orquestração, quando
  alguém os lê, então eles descrevem o fluxo `/sdd` do Claude Code (fases e os 2 pontos de
  aprovação do usuário) e continuam descrevendo o fluxo do OpenCode como alternativa.

### Critérios Transversais

- **AC-20:** Erros retornam no formato `{ message: "..." }` em português.
- **AC-21:** `dotnet test` passa sem falhas, e cada AC de US-01 a US-03 tem teste automatizado.
- **AC-22:** `npm run build` passa sem erros.
- **AC-23:** Uma tela ou um cliente antigo, que não informa a versão do chamado que leu, continua
  funcionando como hoje (sem a proteção do AC-09), para o deploy não quebrar quem estiver com a
  página aberta.

---

## 5. Rastreabilidade

| Critério | Arquivo de Teste | Método de Teste | Status |
|----------|-----------------|-----------------|--------|
| AC-01, AC-02 | `WebApi/Services/SlaAlertaNotificadorTests.cs` + ao vivo | `Notificar_EnviaParaAdminsEParaOsAtendentesQueVeem`, `Notificar_NuncaUsaOGrupoDeAtendimentoInteiro`; ao vivo: 3 Atendentes × 23 chamados reais atrasados, recebe ⇔ vê (69/69) | ✅ |
| AC-03 | `ChamadosHubTests.cs`, `SlaAlertaNotificadorTests.cs` + ao vivo | `OnConnectedAsync_SoAdminEntraNoGrupoAdmins`; ao vivo 25/26 (o 1º alerta disparou junto com a entrada no grupo — corrida da partida, ver review) | ✅ |
| AC-04 | `ChamadosHubTests.cs` + ao vivo | Solicitante só no grupo `Todos`; ao vivo não recebeu nenhum alerta | ✅ |
| AC-05 | `WebApi/Services/SlaAlertasEnviadosTests.cs` | `MesmaSituacaoNaVerificacaoSeguinte_NaoNotificaDeNovo` e mais 4 (review R-01) | ✅ |
| AC-06, AC-07, AC-08 | `AdicionarAnexoHandlerTests.cs` + ao vivo | `Handle_ComComentarioQueNaoPertenceAoChamado_DeveRecusarSemFazerUpload`, `Handle_SemComentario_NaoConsultaComentario`, `Handle_ComComentarioId_DeveVincularAnexoAoComentario`; ao vivo 400 com a mensagem, PDF aceito no próprio chamado | ✅ |
| AC-09, AC-10 (servidor), AC-13, AC-23 | `Concorrencia/VersaoChamadoBehaviourTests.cs` + ao vivo | `VersaoDiferente_Recusa409ComAMensagemDoAC10`, `Comentario_NuncaDaConflito`, `SemVersaoInformada_SegueSemConsultar`; ao vivo 409/204 e 404 antes de 409 | ✅ |
| AC-10 (tela), AC-12 | `frontend/e2e/conflito.spec.ts` + `VersaoChamadoTests.cs` | `detalhe: ação sobre versão desatualizada...`; `De_IgnoraFracaoAbaixoDeMicrossegundo` | ✅ |
| AC-11 | — | **Adiado** (futura feature "Editar chamado") | ⏭️ |
| AC-14 | `frontend/e2e/conflito.spec.ts` | `kanban: mover cartão desatualizado...` | ✅ |
| AC-15, AC-16 | `frontend/e2e/*` | 14/14 passando; títulos `[TESTE-E2E]` | ✅ (limpeza pendente de OK) |
| AC-17 | `dotnet build` | sem MSB3277 | ✅ |
| AC-18 | `frontend/public/favicon.png` | 3.569 bytes, 64×64, conferido visualmente | ✅ |
| AC-19 | STATE.md regra 4, `docs/GUIA-ORQUESTRACAO-SDD.md` | Leitura | ✅ |
| AC-20..AC-22 | gates | 438 testes, builds ok | ✅ |

---

## 6. Decisões (2026-10-02, com o usuário)

| Decisão | Alternativa considerada | Motivo da escolha |
|---------|------------------------|-------------------|
| Pacote: SLA filtrado, validação de anexo, arrumação técnica, edição simultânea e docs de processo | Separar em features menores | Tudo sobe no mesmo deploy |
| Conflito vale para **todas** as ações que alteram o chamado | Só editar título/descrição | Duas pessoas resolvendo/encerrando ao mesmo tempo também é conflito real |
| Comentário e anexo nunca dão conflito | Tratar como alteração | São acréscimos e não sobrescrevem nada |
| No conflito: avisa e recarrega, mantendo o texto digitado na edição | Recarregar e fechar o formulário | Não perder o que a pessoa escreveu |
| E2E: rodar contra o banco real com `[TESTE-E2E]` e limpar com OK do usuário | Só atualizar, sem rodar | Os testes só têm valor se rodarem |
| Unificar pastas de migrations fica fora | Incluir | Risco em migrations já aplicadas em produção |
| AC-11 adiado; modal "Editar chamado" vira feature própria (Solicitante: os que abriu; Atendente: os que assumiu; Admin: todos) | Criar o modal agora | Esta feature é de correções para o deploy |

---

## 7. Dependências

- Depende de: `autorizacao-chamados` (regra de visibilidade reutilizada no alerta) e
  `area-e-tipo-do-chamado` (Área/Tipo nos testes de ponta a ponta), ambas em `main`.
- Bloqueia: o deploy (o usuário quer subir tudo junto) e a limpeza de Categorias.

---

## 8. Gate Checks

- [x] `dotnet build` — 0 erros, sem aviso de conflito de versão no projeto de testes
- [x] `dotnet test` — 444 testes, 0 falhas
- [x] `npm run build` — 0 erros
- [x] E2E Playwright — 14/14; dados de teste limpos com OK do usuário (2026-10-02)
- [x] ACs verificados por testes automatizados e ao vivo (AC-11 adiado)
- [x] Obsidian atualizado (regra 6) — SLA, Anexos, Acompanhamento, Kanban, ADR-009
- [x] `spec.md` com status final; `STATE.md` e `ROADMAP.md` atualizados
- [ ] PR aberto com base `develop` (checkpoint 2 — aguardando o usuário)
