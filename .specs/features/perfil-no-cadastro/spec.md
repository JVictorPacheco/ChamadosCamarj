# Perfil e Equipe Valem na Hora (Segurança) — Especificação

> **SDD:** fechada
> **Status:** `Concluída`
> **Branch:** `feature/perfil-no-cadastro`
> **Criada em:** 2026-10-09
> **Atualizada em:** 2026-10-09
> **Origem:** pendência de segurança registrada no STATE (2026-10-05, ampliada em 2026-10-06) e escolha do usuário em 2026-10-09 pela "opção A" (conferir o cadastro, não o token).

---

## 1. Problema

**Situação atual:**
O sistema decide o que cada pessoa pode fazer pelo perfil e pela equipe que estavam gravados no login dela. Esse login vale 10 horas. Se o Admin rebaixa alguém, troca a equipe ou desativa a conta durante esse período, o servidor continua obedecendo ao login antigo. Hoje:
- um Admin rebaixado continua podendo administrar usuários, grupos, tipos e acessos;
- um Atendente que virou Solicitante continua podendo assumir e tratar chamados;
- quem trocou de equipe continua enxergando e atendendo os chamados da equipe antiga;
- quem teve a conta desativada continua usando o sistema até o login vencer;
- quem estava conectado no canal de tempo real continua recebendo avisos restritos (ex.: alertas de SLA dos Admins).

A tela já desconecta a pessoa quando o perfil muda, mas isso só protege quem usa o sistema pelo navegador. Quem chama o servidor direto continua com os direitos antigos.

**Impacto:**
Desligamento ou rebaixamento por motivo de confiança não vale na hora. Durante até 10 horas a pessoa mantém acesso que o Admin acabou de tirar.

**Solução esperada:**
Perfil, equipe e situação da conta (ativa ou desativada) passam a ser conferidos no cadastro a cada ação, em toda a API e no tempo real, de modo que qualquer mudança feita pelo Admin vale no pedido seguinte.

---

## 2. Fora de Escopo

- [ ] Invalidar os logins antigos quando o perfil ou a equipe mudam (segunda camada de segurança; feature futura, registrada no ROADMAP).
- [ ] Mudar a regra de quem vê o quê entre os perfis; só muda **de onde** vem o perfil/equipe.
- [ ] Encurtar a validade do login (10 h) ou criar renovação de login.
- [ ] Mudar as mensagens de desconexão da tela ("Seu perfil foi alterado. Entre novamente.").
- [ ] Mudar o fluxo de login, de redefinição de senha ou de login com Google.

---

## 3. User Stories

| ID | User Story |
|----|------------|
| US-01 | Como **Admin**, quero que ao rebaixar ou trocar o perfil de alguém os novos direitos valham no pedido seguinte dela, para que ninguém mantenha acesso que eu acabei de tirar. |
| US-02 | Como **Admin**, quero que a troca de equipe valha na hora para os chamados que a pessoa enxerga e atende, para que ela não mantenha acesso aos chamados da equipe antiga. |
| US-03 | Como **Admin**, quero que desativar uma conta bloqueie a pessoa na hora, para que ela não use o sistema até o login vencer. |
| US-04 | Como **Admin**, quero que o canal de tempo real também siga o cadastro, para que quem perdeu direitos pare de receber avisos restritos. |
| US-05 | Como **Atendente ou Solicitante**, quero que meu uso normal continue igual, para que a mudança de segurança não atrapalhe quem não teve o cadastro alterado. |

---

## 4. Critérios de Aceitação

### US-01 — Perfil vale na hora

- **AC-01:** Dado um Admin que teve o perfil trocado para Solicitante enquanto estava logado, quando ele tenta qualquer ação de Administração (usuários, grupos, tipos, controle de acesso), então o servidor recusa com a mensagem de falta de permissão.
- **AC-02:** Dado um Atendente que teve o perfil trocado para Solicitante enquanto estava logado, quando ele tenta assumir, reatribuir, resolver ou fechar um chamado, então o servidor recusa.
- **AC-03:** Dado um Solicitante que foi promovido a Atendente enquanto estava logado, quando ele pede a fila de chamados, então passa a ser atendido com o escopo de Atendente, sem precisar entrar de novo.
- **AC-04:** Dado alguém com o perfil trocado, quando faz qualquer pedido, então o servidor usa o perfil do cadastro em todos os pontos que dependem de perfil: administração, ações sobre chamados, listagem e detalhe de chamados, comentários, anexos, histórico, dashboard, relatório e chat.
- **AC-05:** Dado alguém com o perfil trocado, quando a tela detecta a mudança, então continua desconectando a pessoa com a mensagem atual (comportamento existente preservado).

