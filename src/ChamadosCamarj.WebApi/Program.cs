using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ChamadosCamarj.Application.Common.Behaviours;
using ChamadosCamarj.Domain.Interfaces;
using ChamadosCamarj.Infrastructure.Data;
using ChamadosCamarj.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────
// Database
// ─────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

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
// Controllers + Swagger
// ─────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ChamadosCamarj API", Version = "v1" });
});

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.MapControllers();

// ─────────────────────────────
// Auto-migration (dev)
// ─────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
