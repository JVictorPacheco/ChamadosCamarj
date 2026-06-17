using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ChamadosCamarj.Application.Common.Behaviours;
using ChamadosCamarj.Domain.Entities;
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
// OpenAPI (nativo .NET 10)
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
    app.MapScalarApiReference();
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

    if (!db.Categorias.Any())
    {
        db.Categorias.AddRange(
            new Categoria("Autorização", "Pedidos de autorização"),
            new Categoria("Atendimento", "Atendimento geral"),
            new Categoria("Super e Tendência", "Assuntos de supervisão e tendências"),
            new Categoria("Reembolso", "Solicitações de reembolso"),
            new Categoria("Financeiro", "Assuntos financeiros")
        );
        db.SaveChanges();
    }
}

app.Run();
