# Spec — Forçar Encerramento (Admin)

> Status: IMPLEMENTADA (2026-07-19) — verificada via curl contra o Supabase real; falta só clique real no navegador (bloqueado pelo Client ID do Google, mesma pendência de sempre)
> Criado em: 2026-07-19
> Item pendente da Fase 6 (`.specs/project/ROADMAP.md`, `docs/obsidian/👥 Perfis de Usuário.md`). Retomado nesta sessão enquanto o login Google real (T09/F5b) aguarda o Client ID da TI.

---

## Problem Statement

Hoje, `Chamado.Fechar()` só aceita fechar um chamado que já passou por `Resolvido` (`Chamado.cs`, linha 99), e nenhum perfil — nem Admin — tem uma forma de pular esse fluxo. Na prática, chamados abertos por engano, duplicados, ou "empacados" em `Aberto`/`EmAndamento` sem que ninguém os resolva formalmente ficam presos no sistema sem uma saída limpa. O Admin precisa de uma ação excepcional para encerrar esses casos, mas isso não pode virar uma porta para encerrar chamados sem deixar rastro — o projeto já trata auditoria (`HistoricoEntrada`) como não-negociável (ver Fase 6, Relatório Mensal, Arquivo de Chamados).

## Contexto (decisões confirmadas com o usuário em 2026-07-19)

| Decisão | Escolha |
|---|---|
| De quais status pode forçar | **Qualquer status não-final** — Aberto, EmAndamento ou Resolvido — direto para Fechado, pulando a exigência normal de passar por Resolvido |
| Justificativa | **Obrigatória.** Sem motivo preenchido, a ação é rejeitada |
| Auditoria | O motivo é registrado no `HistoricoEntrada`, no mesmo padrão usado por Reatribuir/AlterarPrioridade/etc |
| Quem pode | Só Admin |

## Out of Scope

| Item | Motivo |
|---|---|
| Permitir Cancelar um chamado já Fechado | Não foi pedido — `Cancelar()` continua com as regras atuais (bloqueado a partir de Fechado/Cancelado) |
| Reabrir um chamado forçadamente encerrado | `Chamado.Reabrir()` já existe e já cobre esse caso (Resolvido/Fechado/Cancelado → Aberto) — não duplicar |
| Editar/apagar uma entrada de histórico já criada | Auditoria é append-only no projeto inteiro, sem exceção aqui |
| Notificação especial (email/push) para encerramento forçado | Não pedido; o SignalR já notifica mudança de status genericamente (`StatusAlteradoNotification`) |

---

## User Stories

### P1: Admin força o encerramento de um chamado fora do fluxo normal ⭐ MVP

**User Story**: Como Admin, quero fechar um chamado diretamente (de Aberto, EmAndamento ou Resolvido), informando um motivo, para encerrar casos excepcionais (duplicado, aberto por engano, sem dono) sem deixar o chamado preso e sem perder rastreabilidade.

**Why P1**: É o pedido inteiro desta feature — sem isso não há entrega.

**Acceptance Criteria**:

1. WHEN um Admin aciona "Forçar Encerramento" num chamado `Aberto`, `EmAndamento` ou `Resolvido`, informando um motivo THEN o sistema SHALL fechar o chamado (`Status = Fechado`) e registrar uma entrada em `HistoricoEntrada` com o motivo informado
2. WHEN o motivo não é informado (vazio ou só espaços) THEN o sistema SHALL rejeitar a ação com erro de validação, sem alterar o chamado
3. WHEN um usuário não-Admin (Atendente ou Solicitante) tenta forçar o encerramento THEN o sistema SHALL responder 403, mesmo chamando o endpoint diretamente (não só esconder o botão)
4. WHEN o chamado já está `Fechado` ou `Cancelado` THEN o sistema SHALL rejeitar a ação — não há o que "forçar" num chamado já finalizado
5. WHEN o encerramento forçado é aplicado a partir de `Aberto`/`EmAndamento` (sem `DataConclusao` prévia) THEN o sistema SHALL preencher `DataConclusao` com o momento do encerramento, pelos mesmos motivos que o Relatório Mensal e o Arquivo de Chamados dependem dessa data para chamados finalizados

