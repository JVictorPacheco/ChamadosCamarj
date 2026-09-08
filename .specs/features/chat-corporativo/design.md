# Chat Corporativo — Design Técnico

> **Criado em:** 2026-08-29
> **Feature obriga design.md porque:** toca todas as camadas (Domain, Application, Infrastructure, WebApi, Frontend), introduz 6 novas entidades, novo SignalR Hub e novo bucket de storage.

---

## 1. Visão Geral da Solução

Chat em tempo real via SignalR (já presente no projeto), com controle de acesso por `ChatPerfil` no `UsuarioPerfil`, presença via heartbeat periódico do frontend, e armazenamento de mensagens/arquivos em PostgreSQL + Supabase Storage (bucket separado `chat-arquivos`). Toda ação relevante gera registro em `ChatHistorico` para auditoria completa.

---

## 2. Mudanças no Domain

### 2.1 Novos Enums

```csharp
// Domain/Enums/ChatPerfil.cs
public enum ChatPerfil
{
    SemAcesso = 0,
    Participante = 1,
    CriadorDeGrupo = 2
}

// Domain/Enums/ChatConversaTipo.cs
public enum ChatConversaTipo { Privada, Grupo }

// Domain/Enums/ChatMensagemTipo.cs
public enum ChatMensagemTipo { Texto, Arquivo, Sistema }

// Domain/Enums/StatusPresenca.cs
public enum StatusPresenca { Online, Ausente, Offline }

// Domain/Enums/ChatAcao.cs
public enum ChatAcao
{
    AcessoConcedido,
    AcessoRevogado,
    MensagemEnviada,
    MensagemEditada,
    MensagemDeletada,
    ArquivoEnviado,
    GrupoCriado,
    GrupoDeletado,
    ParticipanteAdicionado,
    ParticipanteRemovido,
    ReacaoAdicionada,
    ReacaoRemovida
}
```

### 2.2 Alteração em Entidade Existente

```csharp
// Domain/Entities/UsuarioPerfil.cs — adicionar propriedade
public ChatPerfil ChatPerfil { get; private set; } = ChatPerfil.SemAcesso;

public void DefinirChatPerfil(ChatPerfil perfil)
{
    ChatPerfil = perfil;
    DataAtualizacao = DateTime.UtcNow;
}
```

> ⚠️ **Mudança de contrato:** `UsuarioPerfil` ganha nova propriedade. Confirmar que nenhum handler existente quebra com a migration `AddChatPerfilUsuario`.

### 2.3 Novas Entidades

