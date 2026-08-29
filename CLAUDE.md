# CLAUDE.md — ChamadosCamarj

> Este arquivo é carregado automaticamente em toda sessão de IA (Claude Code, openCode, etc.).
> Leia **antes** de qualquer ação. As regras aqui são permanentes e se sobrepõem a instruções ad hoc.

---

## 1. Contexto do Projeto

Sistema interno de gestão de chamados corporativos da **CAMARJ** (Câmara de Arbitragem).

| Item | Detalhe |
|------|---------|
| **Stack backend** | .NET 9 + Clean Architecture + CQRS (MediatR) + PostgreSQL (Supabase) |
| **Stack frontend** | React 19 + TypeScript + Vite + TailwindCSS v4 + Shadcn/ui |
| **Auth** | Email + senha com `PasswordHasher` (ASP.NET Core Identity) |
| **Real-time** | SignalR |
| **Storage** | Supabase Storage (anexos) |
| **Deploy** | Frontend → Cloudflare Pages | Backend → Azure App Service F1 |
| **Prod URL** | `https://chamados.okurumin.com.br` |

---

## 2. Onde está tudo — leia antes de agir

| O que procurar | Onde está |
|----------------|-----------|
| Estado atual, sessões, decisões | `.specs/project/STATE.md` |
| Roadmap de fases | `.specs/project/ROADMAP.md` |
| Visão geral do projeto | `.specs/project/PROJECT.md` |
| Convenções de código | `.specs/codebase/CONVENTIONS.md` |
| Arquitetura | `.specs/codebase/ARCHITECTURE.md` |
| Stack detalhada | `.specs/codebase/STACK.md` |
| Estrutura de diretórios | `.specs/codebase/STRUCTURE.md` |
| Débito técnico | `.specs/codebase/CONCERNS.md` |
| Estratégia de testes | `.specs/codebase/TESTING.md` |
| Specs de features | `.specs/features/{nome-feature}/` |
| Template de nova feature | `.specs/features/FEATURE-TEMPLATE/` |
| Guia de orquestração IA | `docs/GUIA-ORQUESTRACAO-SDD.md` |

**Regra:** leia `.specs/project/STATE.md` antes de retomar qualquer trabalho. É a memória do projeto.

---

## 3. Gitflow — sempre respeitar

```
main       → produção (nunca commitar direto)
develop    → integração (base para feature branches)
feature/*  → nova funcionalidade (a partir de develop)
fix/*      → correção de bug (a partir de develop)
hotfix/*   → correção urgente em produção (a partir de main)
```

- Toda branch nova parte de `develop` (confirmar o dropdown no GitHub antes de abrir PR)
- PRs sempre têm `develop` como base, não `main`
- `main` só recebe merge de `develop` (release) ou `hotfix/*`

---

## 4. Constitution — Regras de Processo (OBRIGATÓRIAS)

Estas 4 regras são permanentes. Se notar que uma está prestes a ser quebrada, **pare e avise o usuário antes de prosseguir**.

### Regra 1 — Perguntas sem resposta não viram suposições

Se uma pergunta de clarificação ficar sem resposta e a decisão afetar comportamento de produto (não só detalhe técnico reversível):
- **Pare e pergunte de novo** antes de prosseguir para Design/Execute
- Se for necessário seguir por prioridade de tempo: marque como `⚠️ PENDENTE DE CONFIRMAÇÃO` no topo da spec e no final da sessão — **nunca como "decisão tomada"**

### Regra 2 — Spec antes do código, sempre

Antes de implementar qualquer mudança de comportamento (novo filtro, novo campo, ajuste de RBAC, nova feature):
1. Atualizar ou criar a `spec.md` correspondente em `.specs/features/{nome}/`
2. Usar o template em `.specs/features/FEATURE-TEMPLATE/spec.md`
3. Só depois implementar

Extensões pequenas de feature existente também exigem atualização da spec antes — não depois.

### Regra 3 — Mudança de contrato é sinalizada antes

Se uma mudança remove ou altera método de interface usada por múltiplos consumidores, muda assinatura pública, ou qualquer coisa que outra camada dependa:
- **Avisar o usuário antes de aplicar**, não só narrar no resumo final
- Aguardar confirmação explícita antes de prosseguir

### Regra 4 — Fluxo de orquestração SDD

Para cada feature nova seguir o ciclo:
```
Specify → Design → Tasks → Execute → Gate Checks → Commit/Merge
```
Guia completo de orquestração: `docs/GUIA-ORQUESTRACAO-SDD.md`

---

## 5. Fluxo SDD — passo a passo

### Passo 1: Specify
- Criar `.specs/features/{nome-da-feature}/spec.md` usando o template
- Definir: problema, user stories (formato padrão), critérios de aceitação numerados e testáveis
- **Não prosseguir sem spec aprovada**

### Passo 2: Design (obrigatório quando...)
Criar `design.md` se a feature:
- Toca mais de uma camada (backend + frontend)
- Introduz nova entidade ou tabela
- Altera contrato de interface existente
- Tem ambiguidade técnica não óbvia

Usar template em `.specs/features/FEATURE-TEMPLATE/design.md`

### Passo 3: Tasks
- Criar `tasks.md` com checklist granular por camada
- Última tarefa: sempre `[ ] Atualizar spec.md com estado final` e `[ ] Atualizar STATE.md`

### Passo 4: Execute
- Implementar seguindo `.specs/codebase/CONVENTIONS.md`
- Atualizar spec se surgir mudança de escopo

### Passo 5: Gate Checks (obrigatório antes de commitar)
```powershell
# Backend
dotnet test tests/ChamadosCamarj.UnitTests/

# Frontend
cd frontend; npm run build
```
Ambos devem passar **sem erros e sem warnings**.

### Passo 6: Commit / Merge
- Commitar na branch `feature/*` ou `fix/*`
- Abrir PR com base `develop`
- Após merge: atualizar `STATE.md` com o resumo da sessão

---

## 6. Atualização de Documentação — obrigatória ao final de cada sessão

Ao encerrar qualquer sessão de implementação, atualizar:

| Arquivo | O que atualizar |
|---------|-----------------|
| `.specs/project/STATE.md` | Resumo da sessão, decisões tomadas, aprendizados |
| `.specs/features/{nome}/spec.md` | Status final, ACs concluídos |
| `.specs/features/{nome}/tasks.md` | Checkboxes marcados |
| `.specs/project/ROADMAP.md` | Status da fase/feature se concluída |

**Nunca encerre uma sessão sem atualizar STATE.md.**

---

## 7. Proibições explícitas

- Não commitar direto em `main` ou `develop`
- Não usar `AutoMapper` (mapeamento manual via extension methods)
- Não injetar `IConfiguration` diretamente em handlers (usar `IOptions<T>`)
- Não usar `DbContext` direto em handlers (usar repositórios)
- Não usar toast libraries (Sonner, Toastr) no frontend
- Não hardcodar cores no frontend (usar tokens do tema)
- Não usar `export default` em componentes React (exceto `App`)
- Não usar `isLoading` no TanStack Query v5 (usar `isPending`)
- Não implementar sem spec atualizada primeiro

---

## 8. Referências rápidas

```powershell
# Rodar testes backend
dotnet test tests/ChamadosCamarj.UnitTests/

# Build frontend
cd frontend; npm run build

# API local
http://localhost:5000/api

# Supabase Dashboard
https://supabase.com (projeto: ChamadosCamarj)

# Produção
https://chamados.okurumin.com.br
```
