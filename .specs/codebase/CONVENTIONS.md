# Convenções de Código — ChamadosCamarj

> Extraído do código real do projeto, não de suposições. Atualizado em 2026-07-27.
> Ver `AGENTS.md` para stack, estrutura e comandos. Ver `STATE.md` (seção "Constitution") para regras de processo SDD.

---

## 1. Geral

### 1.1 Idioma
- **Documentação** (`.specs/`, `README.md`, docs em geral): Português.
- **Código (identificadores):** Português para nomes de domínio (`Chamado`, `UsuarioPerfil`, `SenhaHash`, `reatribuirChamado`). Inglês para termos técnicos/genéricos (`cancellationToken`, `onSuccess`, `isPending`).
- **Mensagens de erro/validação:** Português (`"Email é obrigatório."`, `"Chamado não encontrado."`).
- **Comentários no código:** Português, só quando a lógica não for autoexplicativa. Sem comentários óbvios.
- **HTML lang:** `pt-BR` no `index.html`.

### 1.2 Nomenclatura
- PascalCase para classes, métodos, propriedades, records.
- camelCase para variáveis locais, parâmetros.
- Sufixos: `Response` (DTO de saída), `Request` (DTO de entrada via body), `Command`/`Query` (MediatR).
- Arquivos: um tipo por arquivo, nome do arquivo = nome da classe (ex: `Chamado.cs` → `class Chamado`).

### 1.3 Git
- Branch ativa: `develop`. Nunca commitar direto em `main`.
- PRs com base `develop`. Conferir dropdown `base` antes de abrir (o GitHub não fixa `develop`).

---

## 2. Backend — .NET 9 / Clean Architecture / CQRS

### 2.1 Estrutura do Projeto
```
src/
├── ChamadosCamarj.Domain/         → Entidades, Enums, Interfaces (repositórios e serviços)
│   ├── Common/BaseEntity.cs
│   ├── Entities/
│   ├── Enums/
│   └── Interfaces/
├── ChamadosCamarj.Application/    → CQRS, DTOs, Serviços de aplicação, Exceptions
│   ├── Common/                    → Exceptions, Behaviours, Interfaces (IJwtTokenService, ICurrentUserService, IEmailSender), Settings
│   ├── Features/{Feature}/
│   │   ├── Commands/              → Command + Handler + Validator no mesmo diretório
│   │   ├── Queries/
│   │   ├── DTOs/
│   │   └── Validators/
│   └── Mappings/                  → Extension methods de mapping (entidade → Response)
├── ChamadosCamarj.Infrastructure/  → EF Core, Migrations, Repos, Serviços externos
│   ├── Data/                      → DbContext, Configurations (Fluent API), DatabaseSeeder
│   ├── Migrations/
│   ├── Repositories/
│   └── Services/                  → Implementações concretas de serviços externos
└── ChamadosCamarj.WebApi/         → Controllers, Program.cs, Middleware, Hubs, Services
    ├── Controllers/
    ├── Middleware/
    ├── Hubs/
    └── Services/                  → Implementações que dependem de HttpContext (ex: CurrentUserService)
```

### 2.2 Namespaces
- File-scoped: `namespace ChamadosCamarj.Domain.Entities;` (sempre com `;`).
- Padrão: `ChamadosCamarj.<Camada>.<Subpasta>`.

### 2.3 Entidades (Domain Layer)

#### Herança
Toda entidade herda de `BaseEntity`:
```csharp
// src/ChamadosCamarj.Domain/Common/BaseEntity.cs
public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }
}
```

#### Padrão de construtor
```csharp
public class UsuarioPerfil : BaseEntity
{
    // 1. Construtor privado sem parâmetros para EF Core (sempre presente)
    private UsuarioPerfil() { }

    // 2. Construtor público com validação inline via ArgumentException
    public UsuarioPerfil(string email, string nome, Perfil perfil)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("E-mail é obrigatório.", nameof(email));
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));

        Email = email.Trim().ToLowerInvariant();
        Nome = nome;
        Perfil = perfil;
        Ativo = true;
    }

    // 3. Propriedades com private set — nunca setter público
    public string Email { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public Perfil Perfil { get; private set; }
    public bool Ativo { get; private set; }
    public string? SenhaHash { get; private set; }

    // 4. Navegação EF: private set, nullable (?), comentário // Navegação EF
    // Navegação EF
    public Categoria? Categoria { get; private set; }

    // 5. Coleções inicializadas com collection expression
    public ICollection<Comentario> Comentarios { get; private set; } = [];

    // 6. Métodos de negócio na própria entidade (rich domain model)
    public void DefinirSenhaHash(string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("Hash de senha é obrigatório.", nameof(senhaHash));
        SenhaHash = senhaHash;
        DataAtualizacao = DateTime.UtcNow;
    }
}
```

