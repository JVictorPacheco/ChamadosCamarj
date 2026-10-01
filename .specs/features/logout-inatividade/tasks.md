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
| recarregar a tela de login | a mensagem não reaparece ✅ |
| 2 abas: A trabalhando, B parada há 30 min | as duas continuam logadas ✅ |
| 2 abas: nenhuma ativa há 21 min | as duas saem ✅ |

Durante a verificação, o teste de duas abas falhou na primeira tentativa por **erro do próprio teste**: o relógio do Playwright é um só por contexto, e ele foi adiantado duas vezes por passo. Corrigido o teste, o cenário passou. O código não mudou.
