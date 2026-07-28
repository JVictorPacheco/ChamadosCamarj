# Roadmap — ChamadosCamarj

> Última atualização: 2026-07-27

## ✅ Fase 0 — Setup

- [x] SPEC finalizado e Obsidian estruturado
- [x] Solução .NET 9 criada (4 projetos)
- [x] Repositório GitHub
- [x] Docker Compose (PostgreSQL local)
- [x] SQLite configurado para dev

## ✅ Fase 1 — Domain Layer (COMPLETA)

- [x] `BaseEntity` (Id Guid, DataCriacao, DataAtualizacao)
- [x] Entidades: `Chamado`, `Comentario`, `Categoria`, `Anexo`
- [x] Enums: `StatusChamado`, `PrioridadeChamado`, `OrigemChamado`, `TipoComentario`
- [x] Interfaces: `IChamadoRepository`, `ICategoriaRepository`, `IEmailReceiverService`, `IStorageService`
- [x] Lógica de negócio: Atribuir, Resolver, Fechar, Reabrir, Cancelar, AlterarPrioridade
- [x] SLA calculado automaticamente no construtor do Chamado
- [x] Migration inicial (PostgreSQL schema)

## ✅ Fase 2 — CQRS + API (COMPLETA)

- [x] Commands: Abrir, Atribuir, Atualizar, Comentar, Resolver, Fechar, Cancelar
- [x] Queries: Listar (com filtros e paginação), ObterPorId
- [x] Validators: todos os Commands
- [x] Controllers: ChamadosController (endpoints completos), CategoriasController
- [x] Pipeline Behavior (FluentValidation)
- [x] Mapeamento manual `Chamado → ChamadoResponse`
- [x] OpenAPI + Scalar UI
- [x] Seed das 5 categorias da CAMARJ

## ✅ Fase 2.5 — Correções antes de avançar (COMPLETA)

> Identificados no mapeamento — todos resolvidos em 2026-06-19

- [x] C-01: Resolver conflito SQLite dev vs PostgreSQL migration — migrado para Supabase (session pooler)
- [x] C-02: Mover filtros de ListarChamados para query no banco
- [x] C-03: CategoriasController usar MediatR
- [x] C-04: Remover seed inline de Program.cs, usar DatabaseSeeder
- [x] C-05: Criar validators para Atribuir e Comentar
- [x] C-06: Criar Commands + Endpoints para Fechar e Cancelar
- [x] C-07: Corrigir migration — adicionar ComentarioId em Anexos
- [x] C-09: Criar projeto de testes (Domain unit tests) — 59 testes passando
- [x] 3 bugs de teste manual corrigidos: categoria inexistente sem validação, transições de status sem guard, DbUpdateConcurrencyException ao comentar

## ✅ Fase 3 — Frontend: Portal do Solicitante (COMPLETA)

> Spec em `.specs/features/frontend-portal-solicitante/spec.md`. Escopo: visão do Solicitante. Auth mockada.

- [x] API-01: endpoint `GET /api/chamados/{id}/comentarios`
- [x] API-02: filtro `solicitanteEmail` em `GET /api/chamados`
- [x] Setup React 19 + Vite + TailwindCSS v4 + Shadcn/ui, tema dark customizado
- [x] Seletor de perfil mockado (Admin/Atendente/Solicitante) — Google Workspace real fica pra Fase 6
- [x] Abertura de chamado — `AbrirChamadoPage`
- [x] Lista de chamados com filtros + paginação — `ChamadosListPage`
- [x] Detalhe do chamado — `ChamadoDetailPage` (404 + RBAC-lite de UI)
- [x] Comentários públicos — `ComentarioList` / `ComentarioForm`
- [x] 1 teste E2E (Playwright) cobrindo o fluxo feliz completo

## ✅ Fase 5 — Kanban + Dashboard + Ações de Atendente (COMPLETA)

> Spec em `.specs/features/fase-5-kanban-dashboard/spec.md`. Mergeado em `develop` e `main` (2026-06-30).

