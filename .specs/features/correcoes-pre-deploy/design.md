# Correções Pré-Deploy — Design Técnico

> **Spec:** `spec.md` (aprovada em 2026-10-02)
> **Autor:** Claude Code (Opus 5.5), sessão única — design, backend e frontend
> **Data:** 2026-10-02

---

## 1. Visão Geral da Solução

Cinco blocos independentes, que podem ser commitados em separado:

| Bloco | ACs | Onde |
|---|---|---|
| A. Alerta de SLA por visibilidade | AC-01..05 | `SlaMonitorService` → novo `SlaAlertaNotificador` + regra única de visibilidade do repositório |
| B. Anexo × comentário | AC-06..08 | `AdicionarAnexoCommandHandler` + método novo no repositório |
| C. Edição simultânea | AC-09..14, AC-23 | campo `versao` na resposta + cabeçalho `If-Match` + `VersaoChamadoBehaviour` + telas |
| D. Arrumação técnica | AC-15..18 | `frontend/e2e`, `.csproj` de testes, `frontend/public/favicon.png` |
| E. Processo | AC-19 | STATE.md (regra 4), `docs/GUIA-ORQUESTRACAO-SDD.md` |

---

## 2. Bloco A — Alerta de SLA só para quem vê (AC-01..05)

**Hoje:** `SlaMonitorService.cs:61,72` envia para o grupo SignalR `Atendimento` (todo Atendente e
Admin).

**Design:**
- `IChamadoRepository.ListarAtendentesQuePodemVerAsync(Guid chamadoId)` → `IReadOnlyList<Guid>`.
  Busca os `UsuariosPerfil` **ativos** com perfil Atendente e, para cada um, monta um
  `ContextoAcesso` e reaproveita o `AplicarVisibilidade` (regra ÚNICA, `ChamadoRepository.cs:185`).
  Não duplica a regra. São poucas consultas, e só quando um chamado **muda** de situação de prazo
  (o monitor já evita repetição pelo `_notificados`, o que preserva o AC-05).
- Novo grupo SignalR `Admins` em `ChamadosHub.OnConnectedAsync`, definido pelo claim `perfil` do
  token, como o `Atendimento`.
- Nova classe `WebApi/Services/SlaAlertaNotificador.cs` (scoped), com `IHubContext<ChamadosHub>` e
  `IChamadoRepository`. O método `NotificarAsync(chamadoId, numero, evento, mensagem)` envia para
  `Clients.Group("Admins")` e para `Clients.Users(ids dos Atendentes que veem)`. O `SlaMonitorService`
  passa a resolver o notificador no escopo que já cria e deixa de enviar direto. Assim o envio fica
  testável sem o `BackgroundService`, o que fecha o buraco "sem teste" do R-04 anterior.
- O payload do evento (`chamadoId`, `numero`, `mensagem`) não muda. O frontend não muda.
- `Clients.User(id)` já funciona pelo claim `sub` (`SubClaimUserIdProvider`, usado pelo chat).
- Solicitante (AC-04) nunca entra em `Admins` nem na lista de Atendentes.

## 3. Bloco B — Anexo × comentário (AC-06..08)

- `IChamadoRepository.ComentarioPertenceAoChamadoAsync(Guid comentarioId, Guid chamadoId)` → `bool`.
- Em `AdicionarAnexoCommandHandler`, **antes do upload** e se houver `ComentarioId`, chamar esse
  método. Se for `false`, lançar `BadRequestException("O comentário informado não pertence a este
  chamado.")`. Comentário inexistente cai no mesmo caso (AC-07). Como o upload ainda não aconteceu,
  nenhum arquivo é guardado (AC-06).

## 4. Bloco C — Edição simultânea (AC-09..14, AC-23)

