# Logout por Inatividade — Especificação

> **SDD:** em-review
> **Status:** `Em andamento`
> **Branch:** `feature/logout-inatividade`
> **Criada em:** 2026-10-01
> **Atualizada em:** 2026-10-01
> **Aprovação:** a regra (20 minutos, desligar automaticamente) foi decidida e confirmada pelo
> usuário em 2026-07-18 (`.specs/features/fase-6-admin-log/design-t09-google-oauth.md`). Em
> 2026-10-01 o usuário pediu para tratar esta pendência e delegou a execução. Os itens marcados
> como **proposta** abaixo são detalhes de experiência reversíveis, sinalizados para revisão.

---

## 1. Problema

**Situação atual:**
O logout automático por inatividade foi decidido em 2026-07-18 ("protege contra alguém deixar o
computador desbloqueado com a aba aberta"), e o código que faz isso existe
(`frontend/src/hooks/useInactivityLogout.ts`). Só que ele **nunca foi ligado**: foi criado no
commit `8166ff0` e nenhuma tela o usa. Hoje uma sessão fica aberta até o token expirar, que leva
horas.

**Impacto:**
Um computador desbloqueado no setor deixa qualquer pessoa ver e agir nos chamados de quem saiu,
inclusive como Admin.

**Solução esperada:**
Depois de 20 minutos sem nenhuma interação, o usuário é desconectado e volta para a tela de login
com um aviso do motivo.

---

## 2. Fora de Escopo

- Mudar a duração do token ou criar renovação de sessão.
- Tornar o tempo configurável pelo Admin.
- Aviso prévio do tipo "você será desconectado em 1 minuto, deseja continuar?" (**proposta** para
  uma próxima versão, se o usuário quiser).

---

## 3. User Stories

| ID | User Story |
|----|------------|
| US-01 | Como **CAMARJ**, quero que uma sessão esquecida aberta se encerre sozinha, para que ninguém use o acesso de outra pessoa num computador desbloqueado. |
| US-02 | Como **usuário**, quero continuar conectado enquanto estou trabalhando, mesmo em várias abas, para não ser interrompido. |

---

## 4. Critérios de Aceitação

- **AC-01:** Dado um usuário logado, quando passam **20 minutos** sem mouse, teclado, clique ou
  rolagem em **nenhuma** aba do sistema, então ele é desconectado e levado para a tela de login.
- **AC-02:** Dado um usuário desconectado por inatividade, quando a tela de login aparece, então
  ela mostra a mensagem **"Sua sessão foi encerrada por inatividade. Entre novamente."**
  (*proposta de texto*).
- **AC-03:** Dado um usuário com o sistema aberto em duas abas, quando ele trabalha em uma e deixa
  a outra parada, então **não** é desconectado: atividade em qualquer aba conta para todas.
- **AC-04:** Dado um usuário usando o sistema, quando a tela se atualiza sozinha (tempo real,
  chat, contadores), então isso **não** conta como atividade, e também **não** atrasa nem adianta
  o desligamento.
- **AC-05:** Vale para os três perfis. A tela de login não é afetada.
- **AC-06:** `npm run build` passa sem erros.

---

## 5. Rastreabilidade

| Critério | Verificação | Resultado |
|----------|-------------|-----------|
| AC-01, AC-04 | Playwright com relógio simulado: 19 min (fica) · mouse + 19 min (fica) · 21 min (sai) | ✅ |
| AC-02 | Mensagem na tela de login; não reaparece ao recarregar | ✅ |
| AC-03 | Duas abas: A ativa e B parada há 30 min (ficam) · nenhuma ativa há 21 min (saem) | ✅ |
| AC-05 | Hook só no `AppLayout` (área logada) | ✅ por código |
| AC-06 | `tsc -b` + `vite build` + lint | ✅ |

---

## 6. Decisões Técnicas

| Decisão | Alternativa considerada | Motivo da escolha |
|---------|------------------------|-------------------|
| O hook guarda o callback numa `ref` e o efeito depende só de `minutos` | Exigir `useCallback` de quem chama | Hoje o `sair` do `AppLayout` é recriado a cada renderização. Com o callback como dependência, o timer reiniciaria a cada atualização da tela e **nunca dispararia** (AC-04) |
| Última atividade gravada em `localStorage` (no máximo 1 vez a cada 15 s) e conferida quando o timer vence; se outra aba teve atividade recente, reagenda pelo tempo que falta | Um timer independente por aba | O token é compartilhado entre abas: uma aba esquecida deslogaria quem está trabalhando em outra (AC-03) |
| Ligado no `AppLayout` (só existe com usuário logado) | No `AuthProvider` | O `AuthProvider` também envolve a tela de login (AC-05) |
| Testes: verificação manual com o tempo reduzido + `npm run build` | Testes unitários de frontend | O projeto não tem testes unitários de frontend (decisão registrada em `TESTING.md`) |

**Pontos de toque cross-feature:** `AppLayout` (compartilhado por todas as telas autenticadas):
só ganha a chamada do hook e o `navigate` com o motivo; `LoginPage`: só ganha a mensagem.

---

## 7. Dependências

- Nenhuma.

---

## 8. Gate Checks

- [ ] `npm run build` — 0 erros
- [ ] `dotnet test` — sem regressão (não há mudança de backend)
- [ ] ACs verificados
- [ ] STATE, ROADMAP e Obsidian (`Acesso e Login`) atualizados
- [ ] PR aberto com base `develop`
