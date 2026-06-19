# Convenções de Código

## Linguagem e nomenclatura

- **Idioma do código:** Português (nomes de domínio, métodos, propriedades)
- **Idioma de commits/docs:** Português
- **Namespaces:** `ChamadosCamarj.<Camada>.<Subpasta>`
- **File-scoped namespaces:** sim (`namespace X;`)
- **Implicit usings:** sim

## Entidades (Domain)

- Herdam de `BaseEntity` (Id Guid, DataCriacao, DataAtualizacao)
- Construtor privado sem parâmetros para EF Core: `private Entidade() { }`
- Construtor público com validação inline via `ArgumentException`
- Propriedades com `private set` — sem setters públicos
- Métodos de negócio na própria entidade (rich domain model): `Atribuir()`, `Resolver()`, `Fechar()`, etc.
- Coleções inicializadas com collection expression: `= []`

## Commands e Queries (Application)

- Usam `record` com parâmetros posicionais
- Commands retornam `ChamadoResponse` ou `IRequest<Unit>` (não implementado ainda)
- Queries retornam coleções ou nullable DTO
- Pattern: `NomeAcaoCommand.cs` + `NomeAcaoCommandHandler.cs` no mesmo diretório

## DTOs

- Usam `record` posicional
- Sufixo `Response` para saída, `Request` para entrada via body, `Command` para MediatR requests

## Validators (FluentValidation)

- Classe separada: `NomeCommandValidator.cs`
- Injetada automaticamente pelo pipeline `ValidationBehaviour`
- Apenas `AbrirChamadoCommand` e `AtualizarChamadoCommand` têm validators

## Controllers

- Herdam `ControllerBase` + `[ApiController]` + `[Route("api/[controller]")]`
- `[Produces("application/json")]`
- Usam `IMediator` para dispatchar — **exceto** `CategoriasController` que injeta repo direto
- `CancellationToken` em todos os endpoints
- Retornam `ActionResult<T>` tipado com `[ProducesResponseType]`

## Mapeamento

- Extension methods em `ChamadoMappings.cs`: `chamado.ToResponse()`
- Sem AutoMapper — mapeamento manual explícito

## Injeção de Dependência

- Registros em `Program.cs`
- Escopo padrão: `AddScoped`

## JSON

- Enums como string via `JsonStringEnumConverter`

## EF Core

- `ApplyConfigurationsFromAssembly` para carregar configs Fluent API
- Queries com `AsNoTracking()` e `Include()` explícito
- `db.Database.MigrateAsync()` na inicialização (dev e prod) — sem `EnsureCreated()`
