# Handoff

**Date:** 2026-07-16
**Feature:** Melhorias de visualização de dados (Dashboard + Relatório Mensal) + spec nova (Arquivo de Chamados) + decisão de arquitetura pro login (F5a/F5b)
**Task:** Sessão de 2026-07-15 decidiu dividir o login em F5a (mock por e-mail + cadastro de usuários, não implementado ainda) e F5b (Google real). Sessão de 2026-07-16 (ambiente separado) reconectou ao Supabase, aplicou 4 melhorias de dataviz e criou a spec `arquivo-de-chamados`. Ambas já commitadas/pushadas em `develop`. Próximo passo de código: T09a-T09e (F5a).

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

## In Progress

Nada em execução.

## Pending

1. **Implementar a spec `arquivo-de-chamados`** (Design → Tasks → Execute) — está só no estágio de Spec. Ver notas técnicas na própria spec: estender `ListarChamadosQuery.Status` pra aceitar lista (ou um novo parâmetro `Finalizados=true`), e adicionar o filtro de prioridade que já existe no backend mas não no componente `FiltroChamados.tsx` do frontend.
2. **F5a (T09a-T09e)** — Login mockado por e-mail + cadastro de usuários (Admin). Entidade `UsuarioPerfil`, `UsuariosController` CRUD, tela `Admin > Usuários`, `LoginPage` substitui `ProfileSelector`. Nada implementado ainda — só spec/design/tasks em `.specs/features/fase-6-admin-log/`. **Próximo passo de código relacionado a login.**
3. **T09 (F5b)** — Login real via Google Workspace: endpoint `POST /auth/google`, JWT. Depende de F5a estar pronta.
4. **T15** — Frontend: substituir `LoginPage` (F5a) pelo fluxo OAuth real. Depende de T09.
5. **Documento pra TI** — usuário pediu um texto explicando os pré-requisitos de infra pro Google Workspace OAuth (Client ID, domínio autorizado, redirect URIs, Workspace admin console). Ainda não escrito.
6. Quando T09 (real) entrar: trocar `UsuarioId`/`UsuarioNome` (hoje enviados pelo cliente nos commands de Reatribuir/AlterarPrioridade/Resolver/Fechar/Cancelar) por extração via claims do JWT no backend.
7. "Forçar encerramento" (Admin fechar/cancelar fora do fluxo normal) — item da spec da Fase 6 ainda não abordado.
8. Revisar se Dashboard/Kanban/Fila (soft-RBAC) precisam do mesmo bloqueio real que o Relatório Mensal recebeu, ou se ficam assim até T09. A nova tela `Admin > Usuários` (F5a) também precisa dessa decisão no Execute.
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
