# Handoff

**Date:** 2026-07-18
**Feature:** Os 3 passos confirmados pelo usuário em 2026-07-16 estão TODOS concluídos + Design do T09/F5b (login real Google) adiantado
**Task:** Passo 2 (`arquivo-de-chamados`) e passo 3 (documento pra TI) concluídos e commitados. Depois disso, adiantado o **Design** completo do T09/F5b (`.specs/features/fase-6-admin-log/design-t09-google-oauth.md`), com pesquisa de documentação atual (Context7 + Microsoft Learn) e 3 decisões de segurança confirmadas com o usuário (chave JWT simétrica, token 8-12h sem refresh, logout automático por 20min de inatividade). **Ainda não commitado** — ver Pending. Execute do T09 continua bloqueado até a TI devolver o Client ID, mas o Design/Tasks não dependem disso.

## Completed ✓

- **Reconectado ao Supabase** — `user-secrets` configurado com a connection string do Session pooler **no ambiente daquela sessão** (ver nota de isolamento de ambiente em Context). **Nota para quem reconectar de novo:** o usuário teve bastante dificuldade com isso — resetou a senha do Supabase várias vezes e continuava dando `28P01: password authentication failed`. A causa raiz: no dashboard do Supabase, o campo que mostra uma senha sugerida **não a aplica de verdade** até clicar no botão de confirmar/reset — o usuário estava copiando a sugestão sem confirmar, então a senha antiga continuava sendo a válida. Se isso voltar a acontecer, confirmar que o reset foi de fato clicado, não só a senha "gerada" visualmente.
- **Aplicação rodou contra o Supabase real** — migration pendente (`AddHistoricoEntrada`) aplicada automaticamente no restart da API. Usuário testou manualmente e aprovou o resultado da Fase 6/7 contra dados reais.
- **Spec nova criada:** `.specs/features/arquivo-de-chamados/spec.md` — tela separada pra chamados finalizados (Resolvido/Fechado/Cancelado), com filtro por período (DataCriacao) e prioridade, reaproveitando `GET /api/chamados`. Decisão explícita do usuário: **nunca apagar chamados** (contrariaria auditoria/`HistoricoEntrada`/Relatório Mensal) — a solução é uma tela de leitura filtrada, não exclusão. Status: DRAFT, aguardando Design. Ainda **não implementada**.
- **4 melhorias de visualização de dados implementadas, testadas e já em `develop`:**
  1. `DonutChart`/`CategoriaChart`/`DashboardPage`/`RelatorioMensalPage` pararam de usar hex fixo (`#f59e0b`, `#3b82f6`, etc.) e passaram a usar os tokens do tema (`var(--chart-1)`..`var(--chart-5)`, `var(--status-good)`, `var(--status-critical)`) definidos em `frontend/src/index.css`.
  2. **Corrigido bug real no próprio tema:** `--chart-1..5` do modo **claro** eram literalmente escala de cinza (`oklch(0.87 0 0)` etc, chroma zero — sobra do boilerplate shadcn, só o dark tinha sido customizado com a "Paleta Camarj"). Substituído por 5 cores validadas (`#2a78d6, #1baf7a, #eda100, #008300, #4a3aa7`).
  3. **Rosca (Donut) ganhou labels diretos nas fatias** (`DonutChart.tsx`) — antes o valor só aparecia no hover do Tooltip, inútil no PDF exportado do Relatório Mensal. `isAnimationActive={false}` setado na `Pie` pra garantir que os labels apareçam sempre no print.
  4. **Variação % do Relatório Mensal agora tem cor de sinal** — `DashboardKpi` ganhou prop `subtextoTom?: 'bom' | 'ruim'`. Mais Resolvidos = verde; mais Cancelados = vermelho, menos Cancelados = verde. "Abertos" ficou sem tom.
  - `npm run build`/`npm run lint` limpos. Usuário concordou explicitamente com o diagnóstico e as 4 mudanças antes da implementação.