**Independent Test**: Como Admin, abrir um chamado de teste, forçar o encerramento direto de `Aberto` com um motivo, e confirmar: (a) o chamado aparece como `Fechado` no Kanban/Arquivo, (b) o histórico do chamado mostra a entrada com o motivo, (c) tentar a mesma ação como Atendente retorna 403.

---

## Edge Cases

- WHEN o motivo é muito curto (ex: "ok", "x") THEN o sistema SHALL exigir um mínimo de caracteres — motivo precisa ser uma justificativa real, não um preenchimento vazio de sentido (ver Notas Técnicas para o valor escolhido)
- WHEN o motivo é excessivamente longo THEN o sistema SHALL aplicar um limite máximo, no mesmo padrão dos outros campos de texto do projeto (ex: descrição de chamado, comentário)
- WHEN o encerramento forçado acontece a partir de `Resolvido` (já tinha `DataConclusao`) THEN o sistema SHALL preservar a `DataConclusao` original, não sobrescrever com o momento do encerramento forçado
- WHEN a ação é bem-sucedida THEN a UI SHALL atualizar o card/detalhe do chamado e notificar via SignalR, igual às outras mudanças de status

---

## Requirement Traceability

| Requirement ID | Story | Phase | Status |
|---|---|---|---|
| FORC-01 | P1: Encerrar direto de qualquer status não-final | T2, T4 | Verified |
| FORC-02 | P1: Motivo obrigatório, validado (min/max) | T3 | Verified |
| FORC-03 | P1: Só Admin, bloqueio real (não só UI) | T4 | Verified |
| FORC-04 | P1: Auditoria no HistoricoEntrada | T1, T4 | Verified |
| FORC-05 | P1: Bloqueado se já Fechado/Cancelado | T2, T4 | Verified |
| FORC-06 | P1: DataConclusao preenchida só se ainda não existir | T2 | Verified |

**Coverage:** 6 total, 6 mapped e verificados (testes automatizados + curl manual contra o Supabase real). Ver `tasks.md` (T9) pro detalhe da verificação e pro bug de auditoria (`UsuarioId` zerado) encontrado e corrigido no processo.

---

## Notas Técnicas (para a fase de Design)

- **Guard de Admin real**: hoje `Reatribuir`/`AlterarPrioridade`/`Fechar`/`Cancelar` **não têm nenhuma checagem de perfil no backend** — só o frontend esconde os botões (mesmo "RBAC soft" documentado como pendência em `STATE.md`, item 6). Forçar Encerramento é uma ação mais sensível (bypassa um invariante de domínio), então precisa de um guard real desde o início — reaproveitar `PerfilRequisitanteGuard.ExigirAdmin()` (`src/ChamadosCamarj.Application/Common/Authorization/PerfilRequisitanteGuard.cs`), hoje usado só em `Usuarios`, seguindo o mesmo padrão: o Controller passa `_currentUser.Perfil` no command, o Handler chama o guard antes de tocar no agregado.
- **Ação de histórico nova**: `AcaoHistorico` (enum) precisa de um valor novo, `EncerramentoForcado`. Como a coluna é `HasConversion<string>()` com `HasMaxLength(30)` (ver `HistoricoEntradaConfiguration.cs`), adicionar um valor ao enum **não exige migration** — é só uma string nova dentro do limite de 30 caracteres. `"EncerramentoForcado"` tem 19 caracteres, cabe.
- **Onde guardar o motivo**: reaproveitar `HistoricoEntrada.DetalheNovo` (já é `text`, sem limite de banco) — sem coluna nova, sem migration.
