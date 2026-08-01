# Estratégia de Testes

## Status atual

**Backend:** 215 testes de backend passando (`tests/ChamadosCamarj.UnitTests/`) — Domain, Application Handlers (auth, usuarios, chamados, anexos, relatorios), Validators.

**Frontend:** 12 testes E2E com Playwright (`frontend/e2e/`):
- `login.spec.ts` — sucesso e credenciais inválidas
- `chamados.spec.ts` — abrir chamado, listar, arquivo
- `admin.spec.ts` — usuários, categorias, grupos
- `dashboard.spec.ts` — dashboard, fila, relatório mensal
- `fluxo-completo.spec.ts` — login → abrir → comentar → listar

`npm run build` (TS + Vite) é o gate check de tipo/import.

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
- Auth: `LoginCommandValidator`, `EsqueciSenhaCommandValidator`, `ResetarSenhaCommandValidator`
- Usuarios: `CriarUsuarioCommandValidator`, `AtualizarUsuarioCommandValidator`

## Frontend — cobertura atual

- **E2E (Playwright):** Testes de fluxo completo em `frontend/e2e/`, headless, `npm run test:e2e`
- **Sem testes unitários/componente:** decisão do usuário — verificação manual no navegador + E2E cobrem o necessário
- **Gate check:** `npm run build` (TS + Vite build) pega erros de tipo/import antes de qualquer coisa rodar

## Gate checks

```bash
# Backend
dotnet test --no-build --verbosity normal

# Frontend (dentro de frontend/)
npm run build              # fe-build — TS + Vite
npm run dev                # fe-dev-boot — sobe sem crash
npm run test:e2e           # fe-e2e — precisa da API (dotnet run) e do dev server rodando
```