- [x] Kanban com drag & drop (dnd-kit) entre colunas de status
- [x] Dashboard com métricas: total por status, alertas de SLA, chamados recentes
- [x] Notificações SignalR em tempo real (criação, mudança de status, comentários)
- [x] Fila de Atendimento — lista de chamados Abertos ordenados por prioridade
- [x] Botão Assumir na Fila de Atendimento
- [x] Ações no Detalhe do Chamado: Assumir, Resolver, Fechar, Cancelar (por perfil + status)
- [x] RBAC de UI: atendentes/admin veem ações corretas, solicitante só vê o que é seu
- [x] "Meus Chamados" diferenciado por perfil (Admin=todos, Atendente=responsavelId, Solicitante=solicitanteEmail)
- [x] Bug fix: Link aninhado no card da Fila eliminado
- [x] Bug fix (2026-07-14): Dashboard não mostrava Cancelados/Resolvidos (só "hoje"); card "Abertos" agora detalha assumidos vs em espera
- [x] Retrabalho (2026-07-14/15): gráfico de Tendência (linha, 7 dias) substituído por rosca "Distribuição por situação" (Aguardando/Assumido/Resolvido/Encerrado/Cancelado, foto do momento); KPIs simplificados pra só Resolvidos Hoje + Tempo Médio, já que a rosca cobre o resto

## 📧 Fase 4 — Integração Email + Storage (Storage CONCLUÍDO — 2026-07-21; Email ainda pendente)

> Storage de Anexos implementado e verificado de ponta a ponta contra o Supabase real. Email/IMAP segue sem data — depende de senha de app das caixas suporte@/ti@camarj.com.br, que o usuário ainda não tem. Spec/design/tasks completos em `.specs/features/anexos-storage/`.

- [ ] `EmailReceiverService` (IMAP/MailKit — suporte@camarj.com.br / ti@camarj.com.br)
- [ ] Parsing de e-mail → Chamado automático
- [ ] Resposta automática ao solicitante
- [x] `StorageService` (Supabase Storage S3) — `SupabaseStorageService`, pacote NuGet `Supabase`, bucket `chamados-anexos`
- [x] Upload/download de anexos no portal — upload multipart (PDF/imagens/Word/Excel/ZIP, máx 10MB), listagem, download via URL assinada (expira 1h). `Anexo.EnviadoPorId/Nome` via `ICurrentUserService` (não client-supplied)
- [x] Verificado de ponta a ponta contra o Supabase Storage real: upload, listagem, geração de URL e download real (conteúdo conferido byte a byte)
- [x] **Anexar já ao abrir chamado ou ao responder um comentário** (2026-07-24) — antes só dava na tela de Detalhe; múltiplos arquivos de uma vez, reaproveita o `comentarioId` que o backend já aceitava
- [x] **Remover anexo (2026-07-24)** — exclusão real (Storage + banco), pop-up de confirmação, RBAC real: Admin remove qualquer um, Atendente/Solicitante só o que enviaram. Reverte a decisão original "nunca remove anexo" — ver `.specs/features/anexos-storage/spec.md`
- **2 bugs reais encontrados e corrigidos (2026-07-21):** (1) API não subia sem a Service Role Key configurada — `ValidateOnBuild` do ASP.NET Core, corrigido com `NullStorageService` como fallback; (2) SDK `Supabase` v1.3.0 devolve `CreateSignedUrl` com um `?` sobrando no final, quebrando o JWT — corrigido com `TrimEnd('?')`. Ver `STATE.md` (Aprendizados) pro detalhe completo

## 🔐 Fase 6 — Admin Completo + Log + Google Workspace (código do Google MANTIDO, mas SUBSTITUÍDO por login e-mail/senha)

> **ATUALIZAÇÃO 2026-07-24: a TI informou que o Client ID do Google OAuth está fora do plano da CAMARJ** — não é mais uma questão de aguardar, é uma mudança de direção. O login real via Google (T09/F5b, descrito abaixo) segue implementado e commitado, mas não vai ser usado em produção. Decisão: login por **e-mail + senha** no lugar do botão do Google, reaproveitando toda a infraestrutura de JWT/RBAC já existente. Ver `.specs/features/auth-email-senha/` (spec + tasks) para o detalhe completo — feature **EM ANDAMENTO**, começada em 2026-07-24.

> **Pausada em 2026-07-14, retomada em 2026-07-15/18.** T01-T14 mergeados em 2026-07-15; F5a implementada em 2026-07-16; T09/F5b/T15 (login real Google) implementados em 2026-07-18 — histórico abaixo preservado como está, mesmo com a mudança de direção do login.

