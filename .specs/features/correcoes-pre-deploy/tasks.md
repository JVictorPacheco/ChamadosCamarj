# Correções Pré-Deploy — Tasks

> **Branch:** `feature/correcoes-pre-deploy`
> **Spec:** `spec.md` | **Design:** `design.md`
> **Ferramenta:** Claude Code (Opus 5.5) para spec, design, backend e frontend; review por sub-agente novo
> **Gate checks:** `dotnet build` + `dotnet test tests/ChamadosCamarj.UnitTests/` + `npm --prefix frontend run build`

---

## Backend

### Bloco B — Anexo × comentário (menor, primeiro)
- [x] **T01** `IChamadoRepository.ComentarioPertenceAoChamadoAsync` + implementação; checagem em
  `AdicionarAnexoCommandHandler` antes do upload (`BadRequestException` com o texto do AC-06).
  *AC-06, AC-07, AC-08 · design §3 · contrato C4, C7.*
  **Pronto quando:** testes do handler passam: comentário de outro chamado → 400 e `UploadAsync`
  nunca chamado; comentário inexistente → 400; comentário do próprio chamado e sem comentário →
  upload feito.

### Bloco A — Alerta de SLA
- [x] **T02** `IChamadoRepository.ListarAtendentesQuePodemVerAsync` (Atendentes ativos, reaproveita
  `AplicarVisibilidade`). *AC-01, AC-02 · design §2 · C4.*
  **Pronto quando:** compila; o método não duplica a regra de visibilidade (usa `AplicarVisibilidade`).
- [x] **T03** Grupo `Admins` em `ChamadosHub.OnConnectedAsync`. *AC-03, AC-04 · C5.*
  **Pronto quando:** teste do `OnConnectedAsync` mostra que o Admin entra em `Todos`, `Atendimento` e
  `Admins`, o Atendente não entra em `Admins` e o Solicitante só entra em `Todos`.
- [x] **T04** `SlaAlertaNotificador` (envia para `Admins` + `Clients.Users(atendentes que veem)`),
  registrado em DI, e `SlaMonitorService` passa a usá-lo. *AC-01..05 · design §2.*
  **Pronto quando:** testes do notificador provam o envio para `Admins` e para exatamente os ids
  devolvidos pelo repositório, e que lista vazia não chama `Users`. O monitor não referencia mais
  `GrupoAtendimento` e mantém o `_notificados` (AC-05).

### Bloco C — Edição simultânea (backend)
- [x] **T05** `VersaoChamado.De` + `ChamadoResponse.Versao` + `ChamadoMappings`. *AC-09, AC-12 · §4.1 · C1.*
  **Pronto quando:** teste: `Ticks` com fração abaixo de microssegundo dá a mesma versão que o valor
  truncado; `DataAtualizacao` nula usa `DataCriacao`; `ToResponse` preenche `Versao`.
- [x] **T06** `ChamadoPermissoes.AlteraChamado(acao)`. *AC-09, AC-13.*
  **Pronto quando:** teste por ação: as 11 ações de alteração dão true; `Ver`, `ComentarPublico`,
  `ComentarInterno` e `Anexar` dão false.
- [x] **T07** `IChamadoRepository.ObterVersaoAsync`, `IVersaoLidaAccessor` (WebApi, lê `If-Match`
  sem aspas), `VersaoChamadoBehaviour` registrado depois do `AcessoChamadoBehaviour`, e a mensagem
  do 409 do repositório igual à do AC-10. *AC-09, AC-10, AC-13, AC-23 · §4.2 · C2, C3, C6.*
  **Pronto quando:** testes do behaviour cobrem: versão igual → segue; versão diferente → 409 com o
  texto do AC-10; sem `If-Match` → segue sem consultar (AC-23); ação não-alteração com versão
  diferente → segue (AC-13); request que não é de chamado → segue. Teste do accessor cobre o
  `If-Match` com e sem aspas. Conferir que os 11 commands de alteração implementam
  `IRequerAcessoChamado` com a ação correta.

## Frontend

### Bloco C — Edição simultânea (tela)
- [ ] **T08** `types/api.ts` (`versao`) + `api.ts`: as 10 funções de ação recebem `versao?` e
  enviam `If-Match` (helper único). *AC-09, AC-23 · §4.3 · C1, C2.*
  **Pronto quando:** `npm run build` ok.
