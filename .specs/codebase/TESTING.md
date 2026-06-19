# Estratégia de Testes

## Status atual

**Sem testes implementados.** O diretório `tests/` mencionado no README não existe.

## O que testar (prioridade)

### 1. Domain (unitários — sem infraestrutura)

Lógica de negócio pura das entidades:

- `Chamado.CalcularDataLimite()` — SLA por prioridade
- Transições de estado: Abrir → Atribuir → Resolver → Fechar → Reabrir
- Transições inválidas (ex: Fechar um Cancelado)
- `Chamado.Cancelar()` só de Aberto/EmAndamento
- Validações de construtor (ArgumentException)

### 2. Application Handlers (integração leve — mock do repositório)

- `AbrirChamadoCommandHandler` — cria chamado e persiste
- `ListarChamadosQueryHandler` — filtros e paginação
- `AtribuirChamadoCommandHandler` — muda status para EmAndamento
- `ResolverChamadoCommandHandler` — seta DataConclusao

### 3. Validators (unitários)

- `AbrirChamadoCommandValidator` — campos obrigatórios, email válido
- `AtualizarChamadoCommandValidator`

## Estrutura recomendada

```
tests/
└── ChamadosCamarj.UnitTests/
    ├── Domain/
    │   ├── ChamadoTests.cs
    │   └── CategoriaTests.cs
    ├── Application/
    │   ├── AbrirChamadoHandlerTests.cs
    │   └── ListarChamadosHandlerTests.cs
    └── Validators/
        └── AbrirChamadoValidatorTests.cs
```

## Ferramentas recomendadas

- **xUnit** — framework de testes
- **Moq** — mock de repositórios
- **FluentAssertions** — assertions legíveis

## Gate checks (para futura CI)

```bash
dotnet test --no-build --verbosity normal
```
