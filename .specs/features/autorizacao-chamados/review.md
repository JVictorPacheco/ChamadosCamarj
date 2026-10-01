# Review: autorizacao-chamados — rodada 1

> **Revisor:** sub-agente independente (Claude Code — Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-01
> **Escopo:** `git diff origin/develop...feature/autorizacao-chamados` — 55 arquivos, +1486/-202 linhas (commits `7e92fb8`, `ccb6a29`, `d6e0b6b`)
> **Veredito:** APROVADO COM RESSALVAS (nenhum bloqueante; o merge depende da T16, porque o filtro de visibilidade, que é o centro da feature, não tem teste automatizado: ver R-01)

Gates conferidos pelo revisor nesta rodada: `dotnet test tests/ChamadosCamarj.UnitTests/` → **369 aprovados, 0 falhas**; `npm --prefix frontend run build` → **ok**. A API não foi executada e o banco não foi acessado.

## Cobertura da spec

| ID | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim (código); falta verificar ao vivo (T16) | `ChamadoRepository.cs:177-205` (`AplicarVisibilidade`, ramo sem grupo: `SolicitanteEmail.ToLower() == email`); `ChamadoRepository.cs:252`: `solicitanteEmail` virou filtro AND e não substitui mais a regra; `ListarChamadosQueryHandler` monta o contexto pelo token (teste `Handle_DevePassarOContextoDoUsuarioLogadoParaORepositorio`). O SQL não tem teste (R-01). |
| AC-02 | Sim (código); falta T16 | `ChamadoRepository.cs` ramo com grupo: abriu **ou** `membros.Any(u => u.Id == c.ResponsavelId \|\| u.Email == c.SolicitanteEmail.ToLower())`. Frontend parou de mandar `solicitanteEmail`: `ChamadosListPage.tsx:49`, `ArquivoChamadosPage.tsx:27`. |
| AC-03 | Sim | `ObterChamadoPorIdQuery`, `ListarAnexosQuery`, `ObterUrlDownloadAnexoQuery`, `ListarHistoricoQuery` e `ListarComentariosQuery` implementam `IRequerAcessoChamado` (Ver); `AcessoChamadoBehaviour.cs:109-110` → `NotFoundException`. O download confere que o anexo é do chamado da URL (`ObterUrlDownloadAnexoQueryHandler.cs:24-26`). Testes: `ChamadoQueNaoPodeVer_DaNotFound_ENaoChamaOHandler`, `DownloadDeAnexo_ChecaOChamadoDaUrl`, `Handle_QuandoAnexoEhDeOutroChamado_DeveLancarNotFoundSemGerarUrl`. |
| AC-04 | Sim | Bloqueio local removido (`ChamadoDetailPage.tsx` ~297); o 404 mostra "Chamado não encontrado." (`ChamadoDetailPage.tsx` ~271-281). Falta conferir na tela (T16). |
| AC-05 | Parcial (ver R-02) | `ChamadoPermissoes.cs:32-37`. Teste `Solicitante_NaoPodeAcoesDeAtendimento_NemNoProprioChamado`. Num chamado que ele **vê**, o Solicitante recebe 403. Num chamado que **não vê**, recebe 404, e não a mensagem de recusa. |
| AC-06 | Sim | `ChamadoPermissoes.cs:30` mais `AcessoChamadoBehaviour.cs:112-117`, que carrega o e-mail do chamado. Botão: `ChamadoDetailPage.tsx:62,128`. Testes: `Solicitante_CancelaOProprioChamado`, `Solicitante_DoGrupo_NaoCancelaChamadoDoColega`, `Solicitante_PodeCancelarChamadoQueAbriu_IgnorandoMaiusculas`. A regra de status Aberto/EmAndamento continua no handler. |
| AC-07 | Sim | `ChamadoPermissoes.cs:28` (Ver/ComentarPublico/Anexar liberados após a checagem de visibilidade). Teste `Solicitante_DoGrupo_PodeVerComentarEAnexarChamadoQueNaoAbriu`. |
| AC-08 | Sim (código); falta T16 | Ramos de Atendente em `AplicarVisibilidade` (fila + responsável + abriu + grupo). O detalhe e os sub-recursos passam pelo mesmo `PodeVerAsync` (`ChamadoRepository.cs:207-211`). |
| AC-09 | Sim | `ChamadoPermissoes.cs:36` (`_ => false`). Teste `Atendente_NaoPodeAcoesExclusivasDoAdmin` e `Atendente_PodeAcoesDeAtendimento`. |
| AC-10 | Sim, com ressalva (R-03) | Criar interno: `ComentarChamadoCommand` → `ComentarInterno` (teste `Solicitante_ComentarioInterno_DaForbidden`). Listar: `ListarComentariosQueryHandler.cs:28` (já existia). A contagem `QuantidadeComentarios` ainda inclui os internos. |
| AC-11 | Sim | `ChamadoRepository.cs:179` (Admin retorna a query sem filtro, com ou sem grupo); `ChamadoPermissoes.cs:15`. Teste `Admin_ComGrupo_PodeTodasAsAcoes`. |
| AC-12 | Sim | `ChamadosController.cs:87-93` usa `_currentUser.Nome/Email`; `CurrentUserService.Email` lê o claim `email`, que o `JwtTokenService.cs:27` emite. O DTO ficou com campos opcionais. Não há teste de controller; a evidência é a leitura do código (T10). |
| AC-13 | Sim (código); falta T16 | Listagem, busca por número (`busca` numérico dentro de `ListarAsync`, aplicada depois da visibilidade) e as 6 contagens do Dashboard passam por `AplicarVisibilidade` (`ChamadoRepository.cs:294-351`). Dashboard para Solicitante → 403 (`ObterMetricasQueryHandler`, `ObterDistribuicaoQueryHandler`; teste `Handle_QuandoSolicitante_DeveLancarForbidden` só para Distribuição). |
| AC-14 | Depende da T16 | As mudanças de comportamento intencionais estão documentadas no design §7: Atendente sem grupo perde a visão total no Kanban e na busca; Solicitante passa a ver os chamados do grupo. Não encontrei regressão fora disso (ver "Constitution e contratos"). |
| AC-15 | Sim | `ExceptionHandlingMiddleware`: Forbidden/NotFound → `{ message }`. As mensagens estão em português e não trazem dados do chamado (só o id que o próprio cliente mandou). |
| AC-16 | Sim no detalhe e nos sub-recursos; nas ações, ver R-02 | Inexistente e invisível caem no mesmo `NotFoundException("Chamado", id)` do behaviour, com a mesma mensagem. |
| AC-17 | Parcial (R-01) | A política (`ChamadoPermissoesTests`) e o behaviour (`AcessoChamadoBehaviourTests`) têm testes cobrindo os 5 perfis. O **filtro de visibilidade** não tem teste automatizado (limitação registrada em `tasks.md`). Suíte: 369/0. |
| AC-18 | Sim | `npm run build` ok, conferido pelo revisor. |
| AC-19 | Sim | `SignalRNotificationHandlers.cs:21-25` (ChamadoCriado sem título), `:67-70` (ComentarioAdicionado só com id); `SlaMonitorService.cs:61-75` sem título; tipos do front em `signalr-events.ts`. Ressalva em R-04. |
| AC-20 | Sim | `AtualizarChamadoCommand` → `Editar`, liberado só para Atendente/Admin (`ChamadoPermissoes.cs:32-34`). Teste na matriz. |
| AC-21 | Sim (código); falta T16 | `RelatoriosController.cs:43-49`. Sem teste automatizado (previsto na T12/T16). |

**Busca por endpoints que escaparam da checagem:** todos os 18 commands e queries de `Features/Chamados` que recebem o id de um chamado implementam `IRequerAcessoChamado`. Os que ficam de fora são `AbrirChamadoCommand` e `ListarChamadosQuery`, que não recebem id, e a listagem aplica a regra no repositório. Não há `Send` de request de chamado fora dos controllers. `Chat`, `Relatorios` e `Dashboard` não acessam `Chamado` por outro caminho. `ObterPorResponsavelAsync`, `ObterAtrasadosAsync` e `ContarPorStatusAsync`, que não têm filtro de visibilidade, não são chamados por ninguém. Os validators de chamado não consultam o banco, então rodar o `ValidationBehaviour` antes do de acesso não vaza a existência do chamado. A fallback policy `RequireAuthenticatedUser` cobre controllers e hubs.

## Achados

### 🟡 Atenção R-01: o filtro de visibilidade, núcleo da feature, não tem nenhum teste automatizado
- **Onde:** `src/ChamadosCamarj.Infrastructure/Repositories/ChamadoRepository.cs:177-205`
- **Problema:** AC-01, AC-02, AC-08, AC-11 e AC-13 dependem só de `AplicarVisibilidade`, e nenhum dos 369 testes executa esse método, porque o projeto de testes não tem provedor EF. Os testes de handler só confirmam que o `ContextoAcesso` chega ao repositório (mock). A limitação foi registrada no `tasks.md`, mas não há registro de que o usuário a aprovou, e o AC-17 pede "testes cobrindo cada perfil".
- **Cenário de falha:** alguém troca `||` por `&&`, remove o `c.ResponsavelId == null` do Atendente ou esquece o `if Admin`. Os 369 testes continuam verdes, e Solicitantes passam a ver chamados alheios (ou o Admin para de ver tudo) sem que nada acuse.
- **Correção sugerida:** no mínimo, não fazer o merge antes da T16 com os 5 perfis anotados na Rastreabilidade. Melhor: adicionar `Microsoft.EntityFrameworkCore.Sqlite` (em memória) ao projeto de testes e cobrir `ListarAsync`/`PodeVerAsync` com os 5 perfis (dá para fazer numa feature curta logo em seguida). Registrar explicitamente no STATE que o AC-17 ficou parcial.

### 🟡 Atenção R-02: ação sobre chamado que o usuário não vê responde 404, mas a spec diz "sem permissão"
- **Onde:** `src/ChamadosCamarj.Application/Common/Behaviours/AcessoChamadoBehaviour.cs:109-110`; spec `AC-16` (nota "Aprovado pelo usuário em 2026-10-01: ... as ações respondem 'sem permissão'") e `AC-05` ("recebe a recusa", que pela definição da spec é "Você não tem permissão...").
- **Problema:** o design (§3 passo 2 e §5) manda 404 para qualquer request sobre chamado invisível, inclusive as ações. A spec aprovada diz que as ações respondem "sem permissão". O código segue o design, então spec e design se contradizem e o AC-05 fica atendido só em parte.
- **Cenário de falha:** um Solicitante sem grupo envia `PATCH /api/chamados/{id de um chamado do RH}/resolver`. Recebe `404 { message: "Chamado com o id '...' não foi encontrado." }`, quando a spec espera `403 "Você não tem permissão..."`. O chamado não muda e nada vaza, então não é falha de segurança, mas um teste de aceite escrito a partir da spec falharia.
- **Correção sugerida:** decidir com o usuário. O recomendado é manter o 404, que é mais seguro e coerente com o AC-16, e corrigir o texto da spec (AC-16 e definição de "Recusa") antes de fechar. A alternativa é responder 403 nas ações para chamado inexistente **e** invisível, para continuar sem distinguir os dois.

### 🟡 Atenção R-03: `QuantidadeComentarios` conta os comentários internos para o Solicitante
- **Onde:** `src/ChamadosCamarj.Application/Mappings/ChamadoMappings.cs:28` (usado no detalhe e na listagem)
- **Problema:** o AC-10 diz que o Solicitante "nunca recebe os internos". O conteúdo foi protegido, mas a contagem continua revelando que existem.
- **Cenário de falha:** um chamado tem 2 comentários públicos e 1 interno. O Solicitante (dono ou do grupo) recebe `quantidadeComentarios: 3` em `GET /api/chamados` e `GET /api/chamados/{id}`, mas a lista de comentários mostra 2. Ele conclui que a equipe escreveu algo escondido sobre o chamado dele.
- **Correção sugerida:** é um comportamento anterior à feature. Pode ficar fora desta entrega, desde que registrado como pendência no STATE. Se for corrigir, contar só os públicos quando o perfil do contexto for Solicitante (o mapeamento recebe o perfil ou o repositório projeta as duas contagens).

### 🟡 Atenção R-04: alertas de SLA de todos os chamados continuam indo para todos os usuários, inclusive Solicitantes
- **Onde:** `src/ChamadosCamarj.WebApi/Services/SlaMonitorService.cs:61,72` (`Clients.All`); toast em `frontend/src/layouts/AppLayout.tsx:62-65`
- **Problema:** o AC-19 foi cumprido ao pé da letra, porque o alerta não leva mais o título. Mas cada usuário conectado continua recebendo número e situação de prazo de **todos** os chamados da empresa, e o AC-16 diz que o sistema não deve revelar que um chamado invisível existe.
- **Cenário de falha:** um Solicitante do Comercial, logado, vê o toast "CAM-512 — PRAZO ESTOURADO!" de um chamado do RH. Ao clicar ou buscar, recebe "não encontrado", mas já ficou sabendo que o chamado existe e que está atrasado.
- **Correção sugerida:** é fora do escopo mínimo do AC-19, então decidir com o usuário. Opção barata: o front só mostra o toast de SLA para Atendente/Admin. Opção completa: o monitor envia para os grupos SignalR certos (por perfil ou por usuário). Se ficar como está, registrar como pendência.

## Constitution e contratos

- **Regra 1 (pergunta sem resposta não vira suposição):** cumpre. As decisões do Plan estão no design §9 e nas notas de aprovação de 2026-10-01 na spec. Única pendência de alinhamento: R-02, onde spec e design divergem.
- **Regra 2 (spec antes do código):** cumpre. O commit `7e92fb8` (spec/design/tasks) vem antes dos commits de código, e os AC-19 a AC-21 foram incluídos na spec antes da implementação.
- **Regra 3 (contrato sinalizado antes):** cumpre. `IChamadoRepository` (ListarAsync + PodeVerAsync + contagens), `ICurrentUserService.Email`, `ListarChamadosQuery` sem os 3 campos, `ObterUrlDownloadAnexoQuery` com `ChamadoId`, payloads SignalR e `AbrirChamadoRequest` opcional estão todos no design (§2, §3 "Ajuste na implementação", §5). Divergência pequena: o design diz que `ContarPorStatusAsync` também receberia `ContextoAcesso`, e ele ficou sem filtro (não tem uso hoje; ver Sugestões).
- **Regra 4 (orquestração SDD):** cumpre. Esta review é feita por um sub-agente independente.
- **Regra 5 (não quebrar fora do escopo):** pontos de toque conferidos:
  - Fila, Kanban e busca: mesma `ListarAsync`. A mudança para o Atendente sem grupo é intencional e documentada. Sem teste (R-01).
  - Dashboard: os testes de Distribuição foram atualizados; Métricas não tem teste novo para Solicitante → 403.
  - Relatório: o front já mandava `responsavelId` = próprio id para Atendente (`RelatorioMensalPage.tsx:37`), então nada muda para ele.
  - SignalR: nenhum consumidor no front lia `titulo`, `autor` ou `conteudo` (build ok). O toast de SLA usa `mensagem`, que continua no payload.
  - Chat, Usuários e Grupos: não foram tocados.
  - `ForcarEncerramento`: a checagem própria continua no handler.

  O que falta é a verificação manual (T16).
- **Regra 6 (Obsidian):** **ainda não cumprida**, mas isso é esperado na fase de fechamento. `docs/obsidian/00 Visão/Perfis e Permissões.md` ainda diz "Solicitante: os chamados que **ele abriu**" e "Atendente: ... sem responsável, os seus e os dos colegas", sem "os que abriu" e sem a definição nova de grupo. Precisa ser atualizada antes do PR, junto com `Grupos e Equipes`.
- **Gates:** a T17 está desmarcada no `tasks.md`, mas o revisor rodou os gates e os dois passaram (369/0 e build do front ok). A Rastreabilidade da spec continua "Pendente".

## Sugestões (não bloqueiam)

- **Teste de arquitetura:** um teste por reflexão que falhe se algum `IRequest` em `Features/Chamados` com propriedade `Id`/`ChamadoId` não implementar `IRequerAcessoChamado`. Hoje a garantia é só a convenção comentada na interface.
- **Remover ou filtrar** `ContarPorStatusAsync`, `ObterPorResponsavelAsync`, `ObterAtrasadosAsync` e os outros métodos de leitura sem filtro de visibilidade que não têm uso, para que um consumidor futuro não vaze dados por eles.
- **`AdicionarAnexoCommandHandler`** não confere se o `comentarioId` pertence ao chamado da URL (anterior à feature). Um anexo do chamado A pode ser vinculado a um comentário do chamado B. Não vaza dados, mas gera inconsistência.
- **Atendente não acha na lista os chamados que ele mesmo abriu:** `ChamadosListPage.tsx:49` e `ArquivoChamadosPage.tsx:27` mandam `responsavelId` = próprio id, então os chamados que ele abriu como solicitante, e que outro atendente assumiu, só aparecem no Kanban. O comportamento é anterior à feature e o servidor já permite vê-los (AC-08). Vale uma aba "Abertos por mim" numa próxima feature.
- **`DashboardPage`** dispara as duas queries antes de checar o perfil, então o Solicitante gera dois 403 (com retries do React Query) ao abrir a URL. É inofensivo, mas dá para passar `enabled: perfil.tipo !== 'Solicitante'`.
- **A regra do Relatório (AC-21) está no controller** (`RelatoriosController.cs:43-49`), e não num handler ou behaviour como o resto da autorização. Funciona e foi o que o design pediu. Movê-la para o handler, com `ICurrentUserService`, permitiria testá-la com unidade como o Dashboard.
- **Performance:** `c.SolicitanteEmail.ToLower()` impede o uso de índice em `SolicitanteEmail`. Se o volume crescer, normalizar o e-mail na gravação (`Chamado` já recebe o e-mail do JWT, que vem em minúsculas) e comparar direto.
- **Escopo do branch:** o commit `7e92fb8` inclui `.specs/features/area-e-tipo-do-chamado/spec.md`, que é de outra feature. Avaliar se deve ir neste PR ou num commit de docs separado.
- **Teste faltando:** `ObterMetricasQueryHandler` com Solicitante → `ForbiddenException` (só a Distribuição tem esse teste).

## Verificação dos achados pela sessão principal (2026-10-01)

| Achado | Confirmado? | Observação |
|---|---|---|
| R-01 | Sim, parcialmente mitigado | A tradução EF→SQL foi verificada fora do repositório (console temporário com `ToQueryString()`, sem conexão ao banco) para os 5 perfis: Solicitante sem/com grupo, Atendente sem/com grupo e Admin com grupo. O SQL gerado bate com a regra. Continua sem teste automatizado de comportamento: a limitação está registrada no `tasks.md` e a cobertura vem da T16. |
| R-02 | Sim | O código responde 404 a ações em chamado invisível (não revela que existe). O texto do AC-16 diz "as ações respondem 'sem permissão'". **Decisão do usuário pendente:** manter o 404 e corrigir o texto da spec (recomendado) ou mudar para 403. |
| R-03 | Sim | `ChamadoMappings.cs:28` usa `Comentarios.Count`, que inclui os internos. **Comportamento anterior à feature**; expõe só a quantidade, não o conteúdo. |
| R-04 | Sim | `SlaMonitorService.cs:61,72` usa `Clients.All`. Depois do AC-19 o alerta leva só o número do chamado, sem título. **Comportamento anterior à feature.** |