### 4.1 Versão do chamado
- `Application/Common/VersaoChamado.cs`: `De(DateTime? dataAtualizacao, DateTime dataCriacao)` →
  string com os **microssegundos** de `(dataAtualizacao ?? dataCriacao)` (`Ticks / 10`).
  - Microssegundos porque é a precisão do Postgres e o Npgsql trunca `Ticks` para microssegundo.
    O valor calculado em memória logo após o `UtcNow` e o valor relido do banco dão a mesma string.
  - `?? dataCriacao` resolve o chamado nunca alterado, com `DataAtualizacao` nula. A primeira
    alteração muda a versão.
  - O cliente trata a versão como texto opaco e nunca converte para `Date` do JS (milissegundos
    perderiam precisão).
- `ChamadoResponse` ganha `string Versao` no fim (contrato: campo novo). `ChamadoMappings.ToResponse`
  é o único lugar que monta o DTO.

### 4.2 Checagem no servidor
- O cliente envia `If-Match: "<versao>"`. É o cabeçalho HTTP padrão, e o CORS já tem
  `AllowAnyHeader` (`Program.cs:213`).
- `Application/Common/Interfaces/IVersaoLidaAccessor` (`string? VersaoLida`). A implementação em
  WebApi lê o `If-Match` do `HttpContext`, sem aspas, e é registrada como scoped.
- `IChamadoRepository.ObterVersaoAsync(Guid id)` → `string?`: consulta leve, só das duas datas, sem
  tracking.
- `Application/Common/Behaviours/VersaoChamadoBehaviour`, registrado **depois** do
  `AcessoChamadoBehaviour`, para que quem não vê o chamado continue recebendo 404 e não 409. Ele
  age quando o request é `IRequerAcessoChamado` e
  `ChamadoPermissoes.AlteraChamado(acao)` é verdadeiro:
  - ações que conferem: `Assumir`, `Resolver`, `Encerrar`, `Reabrir`, `AlterarStatus`, `Cancelar`,
    `Editar`, `ReclassificarTipo`, `Reatribuir`, `AlterarPrioridade`, `ForcarEncerramento` (AC-09);
  - ações que não conferem: `Ver`, `ComentarPublico`, `ComentarInterno`, `Anexar` (AC-13).
    Comentário e anexo também não mudam o `DataAtualizacao`, então não invalidam a versão de
    ninguém.
  - Sem `If-Match`, segue sem checar, como hoje (AC-23).
  - Com versão diferente, lança `ConflictException` com o texto do AC-10.
- A mensagem do `ChamadoRepository.AtualizarAsync` (409 do token do EF, na janela entre leitura e
  escrita) passa a usar o mesmo texto do AC-10, para a tela ter uma mensagem só.
- **Limite conhecido:** entre a checagem do behaviour e o save sobra uma janela de milissegundos.
  Ela continua coberta pelo token de concorrência do EF, mas só se a outra escrita cair entre o
  `SELECT` e o `UPDATE` do handler. É aceitável: o caso real do AC-09 (minutos entre abrir e
  salvar) fica coberto.

### 4.3 Frontend
- `types/api.ts`: `ChamadoResponse.versao: string`.
- `features/chamados/api.ts`: as funções das ações do AC-09 (`alterarStatus`, `atribuirChamado`,
  `resolverChamado`, `fecharChamado`, `cancelarChamado`, `reabrirChamado`, `reatribuirChamado`,
  `alterarPrioridade`, `forcarEncerramento`, `reclassificarTipo`) recebem `versao?: string` e
  enviam `If-Match` quando presente. Um helper `cabecalhoVersao(versao)` evita repetir isso.
- `hooks/useAcoesChamado.ts`: as variáveis da mutation passam a incluir `versao`. Em erro 409, o
  `onError` chama o mesmo `invalidarChamado` (AC-10: recarrega). A mensagem exibida é a do servidor,
  pelo bloco de erro que a página de detalhe já tem.
