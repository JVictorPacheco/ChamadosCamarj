# 🚀 FASE 6 — INSTRUÇÕES DE STARTUP

> ⚠️ **Este guia foi escrito num estágio intermediário da Fase 6 e ficou desatualizado.** Hoje o frontend da Fase 6 (T10-T14) está **100% integrado e verificado via Playwright** — não há mais nada "pra você fazer". A Fase 7 (Relatório Mensal) também já foi entregue. Ver `.specs/project/STATE.md` para o estado real.
>
> O caminho `src/ChamadosCamarj.Web/` citado abaixo **está errado** — foi o local onde uma versão inicial dos componentes acabou sendo commitada por engano; eles foram descartados e reescritos do zero em `frontend/src/features/chamados/` (a localização correta do frontend, na raiz do repo). Ver `.specs/project/STATE.md` (Aprendizados).

## 📋 Pré-requisitos

- .NET 9+ instalado
- Node.js + npm instalado
- Branch `feature/fase-6-admin-log` (contém Fase 6 completa T01-T14 + Fase 7 completa)
- Dependências restauradas (`dotnet restore`, `npm install` dentro de `frontend/`)

---

## 🎯 Como Rodar

### Terminal 1 — Backend
```bash
dotnet run --project src/ChamadosCamarj.WebApi
```

Aguarde até ver:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### Terminal 2 — Frontend
```bash
cd frontend
npm run dev
```

Aguarde até ver:
```
  VITE v6.x.x  ready in xxx ms
  Local:  http://localhost:5173
```

---

## 🌐 URLs de Acesso

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **Frontend** | http://localhost:5173 | Interface da aplicação |
| **Backend API** | http://localhost:5000 | API REST |
| **Scalar (docs interativa)** | http://localhost:5000/scalar | Ambiente Development |

---

## 🔐 Login

Não existe login por email/senha — a autenticação é um **seletor de perfil mockado** salvo em `localStorage`:

| Perfil | Nome | Uso |
|--------|------|-----|
| Solicitante | Ana Colaboradora | Abrir chamados, comentar publicamente |
| Atendente | Fábio | Assumir, resolver, alterar prioridade, comentário interno |
| Admin | Victor | Tudo do Atendente + reatribuir, ver todos os chamados, Relatório Mensal completo |

O login Google Workspace real (T09/T15) ainda está pendente — ver [[🔐 Google Workspace]] / `.specs/project/STATE.md`.

---

## 🧪 O que testar

As features da Fase 6 já estão implementadas e verificadas, mas se quiser reproduzir manualmente:

```
[T1] Reatribuição de Chamado
     → Detalhe do chamado → Botão "Reatribuir" → selecionar novo responsável

[T2] Alterar Prioridade
     → Detalhe do chamado → Badge de Prioridade → editar

[T3] Histórico (Timeline)
     → Detalhe do chamado → seção "Histórico" com timeline

[T4] Comentários Internos
     → Detalhe do chamado → seção "Comentários" → toggle público/interno

[T5] Filtro por Perfil
     → Logar como Solicitante vs. Admin → comentários internos não aparecem pro Solicitante

[T6] Tempo Real (SignalR)
     → 2 abas/navegadores → uma faz uma mudança → a outra atualiza sozinha
```

> Não usar `.specs/FASE-6-TESTES.md` ou `.specs/FRONTEND-FASE-6-TESTES.md` como guia — descrevem a versão obsoleta do frontend (caminho errado, componentes descartados).

---

## 🐛 Se algo não funcionar

### Backend não inicia
```bash
dotnet clean
dotnet restore
dotnet build
dotnet run --project src/ChamadosCamarj.WebApi
```

### Frontend não inicia
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
npm run dev
```

### Porta 5000 ou 5173 já em uso
```bash
lsof -i :5000  # Backend
lsof -i :5173  # Frontend
kill -9 <PID>
```

---

## 📊 Status real da implementação

Ver `.specs/project/ROADMAP.md` para o detalhamento tarefa a tarefa. Resumo:

### Backend — completo (T01-T08)
- [x] `HistoricoEntrada` + `IHistoricoRepository`
- [x] Reatribuição, Alterar Prioridade — endpoints e commands
- [x] Histórico integrado em todos os CommandHandlers
- [x] Comentários internos filtrados por perfil
- [ ] Login Google Workspace (T09) — pendente

### Frontend — completo (T10-T14)
- [x] Reatribuir, Alterar Prioridade, Timeline de Histórico, Comentário interno — todos integrados em `frontend/src/features/chamados/` e verificados via Playwright
- [ ] Login Google Workspace real (T15, depende de T09) — pendente
- [ ] "Forçar encerramento" — ainda não abordado

---

## 💬 Próximos Passos Reais

1. Retomar T09/T15 (login Google Workspace)
2. Abrir PR de `feature/fase-6-admin-log` → `develop`
3. "Forçar encerramento" (item pendente da Fase 6)

Ver `.specs/project/STATE.md` (seção TODOs) para a lista completa e priorizada.
