# CLAUDE.md — ChamadosCamarj

> Este arquivo é lido automaticamente pelo Claude Code no início de toda sessão neste repositório
> (confirmado na prática: assim que ele existe no working directory, o conteúdo aparece sozinho
> no contexto). Por isso ele fica curto e vira **índice**, não fonte de conteúdo — a informação
> real vive nos arquivos abaixo. Se editar algo, edite o destino, não duplique aqui.
>
> Ferramentas diferentes leem arquivos diferentes: Claude Code lê este `CLAUDE.md`; `opencode` lê
> `AGENTS.md`. Os dois precisam existir, mas nenhum dos dois deve reescrever o conteúdo do outro
> nem da Constitution em `STATE.md` — só apontar pra lá.

---

## 1. Leia nesta ordem, sempre

1. **`.specs/project/STATE.md`** — fonte de verdade do projeto: Constitution (regras obrigatórias,
   permanentes), sessão atual, decisões, pendências. Se qualquer outro arquivo divergir do que
   está aqui, **vale o STATE.md**.
2. **`AGENTS.md`** — stack, comandos de build/run/teste, os agentes orquestrados (`@spec`,
   `@build-backend`, `@build-frontend`, `@review`, `@explorar`) e qual modelo roda cada um.
3. **`.specs/codebase/CONVENTIONS.md`** — convenções de código: o que pode e o que não pode
   (mapeamento manual, `IOptions<T>` em vez de `IConfiguration`, repositório em vez de `DbContext`
   direto, `isPending` em vez de `isLoading`, sem toast library, etc.).

## 2. Índice completo

| O que procurar | Onde está |
|----------------|-----------|
| Constitution (regras obrigatórias de processo) | `.specs/project/STATE.md` (topo) |
| Estado atual, sessões, decisões, pendências | `.specs/project/STATE.md` |
| Roadmap de fases | `.specs/project/ROADMAP.md` |
| Visão geral do projeto | `.specs/project/PROJECT.md` |
| Convenções de código | `.specs/codebase/CONVENTIONS.md` |
| Arquitetura | `.specs/codebase/ARCHITECTURE.md` |
| Stack detalhada, comandos de build/run | `AGENTS.md` |
| Estrutura de diretórios | `.specs/codebase/STRUCTURE.md` |
| Débito técnico | `.specs/codebase/CONCERNS.md` |
| Estratégia de testes | `.specs/codebase/TESTING.md` |
| Specs de cada feature (spec/design/tasks/review) | `.specs/features/{nome-feature}/` |
| Template de nova feature | `.specs/features/FEATURE-TEMPLATE/` |
| Passo a passo de orquestração multi-IA | `docs/GUIA-ORQUESTRACAO-SDD.md` |
| Documentação funcional (negócio, regras, ADRs) — atualizar ao concluir feature | `docs/obsidian/` (comece por `Home.md`) |
| Git flow (branches, quem parte de onde) | `AGENTS.md` |

## 3. Regra de ouro

Nunca comece a implementar sem ler `STATE.md` primeiro. Nunca encerre uma sessão sem atualizar
`STATE.md` — isso inclui marcar com precisão o que **não** foi feito (testes pulados, verificação
manual pendente etc.), não só o que foi. Um `STATE.md` desatualizado é pior do que nenhum.

## 4. Automação SDD (skills globais do Claude Code)

As skills `/sdd` (orquestrador) e `/sdd-specify`, `/sdd-plan`, `/sdd-tasks`, `/sdd-implement`,
`/sdd-review`, `/sdd-close` (fases) ficam em `~/.claude/skills/` e são genéricas. Este bloco diz a
elas onde estão as coisas neste projeto — as regras continuam na Constitution do `STATE.md`.

```yaml
# sdd-config
features_dir: .specs/features
feature_naming: slug
files: { spec: spec.md, plan: design.md, tasks: tasks.md, review: review.md }
templates_dir: .specs/features/FEATURE-TEMPLATE
constitution: .specs/project/STATE.md#Regras de Processo (Constitution)
state: .specs/project/STATE.md
roadmap: .specs/project/ROADMAP.md
context: [.specs/codebase/CONVENTIONS.md, .specs/codebase/ARCHITECTURE.md, AGENTS.md]
functional_docs: docs/obsidian
branch: { base: develop, prefix: feature/ }
gates:
  - dotnet build
  - dotnet test tests/ChamadosCamarj.UnitTests/
  - npm --prefix frontend run build
pr: create
```