- [ ] **T09** `useAcoesChamado` (variáveis com `versao`; 409 → invalida e recarrega),
  `ChamadoDetailPage` + modais + `TipoChamadoCampo` passam `chamado.versao`; botões de ação
  desabilitados durante `isFetching` do chamado. *AC-10, AC-12.*
  **Pronto quando:** build ok; verificação ao vivo da T13.
- [ ] **T10** `KanbanBoard`: envia `versao`; em erro, mostra a mensagem do servidor numa faixa acima
  do quadro e o cartão volta à coluna. *AC-14.*
  **Pronto quando:** build ok; verificação ao vivo da T13.

## Bloco D — Arrumação técnica
- [ ] **T11** [P] Alinhar a versão do EF no `.csproj` de testes (reproduzir o aviso antes e
  conferir que sumiu depois). *AC-17 · §5.*
  **Pronto quando:** `dotnet build` sem o aviso de conflito de versão.
- [ ] **T12** [P] Favicon 64×64 PNG ≤ 20 KB, mesmo nome e mesma imagem (PowerShell +
  System.Drawing, script no scratchpad). *AC-18.*
  **Pronto quando:** `favicon.png` ≤ 20 KB e a imagem conferida visualmente.

## Bloco E — Processo
- [ ] **T14** [P] Regra 4 da Constitution (STATE.md) e `docs/GUIA-ORQUESTRACAO-SDD.md` descrevem o
  `/sdd` (fases + 2 aprovações) e mantêm o OpenCode. *AC-19 · §6.*
  **Pronto quando:** os dois textos citam `/sdd` e `@spec`.

## Verificação

- [ ] **T13** Verificação ao vivo (backend e frontend locais, banco real), com contas de teste
  `teste.cpd.*`, criadas e apagadas por script no scratchpad:
  - AC-01..04: o alerta chega a quem vê, não chega a quem não vê, chega ao Admin e não chega ao
    Solicitante (forçando um chamado de teste perto do prazo);
  - AC-06/07 via API;
  - AC-09/10/12/13/14 em duas abas (A e B);
  - AC-23 com chamada sem `If-Match`.
- [ ] **T15** E2E: trocar "Categoria" por Área/Tipo em `chamados.spec.ts` e `fluxo-completo.spec.ts`;
  em `admin.spec.ts`, trocar a página `/admin/categorias` pela de Tipos de chamado; títulos com
  `[TESTE-E2E]`; novo `e2e/conflito.spec.ts`: abre o detalhe, altera o chamado por fora (API) e
  clica numa ação → mensagem do AC-10 e dados recarregados; o mesmo no Kanban (AC-14). Rodar a suíte
  inteira. *AC-10, AC-14, AC-15, AC-16, AC-21 (parte de tela).*
  **Pronto quando:** todos passam. **PARADA:** listar os dados `[TESTE-E2E]`/`teste.cpd.*` criados
  e só apagar com o OK do usuário.
- [ ] **T16** Gates (`dotnet build`, `dotnet test`, `npm run build`) e marcação da rastreabilidade
  na spec (AC → teste). *AC-20, AC-21, AC-22.*

> ROADMAP: registrar no close a futura feature **"Editar chamado"** (modal; Solicitante: os que abriu;
> Atendente: os que assumiu; Admin: todos), que leva o AC-11.

---

## Analyze (2026-10-02)

| Checagem | Resultado |
|---|---|
| Todo AC tem tarefa | Sim. AC-01..05 → T02–T04, T13; AC-06..08 → T01; AC-09/10/12/13/23 → T05–T09, T13; AC-14 → T10, T15; AC-15..18 → T11, T12, T15; AC-19 → T14; AC-20..22 → T16. **AC-11 adiado** (decisão do usuário) |
| Toda tarefa aponta para AC existente | Sim |
| Contratos C1–C7 com tarefa e teste | C1 T05/T08; C2 T07/T08; C3 T07; C4 T01/T02/T07; C5 T03/T04; C6 T07; C7 T01 |
| Nada contradiz "Fora de escopo" | Sim: migrations, Categorias e permissão de editar intocadas |
| Constitution | Sem violação. Parada da T15 (dados no banco real) prevista |
| Corrigido no analyze | AC-21 exige teste automatizado também da parte de tela de AC-10/AC-14 → `e2e/conflito.spec.ts` na T15 |