- **F5a/F5b especificadas** (sessão 2026-07-15): spec de `fase-6-admin-log` dividida — F5a (login mockado por e-mail + `UsuarioPerfil` + cadastro de usuários pelo Admin) como passo intermediário não descartável, F5b (Google OAuth real) reaproveitando a mesma tabela depois. `design.md` e `tasks.md` (T09a-T09e) criados em `.specs/features/fase-6-admin-log/`.
- **F5a IMPLEMENTADA, revisada e PUSHADA em `develop`** (sessão seguinte, mesmo dia 2026-07-16): T09a-T09e completas via Execute do skill spec-driven — enum `Perfil`, entidade/tabela `UsuarioPerfil`, `UsuariosController` (4 endpoints), `AuthContext`/`LoginPage` reescritos, tela `Admin > Usuários` com botão de Desativar/Reativar e bloqueio real de RBAC. Usuários de teste: Victor (Admin), Fábio (Atendente), Ana Colaboradora (Solicitante). Antes do commit, uma revisão de code review sênior (backend + frontend, pedida explicitamente pelo usuário) encontrou 19 itens — os 4 de severidade Alta foram corrigidos na hora (reativação de e-mail desativado quebrando com 500; três casos de erro engolido em silêncio no frontend: desativar usuário, assumir chamado na fila, carregar atendentes pra reatribuir). Os outros 15 (Médio/Baixo) foram documentados em `.specs/codebase/CONCERNS.md` (seção "EM ABERTO") — **usuário pediu explicitamente pra não esquecer de tratá-los depois.** Commitado (`76ce0d1` + `a0747a7`, o segundo corrigindo um arquivo esquecido no primeiro) e confirmado pushado pelo usuário no terminal dele.
- **Débito técnico resolvido e commitado** (2026-07-17): 15 itens (D-01 a D-15) de `CONCERNS.md` corrigidos via 2 agentes em paralelo + 1 correção manual complementar (D-01 precisou do frontend do Kanban enviando `usuarioId`/`usuarioNome` reais, que os agentes tinham deixado passar). 2 decisões de design confirmadas: enum `AcaoHistorico.StatusAlterado`, `PerfilRequisitanteGuard` compartilhado.
- **`arquivo-de-chamados` IMPLEMENTADA e commitada** (2026-07-18): Design → Tasks → Execute completos, seguindo o skill spec-driven do zero. Nova tela "Arquivo" (todos os perfis, mesmo RBAC de "Meus Chamados") lista só chamados Resolvido/Fechado/Cancelado, com filtros de status/prioridade/categoria/busca/período. Backend: `Finalizados=true` + `DataInicio`/`DataFim` em `ListarChamadosQuery`, aditivo (não quebra Kanban/Fila que usam `Status` sozinho). **Bug real encontrado pelo usuário ao testar** (filtrar por data quebrava com 500 — `DateTime Kind=Unspecified` vs coluna `timestamptz` do Postgres) — corrigido no `ListarChamadosQueryHandler` convertendo pra UTC explicitamente, com `DataFim` virando fim do dia. Reteste do usuário confirmou ok. Ajuste de UX também feito: filtro de período só aparece no Arquivo (não em "Meus Chamados", que reaproveitava o mesmo componente sem necessidade), com labels visíveis "De"/"Até". 174 testes passando.

## In Progress

Nada em execução.

## Pending — os 3 passos confirmados em 2026-07-16 estão TODOS concluídos

1. ~~🥇 Resolver o débito técnico da revisão sênior~~ **✅ CONCLUÍDO e commitado em 2026-07-17.**
2. ~~🥈 Implementar a spec `arquivo-de-chamados` "com tudo certinho"~~ **✅ CONCLUÍDO, testado e commitado em 2026-07-18** — ver "Completed" acima pro bug de DateTime/UTC encontrado e corrigido.
3. ~~🥉 Documento pra TI sobre Google Workspace OAuth~~ **✅ CONCLUÍDO em 2026-07-18** — `.specs/features/fase-6-admin-log/oauth-requisitos-ti.md`, escrito em linguagem não-técnica, cobre o que fazer no Google Cloud Console e o que devolver (Client ID). Sinaliza que o redirect URI de produção depende da decisão de hospedagem (pendência separada).

