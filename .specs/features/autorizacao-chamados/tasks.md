# Autorização de Chamados no Servidor — Tasks

> **Branch:** `feature/autorizacao-chamados`
> **Spec:** `spec.md` (AC-01 a AC-21) | **Design:** `design.md`
> **Ferramenta:** Claude Code (Opus 5.5): spec, design, tasks e implementação na sessão principal; review por sub-agente independente
> **Gate checks:** `dotnet build` + `dotnet test tests/ChamadosCamarj.UnitTests/` + `npm --prefix frontend run build`

Cada tarefa: `[ ]` pendente · `[x]` feita · *(ACs)* que atende · **Pronto quando:** critério verificável.

---

## Backend

### Fundação (Domain + contexto do usuário)
- [x] **T01.** `AcaoChamado` (enum) e `ContextoAcesso` (record) no Domain *(AC-01..AC-11)*
      **Pronto quando:** compila.
- [x] **T02.** `ICurrentUserService.Email` + `CurrentUserService` lê o claim `email`; helper `ObterContextoAcesso()` *(AC-12)*
      **Pronto quando:** compila; mocks existentes de `ICurrentUserService` continuam compilando.

### Política e behavior (Application)
- [x] **T03.** `ChamadoPermissoes.Pode(acao, acesso, solicitanteEmailDoChamado)` conforme a tabela do design *(AC-05, AC-06, AC-07, AC-09, AC-10, AC-11, AC-20)*
      **Pronto quando:** `ChamadoPermissoesTests` cobre cada ação × Solicitante/Atendente/Admin, incluindo Cancelar (dono × não dono) e Admin com grupo. Verde.
- [x] **T04.** `IRequerAcessoChamado` + `AcessoChamadoBehaviour` (404 se não vê; 403 se não pode), registrado depois do `ValidationBehaviour` *(AC-03, AC-05, AC-16)*
      **Pronto quando:** `AcessoChamadoBehaviourTests` cobre: request sem a interface passa direto; não vê → `NotFoundException` sem chamar o handler; vê mas não pode → `ForbiddenException` sem chamar o handler; pode → handler chamado. Verde.

### Repositório (Infrastructure) — ⚠️ mudança de contrato aprovada
- [x] **T05.** `AplicarVisibilidade` único no `ChamadoRepository`; `ListarAsync` passa a receber `ContextoAcesso` (remove o ramo antigo de grupo e o bypass do `solicitanteEmail`); `PodeVerAsync` novo *(AC-01, AC-02, AC-08, AC-11)*
      **Pronto quando:** compila. A cobertura por perfil vem da T07 (handler) e da verificação manual T16: o projeto de testes não tem provedor EF, ver "Limitação de teste" abaixo.
- [x] **T06.** Contagens do Dashboard recebem `ContextoAcesso` e usam `AplicarVisibilidade` *(AC-13)*
      **Pronto quando:** compila; Admin passa pelo filtro sem restrição (`if Admin return q`).

### Commands, queries e controllers
- [x] **T07.** `ListarChamadosQuery` carrega `ContextoAcesso`; o handler repassa ao repositório *(AC-01, AC-02, AC-08, AC-11)*
      **Pronto quando:** `ListarChamadosQueryHandlerTests` atualizado (verifica que o contexto do usuário logado chega ao repositório) e verde.
- [x] **T08.** As 6 queries de leitura implementam `IRequerAcessoChamado` (Ver): `ObterChamadoPorId`, `ListarComentarios`, `ListarHistorico`, `ListarAnexos`, `ObterUrlDownloadAnexo` *(AC-03, AC-04, AC-16)*
      **Pronto quando:** compila e os testes de handler existentes passam.
- [x] **T09.** Os 13 commands de ação implementam `IRequerAcessoChamado` com a ação certa (Comentar: Interno ou Público conforme o flag; Atualizar: Editar) *(AC-05, AC-06, AC-07, AC-09, AC-10, AC-20)*
      **Pronto quando:** compila e os testes de handler existentes passam.
