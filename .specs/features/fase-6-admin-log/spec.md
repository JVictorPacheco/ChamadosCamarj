# Spec — Fase 6: Admin Completo + Log de Histórico + Google Workspace

> Status: F1-F4 CONCLUÍDAS. F5a IMPLEMENTADA (login mockado por e-mail). F5b IMPLEMENTADA (código completo, 2026-07-18) — falta só o Client ID real da TI pro teste de ponta a ponta. Ver `design-t09-google-oauth.md` + `tasks-t09-google-oauth.md`.
> Atualizado em: 2026-07-15

---

## Objetivo

Dar ao Admin controle total sobre o fluxo dos chamados, registrar auditoria completa de cada evento, e substituir a autenticação mockada pelo login real via Google Workspace.

---

## Features

### F1 — Reatribuição pelo Admin

**Descrição:** Admin pode mover um chamado de um atendente para outro, mesmo que o chamado já esteja `EmAndamento`.

**Regras de negócio:**
- Só Admin pode reatribuir
- Reatribuição permitida em qualquer status exceto `Fechado` e `Cancelado`
- Ao reatribuir, o `ResponsavelId` e `ResponsavelNome` são atualizados
- Se o chamado estava `Aberto`, passa para `EmAndamento`
- Deve gerar entrada no histórico: `AcaoHistorico.Reatribuido` com `DetalheAnterior` = responsável anterior e `DetalheNovo` = novo responsável

**Endpoint novo:** `PATCH /api/chamados/{id}/reatribuir`

**Body:**
```json
{
  "novoResponsavelId": "guid",
  "novoResponsavelNome": "string"
}
```

**UI:** Botão "Reatribuir" visível no Detalhe do Chamado apenas para Admin, em qualquer status não-final. Abre select com lista de atendentes disponíveis.

---

### F2 — Log de Histórico

**Descrição:** Cada ação relevante no ciclo de vida de um chamado gera uma `HistoricoEntrada`, visível na tela de detalhe do chamado.

**Ações que geram histórico:**
| Ação | Command/Evento |
|------|----------------|
| Chamado criado | `AbrirChamadoCommandHandler` |
| Assumido | `AtribuirChamadoCommandHandler` |
| Reatribuído | `ReatribuirChamadoCommandHandler` |
| Resolvido | `ResolverChamadoCommandHandler` |
| Fechado | `FecharChamadoCommandHandler` |
| Cancelado | `CancelarChamadoCommandHandler` |
| Comentário adicionado | `ComentarChamadoCommandHandler` |
| Prioridade alterada | `AlterarPrioridadeCommandHandler` *(F3)* |

**Entidade `HistoricoEntrada`:**
```csharp
public class HistoricoEntrada : BaseEntity
{
    public Guid ChamadoId { get; private set; }
    public string UsuarioNome { get; private set; }
    public Guid? UsuarioId { get; private set; }
    public AcaoHistorico Acao { get; private set; }
    public string? DetalheAnterior { get; private set; }
    public string? DetalheNovo { get; private set; }
    public DateTime DataHora { get; private set; }
}
```

**Query:** `GET /api/chamados/{id}/historico` → retorna lista ordenada por `DataHora` descrescente.

**UI:** Seção "Histórico" no Detalhe do Chamado, timeline vertical com ícones por tipo de ação. Filtragem de visibilidade por perfil (ver `📋 Histórico de Chamados`).

---

### F3 — Alterar Prioridade

**Descrição:** Admin pode alterar a prioridade de qualquer chamado.

**Regras:**
- Só Admin
- Qualquer status exceto `Fechado` / `Cancelado`
- SLA não é recalculado automaticamente (só no momento da criação)
- Gera entrada no histórico

**Endpoint:** `PATCH /api/chamados/{id}/prioridade` com body `{ "novaPrioridade": "Alta" }`

---

### F4 — Comentários Internos

**Descrição:** Admin e Atendente podem criar comentários visíveis apenas para si mesmos (não para o Solicitante).