### US-02 — Equipe vale na hora

- **AC-06:** Dado um Atendente que foi tirado de uma equipe enquanto estava logado, quando pede a lista de chamados, então deixa de ver os chamados da equipe antiga no pedido seguinte.
- **AC-07:** Dado um Atendente que foi colocado numa equipe enquanto estava logado, quando pede a lista de chamados, então passa a ver os chamados dessa equipe, sem entrar de novo.
- **AC-08:** Dado alguém que perdeu a equipe, quando tenta abrir, comentar, anexar ou agir num chamado que só a equipe antiga permitia, então o servidor recusa.
- **AC-09:** Esta feature substitui a regra "equipe trocada vale só no próximo login" (decisão de 2026-10-06); a regra nova é "equipe vale na hora".

### US-03 — Conta desativada

- **AC-10:** Dado alguém logado cuja conta foi desativada pelo Admin, quando faz qualquer pedido ao servidor, então é recusado como não autenticado e a tela o leva ao login.
- **AC-11:** Dado alguém cuja conta foi apagada, quando faz qualquer pedido com o login antigo, então é recusado do mesmo modo.
- **AC-12:** Dado uma conta desativada e depois reativada, quando a pessoa entra de novo normalmente, então volta a usar o sistema com os direitos do cadastro.

### US-04 — Tempo real

- **AC-13:** Dado um Admin rebaixado que estava conectado ao tempo real, quando surge um alerta restrito a Admins (ex.: SLA de todos os chamados), então ele não o recebe.
- **AC-14:** Dado um Solicitante promovido a Atendente, quando surge um aviso operacional do atendimento, então ele passa a recebê-lo sem entrar de novo.
- **AC-15:** Dado uma conta desativada conectada ao tempo real, quando o Admin a desativa, então a conexão deixa de receber avisos e não consegue se reconectar.
- **AC-16:** Dado alguém que se conecta ao tempo real, quando o servidor decide quais avisos ele recebe, então usa o perfil do cadastro, não o do login.

### US-05 — Uso normal preservado

- **AC-17:** Dado um usuário sem nenhuma mudança de cadastro, quando usa qualquer tela do sistema, então o comportamento é o mesmo de antes (nenhuma tela nova, nenhuma recusa nova).
- **AC-18:** Dado a lista de chamados com muitos usuários usando ao mesmo tempo, quando cada um faz seus pedidos, então a tela não fica perceptivelmente mais lenta que hoje.

### Critérios Transversais

- **AC-19:** Toda recusa por falta de permissão retorna mensagem em português no formato `{ message: "..." }`.
- **AC-20:** Nenhuma rota fica de fora: qualquer rota existente ou futura que dependa de perfil ou equipe passa pela mesma conferência única do cadastro.
- **AC-21:** `dotnet test` e `npm run build` passam sem falhas.

---

## 5. Rastreabilidade

