using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ChamadosCamarj.Application.Common.Behaviours;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;
using ChamadosCamarj.Infrastructure.Repositories;
using ChamadosCamarj.WebApi.Middleware;
using ChamadosCamarj.WebApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────
// Database (dev = SQLite automático, prod = PostgreSQL/Supabase)
// ─────────────────────────────
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=chamados.db"));
}
else
{
    builder.Configuration.AddUserSecrets<Program>();
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

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
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
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
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<ChamadosHub>("/hubs/chamados");

// ─────────────────────────────
// Migrations automáticas + Seed
// ─────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsDevelopment())
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
}

app.Run();
