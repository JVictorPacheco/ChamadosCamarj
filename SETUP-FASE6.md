# 🚀 Setup — Branch da Fase 6

> ⚠️ Este guia foi escrito no início da Fase 6, quando só o backend inicial existia. Hoje a branch `feature/fase-6-admin-log` já contém a **Fase 6 completa (T01-T14, exceto login Google T09/T15) e também a Fase 7 (Relatório Mensal) inteira**, verificadas via Playwright em 2026-07-14/15 — ver `.specs/project/STATE.md` e `.specs/HANDOFF.md`. O `dotnet test --filter Demo` mencionado abaixo referenciava um script de demonstração temporário que já foi removido.
>
> **Próximo passo real:** retomar T09/T15 (login Google Workspace) e abrir PR de `feature/fase-6-admin-log` → `develop` (ver TODOs em `.specs/project/STATE.md`).

## 📍 Você está em: `main` ou `develop`
## 🎯 Quer ir pra: `feature/fase-6-admin-log`

---

## Passo a passo

### 1. Atualizar branches remotas
```bash
git fetch origin
```

### 2. Ir pra branch da Fase 6
```bash
git checkout feature/fase-6-admin-log
```

### 3. Restaurar dependências e buildar o backend
```bash
dotnet restore
dotnet build -c Release
```

### 4. Instalar e rodar o frontend
```bash
cd frontend
npm install
npm run dev
```
Acesse `http://localhost:5173`. Login é o seletor de perfil mockado (Admin/Atendente/Solicitante) salvo em `localStorage`.

### 5. Rodar os testes de backend
```bash
dotnet test tests/ChamadosCamarj.UnitTests/ -v q
```
Esperado: 109 testes passando (contagem em `.specs/HANDOFF.md`; pode já estar maior).

### 6. Rodar a API
```bash
dotnet run --project src/ChamadosCamarj.WebApi
```
Acesse `http://localhost:5000/scalar` para os endpoints.

---

## 📚 Documentação de referência

- Estado atual, decisões e pendências: `.specs/project/STATE.md`
- Roadmap detalhado: `.specs/project/ROADMAP.md`
- Spec da Fase 6: `.specs/features/fase-6-admin-log/spec.md`
- Spec da Fase 7 (Relatório Mensal): `.specs/features/relatorio-mensal/`
- Handoff da última sessão: `.specs/HANDOFF.md`

> `.specs/FASE-6-TESTES.md` e `.specs/FRONTEND-FASE-6-TESTES.md` descrevem uma versão anterior e obsoleta do frontend da Fase 6 (componentes commitados no caminho errado e reescritos do zero depois) — não usar como guia de teste atual.

### Voltar pra develop
```bash
git checkout develop
git pull origin develop
```

---

## ❓ Se algo der errado

### Erro: "fatal: 'feature/fase-6-admin-log' does not exist"
```bash
git fetch origin
git checkout -b feature/fase-6-admin-log origin/feature/fase-6-admin-log
```

### Erro: "Cannot checkout because of uncommitted changes"
```bash
git stash
git checkout feature/fase-6-admin-log
git stash pop
```

### Erro: "dotnet: command not found"
- macOS: `brew install dotnet`
- Windows: baixar em https://dotnet.microsoft.com/download

### Testes falhando?
```bash
rm -rf .vs bin obj
dotnet clean
dotnet restore
dotnet build -c Release
dotnet test
```

---

## 🎯 Checklist

- [ ] `git fetch origin` executado
- [ ] `git checkout feature/fase-6-admin-log` OK
- [ ] `dotnet restore` e `dotnet build` OK
- [ ] `dotnet test` mostra os testes de backend passando
- [ ] `npm install` e `npm run dev` no `frontend/` OK

---

## 💬 Próximos Passos Reais

1. Retomar T09/T15 (login Google Workspace real)
2. Abrir PR de `feature/fase-6-admin-log` → `develop`
3. "Forçar encerramento" (item da Fase 6 ainda não abordado)
