# Review: Área e Tipo do Chamado — rodada 1

> **Revisor:** sub-agente independente (Claude Code, Opus 5.5), sem o contexto da sessão que implementou
> **Data:** 2026-10-02
> **Escopo:** `git diff origin/develop...feature/area-e-tipo-do-chamado`: 110 arquivos, +2425/−698 linhas, 3 commits (`eb4a80b`, `77791b2`, `11ebf04`)
> **Veredito:** BLOQUEADO (1 bloqueante; T13–T16 continuam pendentes, como previsto no `tasks.md`)

**Verificações feitas nesta revisão (sem API e sem banco):**
- `dotnet build`: 0 erros. Os 6 avisos MSB3277 (conflito de versão do EF Core 9.0.1 × 9.0.20 no projeto de testes) já existiam, porque nenhum `.csproj` mudou.
- `dotnet test tests/ChamadosCamarj.UnitTests/`: 390 testes aprovados, 0 falhas.
- `npm run build`: ok. `npm run lint`: 8 avisos, todos em arquivos que a feature não alterou.
- `dotnet ef migrations has-pending-model-changes`: o modelo bate com o snapshot. A API usa EF 9, que recusa `MigrateAsync` quando há divergência, e esse risco não existe aqui.
- `dotnet ef migrations script` (geração offline do SQL) da migration `AddAreaETipoChamado`: o SQL sai correto. Os Guids são formatados sem chaves, as aspas estão certas e tudo roda em uma transação.

## Cobertura da spec

| ID (AC/RF) | Implementado? | Evidência (arquivo:linha ou teste) |
|---|---|---|
| AC-01 | Sim | `AbrirChamadoPage.tsx` (campos Área e Tipo obrigatórios); `AbrirChamadoCommandValidator.cs` (`AreaId`/`TipoId` NotEmpty); `AbrirChamadoCommandHandler.cs:41-47` (área = grupo ativo e tipo ativo); testes `AbrirChamadoHandlerTests.Handle_QuandoAreaInativa_*`, `Handle_QuandoTipoInativo_*`, `AbrirChamadoValidatorTests` |
| AC-02 | Parcial | `AbrirChamadoPage.tsx:49` (`defaultValues.areaId = perfil.grupoId`); `AutenticacaoResponse.GrupoId`; `/auth/me` já devolvia `GrupoId`. Ver R-04: com grupo inativo, o valor padrão fica invisível e inválido |
| AC-03 | Sim | `GET /api/tipos` (ativos); tipos fixos em `AreaETipoPadrao.cs:21-29`. A ordem é alfabética, e não a ordem da spec, que não exige ordem |
| AC-04 | Sim | `KeywordTriagemService.cs` (dicionários de áreas e de tipos); `AbrirChamadoPage.tsx:64-65`. Nenhum teste da triagem foi criado (R-02) |
| AC-05 | Sim | `Chamado.AreaId` → FK para `Grupos`; a migration cria os grupos que faltam a partir das categorias (`AreaETipoPadrao.cs:38-46`) |
| AC-06 | Sim (a conferir em T13) | `UPDATE ... SET "AreaId" = g."Id" FROM Categorias JOIN Grupos ON Nome` (`AreaETipoPadrao.cs:48-52`). Nenhum dado é apagado |
| AC-07 | Sim, sem teste | `ChamadoRepository.cs:210` (ramo do Atendente) e `:213` (ramo do Solicitante), com `\|\| c.AreaId == grupoId`. O ramo sem grupo continua igual. A permissão de cancelar segue em `ChamadoPermissoes` (só quem abriu). Nenhum teste cobre o critério novo (R-02) |
| AC-08 | Sim | `TiposController` + `Features/Tipos/*` + `TiposPage.tsx`; o mapeamento devolve `Tipo?.Nome` mesmo para tipo inativo |
| AC-09 | Sim | `/api/categorias` removido; `GruposPage.tsx` tem o título "Áreas e Grupos"; o menu tem "Áreas e Grupos" e "Tipos de chamado" |
| AC-10 | **Não** | O C# do seeder só cria o que falta, verificando id e nome (`DatabaseSeeder.cs`). Porém o SQL que roda a cada subida recria áreas que o Admin renomeou (**R-01**) |
| AC-11 | Sim, sem teste | Mapeamento `Tipo?.Nome ?? "Não classificado"`; `PATCH /api/chamados/{id}/tipo` (`ChamadosController.cs:162-173`) com `IRequerAcessoChamado` + `AcaoChamado.ReclassificarTipo` (Atendente/Admin); `TipoChamadoCampo.tsx`. Não há teste do handler nem da permissão (R-02) |
| AC-12 | Sim | `ContarPorAreaAsync`/`ContarPorTipoAsync` (dentro de `AplicarVisibilidade`); `DashboardPage.tsx`, gráficos "Por Área" e "Por Tipo" |
| AC-13 | Sim | `ObterRelatorioMensalQueryHandler` (`PorArea`/`PorTipo`); `RelatorioMensalPage.tsx`; `exportar.ts` (CSV com as duas quebras) |
| AC-14 | Sim | `FiltroChamados.tsx` (Área e Tipo); `ChamadosListPage.tsx` lê `areaId`/`tipoId` da URL; `ChamadoRepository.ListarAsync` |
| AC-15 | Sim | `grep -i categori frontend/src` só encontra um comentário do chat (emojis). Ficam pendentes os E2E (`frontend/e2e/*.spec.ts`), conforme o tasks.md, e os comentários de Swagger (`ChamadosController.cs:102`, `DashboardController.cs:19`) |
| AC-16 | Sim | 390 testes verdes; `npm run build` ok |

