# Estratégia de Testes

## Status atual (pós-Fase 3)

**Backend:** 59 testes unitários passando (`tests/ChamadosCamarj.UnitTests/`) — Domain, Application Handlers, Validators.

**Frontend:** 1 teste E2E (Playwright, `frontend/e2e/fluxo-completo.spec.ts`) cobrindo o fluxo feliz completo do Solicitante (login mock → abrir chamado → detalhe → comentar → listar → click no card → detalhe). Decisão do usuário: sem testes unitários/componente isolados nesta fase — só o E2E do happy path + verificação manual no navegador pra UI/visual.

## Backend — cobertura atual

### 1. Domain (unitários — sem infraestrutura)

- `Chamado.CalcularDataLimite()` — SLA por prioridade
- Transições de estado: Abrir → Atribuir → Resolver → Fechar → Reabrir
- Transições inválidas (ex: Fechar um Cancelado)
- `Chamado.Cancelar()` só de Aberto/EmAndamento
- Validações de construtor (ArgumentException)

### 2. Application Handlers (integração leve — mock do repositório)

- `AbrirChamadoCommandHandler`, `ComentarChamadoCommandHandler`, `AtribuirChamadoCommandHandler`, `ResolverFecharCancelarChamadoCommandHandler`
- `ListarChamadosQueryHandler` — filtros (incl. `solicitanteEmail`, API-02) e paginação
- `ObterChamadoPorIdQueryHandler`, `ListarComentariosQueryHandler` (API-01)

### 3. Validators (unitários)

- `AbrirChamadoCommandValidator` — campos obrigatórios, email válido

## Frontend — cobertura atual

- **E2E (Playwright):** 1 teste, fluxo feliz completo, headless, `npm run test:e2e`
- **Sem testes unitários/componente:** decisão do usuário — verificação manual no navegador + o E2E cobrem o necessário nesta fase
- **Gate check de cada task:** `npm run build` (TS + Vite build) pega erros de tipo/import antes de qualquer coisa rodar

## Gate checks

```bash
# Backend
dotnet test --no-build --verbosity normal

# Frontend (dentro de frontend/)
npm run build              # fe-build — TS + Vite
npm run dev                # fe-dev-boot — sobe sem crash
npm run test:e2e           # fe-e2e — precisa da API (dotnet run) e do dev server rodando
```