- `ChamadoDetailPage` e os modais (`ReatribuirModal`, `AlterarPrioridadeModal`,
  `ForcarEncerramentoModal`, diálogos de fechar/cancelar) e `TipoChamadoCampo` mandam
  `chamado.versao` do chamado exibido. A página de detalhe não se atualiza sozinha por SignalR,
  então a versão exibida é a que a pessoa viu (`refetchOnWindowFocus` atualiza os dados na tela,
  o que é desejado).
- Kanban (`KanbanBoard.tsx:59`): envia `chamado.versao`. Hoje o erro é silencioso, e o
  `invalidateQueries` já devolve o cartão à coluna. Passa a mostrar a mensagem do erro numa faixa
  acima do quadro (AC-14), sem biblioteca de toast (CONVENTIONS).
- **AC-11 — não se aplica hoje:** **não existe tela de edição de título/descrição.** O
  `PUT /api/chamados/{id}` só é usado pela API, e nenhuma chamada no `frontend/src` o usa. A
  proteção do AC-09 vale para a API. O AC-11 fica registrado para quando a tela existir. **Decisão
  do usuário pedida na parada do plan.**
- AC-12: depois da própria ação, a mutation invalida o chamado e a nova versão chega pela
  recarga. Clicar numa segunda ação **antes** da recarga terminar pode dar um 409 falso. Para isso,
  os botões ficam desabilitados enquanto o chamado recarrega (`isFetching`), no mesmo espírito do
  `isPending` que já existe.

## 5. Bloco D — Arrumação técnica (AC-15..18)

- **E2E:** `admin.spec.ts`, `chamados.spec.ts` e `fluxo-completo.spec.ts` ainda citam "Categoria".
  Trocar pela escolha de Área e Tipo da tela atual (ver os seletores reais em `AbrirChamadoPage`).
  Títulos criados levam `[TESTE-E2E]` (AC-16). Rodar com backend e frontend locais contra o banco
  real (dev = prod). A limpeza é feita por um script no scratchpad, que lista o que vai apagar e só
  roda com o OK do usuário. **Parada obrigatória** (ação destrutiva em banco real).
- **EF nos testes:** reproduzir o aviso com `dotnet build` e alinhar com um `PackageReference`
  explícito da versão resolvida pelos projetos `src` (`9.*` → 9.0.x mais recente) no `.csproj` de
  testes. Não mexer nos `.csproj` de `src`.
- **Favicon:** gerar a partir do `favicon.png` atual uma versão 64×64 PNG (≤ 20 KB), com a mesma
  imagem e o mesmo nome de arquivo, para o `index.html` não mudar. Ferramenta: a que estiver
  disponível na máquina (Pillow/ImageMagick/.NET), via script no scratchpad.

## 6. Bloco E — Processo (AC-19)

- Regra 4 da Constitution (STATE.md): passa a dizer "fluxo SDD: no Claude Code, `/sdd` (specify →
  plan → tasks → implement → review por sub-agente novo → close, com 2 aprovações do usuário: spec
  e PR); no OpenCode, `@spec` → `@build-*` → `@review`". Guia em `docs/GUIA-ORQUESTRACAO-SDD.md`.
- `docs/GUIA-ORQUESTRACAO-SDD.md`: nova seção "Claude Code (`/sdd`)" no topo, mantendo a do
  OpenCode.

---

## 7. Constitution Check

| Regra | Cumpre? | Justificativa |
|---|---|---|
| 1. Pergunta sem resposta não vira suposição | Sim | 3 decisões tomadas na spec. AC-11 levado ao usuário nesta parada |
| 2. Spec antes do código | Sim | Spec aprovada antes de qualquer código |
| 3. Mudança de contrato avisada antes | Sim | Lista abaixo, apresentada antes de implementar |
| 4. Orquestração SDD | Sim | `/sdd`, com review por sub-agente novo |
| 5. Não quebrar fora do escopo / avisar cross-feature | Sim | Lista abaixo, com estratégia de não-regressão |
| 6. Obsidian atualizado | Planejado | Notas de Chamados (edição simultânea), SLA e Anexos no close |

