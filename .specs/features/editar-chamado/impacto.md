# Análise de impacto — editar-chamado

> **Diff:** `origin/develop...feature/editar-chamado` — 19 arquivos de código (+ specs) · **Data:** 2026-10-02
> **Gates:** `dotnet build` 0 erros (avisos só pré-existentes, ex.: CS8634 — corrigido após o review; com os servidores parados — com o backend rodando, o build acusa MSB3021/3027 de cópia de DLL travada, não de código) · `dotnet test` **474/474** · `npm run build` ok · E2E **17/17** · verificação ao vivo **23/23**
> **Veredito:** IMPACTO COBERTO — nenhum consumidor fora do escopo com comportamento alterado sem teste.

## Resumo
- 🔴 0 · 🟡 4 · ✅ 9

## 🔴 Impacto sem cobertura
Nenhum.

## 🟡 Impacto coberto

| Item alterado | Consumidor fora do escopo | O que muda para ele | Teste que cobre |
|---|---|---|---|
| `ChamadoPermissoes.Pode` — nova assinatura com `DadosDoChamado`; caminho do Admin agora passa por `PodeEditar` antes do `return true` | Todas as 14 ações de chamado (autorizacao-chamados) | Nada para as ações ≠ `Editar`: o `switch` é o mesmo, só sem `Editar` na lista do Atendente | `ChamadoPermissoesTests`: `Admin_ComGrupo_PodeTodasAsAcoes` (todas as ações do enum), `Atendente_PodeAcoesDeAtendimento`, `Atendente_NaoPodeAcoesExclusivasDoAdmin`, `Solicitante_PodeVer_EhCoberturaCompletaDoEnum` — rodando contra a classe real, sem mock |
| `AcessoChamadoBehaviour` (pipeline de todo request de chamado) — consulta o chamado quando `DependeDoChamado` | Todos os commands/queries `IRequerAcessoChamado` | Só `Editar` (sempre) e `Cancelar` de Solicitante (como antes) consultam o chamado; as demais seguem sem consulta extra | `AcoesQueNaoDependemDoChamado_NaoFazemConsultaExtra`; `Solicitante_CancelaOProprioChamado`, `Solicitante_DoGrupo_NaoCancelaChamadoDoColega` (Cancelar inalterado); `ChamadoQuePodeVer_SemPermissaoParaAcao_DaForbidden...` (403 inalterado) |
| `AcaoHistorico.ChamadoEditado` (valor novo, gravado como texto) | Relatório mensal (`HistoricoRepository.ObterEventosParaRelatorioAsync`) | Nenhuma: o filtro `acoesList.Contains(h.Acao)` é feito no SQL (`HistoricoRepository.cs:53`), só Criado/Resolvido/Cancelado chegam a ser lidos | **Só leitura de código:** `HistoricoRepository` não foi alterado; os testes do relatório usam o repositório simulado (mock) e não cobrem o SQL. Comportamento inalterado, por isso não é 🔴 |
| `TimelineHistorico` — ramo novo para `ChamadoEditado` | Histórico de todas as outras ações no detalhe | Nenhuma: as outras ações caem no mesmo JSX de antes (`detalheAnterior → detalheNovo`) | `npm run build` (o `Record<AcaoHistorico,…>` exige todos os rótulos); E2E abre o detalhe com histórico (`editar-chamado.spec.ts`, `fluxo-completo.spec.ts`) |

## ✅ Itens sem consumidor fora do escopo
- `Chamado.AtualizarDados` (retorna `bool`, bloqueia encerrado, não muda versão sem mudança) — único consumidor: `AtualizarChamadoCommandHandler` (grep em `src` e `tests`).
- `ChamadoPermissoes.DependeDoSolicitante` → `DependeDoChamado` — único consumidor: `AcessoChamadoBehaviour`.
- `AtualizarChamadoCommand` + `UsuarioId`/`UsuarioNome` (opcionais) — consumidores: `ChamadosController.Atualizar` e testes.
- `AtualizarChamadoCommandHandler` (transação + histórico) — só a rota `PUT /chamados/{id}`, que nenhuma tela usava antes.
- `PUT /chamados/{id}` passa a responder 400 (encerrado) e 403 (regra nova) — nenhuma tela ou teste anterior chamava a rota.
- `api.ts` `atualizarChamado`, `useAtualizarChamado`, `lib/permissoes.ts`, `EditarChamadoModal` — novos.
- `types/api.ts` `AcaoHistorico` — único consumidor que enumera: `TimelineHistorico` (grep em `frontend/src`).
- `ChamadoDetailPage`/`BotoesAcao` — botão novo antes dos demais; os seletores dos E2E existentes (`Assumir`, `Resolver`, `Cancelar`...) continuam únicos (17/17).

## Mudanças intencionais de comportamento (no escopo, aprovadas — registradas para não parecerem regressão)
- Atendente que vê o chamado mas não é o responsável **perde** a edição pela API (C2).
- Admin **perde** a edição de chamado encerrado (AC-10).
- 2 testes da regra antiga de `Editar` foram trocados pelos da regra nova (design §8).

## Risco fora do código (registrado no design)
- **Versão antiga em produção:** o backend atual de produção não conhece `ChamadoEditado`; ao listar o
  histórico de um chamado editado daria erro de conversão. Hoje só existem entradas assim em
  chamados de teste (`[TESTE-EDIT]`, `[TESTE-E2E]`) criados nesta sessão → **apagar antes de encerrar**.
  Depois do deploy (backend + frontend juntos) o risco deixa de existir.

## Previsto × real
- Pontos de toque do design tocados: `ChamadoPermissoes`, `AcessoChamadoBehaviour`, `Chamado.AtualizarDados`,
  `TimelineHistorico`, `ChamadoDetailPage`/`BotoesAcao`, `types/api.ts` — todos previstos.
- Tocados e não listados na tabela de cross-feature, mas previstos em outra seção do design: `ChamadosController`
  (C4) e `useAcoesChamado.ts` (§4). Nenhum arquivo fora do previsto.

## Re-análise após as correções do review (2026-10-02)
Arquivos alterados depois do review: `EditarChamadoModal.tsx` (R-01 versão adotada uma vez só; R-02
campos não mudados vão com o valor atual; "Salvar" desabilitado aguardando recarga) e
`AtualizarChamadoCommandHandler.cs` (`InvalidOperationException` do domínio vira 400 com a mensagem
do AC-10). Consumidores: o modal só é usado pelo `BotoesAcao`; o handler só pela rota `PUT
/chamados/{id}`. **Nenhum consumidor novo fora do escopo — continua 🔴 0.** Testes: 475/475
(+ `Handle_ChamadoEncerradoNoMeioDoCaminho_DaBadRequestComAMensagem`); E2E reexecutado no close.
