# Editar Chamado — Design Técnico

> **Spec:** `spec.md` (aprovada em 2026-10-02)
> **Autor:** Claude Code (Opus 5.5), sessão única — design, backend e frontend
> **Data:** 2026-10-02

---

## 1. Visão Geral

| Bloco | ACs | Onde |
|---|---|---|
| A. Regra de quem edita (servidor) | AC-01..AC-11, AC-20, AC-22 | `ChamadoPermissoes`, `AcessoChamadoBehaviour` |
| B. Gravação + histórico | AC-16..AC-18 | `AtualizarChamadoCommand(Handler)`, `AcaoHistorico`, controller |
| C. Tela: botão + modal | AC-01..AC-15, AC-19..AC-21 | `ChamadoDetailPage`, `EditarChamadoModal` (novo), `api.ts`, hook, `lib/permissoes.ts` (novo) |
| D. Histórico na tela | AC-17, AC-18 | `TimelineHistorico`, `types/api.ts` |

Sem migration: `HistoricoEntrada.Acao` é gravado como texto (`HasConversion<string>`, 30 caracteres)
e `DetalheAnterior`/`DetalheNovo` são `text`.

---

## 2. Bloco A — Quem edita (servidor)

### 2.1 `ChamadoPermissoes`
- Novo record `DadosDoChamado(string SolicitanteEmail, Guid? ResponsavelId, StatusChamado Status)`.
- `Pode(AcaoChamado acao, ContextoAcesso acesso, DadosDoChamado chamado)` — **substitui** a
  assinatura atual `Pode(acao, acesso, string solicitanteEmail)` (contrato C1). Regras das demais
  ações **inalteradas** (Cancelar continua usando `SolicitanteEmail`).
- Regra nova de `Editar` (spec, "Regra de quem pode editar"):
  `!Encerrado(status) && (Admin || ResponsavelId == acesso.UsuarioId || (AbriuOChamado && ResponsavelId == null))`.
  `Encerrado` = Resolvido, Fechado ou Cancelado. Vale para qualquer perfil (AC-03: Atendente que
  abriu). Solicitante colega de grupo não é quem abriu → não edita (AC-04).
- `DependeDoChamado(acao)` (renomeia `DependeDoSolicitante`): `Cancelar` (como hoje, só para
  Solicitante) **e** `Editar` (sempre — precisa de status e responsável, inclusive para o Admin).
- `EdicaoBloqueadaPorEncerramento(DadosDoChamado)`: true se encerrado.

### 2.2 `AcessoChamadoBehaviour`
- Mantém a ordem: **404** se não vê (inalterado) → carrega o chamado quando `DependeDoChamado`
  (hoje só carregava para Solicitante + Cancelar) → para `Editar` em chamado encerrado lança
  `BadRequestException("Não é possível editar um chamado encerrado. Reabra o chamado para
  editá-lo.")` para **qualquer pessoa que vê**, inclusive Admin (AC-10) → `Pode` → **403** com a
  mensagem atual (AC-02, AC-06).
- O `VersaoChamadoBehaviour` continua **depois** → no AC-20 (B assumiu o chamado do Solicitante
  entre as tentativas) a nova tentativa recebe 403, não 409.

### 2.3 Domínio (defesa em profundidade)
- `Chamado.AtualizarDados` passa a lançar `InvalidOperationException` se encerrado (mesmo padrão de
  `AlterarPrioridade`). Não deve ser alcançado pela API (o behaviour barra antes).
- `Chamado.AtualizarDados` retorna `bool` = houve mudança (título ou descrição diferentes); se não
  houve, não toca `DataAtualizacao` (AC-16: versão não muda).

## 3. Bloco B — Gravação e histórico

- `AtualizarChamadoCommand` ganha `Guid? UsuarioId` e `string UsuarioNome` (padrão dos demais
  commands; controller preenche com `_currentUser`) (C4).
