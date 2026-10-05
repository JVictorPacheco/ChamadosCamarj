# Análise de impacto — controle-de-acesso

> **Diff:** `origin/develop...feature/controle-de-acesso` — 59 arquivos de código (+ specs) · **Data:** 2026-10-05
> **Gates:** `dotnet build` 0 erros (3 avisos pré-existentes: CS0618 em `HistoricoEntradaConfiguration`, CS8634 no registro de `IEmailSender?` — arquivos/linhas não alterados) · `dotnet test` **506/506** (eram 475) · `npm run build` ok · lint 8 avisos, todos pré-existentes · E2E **19/19** · ao vivo **22/22** + relatório da Solicitante sem equipe (1 × 16 do Admin)
> **Veredito:** IMPACTO COBERTO — nenhum consumidor fora do escopo com comportamento alterado sem teste ou verificação.

## Resumo
- 🔴 0 · 🟡 7 · ✅ 9

## 🔴 Impacto sem cobertura
Nenhum.

## 🟡 Impacto coberto

| Item alterado | Consumidor fora do escopo | O que muda para ele | Cobertura |
|---|---|---|---|
| Menu do `AppLayout` por módulo (antes por perfil) | Todo usuário logado (todas as telas) | **Nada para quem não tem ajuste** (= todos hoje): `Padrao` reproduz a matriz antiga | `ModulosDeAcessoTests.Padroes_SaoIguaisAoMenuDeAntesDaFeature`; banco conferido após a migration (7 usuários, 0 com ajuste); ao vivo AC-16; E2E antigos navegam por Kanban/Dashboard/Fila/Relatório/Arquivo (17/17) |
| Rotas com `RequerModulo`/`RequerAdmin` (`App.tsx`) | Todas as telas de módulo e de Administração | Sem o módulo/perfil, redireciona para "Meus chamados" | E2E: todas as telas antigas abertas como Admin; `controle-de-acesso.spec.ts` (rota bloqueada e rota de Admin) |
| Dashboard: "Solicitante → 403" vira `ModuloGuard` (lê o cadastro) | Dashboard (métricas e distribuição) — autorizacao-chamados AC-13 | Para usuários sem ajuste, resultado idêntico. Mais restrito em 2 casos raros e desejados: usuário **desativado** com token válido e quem teve o perfil rebaixado (o cadastro vale na hora) | `ObterDistribuicaoQueryHandlerTests` (com `ModuloGuard` **real**, só o cadastro simulado); métricas: ao vivo (Atendente 200/403, Solicitante 403) e E2E `dashboard carrega metricas` — o handler de métricas não tem teste unitário próprio (já não tinha) |
| Relatório: `ModuloGuard` + escopo do Solicitante (`RelatoriosController`) | Relatório mensal — autorizacao-chamados AC-21 | Admin e Atendente: **mesmo código** de escopo de antes (Atendente continua forçado aos próprios números); só ganhou o caminho do Solicitante com o módulo | E2E `relatorio mensal` (Admin); ao vivo Solicitante 403/200 e filtro (sem equipe: 1 × 16); `ObterRelatorioMensalQueryHandlerTests.Handle_ComIdsVisiveis_*`. **Controller sem teste unitário** (o projeto não testa controllers) — o ramo do Atendente não mudou (leitura de código) |
| `AtualizarUsuarioPerfilCommand.ChatPerfil` opcional + perfil mudou → zera ajustes | Tela de Usuários (editar, ativar/desativar) — chat-corporativo | Ativar/desativar e editar sem mandar o Chat **não mexem no Chat** (antes mandavam o valor atual = mesmo efeito) | Todos os testes antigos de `AtualizarUsuarioPerfilHandlerTests` (inclusive o que despacha `DefinirChatPerfilCommand`) + `Handle_ChatPerfilNulo_NaoMexeNoChat`, `Handle_QuandoPerfilMuda_*`, `Handle_QuandoPerfilNaoMuda_*`; ao vivo AC-15 |
| `AuthContext` (`Perfil.modulos`, `atualizarAcessos`) | Login, sessão salva no navegador, todas as telas | Sessão salva antes desta versão (sem `modulos`) usa o padrão do perfil até o `/auth/me` do boot | E2E de login; build (tipos); leitura de código do `lerPerfilSalvo` |
| `UsuarioPerfilResponse.Modulos` / `AutenticacaoResponse.Modulos` (aditivos) | Lista de Usuários, `/auth/me`, login (senha e Google) | Campo a mais; nada removido | `ObterPerfilAtualHandlerTests.Handle_InformaOsModulosEfetivos`; ao vivo |

## ✅ Itens sem consumidor fora do escopo
- `ModuloSistema`, `ModulosDeAcesso`, `ModuloGuard` — novos; consumidores só nesta feature.
- `UsuarioPerfil.ModulosConcedidos/Retirados`, `AjustarModulos`, `VoltarAoPadraoDeModulos` — novos.
- `AuditoriaAcesso` + repositório + configuração + `DbSet` — novos.
- Migration `AddControleDeAcesso` — aditiva (2 colunas `default 0` + tabela); aplicada com OK do usuário em 2026-10-05; a versão em produção não mapeia as colunas novas e segue funcionando.
- `IChamadoRepository.ListarIdsVisiveisAsync` — novo; reaproveita `AplicarVisibilidade` sem alterá-la; só o Relatório usa.
- `ObterRelatorioMensalQuery.IdsVisiveis` — opcional; `null` = comportamento antigo (`Handle_ComChamadosNoMes_*` e demais testes do relatório passando).
- API `/api/acessos`, `AcessosAtualizadosNotification` + handler SignalR (`Clients.User` — só a pessoa) — novos.
- `ControleAcessoPage`, `useAcessos`, funções de API do admin, `lib/modulos.ts`, evento `AcessosAtualizados` no `useSignalR` — novos/aditivos.
- `ChatPerfilSelect` (removido) — único consumidor era a lista de Usuários (grep em `frontend/src`); a tela de Chat não o usava.

## Mudanças intencionais de comportamento (no escopo, aprovadas)
- Os controles de Chat saem da tela de Usuários e vão para "Controle de acesso" (C9).
- As páginas de Administração passam a ser bloqueadas também pelo endereço (C8; antes só o menu escondia — o servidor já recusava).
- Mudar o perfil em Usuários zera os ajustes de módulos (AC-15).

## Previsto × real
- Pontos de toque do design tocados: `UsuarioPerfil`/configuração, `AtualizarUsuarioPerfilCommandHandler`, login/`/auth/me`/mappings, Dashboard (2 handlers), Relatório (controller + handler), `AppLayout`, `App.tsx`, `UsuariosPage`/`UsuarioFormDialog` — todos previstos.
- Não listado no design: a **remoção do componente `ChatPerfilSelect`** (ficou sem uso com o C9) e o snapshot do EF registrar `ProductVersion` 9.0.19 (anotação da ferramenta). Nenhum consumidor afetado.
