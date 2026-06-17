using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ChamadosCamarj.Application.Common.Behaviours;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;
using ChamadosCamarj.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────
// Database — SQLite
// ─────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=chamadoscamarj.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// ─────────────────────────────
// MediatR + CQRS
// ─────────────────────────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("ChamadosCamarj.Application"));
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
// OpenAPI (built-in .NET 10)
// ─────────────────────────────
builder.Services.AddOpenApi();

// ─────────────────────────────
// Controllers
// ─────────────────────────────
builder.Services.AddControllers();

// ─────────────────────────────
// CORS (React dev)
// ─────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ─────────────────────────────
// Middleware Pipeline
// ─────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ChamadosCamarj API")
               .WithTheme(ScalarTheme.Purple);
    });
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.MapControllers();

// ─────────────────────────────
// Auto-migration + Seed categorias
// ─────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    // Seed categorias se estiver vazio
    if (!db.Categorias.Any())
    {
        db.Categorias.AddRange(
            new ChamadosCamarj.Domain.Entities.Categoria("Autorização", "Pedidos de autorização"),
            new ChamadosCamarj.Domain.Entities.Categoria("Atendimento", "Atendimento geral"),
            new ChamadosCamarj.Domain.Entities.Categoria("Super e Tendência", "Assuntos de supervisão e tendências"),
            new ChamadosCamarj.Domain.Entities.Categoria("Reembolso", "Solicitações de reembolso"),
            new ChamadosCamarj.Domain.Entities.Categoria("Financeiro", "Assuntos financeiros")
        );
        db.SaveChanges();
    }
}

app.Run();
