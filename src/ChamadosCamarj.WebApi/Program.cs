using System.Reflection;
using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ChamadosCamarj.Application.Common;
using ChamadosCamarj.Domain.Entities;
using ChamadosCamarj.Application.Common.Behaviours;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;
using ChamadosCamarj.Infrastructure.Repositories;
using ChamadosCamarj.Infrastructure.Services;
using ChamadosCamarj.WebApi.Middleware;
using ChamadosCamarj.WebApi.Hubs;
using ChamadosCamarj.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────
// User Secrets (senha do Supabase — só na sua máquina)
// ─────────────────────────────
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// ─────────────────────────────
// Database — PostgreSQL (Supabase)
// ─────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada. Use dotnet user-secrets set.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ─────────────────────────────
// MediatR + CQRS
// ─────────────────────────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("ChamadosCamarj.Application"));
    cfg.RegisterServicesFromAssembly(Assembly.Load("ChamadosCamarj.WebApi"));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
});
// ─────────────────────────────
// FluentValidation
// ─────────────────────────────
builder.Services.AddValidatorsFromAssembly(Assembly.Load("ChamadosCamarj.Application"));

// ─────────────────────────────
// Dependency Injection
// ─────────────────────────────
builder.Services.AddScoped<IChamadoRepository, ChamadoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IHistoricoRepository, HistoricoRepository>();
builder.Services.AddScoped<IUsuarioPerfilRepository, UsuarioPerfilRepository>();
builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher<UsuarioPerfil>, PasswordHasher<UsuarioPerfil>>();

builder.Services.AddScoped<IEmailSender>(_ =>
{
    var smtpEmail = builder.Configuration["Email:SmtpEmail"] ?? "suporte@camarj.com.br";
    var smtpSenha = builder.Configuration["Email:SmtpSenha"];
    if (string.IsNullOrWhiteSpace(smtpSenha))
        throw new InvalidOperationException("'Email:SmtpSenha' não configurada. Use dotnet user-secrets set.");
    return new SmtpEmailSender(smtpEmail, smtpSenha);
});

// ─────────────────────────────
// Supabase Storage (Anexos) — Fase 4, metade 1
// ─────────────────────────────
builder.Services.Configure<SupabaseSettings>(builder.Configuration.GetSection("Supabase"));
var supabaseSettings = builder.Configuration.GetSection("Supabase").Get<SupabaseSettings>() ?? new SupabaseSettings();
// Chave de serviço (Service Role Key, Supabase Dashboard > Settings > API) ainda pendente
// (usuário vai atrás em paralelo) — sem ela, a app sobe normalmente, só a feature de
// Anexos fica indisponível (mesma tolerância já aplicada ao GoogleClientId).
if (!string.IsNullOrWhiteSpace(supabaseSettings.Url) && !string.IsNullOrWhiteSpace(supabaseSettings.ServiceRoleKey))
{
    var supabaseClient = new Supabase.Client(supabaseSettings.Url, supabaseSettings.ServiceRoleKey);
    await supabaseClient.InitializeAsync();
    builder.Services.AddSingleton(supabaseClient);
    builder.Services.AddScoped<IStorageService, SupabaseStorageService>();
}
else
{
    // IStorageService precisa estar registrado sempre — o validador de DI do ASP.NET Core
    // (ValidateOnBuild, ativo em Development) derruba a aplicação inteira no Build() se um
    // Handler exige uma dependência não registrada, mesmo que ninguém chame o endpoint.
    builder.Services.AddScoped<IStorageService, NullStorageService>();
}

// ─────────────────────────────
// Autenticação — login real via Google Workspace (T09/F5b)
// ─────────────────────────────
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));
var authSettings = builder.Configuration.GetSection("Auth").Get<AuthSettings>() ?? new AuthSettings();
if (string.IsNullOrWhiteSpace(authSettings.JwtSigningKey))
    throw new InvalidOperationException("'Auth:JwtSigningKey' não configurada. Use dotnet user-secrets set.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Sem isso, o handler remapeia claims de nome curto (ex: "sub") para as URIs longas
        // de ClaimTypes por padrão — e ICurrentUserService.UsuarioId, que lê o claim "sub"
        // pelo nome curto (JwtRegisteredClaimNames.Sub), sempre caía no fallback Guid.Empty.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "ChamadosCamarj",
            ValidateAudience = true,
            ValidAudience = "ChamadosCamarj.Frontend",
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.JwtSigningKey)),
        };

        // SignalR não manda o header Authorization em conexões WebSocket — o token
        // vem via query string na negociação/conexão do hub, então precisa ser lido
        // manualmente daqui pros hubs (única exceção a receber o token fora do header).
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ─────────────────────────────
// OpenAPI (nativo .NET 10)
// ─────────────────────────────
builder.Services.AddOpenApi();

// ─────────────────────────────
// Controllers + JSON (enums como string)
// ─────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ─────────────────────────────
// SignalR — notificações em tempo real
builder.Services.AddSignalR();

// CORS (React dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "https://chamadoscamarj.pages.dev")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ─────────────────────────────
// Middleware Pipeline
// ─────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Doc da API (só em dev) — não deve exigir token, senão ninguém consegue nem
    // ver os endpoints disponíveis antes de já ter um JWT.
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChamadosHub>("/hubs/chamados");

// ─────────────────────────────
// Migrations automáticas + Seed
// ─────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
}

app.Run();
