# Review: correcoes-acesso-chamados — rodada 1

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-01
> **Escopo:** `git diff origin/develop...feature/correcoes-acesso-chamados` — 11 arquivos, +293/-26 linhas (commits `1ba4a48` spec/tasks, `a003c1b` código)
> **Veredito:** APROVADO COM RESSALVAS

Gates rodados pelo revisor na branch: `dotnet build` 0 erros; `dotnet test tests/ChamadosCamarj.UnitTests/` 380 aprovados, 0 falhas; `npm --prefix frontend run build` ok.

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim, com ressalva (R-03) | `AtualizarChamadoCommandHandler.cs:20` usa `ObterPorIdComTrackingAsync`; com tracking, o valor original de `DataAtualizacao` (concurrency token, `ChamadoConfiguration.cs:66`) é o lido do banco, então o save normal não cai mais no `DbUpdateConcurrencyException` → 409 (`ChamadoRepository.cs:42-44`). Teste `AtualizarChamadoHandlerTests.Handle_DeveBuscarComTracking_EPersistirOsDadosNovos` (só verifica qual método do mock foi chamado). A segunda metade do AC (409 continua existindo) não tem teste e só vale na janela entre o load e o save da mesma requisição. T04 (verificação ao vivo) declarada no tasks.md, não reproduzida pelo revisor. |
| AC-02 | Sim | `ChamadoMappings.cs:12,30-32`; `ListarChamadosQueryHandler.cs:58,73,84` (os dois ramos, com e sem filtro de SLA); `ObterChamadoPorIdQueryHandler.cs:24-25`. Perfil ausente/desconhecido vira Solicitante (`CurrentUserServiceExtensions.cs:14-16`), o que é o lado seguro. Testes: `ObterChamadoPorIdHandlerTests.Handle_ParaSolicitante_QuantidadeDeComentariosNaoContaOsInternos` e `Handle_ParaAtendente_QuantidadeDeComentariosContaTodos`. A listagem não tem teste (R-04). Não há outro lugar que mapeie `Chamado` com contagem de comentários (só `AbrirChamadoCommandHandler.cs:72`, chamado recém-criado, sem comentários). |
| AC-03 | Sim (como escrito); ver R-02 | `SlaMonitorService.cs:61,72` envia para `ChamadosHub.GrupoAtendimento`; `ChamadosHub.cs:21-22` põe Atendente/Admin no grupo a partir do claim `perfil` (emitido em `JwtTokenService.cs:29`; `MapInboundClaims = false` em `Program.cs:134`, então o nome do claim bate). Teste `ChamadosHubTests.EhAtendimento_SoAtendenteEAdmin` cobre só o predicado, não a ligação (R-04). |
| AC-04 | Sim | `EntrarGrupo`/`SairGrupo` removidos; o hub só expõe `OnConnectedAsync`/`OnDisconnectedAsync` como instância pública (`EhAtendimento` é `static`, que o SignalR não expõe). Teste `ChamadosHubTests.Hub_NaoExpoeMetodoParaOClienteEntrarEmGrupo`. Busca por consumidores (`git grep` na árvore inteira, inclusive `frontend/`, `tests/`, `docs/`): só aparecem nas specs. Os `conn.invoke(...)` do front (`useChatSignalR.ts`) são do `ChatHub` (`/hubs/chat`), não deste hub. |
| AC-05 | Sim | Gates acima, rodados pelo revisor. |

## Achados

### 🟡 Atenção R-01 — `ComentarioAdicionado` continua anunciando comentários internos para todo mundo, inclusive para o Solicitante dono do chamado
- **Onde:** `src/ChamadosCamarj.Application/Features/Chamados/Commands/ComentarChamadoCommandHandler.cs:33-37` (publica para qualquer tipo) → `src/ChamadosCamarj.WebApi/Notifications/SignalRNotificationHandlers.cs:67-70` (`Clients.Group("Todos")`)
- **Problema:** a US-02 desta spec diz que "nenhuma resposta do sistema" deve revelar a existência de comentários internos. A contagem foi corrigida (AC-02), mas o evento em tempo real continua saindo para o grupo `Todos` também quando o comentário é interno. É a mesma informação que o R-03 da feature anterior apontava, por outro caminho. Não está nos ACs, por isso não é bloqueante.
- **Cenário de falha:** Solicitante com o detalhe do próprio chamado aberto. O Atendente escreve um comentário interno. O navegador do Solicitante recebe `ComentarioAdicionado { chamadoId: <o dele> }` (visível no DevTools → WS), mas a lista de comentários não ganha nada novo. Ele conclui que a equipe escreveu algo escondido no chamado dele. Além disso, todo usuário conectado recebe o evento para chamados que não pode ver (só o GUID).
- **Correção sugerida:** barata — incluir o tipo na `ComentarioAdicionadoNotification` e, quando for interno, enviar só para `ChamadosHub.GrupoAtendimento` (o grupo agora existe). Se não for corrigir nesta entrega, registrar como pendência no STATE.

