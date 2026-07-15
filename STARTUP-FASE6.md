# 🚀 FASE 6 — INSTRUÇÕES DE STARTUP

## 📋 Pré-requisitos

- ✅ .NET 9+ instalado
- ✅ Node.js + npm instalado
- ✅ Você está no branch `feature/fase-6-admin-log`
- ✅ Dependências restauradas (`dotnet restore`, `npm install`)

---

## 🎯 Como Rodar

### **Opção 1: Script Automático (MacBook/Linux)**

```bash
chmod +x start-fase6.sh
./start-fase6.sh
```

✅ Abre o **Backend** automaticamente
⚠️ Você precisa abrir **Frontend** em outro terminal

---

### **Opção 2: Script Automático (Windows)**

```powershell
.\start-fase6.ps1
```

✅ Abre o **Backend** em nova janela
⚠️ Você precisa abrir **Frontend** em outro terminal

---

### **Opção 3: Manual (Recomendado para Debug)**

#### Terminal 1 — Backend
```bash
cd /opt/data/ChamadosCamarj
dotnet run --project src/ChamadosCamarj.WebApi
```

Aguarde até ver:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

---

#### Terminal 2 — Frontend
```bash
cd /opt/data/ChamadosCamarj/src/ChamadosCamarj.Web
npm run dev
```

Aguarde até ver:
```
  VITE v5.x.x  ready in xxx ms
  Local:  http://localhost:3000
```

---

## 🌐 URLs de Acesso

| Serviço | URL | Descrição |
|---------|-----|-----------|
| **Frontend** | http://localhost:3000 | Interface da aplicação |
| **Backend API** | http://localhost:5000 | API REST |
| **Swagger/Docs** | http://localhost:5000/swagger | Documentação interativa |

---

## 🔐 Dados de Teste

### Usuários Pré-cadastrados

| Email | Senha | Perfil | Uso |
|-------|-------|--------|-----|
| `joão@camarj.com.br` | `senha123` | Cliente | Criar chamados |
| `victor@camarj.com.br` | `senha123` | Atendente | Assumir, reatribuir |
| `fábio@camarj.com.br` | `senha123` | Gerente/Admin | Alterar prioridade, ver interno |

---

## 🧪 Guia de Testes (6 Testes Completos)

Abra o arquivo de testes:

```bash
cat .specs/FRONTEND-FASE-6-TESTES.md
```

Ou leia direto em: `.specs/FRONTEND-FASE-6-TESTES.md`

### Quick Reference

```
[T1] Reatribuição de Chamado
     → Dashboard → Meus Chamados → Clicar → Botão "Reatribuir" → Modal

[T2] Alterar Prioridade
     → Dashboard → Chamado → Badge Prioridade → Editar → Modal

[T3] Histórico (Timeline)
     → Chamado → Scroll down → Seção "Histórico" com timeline

[T4] Comentários Internos
     → Chamado → Seção "Comentários" → Checkbox "Interno"

[T5] Filtro por Perfil
     → Login Cliente vs Admin → Diferentes comentários vistos

[T6] Tempo Real (SignalR)
     → 2 navegadores → Um faz mudança → Outro vê instantaneamente
```

---

## 🐛 Se algo não funcionar

### Backend não inicia
```bash
# Limpar e rebuildar
dotnet clean
dotnet restore
dotnet build
dotnet run --project src/ChamadosCamarj.WebApi
```

### Frontend não inicia
```bash
# Limpar node_modules e reinstalar
rm -rf node_modules package-lock.json
npm install
npm run dev
```

### Porta 5000 ou 3000 já em uso
```bash
# Encontrar processo usando a porta
lsof -i :5000  # Backend
lsof -i :3000  # Frontend

# Matar o processo (Linux/Mac)
kill -9 <PID>
```

---

## 📊 Status da Implementação

### Backend ✅
- [x] 28 testes unitários passando
- [x] Endpoints implementados
- [x] Database migrations prontas
- [x] Histórico registrando corretamente
- [x] Comentários filtrando por perfil

### Frontend 🚀
- [x] Componentes criados (ReatribuirModal, AlterarPrioridadeModal, TimelineHistorico, Comentarios)
- [ ] Integrado em ChamadoDetalhes (você precisa fazer!)
- [ ] SignalR conectado (você precisa verificar!)
- [ ] Testes passando

---

## 📝 Próximas Etapas

### 1. Integrar Componentes
No arquivo `src/ChamadosCamarj.Web/src/pages/ChamadoDetalhes.tsx`, adicione:

```tsx
import { ReatribuirModal } from '@/components/Chamados/ReatribuirModal';
import { AlterarPrioridadeModal } from '@/components/Chamados/AlterarPrioridadeModal';
import { TimelineHistorico } from '@/components/Chamados/TimelineHistorico';
import { Comentarios } from '@/components/Chamados/Comentarios';

export function ChamadoDetalhes() {
  // ... seu código
  return (
    <>
      <ReatribuirModal {...props} />
      <AlterarPrioridadeModal {...props} />
      <TimelineHistorico chamadoId={chamadoId} />
      <Comentarios chamadoId={chamadoId} perfilUsuario={userProfile} />
    </>
  );
}
```

### 2. Rodar Testes
```bash
npm run test  # Frontend
dotnet test   # Backend (opcional, já testado)
```

### 3. Testar Manualmente
Siga o guia em `.specs/FRONTEND-FASE-6-TESTES.md`

---

## 🚨 Troubleshooting

| Problema | Solução |
|----------|---------|
| **"Port 5000 already in use"** | `kill -9 $(lsof -t -i:5000)` |
| **"Cannot find module"** | `npm install` no diretório Web |
| **"Build failed"** | `dotnet clean && dotnet build` |
| **"Histórico vazio"** | Faça uma ação (reatribuir/mudar prioridade) |
| **"Comentários não aparecem"** | Atualize a página (F5) |

---

## 💬 Quando Tiver Dúvidas

1. Verifique os **logs** (Terminal/Console)
2. Leia o **guia de testes** (`.specs/FRONTEND-FASE-6-TESTES.md`)
3. Cheque se **Backend tá rodando** (curl http://localhost:5000/swagger)
4. Cheque se **Frontend tá rodando** (http://localhost:3000)

---

**Pronto! Agora pode começar os testes! 🎯**