- [x] **T10.** `ChamadosController`: passa `ObterContextoAcesso()` a todos os requests; `Abrir` usa nome e e-mail do usuário logado *(AC-12)*
      **Pronto quando:** `AbrirChamadoHandlerTests` continua verde; o controller não lê mais `request.SolicitanteNome/Email`.
- [x] **T11.** Dashboard: queries recebem `ContextoAcesso`; Solicitante → 403 *(AC-13)*
      **Pronto quando:** testes de handler do Dashboard atualizados e verdes.
- [x] **T12.** `RelatoriosController`: Solicitante → 403; Atendente → `responsavelId` = próprio id *(AC-21)*
      **Pronto quando:** compila; lógica coberta na verificação manual T16.
- [x] **T13.** SignalR: payloads de `ComentarioAdicionado`, `ChamadoCriado`, `SlaAtencao`, `SlaAtrasado` sem conteúdo nem título *(AC-19)*
      **Pronto quando:** compila; os testes de `WebApi/Notifications` passam (ajustados se verificavam o payload).

## Frontend

- [ ] **T14.** `ChamadoDetailPage`: remove o bloqueio local (linha ~299); botão Cancelar para Solicitante só se ele abriu; 404 mostra "Chamado não encontrado" *(AC-04, AC-06)*
      **Pronto quando:** `npm run build` sem erros.
- [ ] **T15.** `ChamadosListPage` e `ArquivoChamadosPage` param de mandar `solicitanteEmail`; `AbrirChamadoPage` para de mandar nome e e-mail; `useSignalR`/`AppLayout`/`signalr-events.ts` ajustados aos payloads novos *(AC-02, AC-12, AC-19)*
      **Pronto quando:** `npm run build` sem erros.

## Validação

- [ ] **T16.** Verificação por perfil (API local apontando para o Supabase, só leitura e ações em chamados de teste): Solicitante sem grupo, Solicitante com grupo, Atendente sem grupo, Atendente com grupo, Admin com grupo. Lista, detalhe, ação proibida, Kanban, Dashboard, Relatório *(AC-01..AC-14, AC-19..AC-21)*
      **Pronto quando:** cada AC tem resultado anotado na seção Rastreabilidade da spec. **Exige contas de teste no banco real: pedir autorização ao usuário antes de criar ou alterar qualquer conta.**
- [ ] **T17.** Gates: `dotnet build`, `dotnet test`, `npm --prefix frontend run build` *(AC-17, AC-18)*
      **Pronto quando:** os três verdes, com o número de testes anotado.

---

## Limitação de teste (registrada no analyze)

O projeto de testes usa Moq e não tem provedor EF (nem InMemory nem SQLite), então o filtro SQL de
visibilidade (T05) não tem teste automatizado. Mitigação: a regra fica concentrada em **um**
método; a política de ações (T03) e o behavior (T04) são testados de forma pura; e a T16 verifica
os 5 perfis de ponta a ponta. Adicionar um provedor EF de teste é uma mudança de infraestrutura de
teste, fora do escopo desta feature, e fica registrada como sugestão no fechamento.

## Analyze — consistência spec ↔ design ↔ tasks (2026-10-01)

| Checagem | Resultado |
|---|---|
| Todo AC tem tarefa | AC-01..AC-21 cobertos (AC-15 por T04; AC-17/18 por T17) |
| Toda tarefa aponta para AC | Sim |
| Toda mudança de contrato do design tem tarefa | `IChamadoRepository` → T05/T06; `ICurrentUserService` → T02; commands/queries → T07..T09; frontend → T14/T15 |
| Nada contradiz "Fora de escopo" | Relatório: só proteção (AC-21), o conteúdo não muda. Chat não é tocado |
| Constitution | Sem violação; a limitação de teste está registrada acima |
