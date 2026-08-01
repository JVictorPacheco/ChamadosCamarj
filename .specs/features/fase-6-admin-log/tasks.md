# Tasks — F5a: Login mockado por e-mail + Cadastro de Usuários (Admin)

> Ver `design.md` para arquitetura completa. Criado em 2026-07-15.
> Numeração `T09a`-`T09e` para não colidir com T01-T15 já existentes em `spec.md` (T09 real e T15 seguem pendentes, agora dependendo destas).
> **Status: T09a-T09e TODAS CONCLUÍDAS em 2026-07-16.** F5a implementada de ponta a ponta contra o Supabase real, revisada por um review sênior (4 bugs Altos corrigidos, 15 itens Médio/Baixo documentados em `.specs/codebase/CONCERNS.md`), e commitada. Detalhes de execução no `STATE.md`.

## ✅ Pendência de RBAC resolvida (2026-07-16)

`UsuariosPage` (T09e) recebeu bloqueio real de conteúdo (mesmo padrão do Relatório Mensal): quem não é Admin e acessa `/admin/usuarios` direto vê um alerta de bloqueio + botão "Voltar", não a tela renderizada. Antes disso era só RBAC soft (link escondido na sidebar). Decisão confirmada com o usuário.

---

### T09a — Domain + Infrastructure: `UsuarioPerfil` ✅ Concluída (2026-07-16)

**O quê:** Enum `Perfil` (Admin/Atendente/Solicitante), entidade `UsuarioPerfil : BaseEntity`, `IUsuarioPerfilRepository` + implementação, `UsuarioPerfilConfiguration`, migration `AddUsuarioPerfil`, seed de Victor (Admin) e Fábio (Atendente) no `DatabaseSeeder`.

**Onde:** `Domain/Enums/Perfil.cs`, `Domain/Entities/UsuarioPerfil.cs`, `Domain/Interfaces/IUsuarioPerfilRepository.cs`, `Infrastructure/Repositories/UsuarioPerfilRepository.cs`, `Infrastructure/Data/Configurations/UsuarioPerfilConfiguration.cs`, `Infrastructure/Data/Migrations/`, `DatabaseSeeder`.

**Depende de:** nada (base da feature).

**Reaproveita:** padrão de `Categoria.cs` (entidade) e `AcaoHistorico.cs` (enum).

**Pronto quando:** migration aplica limpo em banco local (Docker), seed roda idempotente (rodar a API duas vezes não duplica Victor/Fábio), `dotnet build` limpo.

**Testes:** unit tests de domain para `UsuarioPerfil` (construtor rejeita e-mail/nome vazio, `Atualizar`/`Ativar`/`Desativar` funcionam) — seguir padrão de testes existentes em `tests/ChamadosCamarj.UnitTests` para `Categoria`.

**Gate:** `dotnet build` + `dotnet test` limpos.

---

### T09b — Application: Commands + Queries de `UsuarioPerfil` ✅ Concluída (2026-07-16)

**O quê:** `CriarUsuarioPerfilCommand`, `AtualizarUsuarioPerfilCommand` (+ Handlers + Validators), `ListarUsuariosPerfilQuery`, `ObterUsuarioPerfilPorEmailQuery` (+ Handlers), `UsuarioPerfilResponse` DTO.

**Onde:** `Application/Features/Usuarios/{Commands,Queries,DTOs}/`.

**Depende de:** T09a.

**Reaproveita:** estrutura de `Application/Features/Categorias/`.

**Pronto quando:** commands validam e-mail duplicado (409) e perfil inválido; query por e-mail retorna `null` (não exceção) quando não encontrado ou inativo.

**Testes:** unit tests de validators (e-mail vazio/duplicado rejeitado) e handlers (criar/atualizar/listar/buscar por e-mail).

**Gate:** `dotnet test` limpo.

---

### T09c — WebApi: `UsuariosController` ✅ Concluída (2026-07-16)

**O quê:** Endpoints `GET /api/usuarios`, `POST /api/usuarios`, `PUT /api/usuarios/{id}`, `GET /api/usuarios/por-email`. Checagem de perfil do requisitante (só Admin cria/edita/lista) via query param, seguindo o padrão soft já usado no resto do app.