> Corrigido em 2026-06-25: Camarj usa Google Workspace (Gmail corporativo), não Azure AD.
> Planejado em 2026-07-01: features de Admin e auditoria.
> Spec em `.specs/features/fase-6-admin-log/spec.md`, design/tasks em `design-t09-google-oauth.md`/`tasks-t09-google-oauth.md`.
> Trabalho feito em `feature/fase-6-admin-log`, **mergeada em `develop` via PR #13 em 2026-07-15**. T01-T14 concluídos e verificados via Playwright.

**Backend — completo (T01-T08), incluindo ator real na auditoria:**
- [x] T01 `HistoricoEntrada` + enum `AcaoHistorico` + `IHistoricoRepository`
- [x] T02 `Chamado.Reatribuir()` na domain entity
- [x] T03/T04 `ReatribuirChamadoCommand` + endpoint `PATCH /chamados/{id}/reatribuir`
- [x] T05 Geração de `HistoricoEntrada` integrada em todos os CommandHandlers (Abrir, Atribuir, Resolver, Fechar, Cancelar, Reatribuir, AlterarPrioridade) — `UsuarioId`/`UsuarioNome` agora vêm do `AuthContext` mockado do frontend em vez de "Sistema" fixo (ver Aprendizados)
- [x] T06 `ListarHistoricoQuery` + endpoint `GET /chamados/{id}/historico`
- [x] T07 endpoint `PATCH /chamados/{id}/prioridade`
- [x] T08 Filtro de comentários internos por perfil em `ListarComentariosQueryHandler` (endpoint corrigido pra repassar `perfilUsuario`)
- [x] **F5a (decidido em 2026-07-15, IMPLEMENTADO e MERGEADO em `develop` em 2026-07-16):** Login mockado por e-mail + cadastro de usuários (Admin) — tabela `UsuarioPerfil`, `UsuariosController` (CRUD), tela `Admin > Usuários` com bloqueio real de RBAC, `LoginPage` substitui `ProfileSelector`. T09a-T09e completas, testadas contra o Supabase real, revisadas por um code review sênior (4 bugs Altos corrigidos antes do commit), validadas pelo usuário e pushadas (commits `76ce0d1`/`a0747a7`). Não é descartável: a tabela `UsuarioPerfil` é reaproveitada sem mudança pelo T09 real. Os 15 itens de débito técnico (Médio/Baixo) da revisão foram todos corrigidos em 2026-07-17 (ver `CONCERNS.md`, seção RESOLVIDOS)
- [x] **T09 (F5b) Login Google Workspace real IMPLEMENTADO em 2026-07-18** (código completo — Design → Tasks → Execute): `POST /auth/google`, JWT próprio (simétrico), autenticação global, `ICurrentUserService` substituindo `UsuarioId`/`UsuarioNome`/`perfilRequisitante` client-supplied em todos os Controllers. Logout automático por 20min de inatividade. 177 testes passando. **Falta só o Client ID real da TI** pro teste de ponta a ponta funcionar — ver `.specs/features/fase-6-admin-log/tasks-t09-google-oauth.md`
- [x] Documento pra TI com pré-requisitos de infra — `.specs/features/fase-6-admin-log/oauth-requisitos-ti.md` (2026-07-18). Aguardando a TI configurar e devolver o Client ID

**Frontend — completo (T10-T14, T15), reescrito e verificado em 2026-07-14/2026-07-18:**
- [x] T10-T14 (Reatribuir, Histórico, Alterar Prioridade, Comentário interno) — os componentes originais tinham sido commitados no caminho errado (`src/ChamadosCamarj.Web/...`) usando padrões inexistentes no projeto (axios, toast, shadcn não instalado, tema claro). Reescritos do zero em `frontend/src/features/chamados/`, seguindo os padrões reais (`apiFetch`, erro inline, shadcn via CLI, tema dark). `ComentarioForm`/`ComentarioList` estendidos em vez de duplicados.
- [x] **T15 — Login real via Google Workspace IMPLEMENTADO em 2026-07-18** — `LoginPage` (F5a) substituída pelo botão `GoogleLogin`/`GoogleOAuthProvider`, `AuthContext.loginComGoogle`, `apiFetch` com Bearer + logout automático em 401
- [x] **Forçar encerramento** — Admin fecha um chamado direto de qualquer status não-final (Aberto/EmAndamento/Resolvido), com motivo obrigatório auditado no histórico (`AcaoHistorico.EncerramentoForcado`). Implementado e verificado em 2026-07-19 (`.specs/features/forcar-encerramento/`) — falta só o clique real no navegador, bloqueado pelo Client ID do Google (mesma pendência do T09/F5b)
- [x] ~~Mapeamento conta→perfil no backend~~ → entra pelo F5a (tabela `UsuarioPerfil`)
- [x] **RBAC real (baseado em claims do token Google) IMPLEMENTADO** — `ICurrentUserService` lê `perfil`/`sub`/`name` dos claims do JWT em todos os Controllers
- [ ] Admin: gerenciar categorias, usuários e configurações do sistema

