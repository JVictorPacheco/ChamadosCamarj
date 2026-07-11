# 🚀 Setup Rápido — Fase 6

## 📍 Você está em: `main`
## 🎯 Quer ir pra: `feature/fase-6-admin-log`

---

## ⚡ Opção 1: Comando Rápido (3 linhas)

```bash
git fetch origin
git checkout feature/fase-6-admin-log
dotnet test --filter Demo
```

✅ Pronto! Você tem tudo da Fase 6.

---

## 🤖 Opção 2: Script Automático (recomendado)

### No MacBook (Linux/macOS):
```bash
chmod +x get-fase6.sh
./get-fase6.sh
```

### No Windows (PowerShell):
```powershell
.\get-fase6.ps1
```

**O script faz tudo automaticamente:**
- ✅ Fetch das branches
- ✅ Checkout da branch
- ✅ Restore de dependências
- ✅ Build do projeto
- ✅ Rodar testes (você escolhe qual)

---

## 📝 Opção 3: Passo a Passo Manual

### Passo 1: Atualizar branches remotas
```bash
git fetch origin
```

**Output esperado:**
```
From github.com:JVictorPacheco/ChamadosCamarj
 * [new branch]      feature/fase-6-admin-log -> origin/feature/fase-6-admin-log
```

---

### Passo 2: Ir pra branch da Fase 6
```bash
git checkout feature/fase-6-admin-log
```

**Output esperado:**
```
Switched to a new branch 'feature/fase-6-admin-log'
Branch 'feature/fase-6-admin-log' set up to track remote branch 'feature/fase-6-admin-log' from 'origin'.
```

---

### Passo 3: Verificar que tá na branch certa
```bash
git branch
```

**Output esperado:**
```
  develop
  main
* feature/fase-6-admin-log    ← você está aqui!
```

---

### Passo 4: Ver os commits que foram feitos
```bash
git log --oneline -8
```

**Output esperado:**
```
91b8d52 test: adicionar demo test visual com explicação pra apresentação
620ea86 docs: adicionar guia de testes e script PowerShell para Fase 6
5e9e69f test: adicionar testes unitários para Fase 6
57d357c feat: integrar histórico em todos os handlers (T09)
20828bb feat: implementar alterar prioridade e filtrar comentários internos (T07-T08)
b01c335 feat: adicionar histórico de ações (T05-T06)
bda3e98 feat: implementar reatribuição de chamados (T03-T04)
4d9fe92 feat: adicionar entidades e métodos para Fase 6 (T01-T02)
```

---

### Passo 5: Restaurar dependências
```bash
dotnet restore
```

---

### Passo 6: Build do projeto
```bash
dotnet build -c Release
```

**Output esperado:**
```
Build succeeded. 0 Warning(s) (< 2s)
```

---

### Passo 7: Rodar os testes

#### Opção A: Todos os testes
```bash
dotnet test tests/ChamadosCamarj.UnitTests/ -v q
```

#### Opção B: Apenas Fase 6
```bash
dotnet test --filter "Reatribuir|AlterarPriori|Historico|Fase"
```

#### Opção C: DEMO test (pra impressionar a esposa) ⭐
```bash
dotnet test --filter Demo
```

**Output esperado:**
```
✅ 28 testes passando

Demo Test (com prints bonitos):
🎬 ACT 1: João abre um chamado
🎬 ACT 2: Victor assume
🎬 ACT 3: Fábio muda prioridade
...
✅ Teste Demo concluído com sucesso!
```

---

## 📚 Depois dos Testes

### Ver documentação
```bash
cat .specs/FASE-6-TESTES.md
```

### Ver explicação pra mostrar pra esposa
```bash
cat docs/EXPLICACAO-PARA-ESPOSA.md
```

### Voltar pra develop
```bash
git checkout develop
git pull origin develop
```

---

## ❓ Se algo der errado

### Erro: "fatal: 'feature/fase-6-admin-log' does not exist"
**Solução:**
```bash
git fetch origin
git checkout -b feature/fase-6-admin-log origin/feature/fase-6-admin-log
```

---

### Erro: "Cannot checkout because of uncommitted changes"
**Solução:**
```bash
# Salvar suas mudanças
git stash

# Depois fazer checkout
git checkout feature/fase-6-admin-log

# Recuperar suas mudanças
git stash pop
```

---

### Erro: "dotnet: command not found"
**Solução:**
Você precisa instalar .NET SDK:
- macOS: `brew install dotnet`
- Windows: baixar em https://dotnet.microsoft.com/download

---

### Testes falhando?
```bash
# Limpar cache
rm -rf .vs bin obj

# Fazer rebuild
dotnet clean
dotnet restore
dotnet build -c Release

# Rodar testes de novo
dotnet test
```

---

## 🎯 Checklist

- [ ] `git fetch origin` executado
- [ ] `git checkout feature/fase-6-admin-log` OK
- [ ] `git branch` mostra você na branch certa
- [ ] `dotnet restore` OK
- [ ] `dotnet build` OK
- [ ] `dotnet test --filter Demo` mostra 28 testes passando
- [ ] Leu `docs/EXPLICACAO-PARA-ESPOSA.md`
- [ ] Mostrou pra esposa e impressionou! 😄

---

## 💬 Próximos Passos

1. **Testes Manuais:** Postman/Thunder Client nos endpoints
2. **Code Review:** Revisar commits na branch
3. **Approval:** Dar thumbs up pra merge
4. **Frontend:** Começar Fase 6 do frontend

---

**Qualquer dúvida, é só chamar! 🚀**