#### Factory method (alternativa, usado em `HistoricoEntrada`)
```csharp
public class HistoricoEntrada : BaseEntity
{
    private HistoricoEntrada() { }

    public static HistoricoEntrada Criar(
        Guid chamadoId, string usuarioNome, Guid? usuarioId,
        AcaoHistorico acao, string? detalheAnterior = null, string? detalheNovo = null)
    {
        if (chamadoId == Guid.Empty)
            throw new ArgumentException("ChamadoId não pode ser vazio", nameof(chamadoId));
        // ... validações ...

        return new HistoricoEntrada
        {
            ChamadoId = chamadoId,
            UsuarioNome = usuarioNome,
            // ...
            DataHora = DateTime.UtcNow
        };
    }
}
```

### 2.4 Enums
```csharp
// Domain/Enums/StatusChamado.cs
public enum StatusChamado
{
    Aberto,
    EmAndamento,
    Resolvido,
    Fechado,
    Cancelado
}

// Com JsonStringEnumConverter para serialização como string
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Perfil
{
    Admin = 1,
    Atendente = 2,
    Solicitante = 3
}
```

### 2.5 CQRS: Commands e Queries

#### Command (record posicional)
```csharp
// Features/Auth/Commands/LoginCommand.cs
public record LoginCommand(string Email, string Senha) : IRequest<AutenticacaoResponse>;

// Comando sem retorno (void):
public record EsqueciSenhaCommand(string Email) : IRequest;

// Comando com retorno bool (caso específico):
public record ResetarSenhaCommand(string Token, string NovaSenha) : IRequest<bool>;
```

#### Query
```csharp
// Features/Chamados/Queries/ObterChamadoPorIdQuery.cs
public record ObterChamadoPorIdQuery(Guid Id) : IRequest<ChamadoResponse?>;

// Com paginação:
public record ListarChamadosQuery(
    int Pagina, int TamanhoPagina, string? Status, /* ... */
) : IRequest<PagedResult<ChamadoResponse>>;
```

#### Handler
```csharp
public class LoginCommandHandler : IRequestHandler<LoginCommand, AutenticacaoResponse>
{
    // 1. Constantes no topo
    private const string MensagemCredenciaisInvalidas = "E-mail ou senha inválidos.";

    // 2. Dependências injetadas (repositórios, nunca DbContext direto)
    private readonly IUsuarioPerfilRepository _usuarioPerfilRepository;
    private readonly IPasswordHasher<UsuarioPerfil> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IUsuarioPerfilRepository usuarioPerfilRepository,
        IPasswordHasher<UsuarioPerfil> passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _usuarioPerfilRepository = usuarioPerfilRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AutenticacaoResponse> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {
        // 3. Normalização de input
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        // 4. Acesso via repositório (nunca DbContext direto)
        var usuario = await _usuarioPerfilRepository.ObterPorEmailAsync(
            emailNormalizado, cancellationToken);

        // 5. Exceções de negócio via classes customizadas em Application.Common.Exceptions
        if (usuario is null || !usuario.Ativo)
            throw new UnauthorizedException(MensagemCredenciaisInvalidas);

        // 6. Operações de negócio chamam métodos da entidade
        usuario.DefinirSenhaHash(novoHash);

        // 7. Persistência via repositório
        await _usuarioPerfilRepository.AtualizarAsync(usuario, cancellationToken);

        // 8. Retorno tipado (nunca genérico)
        return new AutenticacaoResponse(token, usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil);
    }
}
```

**Regras:**
- Handlers usam **repositórios**, nunca `ApplicationDbContext` diretamente.
- Handlers NÃO injetam `IConfiguration` diretamente — usam services ou `IOptions<T>`.
- Handlers com `IRequest` (sem tipo de retorno) implementam `IRequestHandler<T>`.
- `CancellationToken` em TODOS os métodos async de repositório.

#### Validator (FluentValidation)
```csharp
// Features/Auth/Validators/LoginCommandValidator.cs
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(c => c.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
```

**Regras:**
- Classe separada com sufixo `Validator`, mesmo diretório do Command ou em pasta `Validators/`.
- Injetado automaticamente pelo pipeline `ValidationBehaviour` (registrado em `Program.cs`).
- **Toda mensagem de validação DEVE ter `.WithMessage()` em português.**