| Critério | Teste automatizado | Verificação ao vivo (2026-10-09, API real + hub) | Status |
|----------|--------------------|--------------------------------------------------|--------|
| AC-01 | `CadastroClaimsValidatorTests.PerfilRebaixado_*`; `PerfilNoCadastroNasRegrasDeAcessoTests.AdminRebaixadoParaAtendente_*` | Admin rebaixado: usuários, edição e controle de acesso → 403 | ✅ |
| AC-02 | `PerfilNoCadastroNasRegrasDeAcessoTests.AtendenteRebaixadoParaSolicitante_*` | Atendente rebaixado: assumir e comentário interno recusados | ✅ |
| AC-03 | `CadastroClaimsValidatorTests.PerfilPromovido_*`; `...SolicitantePromovidoParaAtendente_*` | Solicitante promovido assume chamado com o token antigo | ✅ |
| AC-04 | `CadastroClaimsValidatorTests.PerfilAtualizadoChegaNoCurrentUserENoContextoDeAcesso` | listagem, detalhe, comentário (escopo) conferidos nos casos acima | ✅ |
| AC-05 | testes existentes de `AcessosAtualizados` / `logoutPerfilAlterado` sem alteração | demonstração em tela: aba aberta volta ao login no primeiro clique | ✅ |
| AC-06..08 | `CadastroClaimsValidatorTests.Equipe*`; `...EquipeTrocada_*`, `...EquipeRemovida_*` | sem equipe/outra equipe: chamado e lista saem; na equipe: entram; comentar recusado/aceito | ✅ |
| AC-09 | decisão registrada na spec e no design (substitui a de 2026-10-06) | — | ✅ |
| AC-10 | `CadastroClaimsValidatorTests.ContaDesativada_*` | desativada: lista de chamados e `/auth/me` → 401; login anônimo com token antigo segue ok | ✅ |
| AC-11 | `CadastroClaimsValidatorTests.ContaApagada_*` | conta apagada do banco: 401 | ✅ |
| AC-12 | — (sem código novo) | reativada: login e até o token antigo voltam a valer | ✅ |
| AC-13, 14 | `CadastroDeAcessoAlteradoNotificationHandlerTests` | Atendente rebaixado deixa de receber aviso interno; Solicitante promovido passa a receber | ✅ |
| AC-15 | `CadastroDeAcessoAlteradoNotificationHandlerTests.ContaDesativada_*`; `ConexoesTempoRealTests` | os dois hubs derrubados; reconexão com o token antigo recusada | ✅ |
| AC-16 | `ChamadosHubTests.GruposDoPerfil_SegueOPerfil`; validador | conexão nova nasce com o perfil do cadastro | ✅ |
| AC-17 | 528 testes anteriores verdes sem alteração | Solicitante, Atendente e Admin sem mudança seguem como antes | ✅ |
| AC-18 | `UsuarioPerfilRepositoryTests.*Cache*` (cache de 15 s, invalidação ao gravar) | listagem ~636 ms com cache (era ~690 ms sem); consulta fria ~135 ms | ✅ |
| AC-19 | mensagens atuais preservadas | recusa devolve `{ message }` | ✅ |
| AC-20 | `CadastroClaimsValidatorTests` (ponto único no token) | 41 casos passaram pelo mesmo ponto | ✅ |
| AC-21 | `dotnet test` 573/573 · `npm run build` ok | — | ✅ |

> E2E completo: 25/25 (1 worker; 2 testes de controle-de-acesso oscilaram com 2 workers por falta de memória e passam isolados).

---

## 6. Decisões (2026-10-09, com o usuário)

- Mudança de cadastro vale no pedido seguinte; o servidor passa a usar o perfil/equipe atuais (não recusa até relogar).
- Equipe também vale na hora, substituindo a decisão de 2026-10-06 (review-4 R-02 de `controle-de-acesso`).
- Conta desativada ou apagada é bloqueada na hora.
- Alcance: tudo que depende de perfil/equipe — administração, chamados, chat e tempo real.
- Segunda camada (invalidar logins antigos) fica como feature futura de segurança.
- 2026-10-09 (após a verificação ao vivo): conferir o banco em todo pedido custava ~160 ms por chamada; o usuário aprovou um **cache curto (15 s) com invalidação imediata** a cada mudança feita pelo sistema. "No pedido seguinte" (AC-01 a AC-11) vale para toda mudança feita pelo sistema; só edição direta no banco, ou um segundo servidor de backend, demora até 15 s.

## 7. Dependências

- `controle-de-acesso`: a conferência de módulos e a desconexão da tela já seguem o cadastro; esta feature estende o mesmo princípio ao perfil e à equipe.
- `autorizacao-chamados`, `grupos-equipes`, `chat-corporativo`, `correcoes-pre-deploy` (tempo real): leem perfil/equipe hoje e serão tocadas — análise de impacto obrigatória.

## 8. Gate Checks

- [x] `dotnet build` sem erros
- [x] `dotnet test` sem falhas (576/576)
- [x] `npm --prefix frontend run build` sem erros
- [x] `/analise-cod` sem 🔴