T13 (aplicar a migration e conferir), T14 (verificação ao vivo), T15 (este review) e T16 (gates finais, Obsidian, rastreabilidade da spec) ficam pendentes, como estava previsto.

## Achados

### 🔴 Bloqueante R-01: o seeder recria, a cada subida, a área que o Admin renomeou (quebra o AC-10)
- **Onde:** `src/ChamadosCamarj.Infrastructure/Data/AreaETipoPadrao.cs:38-46` (passo 1 do `SqlPreencherAreaETipo`), chamado em toda subida por `DatabaseSeeder.cs:52`
- **Problema:** o passo 1 do SQL ("para cada categoria sem grupo de mesmo nome, cria o grupo") roda em toda inicialização da API, e não só na migration. A tabela `Categorias` continua no banco (de propósito, até a limpeza futura) com os 8 nomes antigos. Assim, qualquer área cujo nome ainda coincide com o de uma categoria volta a existir quando o Admin a renomeia. As áreas com Guid fixo e `CASE` (Financeiro e Super e Tendência) escapam, porque o PK colide e o `ON CONFLICT DO NOTHING` descarta a inserção. As outras usam `gen_random_uuid()`, e por isso nada colide.
- **Cenário de falha:** depois do deploy, o Admin renomeia a área "Reembolso" (id `b1000000-…-0001`) para "Reembolsos". A API reinicia, por deploy, reciclagem do App Service ou queda. O C# do seeder pula esse grupo, porque o id existe. Em seguida o SQL vê a categoria "Reembolso" sem grupo "Reembolso" e cria um grupo **novo, ativo e vazio**, chamado "Reembolso". A abertura passa a mostrar "Reembolso" e "Reembolsos", a triagem continua sugerindo o id antigo e chamados começam a cair na área duplicada. O mesmo vale para Credenciado, Comercial, Contas Médicas, Autorização/Auditoria, Atendimento e qualquer categoria criada pelo Admin em produção. Isso contraria o texto do AC-10 ("a edição dele permanece").
- **Correção sugerida:** separar o SQL em dois. O passo 1, de criar grupos a partir de categorias, roda **só na migration**. O seeder roda apenas os passos 2 e 3, que já são idempotentes e inofensivos. Se o seeder ainda precisar criar grupos para chamados abertos pela versão antiga na janela do deploy, restringir a inserção às categorias que tenham chamado sem área: `WHERE NOT EXISTS (...) AND EXISTS (SELECT 1 FROM "Chamados" c WHERE c."CategoriaId" = cat."Id" AND c."AreaId" IS NULL)`. Também vale um teste, mesmo que só sobre o texto do SQL do seeder, garantindo que ele não contém `INSERT INTO "Grupos"`.