## 8. Mudanças de Contrato

| # | Mudança | Quem consome | Compatibilidade |
|---|---|---|---|
| C1 | `ChamadoResponse.versao` (campo novo) | Frontend (lista, detalhe, Kanban) | Aditivo; cliente antigo ignora |
| C2 | Cabeçalho `If-Match` aceito nas 11 ações de alteração | Frontend | Opcional; sem ele, comportamento de hoje (AC-23) |
| C3 | Texto do 409 muda para o do AC-10 | Frontend (só exibe `message`) | Só texto |
| C4 | `IChamadoRepository` + 3 métodos (`ListarAtendentesQuePodemVerAsync`, `ComentarioPertenceAoChamadoAsync`, `ObterVersaoAsync`) | Handlers, behaviour, notificador; mocks nos testes | Aditivo |
| C5 | Alerta SLA: de grupo `Atendimento` para grupo `Admins` + usuários específicos | Frontend (`useSignalR` ouve `SlaAtencao`/`SlaAtrasado`) | Mesmo evento e payload; muda só quem recebe |
| C6 | Novo `IPipelineBehavior` global (`VersaoChamadoBehaviour`) | Todo request MediatR | Só age em `IRequerAcessoChamado` de alteração com `If-Match` |
| C7 | `AdicionarAnexo` passa a recusar com 400 um `comentarioId` inválido | Frontend (`UploadAnexoForm`, `ComentarioForm`) | O frontend só envia ids de comentários do próprio chamado |

## 9. Pontos de Toque Cross-Feature

| Arquivo | Dono / outros usuários | Estratégia de não-regressão |
|---|---|---|
| `ChamadoRepository.cs` / `IChamadoRepository.cs` | autorizacao-chamados, dashboard, relatório | Só métodos novos; `AplicarVisibilidade` reaproveitado sem alteração; suíte existente |
| `ChamadosHub.cs` | chat (mesmo hub), correcoes-acesso | Grupo novo aditivo; `Todos`/`Atendimento` intactos; teste novo do `OnConnectedAsync` com Admin |
| `Program.cs` (pipeline MediatR, DI) | todas | Behaviour novo só age com a interface + ação de alteração; teste de que `Ver`/`Comentar`/`Anexar` passam direto |
| `ChamadoMappings.cs` / `ChamadoResponse` | lista, detalhe, Kanban, arquivo, relatório | Campo aditivo; `npm run build` garante os tipos |
| `ChamadoDetailPage.tsx`, modais, `KanbanBoard.tsx`, `TipoChamadoCampo.tsx` | features de chamados, area-e-tipo | Verificação ao vivo das ações sem conflito (AC-12) além do caso de conflito |
| `SignalRNotificationHandlers.cs` | aviso de comentário interno | **Não é alterado** (continua usando `Atendimento`) |
| `frontend/e2e/*` | todas | Rodar a suíte inteira, não só os arquivos alterados |

## 10. Riscos e Mitigações

| Risco | Mitigação |
|---|---|
| 409 falso por precisão de data (`UtcNow` × Postgres) | Versão em microssegundos (4.1) + teste unitário com `Ticks` "quebrados" |
| 409 falso por clique duplo antes da recarga | Botões desabilitados durante `isFetching` |
| Atendente inativo recebendo alerta | Só perfis `Ativo` na consulta |
| E2E sujando o banco de produção | `[TESTE-E2E]` + limpeza com OK do usuário |

## 11. Perguntas em Aberto

Nenhuma. Resolvidas em 2026-10-02 com o usuário:
- Mudanças de contrato C1–C7: **aprovadas**.
- AC-11: **adiado** para a futura feature "Editar chamado" (modal; Solicitante edita os que abriu,
  Atendente os que assumiu, Admin todos). Ver spec, AC-11. Esta feature não muda a permissão de
  `Editar`.