## 📈 Fase 7 — Relatório Mensal (CONCLUÍDA — antecipada)

> **Antecipada em 2026-07-14** na frente de T09/T15 — usuário precisa de um relatório mensal de andamento dos chamados pra apresentar à superintendência todo fim de mês. Spec completo em `.specs/features/relatorio-mensal/` (spec → design → tasks → execute). **Mergeada em `develop` via PR #13 em 2026-07-15.**

- [x] **Relatório mensal**: seletor de mês, totais (abertos/resolvidos/cancelados + variação % vs mês anterior), quebra por categoria e por atendente, cumprimento de SLA (rosca), tempo médio de resolução
- [x] Dados vindos de `HistoricoEntrada` (data real de cada evento — REL-10), não do status atual do chamado
- [x] Exportação CSV (client-side) e PDF (via impressão, `@media print` dedicado) — sem bibliotecas novas
- [x] RBAC: Admin vê tudo, Atendente só os próprios números (sem quebra por atendente), Solicitante bloqueado de verdade (não só link escondido)
- [x] Bug corrigido de quebra: Dashboard tinha `ObterTendenciaAsync` contando "resolvidos" pela data de criação, não de resolução
- [ ] Relatórios por período livre (granularidade além do mês fechado) — fora de escopo por ora
- [ ] SLA tracking com alertas de vencimento (tempo real — diferente do relatório, que é histórico)
- [ ] Exportação CSV/PDF
- [ ] Dashboard de carga por atendente

**Retrabalho de visualização de dados (2026-07-16):** Dashboard e Relatório Mensal revisados como um review de dev sênior — cores de gráfico migradas de hex fixo pra tokens do tema (`--chart-1..5`, `--status-good/critical` em `frontend/src/index.css`, validados com a skill `dataviz`), bug de cor cinza-puro corrigido no tema claro, labels diretos nas fatias da rosca (o hover não existe no PDF exportado), cor de sinal (verde/vermelho) na variação % do Relatório Mensal. Detalhes em `.specs/HANDOFF.md`.

## 🗄️ Fase 8 — Arquivo de Chamados (CONCLUÍDA — 2026-07-18)

> Pedido pelo usuário em 2026-07-16 ao testar a aplicação: chamados finalizados (Resolvido/Fechado/Cancelado) misturados com os ativos nas telas do dia a dia. Decisão explícita: **nunca apagar chamados** (quebraria `HistoricoEntrada`/Relatório Mensal) — solução é uma tela separada de leitura filtrada. Spec/design/tasks completos em `.specs/features/arquivo-de-chamados/`.

- [x] Listar só chamados finalizados, paginado, reaproveitando `GET /api/chamados` (`Finalizados=true`)
- [x] Filtro por período (`DataCriacao`) — bug de DateTime/UTC encontrado pelo usuário ao testar e corrigido na mesma sessão
- [x] Filtro por prioridade (backend já suportava, UI adicionada em `FiltroChamados.tsx`)
- [x] Filtro por status/categoria/busca (reaproveitado `FiltroChamados.tsx`, com `statusOptions` restrito aos 3 finalizados)
- [x] RBAC igual ao padrão de "Meus Chamados" (Admin=todos, Atendente=responsavelId, Solicitante=solicitanteEmail)
- [x] Ajuste de UX pós-teste: filtro de período só aparece na tela Arquivo (não em "Meus Chamados"), com labels visíveis "De"/"Até"

## 🔢 Número do Chamado (CONCLUÍDA — 2026-07-19)

> Pedido pelo usuário: `Guid` interno não é referenciável em conversa/e-mail. Formato escolhido: `CAM-{número}`, sem zero-padding (`CAM-1`, `CAM-42`). Spec/design em `.specs/features/numero-do-chamado/`.

