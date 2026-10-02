# Editar Chamado — Tasks

> **Branch:** `feature/editar-chamado`
> **Spec:** `spec.md` | **Design:** `design.md`
> **Ferramenta:** Claude Code (Opus 5.5) para spec, design, backend e frontend; `/analise-cod`; review por sub-agente novo
> **Gate checks:** `dotnet build` + `dotnet test tests/ChamadosCamarj.UnitTests/` + `npm --prefix frontend run build`
> **Contratos C1–C5 aprovados pelo usuário em 2026-10-02.**

---

## Backend

- [x] **T01** Domínio: `Chamado.AtualizarDados` retorna `bool` (houve mudança), não toca
  `DataAtualizacao` sem mudança e lança `InvalidOperationException` em chamado encerrado.
  *AC-10, AC-16 · design §2.3.*
  **Pronto quando:** testes de domínio: mudança → true e versão muda; texto igual → false e versão
  igual; Resolvido/Fechado/Cancelado → exceção.
- [x] **T02** `ChamadoPermissoes`: `DadosDoChamado`, `Pode(acao, acesso, DadosDoChamado)`, regra nova
  de `Editar`, `DependeDoChamado`, `EdicaoBloqueadaPorEncerramento`; ajustar os 2 testes da regra
  antiga e criar os da nova. *AC-01..AC-11 · C1, C2.*
  **Pronto quando:** testes da regra: quem abriu sem responsável (Solicitante e Atendente) → pode;
  quem abriu com responsável → não; responsável atual → pode; outro Atendente que vê → não;
  Solicitante colega de grupo → não; Admin com/sem grupo → pode; encerrado → ninguém; reaberto (sem
  responsável) → quem abriu pode. Testes das outras ações inalterados e passando.
- [x] **T03** `AcessoChamadoBehaviour`: carrega o chamado quando `DependeDoChamado`; `Editar` em
  encerrado → 400 com a mensagem do AC-10 (antes do 403); 403 pela regra. *AC-02, AC-06, AC-10,
  AC-20, AC-22 · design §2.2.*
  **Pronto quando:** testes do behaviour: 404 continua antes de tudo; Admin em encerrado → 400 com a
  mensagem; Atendente não responsável → 403; ações que não dependem do chamado não fazem a consulta
  extra.
- [x] **T04** `AcaoHistorico.ChamadoEditado`; `AtualizarChamadoCommand` + `UsuarioId`/`UsuarioNome`
  (controller); handler com transação, no-op sem mudança e histórico JSON só dos campos que mudaram.
  *AC-16..AC-18 · C3, C4.*
  **Pronto quando:** testes do handler: só título → histórico com `{"titulo":…}`; título e descrição →
  os dois; sem mudança → nada salvo, nada no histórico; quem editou registrado.

## Frontend

- [x] **T05** `types/api.ts` (`ChamadoEditado`), `api.ts` (`atualizarChamado` com `If-Match`),
  `useAtualizarChamado`, `lib/permissoes.ts` (`podeEditarChamado`). *AC-01..AC-11 (botão), C5.*
  **Pronto quando:** `npm run build` ok.
- [x] **T06** `EditarChamadoModal` + botão "Editar" no `BotoesAcao`: preenchimento ao abrir, limites e
  erros nos campos, salvar sem mudança fecha sem chamar a API, conflito mantém o texto e mostra
  "Texto atual no chamado" e passa a usar a versão nova, 403/400 mantém o texto. *AC-12..AC-15,
  AC-19..AC-21.*
  **Pronto quando:** build ok; verificação na tela (T09).
- [x] **T07** `TimelineHistorico`: "Chamado editado" com antes → depois por campo; JSON inválido mostra
  texto cru. *AC-17, AC-18.*
  **Pronto quando:** build ok; verificação na tela (T09).

## Verificação

- [x] **T08** E2E `e2e/editar-chamado.spec.ts`: Admin edita (histórico aparece); botão ausente em
  chamado encerrado; conflito mantém o texto no modal e o segundo "Salvar" grava. Títulos `[TESTE-E2E]`.
  Suíte inteira passando. *AC-25.*
- [x] **T09** Verificação ao vivo (API + tela) com contas `teste.edit.*` (Solicitante que abre,
  Atendente responsável, outro Atendente da mesma equipe, Solicitante colega): AC-01..AC-11, AC-19,
  AC-20. **PARADA:** dados de teste (chamados com `ChamadoEditado` no histórico — a versão antiga em
  produção não lê esse valor) apagados só com OK do usuário, **antes** do fim da sessão.
- [x] **T10** Gates + `/analise-cod` (sem 🔴 em aberto) + rastreabilidade na spec. *AC-22..AC-25.*

---

## Analyze (2026-10-02)

| Checagem | Resultado |
|---|---|
| Todo AC tem tarefa | AC-01..AC-11 → T02, T03, T05, T09; AC-12..AC-15 → T06; AC-16..AC-18 → T01, T04, T07; AC-19..AC-21 → T06, T08, T09; AC-22..AC-25 → T03, T08, T10 |
| Toda tarefa aponta para AC existente | Sim |
| Contratos C1–C5 com tarefa e teste | C1/C2 T02; C3/C4 T04; C5 T03 |
| Nada contradiz "Fora de escopo" | Sim: só título e descrição; sem notificação; encerrado bloqueado |
| Constitution | Parada da T09 (dados no banco real) prevista; `/analise-cod` na T10 |
