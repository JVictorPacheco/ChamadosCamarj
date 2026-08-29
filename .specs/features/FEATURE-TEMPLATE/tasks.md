# [NOME DA FEATURE] — Tasks

> **Branch:** `feature/{nome-da-feature}`
> **Spec:** `spec.md` | **Design:** `design.md`
> **Gate checks:** `dotnet test` + `npm run build` antes de commitar

---

## Backend

### Domain
- [ ] Criar entidade `NomeEntidade` em `Domain/Entities/`
- [ ] Criar enum `NomeEnum` em `Domain/Enums/`
- [ ] Adicionar interface `MetodoAsync` em `Domain/Interfaces/INomeRepository`

### Application
- [ ] Criar `NomeCommand.cs` + `NomeCommandHandler.cs` + `NomeCommandValidator.cs`
- [ ] Criar `NomeQuery.cs` + `NomeQueryHandler.cs`
- [ ] Criar `NomeResponse.cs` em `Application/Features/Nome/DTOs/`
- [ ] Criar `NomeMappings.cs` (extension method `ToResponse()`)

### Infrastructure
- [ ] Implementar `NomeRepository.cs`
- [ ] Criar migration `AddNomeFeature`
- [ ] Registrar `INomeRepository` → `NomeRepository` em `Program.cs`

### WebApi
- [ ] Criar `NomeController.cs` com endpoints definidos em `design.md`

### Testes
- [ ] Criar `NomeCommandHandlerTests.cs` cobrindo ACs do `spec.md`
- [ ] Criar `NomeQueryHandlerTests.cs`
- [ ] Criar `NomeValidatorTests.cs`

---

## Frontend

### API
- [ ] Criar funções em `features/nome-feature/api.ts`

### Tipos
- [ ] Adicionar `NomeResponse` em `types/api.ts`

### Hooks
- [ ] Criar `useNome.ts` (query) em `features/nome-feature/hooks/`
- [ ] Criar `useAcoesNome.ts` (mutations) em `features/nome-feature/hooks/`

### Componentes
- [ ] Criar `NomePage.tsx`
- [ ] Criar componentes de suporte em `features/nome-feature/components/`

### Rotas
- [ ] Adicionar rota em `App.tsx`
- [ ] Adicionar link na sidebar (se aplicável)

---

## Documentação (obrigatório — última etapa)

- [ ] Atualizar `spec.md`: marcar ACs concluídos na tabela de rastreabilidade
- [ ] Atualizar `spec.md`: mudar status para `Concluída`
- [ ] Atualizar `tasks.md`: marcar todos os checkboxes
- [ ] Atualizar `.specs/project/STATE.md`: resumo da sessão + decisões tomadas
- [ ] Atualizar `.specs/project/ROADMAP.md`: marcar feature como ✅ se aplicável

---

## Gate Checks Finais

- [ ] `dotnet test` — X testes, 0 falhas
- [ ] `npm run build` — 0 erros, 0 warnings
- [ ] PR aberto com base `develop`
- [ ] PR revisado e mergeado em `develop`