**A partir daqui não há mais uma ordem confirmada pelo usuário — perguntar antes de assumir prioridade.** Candidatos naturais (ver `STATE.md` → TODOs):
4. **T09 (F5b) — Design CONCLUÍDO em 2026-07-18** (`design-t09-google-oauth.md`), Tasks/Execute ainda não feitos. Endpoint `POST /auth/google`, JWT simétrico (8-12h, sem refresh), logout automático por 20min de inatividade, `ICurrentUserService` substituindo `UsuarioId`/`UsuarioNome`/`perfilRequisitante` client-supplied. **Execute bloqueado** até a TI devolver o Client ID — mas o **Tasks** (quebra atômica) pode ser feito antes disso, não depende do valor real do Client ID.
5. **T15** — Frontend: substituir `LoginPage` (F5a, já em produção) pelo fluxo OAuth real. Já coberto no design do T09 acima (T09.5/T09.6).
6. ~~Quando T09 (real) entrar: trocar `UsuarioId`/`UsuarioNome` por claims do JWT~~ — já endereçado no design via `ICurrentUserService`, falta só o Execute.
7. "Forçar encerramento" (Admin fechar/cancelar fora do fluxo normal) — item da spec da Fase 6 ainda não abordado.
8. Revisar se Dashboard/Kanban/Fila (soft-RBAC) precisam do mesmo bloqueio real que o Relatório Mensal e o `Admin > Usuários` já receberam, ou se ficam assim até T09.
9. Fase 4 (Email/Storage) segue sem data.
10. **Menor:** os tokens `--chart-1..5` do modo **escuro** passam em contraste/CVD/chroma no validador de paleta, mas falham no check de "lightness band" (ficam um pouco claros/vibrantes demais pra uma superfície escura). Não é urgente — visualmente aprovado pelo usuário.

## Blockers

Nenhum.

## Context — MUITO IMPORTANTE PRA RETOMAR

- Branch de trabalho: `develop`, atualizada com o remoto (a `feature/fase-6-admin-log` já foi mergeada há tempo, não usar mais).
- **user-secrets local NÃO tem o Supabase configurado por padrão neste ambiente** — a reconexão da sessão de 2026-07-16 foi feita num clone/ambiente separado do Claude Code, e os `user-secrets` (`~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`) não persistem entre ambientes diferentes, mesmo sendo o mesmo repositório/`UserSecretsId`. **Sempre rodar `dotnet user-secrets list` dentro de `src/ChamadosCamarj.WebApi` no início da sessão** antes de assumir que já está configurado.
- **Se a API não conectar no Supabase:** ver "Completed" acima sobre o bug do reset de senha no dashboard do Supabase — o sintoma é sempre `28P01: password authentication failed for user "postgres"` mesmo com a senha "certa" colada. Testar a connection string isolada (fora do `dotnet run`, ex: um script `.cs` de arquivo único com `#:package Npgsql@8.0.5`) ajuda a isolar se é a senha ou outra coisa.
- Push/PR precisam ser feitos manualmente pelo usuário no terminal real dele — o ambiente do Claude Code não tem credenciais do GitHub configuradas (`git push` sempre falha aqui, mesmo com a branch trackeada corretamente).
- Decisões e aprendizados completos em `.specs/project/STATE.md` (seções Decisões e Aprendizados — ler antes de mexer em auditoria/histórico, RBAC, métricas de dashboard/relatório, ou login).
- Spec/design/tasks completos do Relatório Mensal em `.specs/features/relatorio-mensal/` — úteis de referência se a Fase 7 for expandida (período livre, SLA em tempo real, etc.)
- A ferramenta `dataviz` (validador de paleta, `node scripts/validate_palette.js "<hex,hex,...>" --mode light|dark --surface <hex>`) foi usada pra validar as cores da sessão de 2026-07-16 — útil pra qualquer mudança futura de cor em gráfico.