```csharp
// Domain/Entities/ChatConversa.cs
public class ChatConversa : BaseEntity
{
    private ChatConversa() { }

    public ChatConversaTipo Tipo { get; private set; }
    public string? Nome { get; private set; }           // só para grupos
    public Guid CriadoPorId { get; private set; }
    public bool Ativa { get; private set; } = true;

    // Navegação EF
    public ICollection<ChatParticipante> Participantes { get; private set; } = [];
    public ICollection<ChatMensagem> Mensagens { get; private set; } = [];

    public static ChatConversa CriarPrivada(Guid criadoPorId) => new() { Tipo = ChatConversaTipo.Privada, CriadoPorId = criadoPorId };
    public static ChatConversa CriarGrupo(string nome, Guid criadoPorId) => new() { Tipo = ChatConversaTipo.Grupo, Nome = nome, CriadoPorId = criadoPorId };
}

// Domain/Entities/ChatParticipante.cs
public class ChatParticipante : BaseEntity
{
    private ChatParticipante() { }

    public Guid ConversaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public DateTime? UltimaLeituraEm { get; private set; }
    public bool Ativo { get; private set; } = true;

    // Navegação EF
    public ChatConversa Conversa { get; private set; } = null!;

    public void MarcarComoLido() { UltimaLeituraEm = DateTime.UtcNow; DataAtualizacao = DateTime.UtcNow; }
    public void Desativar() { Ativo = false; DataAtualizacao = DateTime.UtcNow; }
}

// Domain/Entities/ChatMensagem.cs
public class ChatMensagem : BaseEntity
{
    private ChatMensagem() { }

    public Guid ConversaId { get; private set; }
    public Guid AutorId { get; private set; }
    public string AutorNome { get; private set; } = string.Empty;
    public string? Conteudo { get; private set; }
    public string? ConteudoOriginal { get; private set; }   // preservado ao editar/deletar
    public ChatMensagemTipo Tipo { get; private set; }
    public bool Deletada { get; private set; }
    public DateTime? EditadaEm { get; private set; }
    public Guid? RespostaParaMensagemId { get; private set; } // reply/citação

    // Arquivo (quando Tipo = Arquivo)
    public string? NomeArquivo { get; private set; }
    public string? CaminhoStorage { get; private set; }
    public string? TipoArquivo { get; private set; }
    public long? TamanhoBytes { get; private set; }

    // Navegação EF
    public ChatConversa Conversa { get; private set; } = null!;
    public ICollection<ChatMensagemReacao> Reacoes { get; private set; } = [];

    public void Editar(string novoConteudo)
    {
        ConteudoOriginal ??= Conteudo;
        Conteudo = novoConteudo;
        EditadaEm = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Deletar()
    {
        ConteudoOriginal ??= Conteudo;
        Conteudo = null;
        Deletada = true;
        DataAtualizacao = DateTime.UtcNow;
    }
}

// Domain/Entities/ChatMensagemReacao.cs
public class ChatMensagemReacao : BaseEntity
{
    private ChatMensagemReacao() { }

    public Guid MensagemId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public string Emoji { get; private set; } = string.Empty;

    // Navegação EF
    public ChatMensagem Mensagem { get; private set; } = null!;
}

// Domain/Entities/ChatPresenca.cs
public class ChatPresenca : BaseEntity
{
    private ChatPresenca() { }

    public Guid UsuarioId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public StatusPresenca Status { get; private set; } = StatusPresenca.Offline;
    public DateTime UltimoHeartbeat { get; private set; }

    public void AtualizarHeartbeat()
    {
        Status = StatusPresenca.Online;
        UltimoHeartbeat = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void MarcarAusente() { Status = StatusPresenca.Ausente; DataAtualizacao = DateTime.UtcNow; }
    public void MarcarOffline() { Status = StatusPresenca.Offline; DataAtualizacao = DateTime.UtcNow; }
}

// Domain/Entities/ChatHistorico.cs
public class ChatHistorico : BaseEntity
{
    private ChatHistorico() { }

    public Guid UsuarioId { get; private set; }
    public string UsuarioNome { get; private set; } = string.Empty;
    public ChatAcao Acao { get; private set; }
    public string? Detalhe { get; private set; }       // JSON com contexto da ação
    public Guid? ConversaId { get; private set; }
    public Guid? MensagemId { get; private set; }

    public static ChatHistorico Criar(Guid usuarioId, string usuarioNome, ChatAcao acao, string? detalhe = null, Guid? conversaId = null, Guid? mensagemId = null)
        => new() { UsuarioId = usuarioId, UsuarioNome = usuarioNome, Acao = acao, Detalhe = detalhe, ConversaId = conversaId, MensagemId = mensagemId };
}
```

### 2.4 Novas Interfaces

```csharp
// Domain/Interfaces/IChatConversaRepository.cs
public interface IChatConversaRepository
{
    Task<ChatConversa?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<ChatConversa>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct);
    Task<ChatConversa?> ObterPrivadaEntreUsuariosAsync(Guid usuarioAId, Guid usuarioBId, CancellationToken ct);
    Task AdicionarAsync(ChatConversa conversa, CancellationToken ct);
    Task AtualizarAsync(ChatConversa conversa, CancellationToken ct);
}

// Domain/Interfaces/IChatMensagemRepository.cs
public interface IChatMensagemRepository
{
    Task<ChatMensagem?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<ChatMensagem>> ListarPorConversaAsync(Guid conversaId, int pagina, int tamanhoPagina, CancellationToken ct);
    Task AdicionarAsync(ChatMensagem mensagem, CancellationToken ct);
    Task AtualizarAsync(ChatMensagem mensagem, CancellationToken ct);
}

// Domain/Interfaces/IChatPresencaRepository.cs
public interface IChatPresencaRepository
{
    Task<ChatPresenca?> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken ct);
    Task<IEnumerable<ChatPresenca>> ListarTodasAsync(CancellationToken ct);
    Task AdicionarOuAtualizarAsync(ChatPresenca presenca, CancellationToken ct);
    Task<IEnumerable<ChatPresenca>> ListarAusentesParaMarcarOfflineAsync(DateTime limite, CancellationToken ct);
}

// Domain/Interfaces/IChatHistoricoRepository.cs
public interface IChatHistoricoRepository
{
    Task AdicionarAsync(ChatHistorico historico, CancellationToken ct);
    Task<IEnumerable<ChatHistorico>> ListarPorConversaAsync(Guid conversaId, CancellationToken ct);
}
```

