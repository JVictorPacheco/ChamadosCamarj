# Logout por Inatividade — Tasks

> **Branch:** `feature/logout-inatividade`
> **Spec:** `spec.md` (AC-01 a AC-06). As decisões técnicas estão na seção 6 da spec, sem `design.md`, por ser uma feature pequena.
> **Ferramenta:** Claude Code (Opus 5.5): spec, tasks e implementação na sessão principal; review por sub-agente independente
> **Gate checks:** `npm --prefix frontend run build` (inclui `tsc -b`) + `npm run lint` + `dotnet test` (sem mudança de backend)

- [x] **T01.** `useInactivityLogout`: callback numa `ref`, efeito depende só de `minutos`; última atividade compartilhada entre abas via `localStorage` (gravação no máximo a cada 15 s) e conferida quando o timer vence *(AC-01, AC-03, AC-04)*
      **Pronto quando:** `tsc -b` e build sem erros.
- [x] **T02.** `auth/logoutInatividade.ts` (constante de 20 min + aviso do motivo em `sessionStorage`); `AppLayout` liga o hook; `LoginPage` mostra a mensagem uma vez *(AC-01, AC-02, AC-05)*
      **Pronto quando:** build e lint sem avisos novos.
- [x] **T03.** Verificação na tela com o relógio simulado do Playwright (API e frontend locais, conta de teste apagada no final) *(AC-01..AC-05)*
      **Pronto quando:** cenários abaixo passam.
- [x] **T04.** Gates *(AC-06)*

## Resultado da verificação (2026-10-01)
| Cenário | Resultado |
|---|---|
| 19 min parado | continua logado ✅ |
| mexe o mouse + 19 min parado | continua logado (a contagem reinicia) ✅ |
| 21 min sem interação | volta para o login, com a mensagem; token e perfil apagados ✅ |
| recarregar a tela de login | ~~a mensagem não reaparece~~ comportamento mudou após o review: a mensagem fica até o próximo login ✅ |
| 2 abas: A trabalhando, B parada há 30 min | as duas continuam logadas ✅ |
| 2 abas: nenhuma ativa há 21 min | as duas saem ✅ |
| **Após o review:** 21 min só com eventos `scroll` automáticos | sai ✅ (R-01) |
| **Após o review:** reabrir com última atividade de 25 min atrás | sai na hora, com a mensagem ✅ (R-02) |
| **Após o review:** login novo com marca antiga (3 h) no navegador | continua logado ✅ |
| **Após o review:** a mensagem aparece nos dois caminhos e some no próximo login | ✅ |

Durante a verificação, o teste de duas abas falhou na primeira tentativa por **erro do próprio teste**: o relógio do Playwright é um só por contexto, e ele foi adiantado duas vezes por passo. Corrigido o teste, o cenário passou. O código não mudou.

## Rodada 2 do review (2026-10-01): correções aplicadas, verificação na tela PENDENTE
- R2-01 🔴 (suspensão do computador renovava a sessão): todo gesto e a volta para a aba
  (`focus`/`visibilitychange`) medem o tempo parado pelo relógio de parede antes de renovar.
- R2-02 🟡 (reabertura vencida montava a área logada por um instante): a rota protegida checa antes
  de montar o `AppLayout`; nenhuma tela, API nem heartbeat do chat roda.
- R2-03 🟡 (aviso de inatividade num "Sair" normal): o `AppLayout` apaga o aviso ao montar.
- `npm run build` e lint ok. ~~A verificação na tela dos cenários B (suspensão), C (reabertura) e D
  (Sair após aviso antigo) foi interrompida:~~ **Retomada e concluída em 2026-10-02 (abaixo).** Interrupção original: o sistema ficou sem memória (0,5 GB livres) e o
  Claude Code encerrou a API e o frontend locais. Os dados de teste foram apagados (contagem 0).
  Falta: rodar os cenários B/C/D e a 3ª rodada de review antes do merge.

### Verificação da rodada 2 na tela (2026-10-02, Playwright com relógio simulado)
| Cenário | Resultado |
|---|---|
| A) regressão: atividade aos 19 min + 19 min / 21 min parado | fica / sai com aviso ✅ |
| B) relógio salta 90 min sem os timers dispararem (suspensão) + mexe o mouse | sai, com aviso ✅ (R2-01) |
| C) reabrir com última atividade de 25 min atrás, a partir de página em branco | vai direto ao login com aviso e **0 chamadas à API** ✅ (R2-02) |
| D) aviso antigo na aba + sessão ativa + "Sair" normal | login **sem** o aviso ✅ (R2-03) |