- Handler: carrega com tracking → `var mudou = chamado.AtualizarDados(...)`; se `!mudou`, retorna sem
  salvar nem registrar (AC-16). Senão, na mesma transação (`IUnitOfWork`, como `AlterarStatus`):
  `AtualizarAsync` + `RegistrarHistoricoAsync(AcaoHistorico.ChamadoEditado, detalheAnterior,
  detalheNovo, ...)`.
- `AcaoHistorico.ChamadoEditado = 13` (C3).
- Detalhe do histórico (AC-17/AC-18): JSON só com os campos que mudaram, em `DetalheAnterior` e
  `DetalheNovo`, ex.: `{"titulo":"Antigo"}` / `{"titulo":"Novo"}` ou
  `{"titulo":"…","descricao":"…"}`. Serializado com `System.Text.Json` (camelCase). Colunas são `text`
  — sem limite (descrição até 5000).
- Sem notificação nova; o `StatusAlterado`/SignalR **não** é disparado (spec: sem notificação). A
  lista e o Kanban se atualizam pela invalidação de cache de quem editou (AC-15); para os demais,
  na próxima recarga — como as outras edições sem evento.
- Validação de tamanho: `AtualizarChamadoCommandValidator` já tem 200/5000 (= abertura) (AC-13).

## 4. Bloco C — Tela

- `features/chamados/lib/permissoes.ts` (novo): `podeEditarChamado(chamado, perfil)` — espelho da
  regra do servidor (status, responsável = `perfil.id`, quem abriu = e-mail igual ignorando
  maiúsculas, Admin). Só para mostrar/esconder o botão; o servidor é quem decide (AC-22).
- `api.ts`: `atualizarChamado(id, { titulo, descricao }, versao)` → `PUT /chamados/{id}` com
  `If-Match` (helper `cabecalhoVersao` existente).
- `hooks/useAcoesChamado.ts`: `useAtualizarChamado(chamadoId, versao)` no mesmo padrão
  (`onSuccess` invalida; `onError` 409 → `recarregarSeConflito`).
- `components/EditarChamadoModal.tsx` (novo), no padrão dos modais existentes
  (`AlterarPrioridadeModal`): campos Título (máx. 200) e Descrição (máx. 5000), com contador e erro
  no campo (AC-13); "Salvar"/"Cancelar".
  - Ao **abrir**: preenche com o texto atual e guarda a versão (AC-12, AC-14).
  - "Salvar" sem mudança → fecha sem chamar a API (AC-16).
  - **Conflito (409)** (AC-19): o modal **não fecha** e **mantém o texto digitado**; mostra a
    mensagem do servidor e, abaixo, "Texto atual no chamado" (título/descrição recarregados, só
    leitura) para a pessoa conferir. Quando o chamado termina de recarregar, o modal passa a usar a
    **versão nova**, então o próximo "Salvar" grava (AC-20) — a pessoa viu o aviso e o texto atual.
  - **403/400** (perdeu a permissão ou o chamado foi encerrado no meio): mostra a mensagem do
    servidor e mantém o texto (para copiar); "Salvar" segue disponível, mas a recusa se repete.
- `ChamadoDetailPage` / `BotoesAcao`: botão "Editar" (variant outline) quando
  `podeEditarChamado`; desabilitado durante a recarga (`recarregando`, como os outros).
  Visível também para Solicitante (hoje `BotoesAcao` já é renderizado para todos os perfis).

## 5. Bloco D — Histórico na tela

- `types/api.ts`: `AcaoHistorico` ganha `'ChamadoEditado'`.
- `TimelineHistorico`: label "Chamado editado"; para essa ação, faz `JSON.parse` dos detalhes e
  mostra **por campo** "Título: antigo → novo" e "Descrição:" com antes/depois em blocos (texto
  longo, quebra de linha preservada). Se o JSON não parsear, mostra o texto cru (não quebra a tela).
  As demais ações seguem como hoje.

---

## 6. Constitution Check