- [x] Coluna `Numero` gerada por sequence do Postgres (não pela aplicação — evita corrida em criações concorrentes)
- [x] Migration com backfill cronológico (`ORDER BY DataCriacao`) dos chamados já existentes, verificado contra o Supabase real (37 chamados, números únicos 1-37, ordem cronológica exata; chamado novo criado depois recebeu 38)
- [x] Exibido em `CAM-{número}` no `ChamadoCard` (Lista, Arquivo, Fila, Kanban) e no cabeçalho do Detalhe
- [x] Busca/filtro por número (2026-07-20) — campo de busca já existente reconhece `"42"` ou `"CAM-42"`, sem filtro novo na UI (`ChamadoRepository.ParseNumeroChamado`)

## 🔒 RBAC real do Dashboard/Kanban/Fila (CONCLUÍDA — 2026-07-20)

> As 3 telas só escondiam o link da sidebar pro Solicitante, sem bloquear a rota de verdade. Aplicado o mesmo padrão já usado em Relatório Mensal/Admin > Usuários.

- [x] Bloqueio real (Alert + "Voltar") nas 3 telas pro Solicitante — sem guard novo no backend (mesma decisão do Relatório Mensal, dado não é mais sensível entre Admin/Atendente)

## 🔐 Auth por E-mail e Senha (CONCLUÍDA — 2026-07-27)

> Substitui o login Google OAuth (fora do plano da CAMARJ). Spec/design/tasks completos em `.specs/features/auth-email-senha/`. Decisão tomada em 2026-07-24: Google OAuth não vai poder ser usado em produção. Login por email+senha implementado.

- [x] AUTH-01: Coluna `SenhaHash` + migration `AddSenhaHashUsuarioPerfil`
- [x] AUTH-02: `IJwtTokenService` extraído e compartilhado
- [x] AUTH-03: `POST /auth/login` (email+senha) — backend
- [x] AUTH-04: Cadastro de usuário exige senha inicial (mín 8 caracteres)
- [x] AUTH-05: `PATCH /usuarios/{id}/senha` (Admin redefine)
- [x] AUTH-06: Testes unitários atualizados — 218 testes passando
- [x] AUTH-07: Frontend: tela de login (email+senha) — substitui GoogleLogin
- [x] AUTH-08: Frontend: campo de senha no cadastro de usuário
- [x] AUTH-09: Frontend: botão "Redefinir senha" no Admin

## 👥 Grupos/Equipes para Chamados (CONCLUÍDO — 2026-07-28)

> Usuários pertencem a um grupo (ex: Reembolso, Credenciado, Comercial, Contas Médicas, Autorização/Auditoria). Atendentes do mesmo grupo podem ver e interagir nos chamados dos colegas — cobre férias, ausências e trabalho em equipe.

- [x] Tabela `Grupo` + relação `UsuarioPerfil.GrupoId`
- [x] Migration `AddGrupo` com seed de 6 grupos
- [x] Nova regra de RBAC: Atendente vê chamados do seu grupo (não só os próprios)
- [x] Admin gerencia Grupos (CRUD) — tela `/admin/grupos`
- [x] `grupo_id` claim no JWT via `ICurrentUserService`
- [x] Frontend: GruposPage, GrupoFormDialog, dropdown no UsuarioFormDialog, coluna na UsuariosPage
- [x] 218 testes passando, build limpo

## 📂 Novas Categorias (CONCLUÍDO — 2026-07-28)

- [x] Renomear "Autorização" → "Autorização/Auditoria"
- [x] Adicionar: Credenciado
- [x] Adicionar: Comercial
- [x] Adicionar: Contas Médicas
- [x] Total: 8 categorias (eram 5)
- [x] Seeder com upsert inteligente (funciona em banco novo e existente)

## 🎨 Tema Claro + Logo CAMARJ + Olhinho (CONCLUÍDO — 2026-07-28)

- [x] Toggle claro/escuro na LoginPage, ResetarSenhaPage e AppLayout
- [x] Respeitar preferência do sistema (`prefers-color-scheme`) + persistir em localStorage
- [x] Logo CAMARJ no topo da sidebar
- [x] Toggle mostrar/ocultar senha (ícone de olho 👁) em todos os campos de senha
- [x] Tema claro: branco + verde institucional CAMARJ (teal-600)
- [x] ThemeProvider + useTheme hook