---

## 3. Mudanças no Application (CQRS)

### 3.1 Commands

```
Application/Features/Chat/Commands/
├── DefinirChatPerfil/
│   ├── DefinirChatPerfilCommand.cs        → (UsuarioId, ChatPerfil)
│   └── DefinirChatPerfilCommandHandler.cs → atualiza UsuarioPerfil + notifica via SignalR + loga
├── EnviarMensagem/
│   ├── EnviarMensagemCommand.cs           → (ConversaId, Conteudo, RespostaParaMensagemId?)
│   └── EnviarMensagemCommandHandler.cs    → persiste + broadcast SignalR + loga
├── EnviarArquivo/
│   ├── EnviarArquivoCommand.cs            → (ConversaId, Arquivo IFormFile)
│   └── EnviarArquivoCommandHandler.cs     → upload Storage + persiste + broadcast + loga
├── EditarMensagem/
│   ├── EditarMensagemCommand.cs           → (MensagemId, NovoConteudo)
│   └── EditarMensagemCommandHandler.cs    → valida autoria + edita + broadcast + loga
├── DeletarMensagem/
│   ├── DeletarMensagemCommand.cs          → (MensagemId)
│   └── DeletarMensagemCommandHandler.cs   → valida autoria ou Admin + deleta + broadcast + loga
├── AdicionarReacao/
│   ├── AdicionarReacaoCommand.cs          → (MensagemId, Emoji)
│   └── AdicionarReacaoCommandHandler.cs   → toggle reação + broadcast + loga
├── MarcarComoLido/
│   ├── MarcarComoLidoCommand.cs           → (ConversaId)
│   └── MarcarComoLidoCommandHandler.cs    → atualiza UltimaLeituraEm + notifica leitura
├── AtualizarPresenca/
│   ├── AtualizarPresencaCommand.cs        → (Status? — null = heartbeat)
│   └── AtualizarPresencaCommandHandler.cs → upsert ChatPresenca + broadcast status
├── CriarConversa/
│   ├── CriarConversaCommand.cs            → (DestinatarioId)
│   └── CriarConversaCommandHandler.cs     → cria ou retorna existente + loga
└── CriarGrupo/
    ├── CriarGrupoCommand.cs               → (Nome, ParticipanteIds[])
    └── CriarGrupoCommandHandler.cs        → valida CriadorDeGrupo + cria + notifica + loga
```

### 3.2 Queries

```
Application/Features/Chat/Queries/
├── ListarConversas/
│   ├── ListarConversasQuery.cs            → ()
│   └── ListarConversasQueryHandler.cs     → retorna conversas do usuário com última mensagem
├── ListarMensagens/
│   ├── ListarMensagensQuery.cs            → (ConversaId, Pagina)
│   └── ListarMensagensQueryHandler.cs     → paginado, mais recentes primeiro
├── ListarPresencas/
│   ├── ListarPresencasQuery.cs            → ()
│   └── ListarPresencasQueryHandler.cs     → todos os usuários com status atual
└── ListarHistoricoChat/
    ├── ListarHistoricoChatQuery.cs        → (ConversaId?)
    └── ListarHistoricoChatQueryHandler.cs → Admin only
```

### 3.3 DTOs