### 🟡 Atenção R-02: as regras novas de autorização e de reclassificação não têm teste
- **Onde:** `ChamadoRepository.cs:210,213` (visibilidade por área), `ReclassificarTipoChamadoCommandHandler.cs`, `ChamadoPermissoes.cs` (`ReclassificarTipo`), `KeywordTriagemService.cs`, `DatabaseSeeder.cs`
- **Problema:** a seção 7 do design promete um "teste do novo critério" de visibilidade, e a regra 5 da constitution pede cobertura nos pontos de toque entre features. Os 390 testes são os antigos, ajustados ao novo construtor. Nenhum teste novo exercita o AC-07, o AC-11 (handler e permissão), o AC-04 nem o AC-10.
- **Cenário de falha:** alguém remove `|| c.AreaId == grupoId` de um dos dois ramos, ou acrescenta essa condição no ramo **sem** grupo (o que daria acesso a todo chamado com área nula a quem não tem grupo). Outra possibilidade é trocar a regra para `ReclassificarTipo => true`, o que deixa o Solicitante reclassificar. Nos três casos os 390 testes continuam verdes.
- **Correção sugerida:** no mínimo: (a) testes de `ChamadoPermissoes.Pode(ReclassificarTipo, …)` para Solicitante (false), Atendente e Admin (true); (b) teste do handler de reclassificação: tipo inativo dá NotFound, e o histórico grava `TipoReclassificado` com o valor anterior e o novo; (c) teste da visibilidade por área, se houver como rodar o repositório (por exemplo SQLite em memória), ou ao menos um roteiro registrado na T14; (d) dois ou três casos da triagem ("erro" → Incidente, "como faço" → Dúvida).

### 🟡 Atenção R-03: o cache `['tipos']` é compartilhado entre a abertura (só ativos) e o Admin (todos)
- **Onde:** `frontend/src/features/chamados/hooks/useTiposEAreas.ts:6` e `frontend/src/features/admin/hooks/useTipos.ts:11`. As duas consultas usam `queryKey: ['tipos']`, com `queryFn` diferentes (`/tipos` × `/tipos?apenasAtivos=false`), e o `staleTime` global é de 30 s (`App.tsx:37`).
- **Problema:** o TanStack Query trata as duas consultas como uma só. O problema já existia com `['categorias']`, mas agora existe um tipo inativo que **nunca** pode ser escolhido ("Não classificado").
- **Cenário de falha:** (1) o Admin abre "Tipos de chamado" e, em menos de 30 s, abre um chamado novo ou o detalhe de um chamado. Os seletores de Tipo listam "Não classificado" e os tipos desativados. Se ele escolher um desses, recebe 404 ("A área ou o tipo escolhido não está mais disponível") ou um erro na reclassificação. (2) Na ordem inversa, o Admin passa pela abertura e entra em "Tipos de chamado": a tela mostra só os ativos, e os desativados somem até o cache vencer, então ele não consegue reativá-los.
- **Correção sugerida:** usar chaves distintas, por exemplo `['tipos', 'ativos']` e `['tipos', 'todos']`. As mutações continuam invalidando com o prefixo `['tipos']`.

