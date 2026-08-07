# Guia Prático: Orquestração de IA + SDD no ChamadosCamarj

> Como usar os 5 agentes de IA com Spec-Driven Development, do planejamento ao deploy, garantindo que nada se perca.

---

## Os 5 agentes e seus papéis

| Agente | Modelo | Função | Permissões |
|---|---|---|---|
| `@spec` | Kimi K3 | Criar specs, planejamento, design | Edita arquivos `.specs/`, sem bash |
| `@build-backend` | GLM-5.2 | Implementar C#/.NET/CQRS/EF Core | Edita e executa comandos |
| `@build-frontend` | Kimi K2.7 Code | Implementar React/TS/Tailwind/shadcn | Edita e executa comandos |
| `@review` | Grok 4.5 | Revisar código, segurança, boas práticas | Só lê, não edita nem executa |
| `@explorar` | DeepSeek V4 Flash | Explorar código, buscar referências | Só lê, pode executar comandos |

---

## Fluxo completo: da ideia ao merge

```
Nova demanda
    │
    ▼
[1] @spec ───► Cria spec.md + tasks.md em .specs/features/nome-da-feature/
    │          SALVAR: os arquivos são criados automaticamente no disco
    │
    ▼
[2] @build-backend ───► Implementa parte do backend
    │                   SALVAR: pedir "commite as alterações" ou "salve em arquivos"
    │
    ▼
[3] @build-frontend ───► Implementa parte do frontend
    │                     SALVAR: pedir "commite as alterações"
    │
    ▼
[4] @review ───► Revisa tudo o que foi feito
    │            SALVAR: pedir "salve esta revisão em .specs/features/nome/review.md"
    │
    ▼
[5] Gate checks ───► dotnet test + npm run build
    │
    ▼
[6] Commit final + merge em develop
```

---

## Passo a passo detalhado

### Passo 0 — Antes de começar

```
git checkout develop
git pull
git checkout -b feature/nome-da-feature
```

**Regra:** SEMPRE criar branch nova a partir de `develop`. Nunca commitar direto em `main`.

---

### Passo 1 — `@spec` (Kimi K3)

**O que faz:** Especifica a feature seguindo SDD (problem statement, user stories, acceptance criteria, tasks).

**Como usar:**
```
@spec Crie a spec para [descreva a demanda].
O projeto está em C:\Users\jpacheco.CAMARJ.001\Projects\ChamadosCamarj.
Salve em .specs/features/nome-da-feature/spec.md e .specs/features/nome-da-feature/tasks.md.
Leia o STATE.md, CONVENTIONS.md e o código relevante antes de começar.
```

**O que deve ser salvo:**
- `.specs/features/nome-da-feature/spec.md` — especificação
- `.specs/features/nome-da-feature/tasks.md` — lista de tarefas
- `.specs/features/nome-da-feature/design.md` — (opcional, se a feature for complexa)

**Checklist de saída:**
- [ ] Os 3 arquivos existem no disco
- [ ] `spec.md` tem ACs (acceptance criteria) claros
- [ ] `tasks.md` tem tarefas numeradas e rastreáveis ao código
- [ ] Nenhuma pergunta de clarificação ficou sem resposta

---

### Passo 2 — `@build-backend` (GLM-5.2)

**O que faz:** Implementa código C#/.NET seguindo Clean Architecture + CQRS.

**Como usar:**
```
@build-backend Implemente as tarefas de backend da spec em
.specs/features/nome-da-feature/tasks.md.
Projeto em C:\Users\jpacheco.CAMARJ.001\Projects\ChamadosCamarj.
Branch: feature/nome-da-feature.
Leia o CONVENTIONS.md e o STRUCTURE.md em .specs/codebase/.
Ao terminar, COMMITE as alterações.
```

**IMPORTANTE:** Sempre pedir para commitar ao final. Não confiar que o agente vai lembrar.

**Checklist de saída:**
- [ ] `dotnet build` passa
- [ ] `dotnet test` passa (215+ testes)
- [ ] Migration criada (se adicionou entidade/campo)
- [ ] Commit feito com mensagem descritiva

---

### Passo 3 — `@build-frontend` (Kimi K2.7 Code)

**O que faz:** Implementa React/TypeScript/Tailwind/shadcn.