```csharp
public record ChatConversaResponse(Guid Id, ChatConversaTipo Tipo, string? Nome, string? UltimaMensagem, DateTime? UltimaMensagemEm, int NaoLidas);
public record ChatMensagemResponse(Guid Id, Guid AutorId, string AutorNome, string? Conteudo, ChatMensagemTipo Tipo, bool Deletada, DateTime? EditadaEm, Guid? RespostaParaMensagemId, string? RespostaConteudo, IEnumerable<ChatReacaoResponse> Reacoes, DateTime DataCriacao);
public record ChatReacaoResponse(string Emoji, int Quantidade, bool ReagiuEu);
public record ChatPresencaResponse(Guid UsuarioId, string UsuarioNome, StatusPresenca Status);
public record ChatArquivoResponse(string NomeArquivo, string UrlAssinada, string TipoArquivo, long TamanhoBytes);
```

---

## 4. Mudanças no Infrastructure

### 4.1 Migrations

| Migration | Tipo | Reversível | Impacto em dados existentes |
|-----------|------|-----------|----------------------------|
| `AddChatPerfilUsuario` | AddColumn | Sim | `ChatPerfil = SemAcesso (0)` para todos os usuários existentes |
| `AddChatFeature` | CreateTable (6 tabelas) | Sim | Nenhum |

### 4.2 Repositórios

Implementar em `Infrastructure/Repositories/Chat/`:
- `ChatConversaRepository.cs`
- `ChatMensagemRepository.cs`
- `ChatPresencaRepository.cs`
- `ChatHistoricoRepository.cs`

### 4.3 Background Service — Presença

```csharp
// Infrastructure/Services/ChatPresencaWorker.cs
// IHostedService que roda a cada 60 segundos:
// - Marca como Ausente: UltimoHeartbeat < agora - 5min
// - Marca como Offline: UltimoHeartbeat < agora - 15min
// - Broadcast via ChatHub para todos os clientes
```

### 4.4 Storage

- Bucket: `chat-arquivos` (criar no Supabase antes da implementação)
- Reusar `IStorageService` e `SupabaseStorageService` já existentes
- Path dos arquivos: `chat/{conversaId}/{mensagemId}/{nomeArquivo}`
- URL assinada: 1 hora de validade (mesmo padrão dos chamados)

---

## 5. Mudanças no WebApi

### 5.1 Novo SignalR Hub

```csharp
// WebApi/Hubs/ChatHub.cs
// Métodos client-side que o servidor chama:
// - NovaMensagem(ChatMensagemResponse)
// - MensagemEditada(ChatMensagemResponse)
// - MensagemDeletada(Guid mensagemId)
// - ReacaoAtualizada(Guid mensagemId, IEnumerable<ChatReacaoResponse>)
// - PresencaAtualizada(ChatPresencaResponse)
// - AcessoRevogado()
// - DigitandoIniciou(Guid conversaId, string usuarioNome)
// - DigitandoParou(Guid conversaId)
// - MensagemLida(Guid conversaId, Guid usuarioId, DateTime leituraEm)
// - NovaConversa(ChatConversaResponse)
// - ParticipanteAdicionado(Guid conversaId, ChatParticipanteResponse)
// - ParticipanteRemovido(Guid conversaId, Guid usuarioId)

// Grupos SignalR por conversa: "chat-{conversaId}"
// Grupo de presença global: "presenca-global"
```

### 5.2 Novos Endpoints

| Método | Rota | Auth | ChatPerfil mínimo | Body | Resposta |
|--------|------|------|------------------|------|----------|
| `GET` | `/api/chat/conversas` | JWT | Participante | — | `200 IEnumerable<ChatConversaResponse>` |
| `POST` | `/api/chat/conversas` | JWT | Participante | `{ destinatarioId }` | `201 ChatConversaResponse` |
| `POST` | `/api/chat/grupos` | JWT | CriadorDeGrupo | `{ nome, participanteIds[] }` | `201 ChatConversaResponse` |
| `GET` | `/api/chat/conversas/{id}/mensagens` | JWT | Participante | — | `200 PagedResult<ChatMensagemResponse>` |
| `POST` | `/api/chat/conversas/{id}/mensagens` | JWT | Participante | `{ conteudo, respostaParaMensagemId? }` | `201 ChatMensagemResponse` |
| `POST` | `/api/chat/conversas/{id}/arquivos` | JWT | Participante | `multipart/form-data` | `201 ChatMensagemResponse` |
| `PATCH` | `/api/chat/mensagens/{id}` | JWT | Participante | `{ conteudo }` | `204` |
| `DELETE` | `/api/chat/mensagens/{id}` | JWT | Participante | — | `204` |
| `POST` | `/api/chat/mensagens/{id}/reacoes` | JWT | Participante | `{ emoji }` | `204` |
| `POST` | `/api/chat/conversas/{id}/leitura` | JWT | Participante | — | `204` |
| `GET` | `/api/chat/presencas` | JWT | SemAcesso OK | — | `200 IEnumerable<ChatPresencaResponse>` |
| `POST` | `/api/chat/presenca/heartbeat` | JWT | SemAcesso OK | — | `204` |
| `PATCH` | `/api/usuarios/{id}/chat-perfil` | JWT | Admin only | `{ chatPerfil }` | `204` |
| `GET` | `/api/chat/historico` | JWT | Admin only | `?conversaId=` | `200 IEnumerable<ChatHistoricoResponse>` |

