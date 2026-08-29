# [NOME DA FEATURE] — Especificação

> **Status:** `Pendente` | `Em andamento` | `Concluída`
> **Branch:** `feature/{nome-da-feature}`
> **Criada em:** YYYY-MM-DD
> **Atualizada em:** YYYY-MM-DD

---

## 1. Problema

> Descreva o problema que esta feature resolve. Seja específico: quem é afetado, qual comportamento atual é inadequado, qual o impacto.

**Situação atual:**
<!-- O que acontece hoje que é problemático? -->

**Impacto:**
<!-- Quem sofre com isso e como? -->

**Solução esperada:**
<!-- Em uma frase: o que esta feature entrega? -->

---

## 2. Fora de Escopo

> Liste explicitamente o que esta spec **não** cobre. Isso evita scope creep e alinhamento falso.

- [ ] Exemplo: integração com sistema externo X (será tratada em spec separada)
- [ ] Exemplo: notificação por e-mail (depende da Fase 4 — IMAP)

---

## 3. User Stories

> Formato obrigatório: `Como [perfil], quero [ação], para que [benefício mensurável].`
> Perfis válidos: `Admin`, `Atendente`, `Solicitante`

| ID | User Story |
|----|------------|
| US-01 | Como **Admin**, quero [ação], para que [benefício]. |
| US-02 | Como **Atendente**, quero [ação], para que [benefício]. |
| US-03 | Como **Solicitante**, quero [ação], para que [benefício]. |

---

## 4. Critérios de Aceitação

> Formato obrigatório: numerados, testáveis, sem ambiguidade.
> Cada AC deve poder ser verificado com um teste automatizado ou passo manual reproduzível.

### US-01 — [título resumido]

- **AC-01:** Dado [contexto], quando [ação], então [resultado esperado e mensurável].
- **AC-02:** Dado [contexto], quando [ação], então [resultado esperado e mensurável].

### US-02 — [título resumido]

- **AC-03:** Dado [contexto], quando [ação], então [resultado esperado e mensurável].

### Critérios Transversais

- **AC-XX:** Todos os endpoints novos exigem autenticação JWT válida.
- **AC-XX:** Erros retornam no formato `{ message: "..." }` em português.
- **AC-XX:** `dotnet test` passa sem falhas após a implementação.
- **AC-XX:** `npm run build` passa sem erros ou warnings após a implementação.

---

## 5. Rastreabilidade

> Mapeie cada AC para o arquivo de teste que o verifica. Preencher após implementação.

| Critério | Arquivo de Teste | Método de Teste | Status |
|----------|-----------------|-----------------|--------|
| AC-01 | `tests/.../NomeDaFeatureTests.cs` | `MetodoQueTestIsso` | ⬜ Pendente |
| AC-02 | `tests/.../NomeDaFeatureTests.cs` | `MetodoQueTestIsso` | ⬜ Pendente |
| AC-03 | Manual (UI) | Passo: ... | ⬜ Pendente |

---

## 6. Decisões Técnicas

> Para features simples que não exigem `design.md`, registre aqui as decisões relevantes.
> Para features complexas (múltiplas camadas, nova entidade, mudança de contrato), use `design.md`.

| Decisão | Alternativa considerada | Motivo da escolha |
|---------|------------------------|-------------------|
| Exemplo | Alternativa X | Motivo Y |

---

## 7. Dependências

> Liste outras features ou condições externas que esta spec depende.

- Depende de: [nome da feature ou serviço externo]
- Bloqueia: [nome da feature que só pode começar após esta]

---

## 8. Gate Checks

> A ser preenchido ao concluir a implementação.

- [ ] `dotnet test` — X testes, 0 falhas
- [ ] `npm run build` — 0 erros, 0 warnings
- [ ] ACs verificados manualmente ou por testes automatizados
- [ ] `spec.md` atualizada com status final
- [ ] `STATE.md` atualizado com resumo da sessão
- [ ] PR aberto com base `develop`