#### Pipeline Behaviours
```csharp
// Common/Behaviours/ValidationBehaviour.cs
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // Valida automaticamente antes de cada Command/Query
    // Se houver falhas, lança ValidationException → middleware retorna 400
}
```

### 2.6 DTOs

```csharp
// Features/Auth/DTOs/AutenticacaoResponse.cs
public record AutenticacaoResponse(
    string Token,
    Guid Id,
    string Nome,
    string Email,
    Perfil Perfil
);
```

- Records posicionais.
- Sufixo `Response` para saída.
- Namespace: `ChamadosCamarj.Application.Features.{Feature}.DTOs`.

### 2.7 Mapeamento (Entidade → DTO)

```csharp
// Application/Mappings/ChamadoMappings.cs
public static class ChamadoMappings
{
    public static ChamadoResponse ToResponse(this Chamado chamado) =>
        new(
            chamado.Id,
            chamado.Numero,
            // ... todos os campos explicitamente
            chamado.Categoria?.Nome,
            chamado.DataCriacao,
            chamado.DataAtualizacao,
            chamado.Comentarios.Count,
            chamado.Anexos.Count
        );
}
```

- Extension methods, classe estática.
- **Sem AutoMapper** — mapeamento manual explícito.
- Cada entidade com mapping tem seu próprio arquivo `{Entidade}Mappings.cs`.

### 2.8 Repositórios

#### Interface (Domain.Interfaces)
```csharp
// Domain/Interfaces/IUsuarioPerfilRepository.cs
public interface IUsuarioPerfilRepository
{
    Task<UsuarioPerfil?> ObterPorEmailAsync(string email, CancellationToken ct);
    Task<UsuarioPerfil?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<UsuarioPerfil>> ListarAsync(CancellationToken ct);
    Task AdicionarAsync(UsuarioPerfil usuario, CancellationToken ct);
    Task AtualizarAsync(UsuarioPerfil usuario, CancellationToken ct);
}
```

#### Implementação (Infrastructure.Repositories)
```csharp
public class UsuarioPerfilRepository : IUsuarioPerfilRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<UsuarioPerfil> _dbSet;

    public UsuarioPerfilRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<UsuarioPerfil>();
    }

    public async Task<UsuarioPerfil?> ObterPorEmailAsync(string email, CancellationToken ct)
    {
        return await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task AtualizarAsync(UsuarioPerfil usuario, CancellationToken ct)
    {
        _dbSet.Update(usuario);
        await _context.SaveChangesAsync(ct);
    }
}
```

**Regras:**
- `AsNoTracking()` em todas as queries de leitura.
- `SaveChangesAsync` após cada operação de escrita.
- `ArgumentNullException` no construtor para `context`.
- CancellationToken em todos os métodos async.
- Nomes de parâmetro variam: `cancellationToken` (ChamadoRepository) ou `ct` (UsuarioPerfilRepository) — ambos aceitos.