> **Nota:** `/api/chat/presencas` e `/api/chat/presenca/heartbeat` são acessíveis a todos os perfis autenticados (inclusive `SemAcesso`) porque presença é visível a todos.

---

## 6. Mudanças no Frontend

### 6.1 Estrutura

```
frontend/src/features/chat/
├── api.ts                        → funções HTTP para todos os endpoints
├── ChatPage.tsx                  → página principal (layout split: lista + conversa)
├── hooks/
│   ├── useConversas.ts           → TanStack Query: lista conversas
│   ├── useMensagens.ts           → TanStack Query: mensagens paginadas
│   ├── usePresencas.ts           → TanStack Query: lista presença
│   ├── useChat.ts                → mutations: enviar, editar, deletar, reagir, marcar lido
│   └── useChatSignalR.ts         → SignalR: subscribe a eventos do ChatHub
├── components/
│   ├── ConversaList.tsx          → lista lateral de conversas com badge de não lidas
│   ├── ConversaItem.tsx          → item da lista com última mensagem e status
│   ├── MensagemList.tsx          → scroll de mensagens com paginação (load more)
│   ├── MensagemItem.tsx          → bolha de mensagem: texto/arquivo/sistema + reações + ações
│   ├── MensagemInput.tsx         → input com emoji picker, upload, reply preview, send
│   ├── TypingIndicator.tsx       → "[Nome] está digitando..."
│   ├── PresencaBadge.tsx         → bolinha colorida de status (verde/amarelo/cinza)
│   ├── PresencaPanel.tsx         → painel lateral com todos os usuários e status
│   ├── CriarGrupoDialog.tsx      → modal de criação de grupo
│   └── ChatPerfilSelect.tsx      → select na tela de Admin > Usuários
```

### 6.2 Novos Tipos

```ts
// types/api.ts
export type ChatPerfil = 'SemAcesso' | 'Participante' | 'CriadorDeGrupo'
export type ChatConversaTipo = 'Privada' | 'Grupo'
export type ChatMensagemTipo = 'Texto' | 'Arquivo' | 'Sistema'
export type StatusPresenca = 'Online' | 'Ausente' | 'Offline'

export interface ChatConversaResponse { id: string; tipo: ChatConversaTipo; nome?: string; ultimaMensagem?: string; ultimaMensagemEm?: string; naoLidas: number }
export interface ChatMensagemResponse { id: string; autorId: string; autorNome: string; conteudo?: string; tipo: ChatMensagemTipo; deletada: boolean; editadaEm?: string; respostaParaMensagemId?: string; respostaConteudo?: string; reacoes: ChatReacaoResponse[]; dataCriacao: string }
export interface ChatReacaoResponse { emoji: string; quantidade: number; reagiuEu: boolean }
export interface ChatPresencaResponse { usuarioId: string; usuarioNome: string; status: StatusPresenca }
```

### 6.3 Heartbeat

```ts
// hooks/useChatSignalR.ts
// useEffect: setInterval de 30s → POST /api/chat/presenca/heartbeat
// Pausa quando document.hidden (aba em background)
// Retoma quando document.visibilitychange → visible
```

### 6.4 Badge no Sidebar