**Como usar:**
```
@build-frontend Implemente as tarefas de frontend da spec em
.specs/features/nome-da-feature/tasks.md.
Projeto em C:\Users\jpacheco.CAMARJ.001\Projects\ChamadosCamarj.
Branch: feature/nome-da-feature.
Leia o CONVENTIONS.md em .specs/codebase/.
Use o MCP shadcn para componentes quando precisar.
Ao terminar, COMMITE as alterações.
```

**Checklist de saída:**
- [ ] `npm run build` passa (0 erros TypeScript)
- [ ] Componentes seguem o padrão: `export` nomeado, `isPending` (não `isLoading`)
- [ ] Cores usam tokens CSS, não hex hardcoded
- [ ] Erros exibidos inline com `<Alert variant="destructive">`
- [ ] Commit feito

---

### Passo 4 — `@review` (Grok 4.5)

**O que faz:** Revisão de código — segurança, correção, convenções, edge cases.

**Como usar:**
```
@review Revise todas as mudanças da feature [nome] na branch [branch].
Projeto em C:\Users\jpacheco.CAMARJ.001\Projects\ChamadosCamarj.
Compare com a spec em .specs/features/nome-da-feature/spec.md.
Verifique contra o CONVENTIONS.md em .specs/codebase/.
Ao final, SALVE a revisão em .specs/features/nome-da-feature/review.md.
```

**O que o review cobre:**
- Segurança (injeção, auth, RBAC)
- Correção (a lógica faz o que a spec pede?)
- Edge cases (nulo, vazio, erro de rede)
- Convenções (segue CONVENTIONS.md?)
- Performance (N+1, queries desnecessárias)

**Checklist de saída:**
- [ ] `review.md` salvo no disco
- [ ] Zero itens bloqueantes (se houver, corrigir antes de prosseguir)
- [ ] Itens de atenção documentados (podem ser tratados depois)

---

### Passo 5 — Gate checks

```powershell
# No diretório do projeto
dotnet test tests/ChamadosCamarj.UnitTests/
cd frontend; npm run build
```

**Regra:** Ambos devem passar sem erros. Se o review encontrou algo, corrigir e rodar de novo.

---

### Passo 6 — Commit final e merge

```powershell
# Conferir o que vai ser commitado
git status
git diff --stat

# Commit (se ainda não foi feito pelo build-backend/build-frontend)
git add -A
git commit -m "feat: descricao clara da feature"

# Merge em develop
git checkout develop
git merge feature/nome-da-feature
```

---

## Resumo: o que salvar em cada etapa

| Etapa | Arquivos salvos | Quem salva |
|---|---|---|
| @spec | `spec.md`, `tasks.md`, `design.md` | O agente cria automaticamente |
| @build-backend | Código C# + commit | Pedir "commite" ao final |
| @build-frontend | Código TS/React + commit | Pedir "commite" ao final |
| @review | `review.md` | Pedir "salve a revisão em..." |
| Gate checks | Nada (só verificação) | Você confere o resultado |

---

## Dicas práticas

1. **Sempre peça pra salvar.** Os agentes não salvam a revisão automaticamente — você precisa pedir "salve em arquivo X".

2. **Commits atômicos.** Um commit por etapa (backend, frontend). Não juntar tudo num commit só.

3. **Use o MCP shadcn.** O `@build-frontend` pode consultar componentes shadcn/ui via MCP — mencione isso no prompt.

4. **Se o review achar algo, corrija antes do merge.** Não empurre débito pra depois se for bloqueante.

5. **Todas as abas compartilham o mesmo projeto.** Pode abrir `@spec` em uma aba, `@build-backend` em outra, etc. Cada uma é independente mas trabalha no mesmo código.

6. **Branch SEMPRE a partir de `develop`.** Conferir com `git branch` antes de começar.

---

## Exemplo real: dashboard-kanban-navegacao

```
[1] @spec → criou spec.md + tasks.md
[2] @build-frontend → implementou Dashboard clicável + Kanban + useSearchParams
[3] (backend foi feito manualmente — o ideal seria @build-backend)
[4] @review → encontrou 5 pontos de atenção
[5] Correções → branch feature/fix-review-dashboard-kanban
[6] @build-frontend → corrigiu 4 dos 5 pontos
[7] @review → aprovou todos os 5 fixes → salvou em review-fixes.md
[8] Gate checks → dotnet test ✅ + npm run build ✅
[9] Aguardando commit
```