### 2.9 Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChamadosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChamadosController> _logger;
    private readonly ICurrentUserService _currentUser;

    public ChamadosController(IMediator mediator, ILogger<ChamadosController> logger,
        ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _logger = logger;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Descrição do endpoint em português.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ChamadoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ChamadoResponse>>> Listar(
        [FromQuery] int pagina = 1,
        CancellationToken cancellationToken = default)
    {
        var query = new ListarChamadosQuery(pagina, /* ... */);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
```

**Regras:**
- Herdam `ControllerBase`. Atributos: `[ApiController]`, `[Route("api/[controller]")]`, `[Produces("application/json")]`.
- `IMediator` injetado para dispatch de Commands/Queries.
- `ICurrentUserService` para obter identidade do usuário autenticado dos claims JWT.
- **`CancellationToken` em TODOS os endpoints.**
- `[AllowAnonymous]` explícito nos endpoints públicos (`/auth/login`, `/auth/esqueci-senha`, etc.).
- `[ProducesResponseType]` com tipo e status code explícitos.
- **Todo endpoint público deve ter `/// <summary>` XML doc em português.**
- Ações do usuário (`Atribuir`, `Resolver`, etc.) vêm do token (`_currentUser`) — nunca do body da requisição.

### 2.10 Injeção de Dependência (Program.cs)

#### Registro padrão
```csharp
// Repositórios e serviços — escopo padrão Scoped
builder.Services.AddScoped<IChamadoRepository, ChamadoRepository>();
builder.Services.AddScoped<IUsuarioPerfilRepository, UsuarioPerfilRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher<UsuarioPerfil>, PasswordHasher<UsuarioPerfil>>();
```

#### Registro com factory (serviços externos com parâmetros)
```csharp
builder.Services.AddScoped<IEmailSender>(_ =>
{
    var smtpEmail = builder.Configuration["Email:SmtpEmail"] ?? "suporte@camarj.com.br";
    var smtpSenha = builder.Configuration["Email:SmtpSenha"];
    if (string.IsNullOrWhiteSpace(smtpSenha))
        throw new InvalidOperationException("'Email:SmtpSenha' não configurada. Use dotnet user-secrets set.");
    return new SmtpEmailSender(smtpEmail, smtpSenha);
});
```

#### Settings tipados (padrão preferido)
```csharp
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));

// No serviço:
public class JwtTokenService : IJwtTokenService
{
    public JwtTokenService(IOptions<AuthSettings> authSettings)
    {
        _authSettings = authSettings.Value;
    }
}
```

**Regras:**
- Escopo padrão: `AddScoped`.
- Settings: preferir `builder.Services.Configure<T>()` + `IOptions<T>` no construtor do serviço.
- Factory (`_ => new ...`) só quando o serviço precisa de parâmetros não-injetáveis (ex: senhas).
- Nunca passar `IConfiguration` para dentro de Handlers.

### 2.11 Migrations (EF Core)

```csharp
// Infrastructure/Migrations/20260724195834_AddSenhaHashUsuarioPerfil.cs
public partial class AddSenhaHashUsuarioPerfil : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SenhaHash",
            table: "UsuariosPerfil",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SenhaHash",
            table: "UsuariosPerfil");
    }
}
```

**Regras:**
- Nome: `{timestamp}_{NomeDescritivo}`. Gerado por `dotnet ef migrations add`.
- Todo `Up()` deve ter `Down()` correspondente (reversível).
- Localização: `src/ChamadosCamarj.Infrastructure/Migrations/`.
- Aplicação automática em `Program.cs`: `await db.Database.MigrateAsync()`.

### 2.12 Configuração Fluent API (EF Core)

```csharp
// Infrastructure/Data/Configurations/UsuarioPerfilConfiguration.cs
public class UsuarioPerfilConfiguration : IEntityTypeConfiguration<UsuarioPerfil>
{
    public void Configure(EntityTypeBuilder<UsuarioPerfil> builder)
    {
        builder.ToTable("UsuariosPerfil");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
```

- Carregadas via `ApplyConfigurationsFromAssembly` no `ApplicationDbContext.OnModelCreating`.
- `ValueGeneratedNever()` para GUIDs (gerados pela aplicação).

### 2.13 Tratamento de Erros

#### Middleware global (`ExceptionHandlingMiddleware`)
```csharp
// Captura sequencial, da mais específica pra genérica:
catch (NotFoundException ex)      → 404, { message: ex.Message }
catch (ConflictException ex)      → 409, { message: ex.Message }
catch (ForbiddenException ex)     → 403, { message: ex.Message }
catch (UnauthorizedException ex)  → 401, { message: ex.Message }
catch (ValidationException ex)    → 400, { errors: [{ campo, erro }] }
catch (InvalidOperationException ex) → 400, { message: ex.Message }
catch (Exception ex)              → 500, { message: "Ocorreu um erro interno." } + log
```

#### Classes de exceção customizadas
```csharp
// Todas em Application/Common/Exceptions/
public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"{name} com o id '{key}' não foi encontrado.") { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
```

**Regras:**
- SEMPRE usar exceções customizadas para erros de negócio (nunca `Exception` genérica).
- Mensagens de erro em português.
- `NotFoundException` recebe nome da entidade + key (formato: `"Chamado com o id 'xxx' não foi encontrado."`).

### 2.14 Serviços de Infraestrutura

```csharp
// Infrastructure/Services/SmtpEmailSender.cs
public class SmtpEmailSender : IEmailSender
{
    private readonly string _remetente;
    private readonly string _senha;

    public SmtpEmailSender(string remetente, string senha)
    {
        _remetente = remetente;
        _senha = senha;
    }

    public async Task EnviarAsync(string para, string assunto, string corpoHtml)
    {
        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Chamados CAMARJ", _remetente));
        message.To.Add(MailboxAddress.Parse(para));
        message.Subject = assunto;
        message.Body = new TextPart("html") { Text = corpoHtml };

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587,
            MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_remetente, _senha);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
```

**Regras:**
- Interface definida em `Application.Common` (ex: `IEmailSender`).
- Implementação em `Infrastructure.Services`.
- Parâmetros de configuração recebidos via construtor (não injetados via DI).

---

## 3. Frontend — React 19 + TypeScript + Vite + Tailwind v4

### 3.1 Estrutura do Projeto
```
frontend/src/
├── auth/                    → AuthContext, LoginPage, ResetarSenhaPage, api.ts
├── layouts/                 → AppLayout.tsx
├── lib/                     → api.ts (apiFetch), utils.ts (helpers)
├── hooks/                   → hooks globais (useSignalR, useInactivityLogout)
├── types/                   → api.ts (tipos compartilhados que espelham DTOs do backend)
├── components/ui/           → SOMENTE primitivas shadcn/ui instaladas via CLI (`npx shadcn add`)
├── features/
│   └── {feature}/
│       ├── api.ts           → funções de acesso à API específicas
│       ├── hooks/           → hooks TanStack Query (um por ação/query)
│       ├── components/      → componentes específicos da feature
│       └── *Page.tsx        → páginas
├── App.tsx                  → Provider tree + rotas
├── main.tsx                 → entry point
└── index.css                → Tailwind @import, @theme inline, variáveis de tema
```

### 3.2 Componentes

```tsx
// Função nomeada com export, não default export (exceto App)
export function LoginPage() {
  // 1. Hooks no topo: estado local, contexto, router
  const { loginComSenha } = useAuth()
  const [email, setEmail] = useState('')
  const [erro, setErro] = useState<string | null>(null)
  const [pendente, setPendente] = useState(false)

  // 2. Handlers definidos dentro do componente
  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setErro(null)
    setPendente(true)
    try {
      await loginComSenha(email, senha)
    } catch (err) {
      // 3. Erro inline: useState<string | null>, sem toast library
      setErro(err instanceof Error ? err.message : 'Erro desconhecido.')
    } finally {
      setPendente(false)
    }
  }

  // 4. JSX com classes Tailwind, usando tokens do tema (nunca cores hardcoded)
  return (
    <div className="flex min-h-svh items-center justify-center p-6">
      <Card className="w-full max-w-md border-border/60 shadow-2xl">
        {/* ... */}
        {erro && (
          <Alert variant="destructive">
            <AlertDescription>{erro}</AlertDescription>
          </Alert>
        )}
      </Card>
    </div>
  )
}
```

**Regras:**
- **Function components** com `export` nomeado (não `export default`, exceto `App`).
- Erro exibido inline com `useState<string | null>` + `<Alert variant="destructive">`.
- NÃO existe toast library (sem `useToast`/Sonner).
- Estados de carregamento: `pendente` boolean + texto condicional no botão (`{pendente ? 'Entrando...' : 'Entrar'}`).
- Evitar `useCallback`/`useMemo` desnecessários — usar só com dependências caras ou props de componentes memoizados.

### 3.3 Componentes shadcn/ui
```tsx
import { Button } from '@/components/ui/button'
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from '@/components/ui/dialog'
```

- Importados de `@/components/ui/`.
- Instalados via `npx shadcn add <componente>`.
- Não editar manualmente, não criar substitutos custom aqui.
- Componentes de feature em `features/{feature}/components/`.

### 3.4 Hooks (TanStack Query)

#### Query (leitura)
```tsx
// features/chamados/hooks/useChamado.ts
import { useQuery } from '@tanstack/react-query'
import { obterChamado } from '../api'

export function useChamado(id: string) {
  return useQuery({
    queryKey: ['chamado', id],
    queryFn: () => obterChamado(id),
  })
}
```

#### Mutation (escrita)
```tsx
// features/chamados/hooks/useAcoesChamado.ts
import { useMutation, useQueryClient } from '@tanstack/react-query'

function invalidarChamado(queryClient: ReturnType<typeof useQueryClient>, id: string) {
  queryClient.invalidateQueries({ queryKey: ['chamado', id] })
  queryClient.invalidateQueries({ queryKey: ['chamados'] })
  queryClient.invalidateQueries({ queryKey: ['historico', id] })
}

export function useAtribuirChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => atribuirChamado(chamadoId),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}
```

**Regras:**
- `queryKey`: array hierárquico (ex: `['chamado', id]`, `['chamados', filtros]`).
- Toda mutation invalida queries relacionadas no `onSuccess`.
- Padrão de invalidação: `chamado` individual + `chamados` (lista) + `historico`.
- `useQuery` retorna `{ data, isPending, error }`. NÃO usar `isLoading` (v5).
- QueryClient configurado em `App.tsx`: retry só em erros 5xx (máx 3 tentativas).

### 3.5 API (acesso HTTP)

```tsx
// lib/api.ts — cliente HTTP único
const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api'

export async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const token = getToken()
  const isFormData = options?.body instanceof FormData

  const response = await fetch(`${BASE_URL}${path}`, {
    ...options,
    headers: {
      ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options?.headers,
    },
  })

  if (!response.ok) {
    if (response.status === 401) {
      clearToken()
      aoDeslogarPorTokenInvalido?.()
    }
    const body = await response.json().catch(() => null)
    throw new ApiError(body?.message ?? 'Ocorreu um erro inesperado.', response.status, body?.errors)
  }

  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}
```

**Regras:**
- `apiFetch` é a **única** função de acesso HTTP. Nunca usar fetch direto nem axios.
- Funções de domínio em `features/{feature}/api.ts`, importando `apiFetch` de `@/lib/api`.
- `ApiError` tem `status`, `message`, `errors` — usar para tratamento condicional (`err instanceof ApiError && err.status === 403`).
- `FormData` upload: NÃO setar `Content-Type` (o browser gera o boundary do multipart).
- Token armazenado em localStorage: `chamados-camarj:token`.

#### Funções de API por feature
```tsx
// features/chamados/api.ts
export function atribuirChamado(chamadoId: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/atribuir`, { method: 'PATCH' })
}

