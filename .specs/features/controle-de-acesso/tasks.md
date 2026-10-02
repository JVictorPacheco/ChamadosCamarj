# Controle de Acesso por Módulo — Tasks

> **Branch:** `feature/controle-de-acesso`
> **Spec:** `spec.md` | **Design:** `design.md`
> **Ferramenta:** Claude Code (Opus 5.5) para spec, design, backend e frontend; `/analise-cod`; review por sub-agente novo
> **Gate checks:** `dotnet build` + `dotnet test tests/ChamadosCamarj.UnitTests/` + `npm --prefix frontend run build`
> **Contratos C1–C9 aprovados pelo usuário em 2026-10-02. Migration só é aplicada com novo OK.**

---

## Backend

- [x] **T01** `ModuloSistema` (`[Flags]`) + `ModulosDeAcesso` (`Padrao`, `Ajustaveis`, `Efetivos`,
  `CalcularAjustes`). *AC-02..AC-08, AC-16 · design §2.*
  **Pronto quando:** testes: padrões batem com a matriz atual (Solicitante = Arquivo; Atendente e Admin =
  os 5); Admin sempre os 5; ajuste dá Dashboard/Relatório a Solicitante; Kanban/Fila nunca para Solicitante
  (nem com ajuste gravado errado); `CalcularAjustes` mínimo e recusa módulo não ajustável.
- [x] **T02** `UsuarioPerfil` (+`ModulosConcedidos`/`ModulosRetirados`, `AjustarModulos`, `VoltarAoPadrao`),
  `AuditoriaAcesso` (entidade, configuração, repositório), migration `AddControleDeAcesso` (gerada, **não
  aplicada**). *AC-04, AC-14, AC-16 · C1.*
  **Pronto quando:** build ok; migration aditiva (só `AddColumn` com default 0 + `CreateTable`); testes de
  domínio.
- [ ] **T03** `Modulos` em `AutenticacaoResponse` e `UsuarioPerfilResponse` (login, `/auth/me`, Google,
  lista de Usuários). *AC-11, AC-13, AC-16 · C2.*
  **Pronto quando:** testes de login/mapping mostram os módulos efetivos.
- [ ] **T04** `ModuloGuard` + Dashboard (2 handlers) e Relatório (controller + `IdsVisiveis` +
  `ListarIdsVisiveisAsync`). *AC-07, AC-12 · C4, C5.*
  **Pronto quando:** testes: Solicitante sem módulo → 403 (como hoje); Solicitante com Dashboard → ok;
  Atendente sem Relatório → 403; Relatório do Solicitante filtrado pelos visíveis; Atendente continua só
  com os próprios.
- [ ] **T05** API `/api/acessos` (listar, detalhe, salvar, voltar ao padrão) com auditoria, Chat via
  `DefinirChatPerfilCommand` e `AcessosAtualizadosNotification` → SignalR. *AC-01..AC-06, AC-09, AC-14 ·
  C3, C6.*
  **Pronto quando:** testes dos handlers: só Admin; recusa alvo Admin/si mesmo/módulo não ajustável;
  auditoria uma linha por item mudado; notificação publicada; Chat delega ao comando existente.
- [ ] **T06** Mudança de perfil em Usuários zera ajustes (AC-15) + `ChatPerfil` opcional no
  `AtualizarUsuarioPerfilCommand`. *AC-10, AC-15 · C7.*
  **Pronto quando:** testes: perfil muda → ajustes zerados, auditoria e notificação; perfil igual → ajustes
  mantidos; `ChatPerfil` null não mexe no Chat; testes existentes do handler passando.

## Frontend

- [ ] **T07** Tipos e API (`modulos` no perfil; `/acessos`), `AuthContext.atualizarModulos`, evento
  `AcessosAtualizados` no `useSignalR`. *C2, C6.*
  **Pronto quando:** build ok.
- [ ] **T08** `AppLayout` por módulo + item "Controle de acesso"; `RequerModulo`/`RequerAdmin` nas rotas;
  aviso "Seu acesso a este módulo foi retirado.". *AC-05, AC-11..AC-13 · C8.*
  **Pronto quando:** build ok; verificação na tela (T12).
- [ ] **T09** `ControleAcessoPage` (lista com busca, painel por pessoa, padrão × ajustado, Chat, salvar,
  voltar ao padrão, auditoria; Admin "acesso total"). *AC-01..AC-04, AC-06, AC-08, AC-09, AC-14.*
  **Pronto quando:** build ok; verificação na tela (T12).
- [ ] **T10** Usuários sem os controles de Chat. *AC-10 · C9.*
  **Pronto quando:** build ok; E2E de admin passando.

## Verificação

- [ ] **T11** **PARADA:** aplicar a migration `AddControleDeAcesso` no banco real **só com OK do usuário**;
  conferir que nada mudou para os usuários atuais (AC-16).
- [ ] **T12** E2E `e2e/controle-de-acesso.spec.ts` (Admin ajusta um módulo de uma conta de teste e a tela
  da pessoa muda) + suíte inteira; verificação ao vivo/tela com contas `teste.acesso.*` (Solicitante com
  Dashboard, Atendente sem Relatório, menu mudando na hora, rota bloqueada, mudança de perfil zerando).
  Dados de teste apagados com OK do usuário. *AC-01..AC-16, AC-19.*
- [ ] **T13** Gates + `/analise-cod` (sem 🔴) + rastreabilidade. *AC-17..AC-19.*

---

## Analyze (2026-10-02)

| Checagem | Resultado |
|---|---|
| Todo AC tem tarefa | AC-01..AC-06 → T05, T09; AC-07/08 → T01, T04, T09; AC-09/10 → T05, T06, T10; AC-11..13 → T03, T07, T08; AC-14 → T02, T05, T09; AC-15 → T06; AC-16 → T01, T11; AC-17..19 → T12, T13 |
| Toda tarefa aponta para AC existente | Sim |
| Contratos C1–C9 com tarefa e teste | C1 T02/T11; C2 T03; C3 T05; C4/C5 T04; C6 T05/T07; C7 T06; C8 T08; C9 T10 |
| Nada contradiz "Fora de escopo" | Sim: Admin não ajustável; sem padrão por perfil editável; sem ações internas; Abrir/Meus fixos; Kanban/Fila nunca para Solicitante |
| Constitution | Paradas: T11 (migration no banco real) e T12 (limpeza); `/analise-cod` na T13 |