```tsx
// layouts/AppLayout.tsx — adicionar ao link do chat
// Soma de naoLidas de todas as conversas
// <Badge variant="destructive">{totalNaoLidas}</Badge>
// Visível apenas para usuários com ChatPerfil !== 'SemAcesso'
```

### 6.5 Nova Rota

```tsx
// App.tsx — dentro de ProtectedRoute
<Route path="/chat" element={<ChatPage />} />
```

---

## 7. Decisões e Alternativas

| Decisão | Escolha | Alternativa descartada | Motivo |
|---------|---------|----------------------|--------|
| Hub SignalR | `ChatHub` separado do hub existente | Unificar em um hub | Separação de responsabilidades; hub de notificações de chamados e hub de chat têm escopos e grupos distintos |
| Presença | Heartbeat 30s + worker background | WebSocket ping nativo | Mais simples, funciona com reconexões, já temos o padrão de background service |
| Storage | Bucket `chat-arquivos` separado | Mesmo bucket dos chamados | Retenção e permissões independentes no futuro |
| Logs | Entidade `ChatHistorico` separada | Reusar `HistoricoEntrada` | `HistoricoEntrada` está acoplada a `ChamadoId`; chat tem escopo diferente |
| Timeout presença | 5min → Ausente, 15min → Offline | 2min → Offline direto | Reduz falsos positivos (troca de aba, reunião rápida) |
| Edição | Só pelo autor, até 24h | Sem limite de tempo | Boas práticas corporativas; evita revisão de histórico antigo |

---

## 8. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|--------------|---------|-----------|
| Volume de heartbeats sobrecarregar a API | Média | Alto | Rate limit no endpoint de heartbeat (max 1 req/30s por usuário via cache/Redis futuro) |
| Múltiplas tabs do mesmo usuário duplicando heartbeat | Alta | Baixo | Backend usa upsert — só atualiza timestamp, não duplica registro |
| Arquivo enviado mas mensagem falha ao persistir | Baixa | Médio | Handler com try/catch: se insert falha, remove arquivo do Storage (mesmo padrão de `AdicionarAnexoCommandHandler`) |
| SignalR desconectado em mobile/aba inativa | Alta | Baixo | Cliente reconecta automaticamente; presença cai para Offline naturalmente |

---

## 9. Perguntas em Aberto

Nenhuma — todas as decisões foram alinhadas com o usuário em 2026-08-29.

---

## 10. Extensão — Confirmação de leitura + preferência de privacidade (2026-09-04)

### 10.1 Dado novo

`UsuarioPerfil` ganha `bool MostrarConfirmacaoLeitura` (default `true`), mesmo padrão de
`ChatPerfil`: propriedade `private set` + método de negócio `DefinirPreferenciaLeitura(bool)`.
Migration `AddMostrarConfirmacaoLeituraUsuarioPerfil` (coluna `NOT NULL DEFAULT true`, sem quebrar
usuários existentes).

O dado bruto de leitura já existe (`ChatParticipante.UltimaLeituraEm`, mantido pelo
`MarcarComoLidoCommand`) — esta extensão só passa a **expor** e **filtrar** esse dado, não cria
tabela nova.

### 10.2 Reciprocidade — onde é aplicada

Decisão: reciprocidade resolvida **no backend**, não só escondida na UI (defesa em profundidade —
um usuário curioso não deve conseguir ver o dado via chamada direta à API mesmo com o toggle
desligado).

Regra única, aplicada em `ObterConversaQueryHandler` (que já retorna `ChatParticipanteInfo` por
participante — ver `ChatConversaDetalheResponse`):

```
Para cada participante P (exceto o próprio requisitante):
  P.UltimaLeituraEmExposta =
      (requisitante.MostrarConfirmacaoLeitura == true)   // eu quero ver leitura alheia?
      && (P.MostrarConfirmacaoLeitura == true)             // ele deixa a leitura dele ser vista?
      ? P.UltimaLeituraEm
      : null
```