### 🟡 Atenção R-04: o formulário de abertura pode guardar uma Área ou um Tipo que não aparece no seletor
- **Onde:** `frontend/src/features/chamados/AbrirChamadoPage.tsx:49` (valor padrão) e `:64-65` (sugestão da triagem); `KeywordTriagemService.cs:13-21` (Guids fixos de área); `useTiposEAreas.ts` (`select` filtra as áreas ativas)
- **Problema:** o valor vem de fora da lista exibida (o `grupoId` do perfil ou o Guid fixo da triagem) e não é validado contra ela. O Select do Radix mostra o campo vazio, mas a validação `required` passa, porque o valor existe.
- **Cenário de falha:** (a) o usuário pertence a um grupo que o Admin desativou: a Área aparece em branco, o usuário preenche o resto e envia, e o backend responde 404. A mensagem aparece **no campo Tipo** ("A área ou o tipo…"), embora o problema seja a Área, que ele nem vê selecionada. (b) O Admin desativa a área "Reembolso" ou o tipo "Melhoria", e a triagem continua sugerindo esses ids fixos, com o mesmo efeito. (c) Em produção já existe um grupo "Financeiro" criado pelo Admin com um Guid aleatório: a migration não cria `b1000000-…-0007` (o grupo de mesmo nome já existe), mas a triagem sugere `…-0007`, um id que não existe. Esse caso depende do que a T13 encontrar.
- **Correção sugerida:** só aplicar o valor padrão ou a sugestão quando o id estiver na lista carregada (`areas?.some(a => a.id === id)`). Em caso de 404, marcar o erro nos dois campos ou na mensagem geral. Opcionalmente, a triagem pode consultar áreas e tipos ativos no banco em vez de usar Guids fixos.

### 🟡 Atenção R-05: a migration cria áreas **ativas** a partir de categorias **desativadas**
- **Onde:** `AreaETipoPadrao.cs:43` (`TRUE` fixo; `Categorias."Ativa"` é ignorado)
- **Problema:** toda categoria sem grupo de mesmo nome vira um grupo ativo, inclusive as que o Admin tinha desativado e as criadas pelo Admin que nunca tiveram chamado.
- **Cenário de falha:** em produção há uma categoria desativada (ou criada para teste), por exemplo "Teste". Depois da migration, "Teste" aparece como Área na abertura de **todos** os usuários e entra no Dashboard. O Admin precisa descobrir isso e desativar a área à mão.
- **Correção sugerida:** usar `cat."Ativa"` em vez de `TRUE` na inserção do grupo. Na T13, listar antes as categorias que não correspondem a nenhum grupo pelo nome (`SELECT c."Nome", c."Ativa" FROM "Categorias" c WHERE NOT EXISTS (SELECT 1 FROM "Grupos" g WHERE g."Nome" = c."Nome")`) para saber o que será criado.

### 🟡 Atenção R-06: durante a convivência com a versão antiga, a T14 pode quebrar a produção para chamados reais
- **Onde:** `ReclassificarTipoChamadoCommandHandler.cs:48` (grava `AcaoHistorico.TipoReclassificado`, que é persistido como **string**, conforme `HistoricoEntradaConfiguration.cs:27`); `AbrirChamadoCommandHandler` (grava `CategoriaId = NULL`)
- **Problema:** a migration é compatível com o código antigo, mas **os dados que o código novo grava não são**. A T14 roda a API nova localmente contra o banco de produção enquanto a produção ainda usa a versão antiga.
- **Cenário de falha:** (1) Na T14, alguém testa o AC-11 reclassificando um chamado **real**, antigo e "Não classificado", que é o caso de uso principal. Na produção, a timeline desse chamado passa a dar erro 500 (`Enum.Parse` de "TipoReclassificado" não existe na versão antiga) até o deploy, e a limpeza "dados de teste apagados no final" não cobre essa entrada no histórico de um chamado real. (2) Os chamados de teste abertos pela versão nova têm `CategoriaId` nulo. A versão antiga faz `Include(Categoria)` em navegação obrigatória (INNER JOIN), então eles somem da lista e do detalhe (404), mas entram no `CountAsync`: o total da paginação fica maior que os itens. Só se resolve apagando os dados.
- **Correção sugerida:** na T14, reclassificar **somente** chamados de teste, criados pela própria T14, e apagar também as linhas de `HistoricoEntradas`. Registrar no tasks.md que a janela entre a T14 e o deploy deve ser curta.

