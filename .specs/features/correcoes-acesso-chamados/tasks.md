# Correções de Acesso a Chamados — Tasks

> **Branch:** `feature/correcoes-acesso-chamados`
> **Spec:** `spec.md` (AC-01 a AC-05). As decisões técnicas estão na seção 6 da spec, sem `design.md`, por ser uma feature pequena.
> **Ferramenta:** Claude Code (Opus 5.5): spec, tasks e implementação na sessão principal; review por sub-agente independente
> **Gate checks:** `dotnet build` + `dotnet test tests/ChamadosCamarj.UnitTests/` + `npm --prefix frontend run build`

- [ ] **T01.** `AtualizarChamadoCommandHandler` usa `ObterPorIdComTrackingAsync` *(AC-01)*
      **Pronto quando:** teste novo `AtualizarChamadoHandlerTests` verifica que o handler busca com tracking e persiste o título novo. Verde.
- [ ] **T02.** `ToResponse(bool incluirInternos = true)`; `ListarChamadosQueryHandler` e `ObterChamadoPorIdQueryHandler` passam `false` para Solicitante *(AC-02)*
      **Pronto quando:** testes cobrem a contagem para Solicitante (só públicos) e para Atendente (total). Verde.
- [ ] **T03.** `ChamadosHub.OnConnectedAsync` coloca Atendente/Admin no grupo `Atendimento`; remove `EntrarGrupo`/`SairGrupo`; `SlaMonitorService` envia para `Atendimento` *(AC-03, AC-04)*
      **Pronto quando:** compila; teste do hub verifica que Solicitante não entra no grupo `Atendimento` e Atendente entra.
- [ ] **T04.** Verificação ao vivo da edição (AC-01) com chamado de teste, apagado no final *(AC-01)*
      **Pronto quando:** PUT por Atendente devolve 204 e o título muda.
- [ ] **T05.** Gates *(AC-05)*
      **Pronto quando:** os três verdes.

## Analyze (2026-10-01)
AC-01 → T01/T04 · AC-02 → T02 · AC-03/AC-04 → T03 · AC-05 → T05. A remoção de `EntrarGrupo`/`SairGrupo` é mudança de contrato sem consumidores (verificado). Nada contradiz o "Fora de escopo".