Isso cobre os dois sentidos pedidos: se eu desligo, `requisitante.MostrarConfirmacaoLeitura` é
`false` pra mim mesmo quando eu olho a conversa → nunca vejo leitura de ninguém (AC-56b). Se o
outro desliga, `P.MostrarConfirmacaoLeitura` é `false` → ninguém vê a leitura dele, mas ele
continua vendo a dos outros normalmente, contanto que o dele próprio esteja ligado (AC-57). E como
a mesma regra roda pra cada par (eu como P na consulta de quem eu enviei mensagem, o destinatário
como requisitante), minha própria mensagem também para de mostrar "Visto" pros outros quando eu
desligo (AC-56a) — sem precisar de nenhuma lógica extra em `EnviarMensagem`/`ListarMensagens`.

`ChatParticipanteInfo` ganha um 3º campo `UltimaLeituraEm` (nullable) com esse valor já filtrado —
o frontend nunca decide reciprocidade, só renderiza o que veio (nulo = não mostra nada).

### 10.3 Cálculo de "Visto" vs "Visto por todos" (frontend)

Só a **última mensagem enviada pelo usuário atual** em cada conversa carrega o indicador (mesmo
padrão do WhatsApp/Telegram — evitar poluir toda a lista de mensagens):

- 1:1: `Visto` + horário, se `UltimaLeituraEm` do outro participante (exposta, já filtrada pela
  regra 10.2) for `>= mensagem.dataCriacao`.
- Grupo: `Visto por todos`, se **todos** os participantes ativos, exceto o autor, tiverem
  `UltimaLeituraEm` exposta `>= mensagem.dataCriacao`. Um único participante com o dado oculto
  (`null`, seja por privacidade dele ou por nunca ter lido) já impede o "por todos".

### 10.4 Novo endpoint (self-service, não passa pelo `PerfilRequisitanteGuard` de Admin)

`PATCH /api/auth/preferencia-leitura` — corpo `{ "mostrar": bool }`, sempre aplica ao próprio
usuário autenticado (`_currentUser.UsuarioId`), sem parâmetro de id na URL — mesmo padrão de
`GET /api/auth/me`. Novo `DefinirPreferenciaLeituraCommand`/Handler/Validator em
`Features/Chat/Commands/DefinirPreferenciaLeitura/`. Sem auditoria em `ChatHistorico` — é uma
preferência de conta, não uma ação de chat (mesmo tratamento dado hoje ao tema claro/escuro, que
também não é auditado).

Campo replicado em `AutenticacaoResponse` e `UsuarioPerfilResponse` (mesmo padrão do `ChatPerfil`)
para o frontend já ter o valor no boot, sem round-trip extra.

### 10.5 Frontend — onde mora a preferência

Menu "Preferências" novo, no rodapé da sidebar (`AppLayout`/`SidebarFooter`, ao lado do toggle de
tema claro/escuro já existente) — primeira entrada de uma tela que pode crescer com outras
preferências futuras. Um `DropdownMenu`/`Popover` simples com um `Switch` (shadcn) — não precisa de
página de rota própria pra uma única opção.

`AuthContext` ganha `perfil.mostrarConfirmacaoLeitura` (vem de `AutenticacaoResponse`/`GET /auth/me`
no boot) + `atualizarPreferenciaLeitura(mostrar)`, mesmo padrão já usado por
`atualizarChatPerfil` — atualiza estado em memória + `localStorage` depois da resposta 204 da API.

### 10.6 Tempo real

Não precisa de evento SignalR novo: quando `MarcarComoLidoCommand` roda, `ChatMensagemLidaNotification`
já é publicada e o frontend já reage a ela invalidando a query de mensagens
(`useChatSignalR.ts:114`, evento `MensagemLida`) — o refetch de `ObterConversaQuery`/mensagens já
traz o `UltimaLeituraEm` atualizado (e já filtrado pela regra 10.2). Mudar o próprio toggle de
preferência não precisa de tempo real — só afeta o que a própria pessoa vê a partir da próxima
consulta.

### 10.7 Decisões confirmadas com o usuário (não presumidas)

| Pergunta | Resposta do usuário |
|---|---|
| Reciprocidade ao desligar? | Sim, estilo WhatsApp — afeta os dois sentidos |
| Escopo do toggle | Global (todas as conversas), não por conversa |
| Onde fica na UI | Menu "Preferências" novo na sidebar, não dentro da tela `/chat` |