### 🟡 Atenção R-07: o `Down` diverge do design e, depois de uso real, perde dados
- **Onde:** `Data/Migrations/20261002114057_AddAreaETipoChamado.cs:100-133`; design §3 ("Volta atrás: … devolve `CategoriaId` a obrigatório")
- **Problema:** o código, corretamente, **não** torna `CategoriaId` obrigatório de novo, e o comentário explica por quê. O design, porém, diz o contrário. Além disso, o `Down` apaga `AreaId`/`TipoId` sem gravar uma categoria nos chamados abertos pela versão nova.
- **Cenário de falha:** a versão nova fica uma semana no ar e um problema obriga a voltar (`Down` + versão antiga). Todos os chamados abertos nessa semana ficam sem categoria, área e tipo. A versão antiga os esconde (INNER JOIN em `Categoria`): eles não aparecem na lista, no detalhe, no Dashboard nem no relatório, embora continuem no banco.
- **Correção sugerida:** atualizar o design §3 para descrever o `Down` real. Se o retorno precisar ser utilizável, fazer o `Down` preencher `CategoriaId` a partir da área antes de apagar as colunas (`UPDATE "Chamados" c SET "CategoriaId" = cat."Id" FROM "Grupos" g JOIN "Categorias" cat ON cat."Nome" = g."Nome" WHERE c."CategoriaId" IS NULL AND c."AreaId" = g."Id"`). Senão, documentar que o `Down` só é seguro antes do deploy.

### 🟡 Atenção R-08: renomear um tipo com o nome de outro dá erro 500
- **Onde:** `AtualizarTipoChamadoCommandHandler.cs:31` (sem checagem de duplicidade; o `CriarTipoChamadoCommandHandler` tem essa checagem); índice único `IX_TiposChamado_Nome`
- **Cenário de falha:** o Admin edita "Melhoria" para "Incidente". O `SaveChanges` viola o índice único, a `DbUpdateException` vira erro 500 genérico e a tela mostra "Serviço indisponível" em vez de "Já existe um tipo com esse nome".
- **Correção sugerida:** repetir a checagem do `Criar` (ignorando o próprio id) e lançar `ConflictException`, que retorna 409.

## Constitution e contratos

- **Regra 1 (sem suposição silenciosa):** cumpre. A lista de palavras da triagem foi declarada como proposta reversível.
- **Regra 2 (spec antes do código):** cumpre. A spec foi aprovada em 01/10 e o design ajustado em 02/10, antes do código. A tabela de rastreabilidade da spec (§5) segue "a definir" e deve ser preenchida na T16.
- **Regra 3 (contrato sinalizado antes):** cumpre. A tabela do design §2 bate com o código: `Chamado`, `ChamadoResponse`, `AbrirChamadoRequest`, filtros, `IChamadoRepository` (`ContarPorArea`/`ContarPorTipo`), `DashboardMetricsResponse` (`PorArea`/`PorTipo` com `PorNomeItem(Nome, Id, Quantidade)`), `RelatorioMensalResponse`, `TriagemSugestao`, `/api/tipos`, login com `grupoId`. Houve uma mudança não listada, mas interna: `EventoRelatorioItem` (`CategoriaNome` → `AreaNome` + `TipoNome`). Sobre `GET /api/grupos`: o design diz "liberado para leitura", mas o endpoint **já** não tinha restrição de perfil antes (sem `[Authorize(Roles)]` e sem guard), então nada mudou no backend.
- **Regra 4 (orquestração SDD):** em andamento (review = T15).
- **Regra 5 (cross-feature):** os pontos de toque do design §7 foram alterados de forma coerente: `AplicarVisibilidade` nos dois ramos com grupo, e o ramo sem grupo intacto. Faltam testes (R-02), e o seeder tem a regressão R-01.
- **Regra 6 (Obsidian):** pendente (T16): Abertura de Chamados, Grupos e Equipes, Perfis e Permissões, Fila/Kanban/Dashboard, Relatório Mensal, Administração e ADR-008.
- **Contratos TS × C#:** conferidos campo a campo: `ChamadoResponse`, `AbrirChamadoRequest`, `TipoChamadoResponse` (`ativo`), `AtualizarTipoChamadoCommand` (`nome`, `descricao`, `ativo`), `TriagemSugestao`, `PorNomeItem`, `PorNomeQuantidadeItem`, `AutenticacaoResponse.grupoId`, `UsuarioPerfilResponse.grupoId`, `AcaoHistorico.TipoReclassificado` e o rótulo na `TimelineHistorico`. Batem.
- **Migration × versão antiga em produção:** compatível. Colunas novas anuláveis, `CategoriaId` só perde o `NOT NULL`, nada é apagado, os novos valores de enum são acrescentados no fim (`AcaoHistorico = 12`; `AcaoChamado` não é persistido) e o seeder antigo só mexe em `Grupos`/`Categorias` pelos ids antigos. A ressalva fica para os **dados** gravados pela versão nova (R-06).
- **Idempotência do SQL:** os passos 2 e 3 são idempotentes (`WHERE "AreaId" IS NULL`, `WHERE "TipoId" IS NULL`). O passo 1 é idempotente apenas no sentido de não duplicar o mesmo nome; o problema está em recriar após uma renomeação (R-01). Casos verificados: `Descricao` nula ou vazia leva a `'Área ' || Nome` (cabe em 300 caracteres); `Categorias.Nome` e `Grupos.Nome` são únicos, com o mesmo tamanho (100), então não há conflito de nome dentro do próprio INSERT; grupo inativo de mesmo nome não é recriado e os chamados apontam para ele, o que é aceitável; nome igual com diferença só de maiúsculas e minúsculas gera grupo separado (ver Sugestões).
- **Guids fixos:** `c1000000-…` fica numa tabela nova. `b1000000-…-0007/0008` continuam a sequência do seed (01–06), e os grupos do Admin usam Guid v4 aleatório. Não há colisão realista.
- **`ExecuteSqlRawAsync`:** sem parâmetros e sem `{`/`}` no texto gerado (o Guid usa o formato "D"), portanto funciona. As aspas simples e duplas estão corretas, e o Npgsql aceita vários comandos por chamada.

