using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Application.Common.Behaviours;
using ChamadosCamarj.Application.Features.Chamados.Commands;
using ChamadosCamarj.Application.Features.Chamados.Queries;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;
using ChamadosCamarj.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────
// Database (SQLite)
// ─────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ─────────────────────────────
// MediatR + CQRS
// ─────────────────────────────
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<AbrirChamadoCommand>();
    cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
});

// ─────────────────────────────
// FluentValidation
// ─────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<AbrirChamadoCommand>();

// ─────────────────────────────
// Dependency Injection
// ─────────────────────────────
builder.Services.AddScoped<IChamadoRepository, ChamadoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();

// ─────────────────────────────
// Controllers + OpenAPI (nativo)
// ─────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ─────────────────────────────
// CORS (para o React dev)
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
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.MapControllers();

// ─────────────────────────────
// Database initialization + Seed
// ─────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    await DatabaseSeeder.SeedAsync(db);
}

app.Run();
