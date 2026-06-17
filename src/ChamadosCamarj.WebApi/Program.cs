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
// Controllers + JSON (enums como string)
// ─────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

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
            new Categoria("Autorização", "Pedidos de autorização") { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567891") },
            new Categoria("Atendimento", "Atendimento geral") { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567892") },
            new Categoria("Super e Tendência", "Assuntos de supervisão e tendências") { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567893") },
            new Categoria("Reembolso", "Solicitações de reembolso") { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567894") },
            new Categoria("Financeiro", "Assuntos financeiros") { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567895") }
        );
        db.SaveChanges();
    }
}

app.Run();