## Sugestões (não bloqueiam)

- O filtro de Tipo da lista só oferece tipos ativos, então não dá para filtrar "Não classificado", que é justamente a fila de trabalho do AC-11. Hoje só se chega a ela pelo clique no gráfico do Dashboard. Vale incluir "Não classificado" no filtro.
- O Admin pode **ativar** ou **excluir** o tipo "Não classificado" pela tela de Tipos. Ativado, ele aparece na abertura. Excluído (possível quando nenhum chamado o usa), o seeder o recria. Vale proteger esse id no `Atualizar`/`Excluir` ou escondê-lo da tela.
- Triagem: a palavra-chave `"?"` em Dúvida pontua qualquer frase com interrogação ("Erro ao salvar?"). Vale conferir como o `MelhorMatch` desempata.
- `defaultValues.areaId` é lido só na primeira renderização. Se o perfil salvo no `localStorage` for anterior ao deploy (sem `grupoId`) e o `/auth/me` responder depois da montagem, a Área não vem preenchida. Dá para usar `reset`/`setValue` quando `perfil.grupoId` chegar e o campo ainda estiver vazio.
- A comparação de nomes categoria × grupo diferencia maiúsculas de minúsculas: uma categoria "comercial", criada pelo Admin, viraria uma área separada de "Comercial". Vale conferir na T13.
- `ReclassificarTipoChamadoCommandHandler`: o `Update(chamado)` marca como modificado o grafo inteiro, inclusive o `TipoChamado` antigo carregado pelo `Include`. Isso gera UPDATEs inúteis e segue o padrão já existente, mas vale confirmar na T14 que o `TipoId` novo é persistido, porque não há teste.
- Comentários de Swagger ainda dizem "categoria": `ChamadosController.cs:102` e `DashboardController.cs:19`.
- O botão "Novo grupo" na tela "Áreas e Grupos" poderia dizer "Nova área/grupo", para combinar com o título.
- E2E (`frontend/e2e/*.spec.ts`) e `README.md`/`frontend/README.md` ainda falam em Categoria. Isso já está registrado no tasks.md.
- A localização da migration (`Data/Migrations/`) diverge de `CONVENTIONS.md §2.11` (`Infrastructure/Migrations/`), mas segue as migrations recentes. Vale atualizar a convenção.
