# Spec — Fase 6: Admin Completo + Log de Histórico + Google Workspace

> Status: PLANEJADO
> Atualizado em: 2026-07-01

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

### F5 — Login Google Workspace

**Descrição:** Substituir o seletor mockado de perfil pelo login real "Sign in with Google".

**Fluxo:**
1. Usuário clica "Entrar com Google" na tela de login
2. Redirect para OAuth2 Google
3. Google retorna token
4. Backend valida token e faz lookup conta→perfil
5. Backend emite JWT próprio
6. Frontend armazena JWT e usa em todas as requisições

**Requisitos de backend:**
- Endpoint `POST /auth/google` que recebe o `id_token` do Google e retorna JWT
- Tabela `UsuarioPerfil` mapeando email → perfil
- Middleware de autenticação JWT nas rotas protegidas

**Requisitos de frontend:**
- Substituir `AuthContext.tsx` e `ProfileSelector.tsx` pelo fluxo OAuth
- `@react-oauth/google` ou `@google-cloud/local-auth`

---

## Tarefas (a detalhar antes do Execute)

### Backend
- [ ] T01 — Criar entidade `HistoricoEntrada` + enum `AcaoHistorico` + `IHistoricoRepository`
- [ ] T02 — Adicionar método `Chamado.Reatribuir()` na domain entity
- [ ] T03 — Criar `ReatribuirChamadoCommand` + Handler + Validator
- [ ] T04 — Criar endpoint `PATCH /chamados/{id}/reatribuir`
- [ ] T05 — Integrar geração de `HistoricoEntrada` nos CommandHandlers existentes
- [ ] T06 — Criar `ListarHistoricoQuery` + endpoint `GET /chamados/{id}/historico`
- [ ] T07 — Criar endpoint `PATCH /chamados/{id}/prioridade`
- [ ] T08 — Filtrar comentários internos por perfil em `ListarComentariosQuery`
- [ ] T09 — Implementar autenticação Google Workspace (JWT)

### Frontend
- [ ] T10 — Adicionar `reatribuirChamado()` em `api.ts` + `useReatribuirChamado()` hook
- [ ] T11 — UI de Reatribuição no Detalhe (botão + select de atendentes)
- [ ] T12 — Seção "Histórico" no Detalhe (timeline com ícones)
- [ ] T13 — Alterar Prioridade no Detalhe (Admin)
- [ ] T14 — Toggle "Comentário Interno" no formulário de comentário
- [ ] T15 — Substituir ProfileSelector pelo fluxo Google OAuth

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
- Login com conta @camarj.com.br funciona via Google — seletor mockado removido