**Onde:** `WebApi/Controllers/UsuariosController.cs`.

**Depende de:** T09b.

**Reaproveita:** padrão de `CategoriasController` (usa MediatR, sem lógica de negócio no controller).

**Pronto quando:** os 4 endpoints respondem corretamente no Scalar (`/scalar`), incluindo 403 pra não-Admin e 404 pra e-mail não encontrado/inativo.

**Testes:** verificação manual via Scalar (endpoints simples, não justificam teste de integração dedicado neste escopo).

**Gate:** `dotnet build` limpo + smoke test manual via Scalar.

---

### T09d — Frontend: `AuthContext` reescrito + `LoginPage` ✅ Concluída (2026-07-16)

**O quê:** `AuthContext.tsx` perde o dicionário fixo `PERFIS`/`ATENDENTES`; `login(email)` vira assíncrono e busca via `GET /usuarios/por-email`. `LoginPage.tsx` substitui `ProfileSelector.tsx` (campo de e-mail, tratamento de erro 404). `ATENDENTES` (usado na UI de Reatribuição) passa a vir de `GET /usuarios` filtrado.

**Onde:** `frontend/src/auth/AuthContext.tsx`, `frontend/src/auth/LoginPage.tsx` (novo, substitui `ProfileSelector.tsx`), `frontend/src/lib/api.ts` (novas funções), qualquer componente que hoje importa `ATENDENTES` de `AuthContext` (checar usos antes de mexer — provavelmente a UI de Reatribuição do Detalhe do Chamado).

**Depende de:** T09c.

**Reaproveita:** layout atual do `ProfileSelector` (logo, card), padrão de erro inline já usado em outras telas do projeto (não usar toast — projeto não tem biblioteca de toast instalada, ver aprendizado registrado no `STATE.md`).

**Pronto quando:** login com `victor@camarj.com.br` cai no perfil Admin, `fabio@camarj.com.br` cai em Atendente, e-mail desconhecido mostra erro sem quebrar a tela.

**Testes:** verificação manual (login com os 2 e-mails seedados + 1 e-mail inexistente).

**Gate:** `npm run build` limpo + verificação manual.

---

### T09e — Frontend: `UsuariosPage` (Admin) ✅ Concluída (2026-07-16)

**O quê:** Tela `/admin/usuarios` — tabela de usuários + form de criar/editar (e-mail, nome, perfil, ativo). Rota visível/acessível só para Admin.

**Onde:** `frontend/src/features/admin/UsuariosPage.tsx` (novo), rotas em `App.tsx`/router, item de menu na sidebar (Admin only).

**Depende de:** T09d (precisa do `AuthContext` já sabendo quem está logado para checar se é Admin).

**Reaproveita:** padrão de formulário existente no projeto (ex: `AbrirChamadoPage`), componentes shadcn já instalados (Table, Dialog/Sheet, Input, Select).

**Pronto quando:** Admin consegue cadastrar um novo usuário (ex: Cátia) e essa pessoa consegue logar em seguida com o e-mail cadastrado.

**Testes:** verificação manual end-to-end (Admin cadastra → novo usuário loga → perfil correto aparece na sidebar).

**Gate:** `npm run build` limpo + verificação manual (UAT simples, ver `references/validate.md` do skill se quiser roteiro formal).

> ⚠️ Durante o Execute, decidir com o usuário se `/admin/usuarios` precisa de bloqueio real de rota (não só link escondido) — está exposto um cadastro de acesso ao sistema, potencialmente mais sensível que o Relatório Mensal (que recebeu bloqueio real). Ver `design.md` → seção `UsuariosPage.tsx`.

---

## Ordem de execução

T09a → T09b → T09c → T09d → T09e (sequencial, sem tasks `[P]` paralelas — cada uma depende da anterior).

## Fora destas tasks

- T09 (renomeado implicitamente para "F5b real"), T15 — login Google Workspace de verdade, permanecem como estavam em `spec.md`, agora com dependência explícita destas T09a-T09e.