### 🟡 Atenção R-02 — alerta de SLA ainda chega a Atendentes que não podem ver o chamado
- **Onde:** `src/ChamadosCamarj.WebApi/Services/SlaMonitorService.cs:61,72`; regra de visibilidade em `src/ChamadosCamarj.Infrastructure/Repositories/ChamadoRepository.cs:177-205`
- **Problema:** o AC-03 foi escrito como "só Atendentes e Admins", e isso foi cumprido. Mas o Atendente não vê todos os chamados: vê os sem responsável, os dele e os do seu grupo. O monitor manda o alerta de **todos** os chamados para **todos** os Atendentes, então o vazamento do R-04 anterior (número + situação de prazo de chamado invisível, contra o ADR-007 citado na própria spec) continua para esse perfil.
- **Cenário de falha:** CAM-512 está atribuído a um Atendente do grupo TI e foi aberto por alguém do TI. Um Atendente do RH, logado, recebe o toast "CAM-512 — prazo estourado"; ao buscar, recebe "não encontrado", mas já sabe que o chamado existe e está atrasado.
- **Correção sugerida:** decidir com o usuário se o AC-03 basta (aceitar e registrar como pendência) ou se o alerta deve respeitar a visibilidade (ex.: Admin no grupo `Atendimento`; demais pelo `Clients.Users(...)` dos que passam em `PodeVerAsync`, ou grupos por `GrupoId` + responsável). Hoje a spec trata o problema como resolvido; isso precisa ficar explícito.

### 🟡 Atenção R-03 — o 409 "legítimo" do AC-01 só existe numa janela de milissegundos; edição concorrente real sobrescreve sem aviso
- **Onde:** `src/ChamadosCamarj.Application/Features/Chamados/Commands/AtualizarChamadoCommand.cs` (não carrega a `DataAtualizacao` que o cliente leu) + `AtualizarChamadoCommandHandler.cs:20-25`
- **Problema:** o AC-01 diz que o 409 "continua acontecendo quando outra pessoa alterou o chamado no meio do caminho". Com o handler recarregando o chamado na própria requisição, o token de concorrência compara o valor de agora com o de agora; o 409 só ocorre se outra escrita cair entre o `SELECT` e o `UPDATE` da mesma requisição. É o mesmo comportamento dos outros 9 handlers (decisão da seção 6), então não é regressão, mas a frase do AC promete mais do que o código faz, e não há teste da metade "409 continua".
- **Cenário de falha:** Atendente A abre o chamado às 10:00. Atendente B corrige o título às 10:01. A salva a descrição às 10:02 com o título antigo no formulário → 204, o título de B é perdido, nenhum 409.
- **Correção sugerida:** ou reescrever o AC-01 para o que o padrão garante ("o controle de concorrência existente continua ativo"), ou, se o 409 entre leitura e escrita for desejado, o comando recebe a `DataAtualizacao` lida pelo cliente e o handler a define como `OriginalValue` antes do save. Em qualquer caso, registrar a decisão.

### 🟡 Atenção R-04 — os testes novos não pegam a regressão das ligações que importam (listagem do AC-02, envio do AC-03, `OnConnectedAsync`)
- **Onde:** `tests/ChamadosCamarj.UnitTests/WebApi/Hubs/ChamadosHubTests.cs`; `tests/ChamadosCamarj.UnitTests/Application/Handlers/ListarChamadosQueryHandlerTests.cs` (não alterado)
- **Problema:** o teste do hub testa só o predicado estático `EhAtendimento`; nada verifica que `OnConnectedAsync` o usa nem que o `SlaMonitorService` envia para o grupo. A listagem (`ListarChamadosQueryHandler`, os dois ramos) não tem teste de contagem para Solicitante. O tasks.md (T03) diz "teste do hub verifica que Solicitante não entra no grupo Atendimento e Atendente entra", o que não é bem o que o teste faz.
- **Cenário de falha:** alguém volta `SlaMonitorService.cs:61` para `Clients.All`, ou apaga o `if` de `ChamadosHub.cs:21`, ou troca `ToResponse(incluirInternos)` por `ToResponse()` em `ListarChamadosQueryHandler.cs:73` → os 380 testes continuam verdes e o vazamento volta (ou ninguém mais recebe alerta).
- **Correção sugerida:** (a) teste de `OnConnectedAsync` com `Mock<IGroupManager>` + `HubCallerContext` falso, verificando `AddToGroupAsync(..., "Atendimento")` para Atendente e ausência para Solicitante; (b) teste do `ListarChamadosQueryHandler` com Solicitante e chamado com 1 interno, nos dois ramos (com e sem `SlaStatus`); (c) opcional, extrair o envio do monitor para um método testável com `Mock<IHubContext<ChamadosHub>>`.