| Regra | Cumpre? | Justificativa |
|---|---|---|
| 1. Sem suposição silenciosa | Sim | Todas as regras decididas com o usuário (spec §6) |
| 2. Spec antes do código | Sim | Spec aprovada 2026-10-02 |
| 3. Contrato avisado antes | Sim | C1–C5 abaixo, apresentados antes de implementar |
| 4. `/sdd` | Sim | — |
| 5. Cross-feature / `/analise-cod` | Sim | Lista abaixo; `/analise-cod` no fim da implementação |
| 6. Obsidian | Planejado | Acompanhamento do Chamado, Perfis e Permissões |

## 7. Mudanças de Contrato

| # | Mudança | Quem consome | Compatibilidade |
|---|---|---|---|
| C1 | `ChamadoPermissoes.Pode(acao, acesso, DadosDoChamado)` substitui a versão com `string`; `DependeDoSolicitante` → `DependeDoChamado` | `AcessoChamadoBehaviour`, `ChamadoPermissoesTests` | Interno ao backend; demais ações com regra idêntica |
| C2 | **Regra de `Editar` muda** (substitui AC-20 de `autorizacao-chamados`: "Solicitante não edita; Atendente que vê edita") | `PUT /chamados/{id}` | Nenhuma tela usa hoje; muda por decisão do usuário |
| C3 | `AcaoHistorico.ChamadoEditado` (novo valor) | Histórico (API e tela), relatório mensal | Relatório filtra só Criado/Resolvido/Cancelado; **versão antiga em produção não conhece o valor** (ver Riscos) |
| C4 | `AtualizarChamadoCommand` + `UsuarioId`/`UsuarioNome` | Controller, testes | Interno |
| C5 | `PUT /chamados/{id}`: 400 para chamado encerrado; 403 pela regra nova | Tela nova | Novo comportamento previsto na spec |

## 8. Pontos de Toque Cross-Feature

| Arquivo | Dono / outros usuários | Estratégia de não-regressão |
|---|---|---|
| `ChamadoPermissoes.cs` | autorizacao-chamados (todas as ações) | Testes existentes das outras ações seguem iguais; **2 testes mudam de propósito** (ver abaixo) |
| `AcessoChamadoBehaviour.cs` | todo request de chamado | Testes existentes do behaviour; teste novo: ações ≠ Editar/Cancelar não carregam o chamado (sem consulta extra) |
| `Chamado.AtualizarDados` | só a edição | Testes de domínio novos |
| `TimelineHistorico.tsx` | histórico de todas as ações | Ações existentes renderizadas como antes (E2E do fluxo completo abre o detalhe) |
| `ChamadoDetailPage` / `BotoesAcao` | ações do chamado | E2E existentes (conflito, fluxo completo) |
| `types/api.ts` (`AcaoHistorico`) | histórico | `npm run build` (o `Record<AcaoHistorico,…>` obriga o label novo) |

**Testes que mudam de propósito (regra C2, não é regressão):**
- `ChamadoPermissoesTests.Solicitante_NaoPodeAcoesDeAtendimento_NemNoProprioChamado` — sai `Editar`
  da lista (quem abriu passa a editar sem responsável); vira testes próprios da regra nova.
- `ChamadoPermissoesTests.Atendente_PodeAcoesDeAtendimento` — sai `Editar` (Atendente só edita como
  responsável ou quem abriu).

## 9. Riscos e Mitigações

| Risco | Mitigação |
|---|---|
| **Versão antiga em produção** lê o histórico e não conhece `ChamadoEditado` → erro ao abrir o histórico de um chamado editado | Nenhuma tela antiga edita; **testes ao vivo só em chamados `[TESTE-*]`, apagados ao final**; deploy de backend junto com o frontend (já é regra) |
| Regra do botão (frontend) diverge da do servidor | Servidor decide; teste E2E "botão ausente" + testes unitários da regra no servidor |
| Edição sem mudança gerar histórico ou mudar versão | `AtualizarDados` retorna `bool`; teste de domínio e de handler |
| 409 em loop no modal | Versão do modal atualizada após a recarga (AC-20); E2E de conflito |

## 10. Perguntas em Aberto

Nenhuma.