**Regras:**
- Campo `Tipo` já existe na entidade `Comentario` (enum `Interno` / `Publico`)
- Backend já suporta — falta apenas filtrar na query `ListarComentarios` por perfil
- Frontend: checkbox ou toggle "Comentário interno" no formulário de comentário

**Endpoint:** O endpoint `POST /chamados/{id}/comentarios` já recebe `tipo` — apenas garantir que funciona corretamente.

---

### F5a — Login mockado por e-mail + Cadastro de Usuários (Admin) [NOVO — decidido em 2026-07-15]

**Descrição:** Passo intermediário antes do login real (F5b). Em vez do seletor de perfil (dropdown Admin/Atendente/Solicitante), a pessoa entra digitando o e-mail corporativo dela (ex: `fabio@camarj.com.br`) e o sistema busca o perfil correspondente numa tabela de usuários. Sem verificação de senha — ainda é mock. O Admin ganha uma tela para cadastrar/editar esses usuários (e-mail + nome + perfil).

**Por que isso não é descartável:** a tabela `UsuarioPerfil` (e-mail → perfil) é exatamente o mapeamento conta→perfil que o F5b (login real) precisa. O F5b só troca a *fonte* do e-mail (do campo digitado para o `id_token` do Google) — a tabela e as telas de cadastro continuam as mesmas.

**Regras de negócio:**
- Só usuário com perfil Admin pode criar/editar/desativar um `UsuarioPerfil` (checagem feita hoje da mesma forma que o resto do app pré-auth real: perfil do requisitante vem do cliente, não de um token — ver Aprendizados do `STATE.md`)
- `Email` é único (case-insensitive) e obrigatório
- `Perfil` é um dos três valores: `Admin`, `Atendente`, `Solicitante`
- Usuário desativado (`Ativo = false`) não consegue mais "logar" (buscar perfil por e-mail retorna erro/404 tratado como "não encontrado")
- Login por e-mail não cadastrado: erro claro ("e-mail não cadastrado — peça para um Admin te cadastrar"), sem criar usuário implicitamente

**Entidade `UsuarioPerfil`:**
```csharp
public class UsuarioPerfil : BaseEntity
{
    public string Email { get; private set; }
    public string Nome { get; private set; }
    public Perfil Perfil { get; private set; } // enum: Admin | Atendente | Solicitante
    public bool Ativo { get; private set; }
}
```

**Endpoints novos:**
- `GET /api/usuarios?perfilRequisitante=Admin` — lista todos (Admin only)
- `POST /api/usuarios` — cria (Admin only), body `{ email, nome, perfil }`
- `PUT /api/usuarios/{id}` — atualiza nome/perfil/ativo (Admin only)
- `GET /api/usuarios/por-email?email=fabio@camarj.com.br` — usado pelo login mockado, retorna o `UsuarioPerfil` ativo ou 404

**UI:**
- Nova tela `Admin > Usuários` (rota protegida, só visível/acessível pra Admin): tabela de usuários + formulário criar/editar (e-mail, nome, perfil, ativo)
- `LoginPage` substitui `ProfileSelector`: campo de e-mail, botão "Entrar" → `GET /usuarios/por-email` → em caso de sucesso, salva o perfil retornado (mesmo mecanismo de `localStorage` de hoje); em caso de 404, mostra a mensagem de erro
- Seed inicial: Victor (Admin) e Fábio (Atendente) — os dois usuários mockados que já existem hoje no `AuthContext.tsx`, agora persistidos no banco em vez de hardcoded no frontend

**Fora de escopo do F5a:** senha (não existe, é só e-mail), qualquer verificação de identidade real, RBAC real de rota no backend (endpoints de `/usuarios` seguem o mesmo padrão soft de autorização do resto do app pré-T09/F5b)

---

### F5b — Login Google Workspace (real)

**Descrição:** Substituir o login mockado por e-mail (F5a) pelo login real "Sign in with Google". Depende de F5a estar pronta — a tabela `UsuarioPerfil` já existe e é reaproveitada sem mudanças de schema.