export function reabrirChamado(chamadoId: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/reabrir`, { method: 'PATCH' })
}
```

- Uma função por endpoint.
- Tipos `Request`/`Response` importados de `@/types/api`.
- Todas retornam `Promise<T>` tipado.

### 3.6 Diálogos/Modais

```tsx
// Padrão: useState boolean + Dialog + onOpenChange
const [confirmarAcao, setConfirmarAcao] = useState<'resolver' | 'encerrar' | 'cancelar' | null>(null)

<Dialog open={confirmarAcao !== null} onOpenChange={(open) => { if (!open) setConfirmarAcao(null) }}>
  <DialogContent>
    <DialogHeader>
      <DialogTitle>{tituloConfirmacao}</DialogTitle>
      <DialogDescription>{descricaoConfirmacao}</DialogDescription>
    </DialogHeader>
    <DialogFooter>
      <Button variant="outline" onClick={() => setConfirmarAcao(null)}>
        Voltar
      </Button>
      <Button variant="destructive" onClick={executarAcao} disabled={isPending}>
        {isPending ? 'Processando...' : 'Confirmar'}
      </Button>
    </DialogFooter>
  </DialogContent>
</Dialog>
```

**Regras:**
- `useState<T | null>` controla abertura (não `boolean` para múltiplos diálogos).
- `onOpenChange` reseta o estado quando fechado.
- `DialogHeader` com `DialogTitle` + `DialogDescription` SEMPRE presentes.
- `DialogFooter` com botões de ação alinhados à direita.
- Resetar estados relacionados ao fechar (`fecharEsqueciSenha`).

### 3.7 Autenticação (AuthContext)

```tsx
// auth/AuthContext.tsx
interface Perfil {
  tipo: TipoPerfil
  id: string
  nome: string
  email: string
}

interface AuthContextValue {
  perfil: Perfil | null
  loginComGoogle: (idToken: string) => Promise<void>
  loginComSenha: (email: string, senha: string) => Promise<void>
  logout: () => void
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth deve ser usado dentro de um AuthProvider')
  return context
}
```

**Regras:**
- `AuthProvider` envolve toda a aplicação.
- `useAuth()` lança erro se usado fora do provider (guarda).
- `logout()` limpa token + perfil do localStorage, seta `perfil = null`.
- Logout automático em 401 via callback registrado em `apiFetch`.

### 3.8 Rotas (react-router v8)

```tsx
// App.tsx
import { BrowserRouter, Navigate, Route, Routes } from 'react-router'

function AppRoutes() {
  return (
    <Routes>
      {/* Rotas públicas (sem proteção) */}
      <Route path="/login" element={<LoginRoute />} />
      <Route path="/resetar-senha" element={<ResetarSenhaPage />} />

      {/* Rotas protegidas (precisa de perfil autenticado) */}
      <Route element={<ProtectedRoute />}>
        <Route path="/chamados" element={<ChamadosListPage />} />
        {/* ... */}
      </Route>

      {/* Catch-all */}
      <Route path="*" element={<Navigate to="/chamados" replace />} />
    </Routes>
  )
}
```

**Regras:**
- `BrowserRouter` no topo (sem `basename`).
- `ProtectedRoute`: se `!perfil`, redireciona pra `/login`.
- `LoginRoute`: se já tem `perfil`, redireciona pra `/chamados`.
- Sem lazy loading nem `Suspense`.
- `SignalRProvider` só monta dentro de `ProtectedRoute` (precisa de token).

### 3.9 CSS / Tailwind / Tema

```css
/* index.css */
@import "tailwindcss";
@import "tw-animate-css";
@import "shadcn/tailwind.css";

@custom-variant dark (&:is(.dark *));

@theme inline {
    --font-sans: 'Geist Variable', sans-serif;
    --font-serif: 'Source Serif 4 Variable', serif;
    --color-border: var(--border);
    --color-destructive: var(--destructive);
    /* ... tokens gerados pelo shadcn/ui ... */
}

:root {
    --card: oklch(1 0 0);
    --foreground: oklch(0.145 0 0);
    /* ... paleta do tema claro ... */
}

.dark {
    --card: oklch(0.205 0 0);
    --foreground: oklch(0.985 0 0);
    /* ... paleta do tema escuro ... */
}
```

**Regras:**
- Tailwind v4 com `@import "tailwindcss"` (não `@tailwind base/components/utilities`).
- Tema inline via `@theme inline { ... }` com variáveis CSS.
- **Nunca usar cores hardcoded.** Usar tokens: `text-foreground`, `bg-card`, `text-muted-foreground`, `border-border`.
- Tema gerenciado pelo `ThemeProvider` em `hooks/useTheme.tsx`. O `<html>` não tem `class="dark"` hardcoded. O provider lê/prefere localStorage, aplica/remove a classe `.dark` no `<html>` dinamicamente.
- Fonte `font-serif` para telas editoriais (Login), `font-heading` (= `font-sans`) para o resto.

### 3.10 TypeScript

```tsx
// types/api.ts — única fonte dos tipos que espelham DTOs do backend
export type StatusChamado = "Aberto" | "EmAndamento" | "Resolvido" | "Fechado" | "Cancelado";
export type PrioridadeChamado = "Baixa" | "Media" | "Alta" | "Urgente";
export type TipoPerfil = "Admin" | "Atendente" | "Solicitante";

export interface ChamadoResponse {
  id: string;
  numero: number;
  titulo: string;
  status: StatusChamado;
  // ...
}
```

- Enums como **type unions de string** (não `enum` do TS), porque o backend serializa como string.
- Interfaces para DTOs em `types/api.ts`.
- Tipos de request específicos da feature podem ficar em `features/{feature}/api.ts`.

### 3.11 Componentes de Input Customizados (PasswordInput)
- Usa o padrão `forwardRef` + spread props para compatibilidade com `react-hook-form`
- Import de `@/components/PasswordInput`
- Usa ícones `Eye`/`EyeOff` do `lucide-react` para toggle de visibilidade
- `tabIndex={-1}` no botão de toggle para não atrapalhar a tabulação do formulário

### 3.12 Tema (ThemeProvider / useTheme)
- `ThemeProvider` definido em `hooks/useTheme.tsx`
- Padrão: `createContext` + componente `ThemeProvider` + hook `useTheme()`
- Envolve toda a aplicação em `App.tsx`
- Botão de toggle no `SidebarFooter` com ícones `Sun`/`Moon` (`lucide-react`)
- Persiste preferência em `localStorage` na chave `camarj-theme`
- Respeita a media query `prefers-color-scheme`
- CSS já possui variáveis tanto em `:root` (tema claro) quanto em `.dark` (tema escuro)

---

## 4. SDD (Spec-Driven Development)

### 4.1 Estrutura `.specs/`
```
.specs/
├── project/
│   ├── STATE.md        → Memória do projeto: sessões, decisões, blockers, pendências
│   ├── ROADMAP.md      → Visão de fases/features planejadas
│   └── PROJECT.md      → Visão geral do projeto (objetivo, stack, usuários)
├── codebase/
│   ├── CONVENTIONS.md  → Este arquivo
│   ├── ARCHITECTURE.md → Arquitetura de alto nível
│   ├── STACK.md        → Stack tecnológica detalhada
│   ├── STRUCTURE.md    → Estrutura de diretórios
│   ├── CONCERNS.md     → Débito técnico / problemas conhecidos
│   ├── INTEGRATIONS.md → Integrações externas (Supabase, Google, SMTP)
│   └── TESTING.md      → Estratégia e comandos de teste
└── features/
    └── {nome-feature}/
        ├── spec.md     → Especificação (problem, user stories, AC, rastreabilidade)
        ├── design.md   → Decisões técnicas (obrigatório — ver regras abaixo)
        └── tasks.md    → Checklist de implementação com status
```

**Template:** Use `.specs/features/FEATURE-TEMPLATE/` como ponto de partida para toda nova feature.

### 4.2 Fluxo SDD

Ciclo obrigatório para toda feature:

```
Specify → Design → Tasks → Execute → Gate Checks → Commit/Merge → Atualizar STATE.md
```

1. **Specify** → `spec.md` com problem statement, user stories (formato padrão), acceptance criteria numerados e testáveis.
2. **Design** → `design.md` com decisões técnicas (ver regra de obrigatoriedade abaixo).
3. **Tasks** → `tasks.md` com checklist granular. Última tarefa sempre: atualizar `spec.md` e `STATE.md`.
4. **Execute** → Implementar seguindo as tasks. Atualizar spec **antes** de qualquer mudança de escopo.
5. **Gate checks** → `dotnet test` + `npm run build` sem erros.
6. **Commit / Merge** → PR com base `develop`.
7. **Atualizar STATE.md** → Resumo da sessão, decisões, aprendizados.

### 4.3 Quando `design.md` é Obrigatório

`design.md` **deve** ser criado quando a feature:

| Condição | Exemplo |
|----------|---------|
| Toca mais de uma camada | Backend + Frontend juntos |
| Introduz nova entidade ou tabela | Nova entidade `Grupo`, nova migration |
| Altera contrato de interface existente | Remove/renomeia método em `IRepository` |
| Tem ambiguidade técnica não óbvia | Duas abordagens válidas que precisam ser decididas antes |

Para features simples (só uma camada, sem novo schema, sem mudança de contrato), as decisões técnicas podem ficar na própria `spec.md`.

### 4.4 Formato Padrão de User Story

```
Como [Perfil: Admin | Atendente | Solicitante],
quero [ação específica e mensurável],
para que [benefício concreto para o usuário ou o negócio].
```

Perfis válidos: `Admin`, `Atendente`, `Solicitante`.

### 4.5 Formato Padrão de Acceptance Criteria

Cada AC deve ser:
- **Numerado** (AC-01, AC-02, ...)
- **Testável** (verificável por teste automatizado ou passo manual reproduzível)
- **Sem ambiguidade** (condição → ação → resultado esperado)

```
Dado [contexto inicial],
quando [ação executada],
então [resultado esperado e mensurável].
```

Exemplo:
```
AC-01: Dado que sou Atendente autenticado,
       quando acesso GET /api/chamados sem filtros,
       então recebo 200 com lista paginada (máx 10 por página) dos chamados do meu grupo.

AC-02: Dado que sou Solicitante autenticado,
       quando acesso GET /api/chamados,
       então recebo 200 apenas com os chamados que eu mesmo abri.
```

### 4.6 Rastreabilidade Spec → Teste

Toda `spec.md` deve ter uma tabela de rastreabilidade mapeando cada AC ao teste que o verifica:

```markdown
| Critério | Arquivo de Teste | Método | Status |
|----------|-----------------|--------|--------|
| AC-01 | `ListarChamadosHandlerTests.cs` | `Handle_Atendente_RetornaApenasChamadosDoGrupo` | ✅ Coberto |
| AC-02 | `ListarChamadosHandlerTests.cs` | `Handle_Solicitante_RetornaApenasSeusChamados` | ✅ Coberto |
| AC-03 | Manual (UI) | Login → acessar /chamados como Solicitante | ⬜ Pendente |
```

### 4.7 Regras de Processo (Constitution)

Quatro regras permanentes. Se notar que uma está prestes a ser quebrada, **pare e avise o usuário antes de prosseguir**.

1. **Perguntas sem resposta não viram suposições.** Se ficar sem resposta em decisão de produto difícil de reverter: parar e perguntar de novo, ou marcar como `⚠️ PENDENTE DE CONFIRMAÇÃO`.
2. **Spec antes do código, sempre.** Atualizar `spec.md` antes de qualquer mudança de comportamento — mesmo extensões pequenas.
3. **Mudança de contrato é sinalizada antes.** Se remover/alterar interface usada por múltiplos consumidores: avisar antes de aplicar.
4. **Fluxo de orquestração SDD.** Seguir o ciclo completo (Specify → Design → Tasks → Execute → Gate Checks → Commit → STATE.md). Guia: `docs/GUIA-ORQUESTRACAO-SDD.md`.

---

## 5. Gate Checks Obrigatórios

Antes de finalizar qualquer feature ou sessão:

```powershell
# Backend
dotnet test tests/ChamadosCamarj.UnitTests/

# Frontend
cd frontend; if ($?) { npm run build }
```

Ambos devem passar **sem erros e sem warnings**. O build do frontend roda `tsc` + Vite.
