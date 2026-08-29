# [NOME DA FEATURE] — Design Técnico

> **Quando este arquivo é obrigatório:**
> - Feature toca mais de uma camada (backend + frontend)
> - Introduz nova entidade ou tabela no banco
> - Altera contrato de interface existente (remove/renomeia método público)
> - Tem ambiguidade técnica que exige decisão antes de implementar
>
> Para features simples (só backend ou só frontend, sem novo schema), as decisões podem ficar na `spec.md`.

---

## 1. Visão Geral da Solução

> Em 3-5 linhas: qual é a abordagem técnica escolhida e por quê.

---

## 2. Mudanças no Domain

> Novas entidades, enums, interfaces ou alterações em existentes.

### Novas Entidades

```csharp
// Exemplo
public class NomeEntidade : BaseEntity
{
    // propriedades
}
```

### Novos Enums

```csharp
public enum NomeEnum { Valor1, Valor2 }
```

### Mudanças em Interfaces

> ⚠️ Se remover ou renomear métodos existentes, sinalize aqui e confirme com o usuário antes de implementar.

| Interface | Mudança | Impacto |
|-----------|---------|---------|
| `INomeRepository` | Adicionar `MetodoAsync` | Nenhum consumidor existente |
| `INomeService` | Remover `MetodoX` | Usado em HandlerA e HandlerB — confirmar antes |

---

## 3. Mudanças no Application (CQRS)

### Novos Commands / Queries

```csharp
public record NomeCommand(/* params */) : IRequest<NomeResponse>;
public record NomeQuery(/* params */) : IRequest<NomeResponse?>;
```

### Novos DTOs

```csharp
public record NomeResponse(/* props */);
```

### Validadores

```csharp
public class NomeCommandValidator : AbstractValidator<NomeCommand>
{
    // regras
}
```

---

## 4. Mudanças no Infrastructure

### Migrations

> Liste as migrations necessárias. Se alterar tabelas existentes, descreva o impacto nos dados.

| Migration | Tipo | Reversível? | Impacto em dados existentes |
|-----------|------|------------|----------------------------|
| `AddNomeCampo` | AddColumn | Sim | Nenhum (nullable) |
| `RenomearTabela` | RenameTable | Sim | Nenhum |

### Mudanças em Repositórios

| Repositório | Método adicionado | Assinatura |
|-------------|------------------|------------|
| `INomeRepository` | `MetodoAsync` | `Task<Tipo> MetodoAsync(params, CancellationToken ct)` |

---

## 5. Mudanças no WebApi

### Novos Endpoints

| Método | Rota | Auth | Body | Resposta |
|--------|------|------|------|----------|
| `POST` | `/api/recurso` | JWT | `NomeRequest` | `201 NomeResponse` |
| `GET` | `/api/recurso/{id}` | JWT | — | `200 NomeResponse` \| `404` |
| `PATCH` | `/api/recurso/{id}/acao` | JWT (Admin) | — | `204` |

---

## 6. Mudanças no Frontend

### Novas Páginas / Componentes

| Arquivo | Tipo | Localização |
|---------|------|-------------|
| `NomePage.tsx` | Página | `features/nome-feature/` |
| `NomeForm.tsx` | Componente | `features/nome-feature/components/` |
| `useNome.ts` | Hook (TanStack Query) | `features/nome-feature/hooks/` |
| `api.ts` | Funções HTTP | `features/nome-feature/` |

### Novos Tipos

```ts
// types/api.ts
export interface NomeResponse {
  id: string
  // ...
}
```

### Mudanças em Rotas

```tsx
// App.tsx — adicionar dentro de ProtectedRoute
<Route path="/nova-rota" element={<NomePage />} />
```

---

## 7. Decisões e Alternativas Consideradas

| Decisão | Opção A (escolhida) | Opção B (descartada) | Motivo |
|---------|--------------------|--------------------|--------|
| Exemplo | Usar X | Usar Y | Motivo Z |

---

## 8. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|--------------|---------|-----------|
| Exemplo de risco | Média | Alto | Estratégia de mitigação |

---

## 9. Perguntas em Aberto

> Liste perguntas que precisam de resposta antes ou durante a implementação.
> Ao responder, mover a resposta para "Decisões" e remover daqui.

- [ ] Pergunta X — aguardando confirmação do usuário
- [ ] Pergunta Y — aguardando definição de negócio