## Constitution e contratos

- Regra 1 (pergunta sem resposta): cumpre — não há pergunta pendente; a spec registra a aprovação do usuário em 2026-10-01.
- Regra 2 (spec antes do código): cumpre — `1ba4a48` (spec/tasks) antecede `a003c1b` (código).
- Regra 3 (contrato sinalizado antes): cumpre na forma — a remoção de `EntrarGrupo`/`SairGrupo` está marcada com ⚠️ na seção 6 da spec, antes do código. A aprovação registrada é genérica ("tratar as três pendências juntas… delegou a execução"); não há registro de que essa remoção específica foi mostrada ao usuário. Sem consumidores encontrados, o risco é baixo.
- Regra 5 (cross-feature): os pontos de toque estão listados na spec. `ToResponse` mantém o padrão `true` para quem não passa nada (só `AbrirChamado`, sem comentários) — preservado. `ChamadosHub`: o grupo `Todos` e o `Clients.User` (badge/chat) não mudaram; Kanban, Fila, Detalhe e Chat não dependem dos métodos removidos (verificado no front: `useSignalR.tsx` só faz `conn.on`). Verificado por leitura, sem teste automatizado desses consumidores.
- Regra 6 (Obsidian): ainda não feito — esperado na fase de fechamento; o gate da spec (seção 8) está aberto.
- Mudanças de contrato previstas x feitas: batem (assinatura de `ToResponse` com parâmetro opcional; construtor de `ObterChamadoPorIdQueryHandler` ganhou `ICurrentUserService`, resolvido por DI; hub sem `EntrarGrupo`/`SairGrupo`; novo grupo `Atendimento`).
- Convenções: `CONVENTIONS.md` §4.6 pede a tabela de rastreabilidade AC → teste na spec; a seção 5 da spec continua "⬜ Pendente / a definir no tasks.md".

## Sugestões (não bloqueiam)

- `ToResponse(bool incluirInternos = true)` é *fail-open*: um handler novo que esquecer o argumento volta a vazar a contagem. Considerar parâmetro obrigatório (ou receber o `Perfil`/`ContextoAcesso`) para forçar a decisão em cada chamada.
- `ChamadosHub.EhAtendimento` compara strings; os handlers usam `ObterContextoAcesso()` com `Enum.TryParse`. Reutilizar o enum `Perfil` deixaria uma única regra de "quem é atendimento".
- O grupo é decidido na conexão: se o Admin rebaixar um Atendente a Solicitante, a conexão aberta continua no grupo `Atendimento` até reconectar (coerente com o resto da API, que confia no claim do JWT até expirar). Vale uma linha na documentação.
- `OnDisconnectedAsync` remove só de `Todos`; o SignalR já tira a conexão de todos os grupos ao desconectar, então o override é desnecessário (não está errado).
- `ChamadoCriado` e `StatusAlterado` continuam indo para `Todos` com o GUID e o status de chamados que o usuário não vê (decisão aceita no AC-19 da feature anterior). Revela volume e ritmo, não identidade; registrar se quiser tratar junto com R-01.
- `ChamadoRepository.AtualizarAsync` chama `_dbSet.Update(chamado)` num grafo já rastreado com `Comentarios` e `Anexos`, gerando UPDATE de cada comentário/anexo a cada edição. Igual aos outros handlers; não é desta feature.
- Atualizar o exemplo de `ToResponse` em `CONVENTIONS.md` §2.7 para a nova assinatura.

## Verificação dos achados pela sessão principal (2026-10-01)

| Achado | Confirmado? | Tratamento |
|---|---|---|
| R-01 | Sim | **Corrigido:** `ComentarioAdicionadoNotification` ganhou `Interno`, e o aviso de comentário interno vai só para o grupo `Atendimento`. Novo AC-06 + `ChamadoSignalRNotificationHandlersTests`. |
| R-02 | Sim | **Pendência:** filtrar o alerta de SLA por quem vê cada chamado exige calcular a visibilidade por usuário a cada alerta. O AC-03, como escrito, está atendido. Registrado no STATE. |
| R-03 | Sim | **Spec corrigida:** o AC-01 não promete mais o 409 em edições simultâneas. A ausência de checagem da versão lida pelo cliente vale para todas as ações e foi registrada no STATE como pendência geral. |
| R-04 | Sim | **Corrigido:** testes novos que falham se o vazamento voltar (contagem na listagem, `OnConnectedAsync`, destino do aviso de comentário). `ToResponse` não tem mais valor-padrão. Continua sem teste: voltar o `SlaMonitorService` para `Clients.All` (serviço em background com escopo de banco). |
| Regra 3 | Observação aceita | A remoção de `EntrarGrupo`/`SairGrupo` está na spec, mas não foi citada nominalmente ao usuário antes. **Informar ao usuário** no resumo final. |
