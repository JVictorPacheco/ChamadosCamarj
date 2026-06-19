# Como testar os 3 fixes de hoje

Base URL: `http://localhost:5000/api`

---

## Fix 1 — Categoria inexistente ao abrir chamado

Antes: dava erro 500 feio (violação de FK no banco).
Agora: `404` limpo, sem nem chegar no banco.

**POST `/chamados`**
```json
{
  "titulo": "Teste categoria inexistente",
  "descricao": "Validando o fix",
  "solicitanteNome": "Maria Souza",
  "solicitanteEmail": "maria.souza@camarj.com.br",
  "categoriaId": "12345678-1234-1234-1234-123456789012",
  "prioridade": "Media"
}
```
**Esperado:** `404` — `{"message":"Categoria com o id '...' não foi encontrado."}`

---

## Fix 2 — Transições de status inválidas

Antes: qualquer transição era aceita (ex: resolver um chamado cancelado).
Agora: `400` limpo quando a transição não faz sentido.

1. Abra um chamado normal (categoria válida, ex: `a1b2c3d4-e5f6-7890-abcd-ef1234567891` = Autorização) e guarde o `id` da resposta.
2. **PATCH `/chamados/{id}/cancelar`** → `204`
3. **PATCH `/chamados/{id}/resolver`** (sem body)
   **Esperado:** `400` — `{"message":"Não é possível resolver um chamado com status 'Cancelado'."}`
4. **PATCH `/chamados/{id}/atribuir`**
   ```json
   { "responsavelId": "11111111-1111-1111-1111-111111111111", "responsavelNome": "Victor" }
   ```
   **Esperado:** `400` — mesma lógica, chamado já cancelado.

Outras transições bloqueadas que também valem testar:
- **Fechar** um chamado que nunca foi resolvido → `400` ("Só é possível fechar um chamado que já foi resolvido.")
- **Cancelar** um chamado já fechado → `400`
- **Reabrir** um chamado que já está Aberto → `400`

---

## Fix 3 — Respostas de erro limpas (middleware global)

Esse é "de brinde": qualquer erro de validação do FluentValidation também passou a vir limpo, em vez de 500.

**POST `/chamados`** com título vazio:
```json
{
  "titulo": "",
  "descricao": "Teste",
  "solicitanteNome": "X",
  "solicitanteEmail": "x@camarj.com.br",
  "categoriaId": "a1b2c3d4-e5f6-7890-abcd-ef1234567891"
}
```
**Esperado:** `400` — `{"errors":[{"campo":"Titulo","erro":"Título é obrigatório."}]}`

---

## Fluxo de vida completo do chamado (caminho feliz)

```
Aberto ──Atribuir──▶ EmAndamento ──Resolver──▶ Resolvido ──Fechar──▶ Fechado
  └──────────────────────Cancelar (de Aberto ou EmAndamento)─────────▶ Cancelado
```

`Atribuir` é opcional — dá pra ir direto de `Aberto` pra `Resolvido` sem atribuir. `Resolver` é **obrigatório** antes de `Fechar`. `Cancelar` é o atalho pra encerrar sem precisar resolver nada.

**1. Atribuir** (Aberto → EmAndamento):
```
PATCH /chamados/{id}/atribuir
{ "responsavelId": "11111111-1111-1111-1111-111111111111", "responsavelNome": "Victor" }
```

**2. Resolver** (→ Resolvido, sem body):
```
PATCH /chamados/{id}/resolver
```

**3. Fechar** (Resolvido → Fechado, sem body):
```
PATCH /chamados/{id}/fechar
```

**Cancelar** (de Aberto ou EmAndamento, sem body):
```
PATCH /chamados/{id}/cancelar
```

---

> Categorias válidas para teste: `GET /api/categorias` (Autorização, Atendimento, Super e Tendência, Reembolso, Financeiro — IDs fixos).
> Lembre de cancelar/limpar os chamados de teste depois, já que dev e prod compartilham o mesmo banco Supabase.