**Fluxo:**
1. Usuário clica "Entrar com Google" na tela de login
2. Redirect para OAuth2 Google
3. Google retorna token
4. Backend valida token e faz lookup do e-mail retornado na tabela `UsuarioPerfil` (mesma tabela do F5a — `GET /usuarios/por-email` internamente)
5. Backend emite JWT próprio
6. Frontend armazena JWT e usa em todas as requisições

**Requisitos de backend:**
- Endpoint `POST /auth/google` que recebe o `id_token` do Google, valida contra o Google, faz lookup em `UsuarioPerfil` e retorna JWT
- Middleware de autenticação JWT nas rotas protegidas
- Trocar `UsuarioId`/`UsuarioNome` client-supplied (nos commands de Reatribuir/AlterarPrioridade/Resolver/Fechar/Cancelar) por extração via claims do JWT

**Requisitos de frontend:**
- Substituir `LoginPage` (F5a) pelo fluxo OAuth — a lógica de "buscar perfil por e-mail" já existe e é reaproveitada, só muda de onde o e-mail vem
- `@react-oauth/google` ou equivalente

**Requisitos de infraestrutura (fora do código, ver documento à parte pra TI):**
- Projeto no Google Cloud Console com OAuth Client ID configurado pro domínio `camarj.com.br`
- Aprovação/registro do app no Google Workspace admin console da CAMARJ
- Redirect URIs de produção e desenvolvimento configurados

---

## Tarefas (a detalhar antes do Execute)

### Backend
- [x] T01 — Criar entidade `HistoricoEntrada` + enum `AcaoHistorico` + `IHistoricoRepository`
- [x] T02 — Adicionar método `Chamado.Reatribuir()` na domain entity
- [x] T03 — Criar `ReatribuirChamadoCommand` + Handler + Validator
- [x] T04 — Criar endpoint `PATCH /chamados/{id}/reatribuir`
- [x] T05 — Integrar geração de `HistoricoEntrada` nos CommandHandlers existentes
- [x] T06 — Criar `ListarHistoricoQuery` + endpoint `GET /chamados/{id}/historico`
- [x] T07 — Criar endpoint `PATCH /chamados/{id}/prioridade`
- [x] T08 — Filtrar comentários internos por perfil em `ListarComentariosQuery`
- [ ] T09 — Implementar autenticação Google Workspace real (JWT) — depende de F5a. Ver `tasks.md` para T09a-T09e (F5a, planejadas)

### Frontend
- [x] T10 — Adicionar `reatribuirChamado()` em `api.ts` + `useReatribuirChamado()` hook
- [x] T11 — UI de Reatribuição no Detalhe (botão + select de atendentes)
- [x] T12 — Seção "Histórico" no Detalhe (timeline com ícones)
- [x] T13 — Alterar Prioridade no Detalhe (Admin)
- [x] T14 — Toggle "Comentário Interno" no formulário de comentário
- [ ] T15 — Substituir `LoginPage` (F5a) pelo fluxo Google OAuth real — depende de T09

> Detalhamento atômico de F5a (T09a-T09e) em `tasks.md`, criado em 2026-07-15 por ser multi-componente (entidade + migration + CRUD + 2 telas).

---

## Dependências

- Fase 5 ✅ (concluída — pré-requisito)
- Google Cloud Console: configurar OAuth2 client_id para o domínio camarj.com.br (Fase 6 / T09)

---

## Critérios de aceite

- Admin consegue reatribuir um chamado `EmAndamento` de Fábio para outro atendente
- Cada ação no chamado aparece na timeline de histórico com data/hora e usuário
- Comentário interno não aparece para Solicitante
- Admin consegue alterar prioridade de qualquer chamado não-final
- **F5a:** Admin consegue cadastrar um usuário (e-mail + nome + perfil) na tela `Admin > Usuários`; a pessoa cadastrada consegue "logar" digitando o e-mail na `LoginPage` e cai no perfil correto; e-mail não cadastrado mostra erro claro
- **F5b:** Login com conta @camarj.com.br funciona via Google — `LoginPage` (F5a) substituída pelo fluxo OAuth real
