# Convenções de Código

## Processo (Spec-Driven Development)

> **Ver `.specs/project/STATE.md`, seção "🧭 Regras de Processo (Constitution)" — 3 regras permanentes sobre como conduzir o ciclo Specify → Design → Tasks → Execute neste projeto (perguntas de clarificação sem resposta, spec antes do código, mudança de contrato sinalizada antes de aplicar). Revisar antes de iniciar qualquer feature nova ou extensão de feature existente.**

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

---

## Frontend (React)

- **Idioma:** nomes de domínio, props e variáveis em português (`chamadoId`, `perfilUsuario`, `reatribuirChamado`); nomes técnicos/genéricos do React em inglês (`onSuccess`, `isPending`)
- **Organização: feature folders, não agrupamento por tipo de arquivo.** Cada domínio vive em `frontend/src/features/<dominio>/`, com `api.ts`, `hooks/`, `components/` e as páginas (`*Page.tsx`) dentro da própria pasta da feature. **Nunca** criar um `components/Chamados/` genérico separado nem um `services/` paralelo — isso já causou um bug real (4 componentes da Fase 6 escritos fora do projeto Vite, em `src/ChamadosCamarj.Web/`, usando convenções que não existem aqui)
- `frontend/src/components/ui/`: **somente** primitivas shadcn/radix instaladas via CLI (`npx shadcn add <componente>`). Não editar à mão nem criar substitutos custom aqui
- `frontend/src/lib/api.ts`: única fonte de acesso HTTP — `apiFetch<T>()` (fetch nativo) + classe `ApiError`. **Não usar axios** nem criar um cliente HTTP alternativo
- `frontend/src/features/<dominio>/api.ts`: funções de acesso à API específicas do domínio, todas chamando `apiFetch` — uma função por endpoint, tipos `Request`/`Response` importados de `types/api.ts`
- `frontend/src/features/<dominio>/hooks/`: um hook por ação/query (`useComentar.ts`, `useAtribuirChamado.ts`), usando `@tanstack/react-query`. Mutations invalidam as queries relacionadas (`chamado`, `chamados`, `historico`) no `onSuccess`
- `frontend/src/types/api.ts`: única fonte dos tipos que espelham os DTOs do backend (`ChamadoResponse`, `ComentarioResponse`, etc.) — conferir contra os DTOs reais (`Application/Features/*/DTOs/`), não assumir
- **Feedback de erro: inline, sem biblioteca de toast.** Padrão: `useState<string | null>` pro erro + `<p className="text-sm text-destructive">`. Não existe `useToast`/Sonner instalado neste projeto
- **Tema:** dark mode único (forçado via `<html class="dark">`), paleta customizada Camarj em `index.css`. Nunca usar cores claras hardcoded (`bg-white`, `text-gray-900`, `bg-gray-50` etc.) — usar os tokens do tema (`bg-popover`, `text-foreground`, `text-muted-foreground`, `border-border`)
- `frontend/src/auth/AuthContext.tsx`: fonte única dos perfis mockados (Admin/Atendente/Solicitante) até a Fase 6 implementar login Google real — reutilizar essa fonte em vez de hardcodar listas de usuários em outros componentes
