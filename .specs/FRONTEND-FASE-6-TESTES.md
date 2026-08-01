# 🧪 GUIA COMPLETO DE TESTES — FRONTEND FASE 6

## 📋 Índice de Testes

1. **[T1] Reatribuição de Chamado**
2. **[T2] Alterar Prioridade**
3. **[T3] Histórico (Timeline)**
4. **[T4] Comentários Internos (Privados)**
5. **[T5] Filtros de Comentário por Perfil**
6. **[T6] Status em Tempo Real (SignalR)**

---

## ✅ [T1] TESTE: Reatribuição de Chamado

### 🎯 O que testa:
Verificar se um atendente consegue reatribuir um chamado para outro atendente.

### 📍 Pré-requisitos:
- ✅ Estar logado como **Victor** (atendente)
- ✅ Ter um chamado **Em Andamento** atribuído a você
- ✅ Ter pelo menos 1 outro atendente na equipe (ex: Fábio)

### 👣 Passos:

**PASSO 1:** Abrir Dashboard
```
1. Vá para http://localhost:3000/dashboard
2. Procure a seção "Meus Chamados"
3. Encontre um chamado com Status = "Em Andamento"
4. Clique nele para abrir detalhes
```

**PASSO 2:** Encontrar botão de Reatribuição
```
5. Na tela de detalhes do chamado, procure:
   - Um botão azul chamado "Reatribuir" ou "👥 Reatribuir"
   - Ou um ícone de "↩️ " para reatribuir
6. Clique nele
```

**PASSO 3:** Abrir modal de reatribuição
```
7. Deve aparecer um modal/dialog com:
   - Título: "Reatribuir Chamado"
   - Campo de seleção: "Atendente" (dropdown)
   - Campo de texto: "Motivo" (opcional)
   - Botões: "Cancelar" e "Reatribuir"
```

**PASSO 4:** Selecionar novo atendente
```
8. Clique no dropdown "Atendente"
9. Selecione "Fábio" (ou outro atendente)
10. (Opcional) Adicione motivo: "Especialista em email"
```

**PASSO 5:** Confirmar reatribuição
```
11. Clique no botão "Reatribuir"
12. Aguarde carregamento (spinner)
```

### ✅ Resultado Esperado:

**Na tela do chamado:**
- ✅ Botão desaparece ou fica desabilitado
- ✅ Status muda para "Reatribuído" (temporário) ou mantém "Em Andamento"
- ✅ Campo "Responsável" agora mostra: **"Fábio"**
- ✅ Toast/notificação verde: "Chamado reatribuído com sucesso!"

**No Dashboard:**
- ✅ Chamado desaparece de "Meus Chamados"
- ✅ Chamado agora aparece na lista de "Fábio" (se você vir a lista dele)

**No histórico (veja [T3]):**
- ✅ Nova entrada: "Victor reatribuiu para Fábio às 14:45"

---

## ✅ [T2] TESTE: Alterar Prioridade

### 🎯 O que testa:
Verificar se um gerente consegue alterar a prioridade de um chamado.

### 📍 Pré-requisitos:
- ✅ Estar logado como **Fábio** (gerente/admin)
- ✅ Ter um chamado aberto (qualquer status)

### 👣 Passos:

**PASSO 1:** Abrir Dashboard
```
1. Vá para http://localhost:3000/dashboard
2. Procure na lista de "Todos os Chamados"
3. Encontre um com Prioridade = "Alta"
4. Clique nele para abrir detalhes
```

**PASSO 2:** Encontrar selector de prioridade
```
5. Na tela de detalhes, procure:
   - Label: "Prioridade"
   - Badge/tag atual: "Alta" (em amarelo)
   - Um botão "Editar" ou ícone de ✏️
6. Clique para editar
```

**PASSO 3:** Abrir modal de prioridade
```
7. Deve aparecer um modal com:
   - Título: "Alterar Prioridade"
   - Opções: [ ] Urgente  [ ] Alta  [ ] Média  [ ] Baixa
   - Botões: "Cancelar" e "Salvar"
8. A opção "Alta" deve estar marcada
```

**PASSO 4:** Selecionar nova prioridade
```
9. Clique na opção "Urgente" (radio button)
10. Observe que a seleção muda
```

**PASSO 5:** Confirmar mudança
```
11. Clique no botão "Salvar"
12. Aguarde carregamento
```

### ✅ Resultado Esperado:

**Na tela do chamado:**
- ✅ Badge de prioridade muda de **"Alta"** → **"Urgente"** (em vermelho/crítico)
- ✅ Toast verde: "Prioridade alterada com sucesso!"
- ✅ Data limite (DataLimite) **encurta** (prazo menor)
  - Exemplo: Era "15 de julho" → Agora "13 de julho"

**No histórico:**
- ✅ Nova entrada: "Fábio alterou prioridade: Alta → Urgente às 14:50"

**No Dashboard:**
- ✅ Chamado agora aparece em destaque/topo (por ser urgente)
- ✅ Ícone 🔴 ou emoji de urgência pode aparecer

---

## ✅ [T3] TESTE: Histórico (Timeline)

### 🎯 O que testa:
Verificar se a timeline de histórico mostra todas as ações realizadas no chamado.

### 📍 Pré-requisitos:
- ✅ Ter um chamado com histórico (foi criado, atribuído, mudou prioridade, etc.)

### 👣 Passos:

**PASSO 1:** Abrir detalhes do chamado
```
1. Vá para http://localhost:3000/dashboard
2. Clique em qualquer chamado
3. Role para baixo até achar uma seção "Histórico" ou "Timeline"
4. Deve haver um botão ou aba "Histórico"
```

**PASSO 2:** Visualizar histórico completo
```
5. Clique em "Histórico" (se não estiver visível)
6. Deve aparecer uma **timeline vertical** com entradas de tempo
```

### ✅ Resultado Esperado:

**Layout da timeline:**
```
┌─────────────────────────────────────────┐
│  📌 HISTÓRICO                           │
├─────────────────────────────────────────┤
│  14:30  📌 Criado                       │
│         João Silva criou o chamado      │
│                                         │
│  14:35  👤 Assumido                     │
│         Victor assumiu o chamado        │
│                                         │
│  14:40  🔴 Prioridade Alterada          │
│         Fábio mudou: Alta → Urgente     │
│                                         │
│  14:45  ↩️  Reatribuído                  │
│         Victor → Fábio                  │
│                                         │
│  15:00  ✅ Resolvido                    │
│         Fábio resolveu o chamado        │
└─────────────────────────────────────────┘
```

**Checklist:**
- ✅ Cada ação tem um **timestamp** (hora exata)
- ✅ Cada ação tem um **ícone** diferente
- ✅ Cada ação mostra **quem fez** e **o que fez**
- ✅ **Ordenação**: Do mais antigo (topo) ao mais recente (embaixo)
- ✅ Detalhes de mudança aparecem (ex: "Alta → Urgente")

---

## ✅ [T4] TESTE: Comentários Internos (Privados)

### 🎯 O que testa:
Verificar se comentários marcados como "Interno" não aparecem para o cliente.

### 📍 Pré-requisitos:
- ✅ Estar logado como **Fábio** (atendente)
- ✅ Ter um chamado aberto
- ✅ Ter um comentário "Interno" já adicionado (ou criar um novo)

### 👣 Passos:

**PASSO 1:** Abrir comentários do chamado
```
1. Vá para http://localhost:3000/dashboard
2. Clique em um chamado
3. Procure a seção "Comentários" ou "Notas"
4. Deve haver uma lista de comentários
```

**PASSO 2:** Verificar tipo de comentário
```
5. Procure por um comentário com:
   - Badge/label "Interno" (em vermelho ou cinza)
   - Ou um ícone 🔒 (cadeado)
   - Texto de exemplo: "Usuário estava com proxy antigo"
6. Compare com comentário "Público" (em verde, sem cadeado)
```

**PASSO 3:** Adicionar novo comentário interno
```
7. Role até "Adicionar Comentário"
8. Escreva: "Precisei resetar a senha no servidor"
9. Procure por um toggle/checkbox: "Este é um comentário interno"
10. Marque o checkbox (ativa o modo "Interno")
11. Clique "Adicionar Comentário"
```

### ✅ Resultado Esperado:

**Comentários na tela:**
```
┌─────────────────────────────────────────┐
│  COMENTÁRIOS                            │
├─────────────────────────────────────────┤
│  ✅ Victor (14:35) — Público            │
│  "Estamos verificando seu email..."     │
│                                         │
│  🔒 Fábio (14:50) — Interno            │
│  "Usuário estava com proxy antigo"      │
│  ← Este comentário é privado!           │
│                                         │
│  🔒 Fábio (15:00) — Interno            │
│  "Precisei resetar a senha no servidor" │
│  ← Este comentário é privado!           │
└─────────────────────────────────────────┘
```

**Checklist:**
- ✅ Comentários "Público" têm badge verde ✅
- ✅ Comentários "Interno" têm badge cinza/vermelho 🔒
- ✅ Ao submeter, comentário aparece com ícone correto
- ✅ Toast: "Comentário adicionado com sucesso!"

---

## ✅ [T5] TESTE: Filtros de Comentário por Perfil

### 🎯 O que testa:
Verificar se cliente vê só comentários públicos, e admin vê todos.

### 📍 Pré-requisitos:
- ✅ Um chamado com **2+ comentários públicos** e **1+ comentário interno**

### 👣 Passos:

**PASSO 1:** Logar como Cliente (João)
```
1. Faça logout (clique em perfil → Logout)
2. Faça login como "joão@camarj.com.br" (cliente)
3. Vá para http://localhost:3000/meu-chamado/[ID]
   (ou acesse pela lista de "Meus Chamados")
4. Role até "Comentários"
```

**PASSO 2:** Verificar comentários vistos
```
5. Você deve ver:
   - ✅ Comentário público do Victor
   - ✅ Comentário público do Fábio
   - ❌ NÃO vê comentários internos (ex: "Precisei resetar...")
6. Contagem deve ser **2 comentários** (só públicos)
```

**PASSO 3:** Logar como Admin (Fábio)
```
7. Faça logout
8. Faça login como "fábio@camarj.com.br" (admin)
9. Vá para o MESMO chamado
10. Roll até "Comentários"
```

**PASSO 4:** Verificar comentários vistos
```
11. Você deve ver:
    - ✅ Comentários públicos
    - ✅ Comentários internos (com 🔒)
12. Contagem deve ser **3+ comentários** (públicos + internos)
```

### ✅ Resultado Esperado:

**Cliente vê:**
```
COMENTÁRIOS (2)
├─ Victor: "Estamos verificando..."  ✅
└─ Fábio: "Problema resolvido!"       ✅
```

**Admin vê:**
```
COMENTÁRIOS (3)
├─ Victor: "Estamos verificando..."  ✅
├─ Fábio: "Usuário com proxy antigo" 🔒
└─ Fábio: "Precisei resetar..."      🔒
```

---

## ✅ [T6] TESTE: Status em Tempo Real (SignalR)

### 🎯 O que testa:
Verificar se mudanças aparecem em tempo real quando outro atendente mexe no chamado.

### 📍 Pré-requisitos:
- ✅ Ter **2 navegadores abertos** (ou 2 abas)
- ✅ Um logado como **Victor**, outro como **Fábio**
- ✅ Ambos visualizando o **MESMO chamado**

### 👣 Passos:

**PASSO 1:** Preparar 2 telas
```
1. Navegador A: Logar como Victor
   - Vá para: http://localhost:3000/chamado/123 (exemplo)
   - Procure por: Status, Prioridade, Histórico

2. Navegador B: Logar como Fábio
   - Vá para a MESMA URL: http://localhost:3000/chamado/123
   - Veja os mesmos dados
```

**PASSO 2:** Victor altera prioridade
```
3. No Navegador A (Victor):
   - Clique em "Alterar Prioridade"
   - Mude de "Alta" → "Urgente"
   - Clique "Salvar"
4. Veja a mudança aparecer no Navegador A
```

**PASSO 3:** Verificar atualização em tempo real
```
5. NO NAVEGADOR B (Fábio):
   - SEM ATUALIZAR A PÁGINA (F5)
   - A prioridade deve mudar de "Alta" → "Urgente"
   - Uma notificação pode aparecer: "Chamado atualizado"
6. O histórico deve mostrar nova entrada (pode precisar de refresh)
```

### ✅ Resultado Esperado:

**Navegador A (Victor):**
```
Antes: Prioridade = Alta
Depois: Prioridade = Urgente ✅
```

**Navegador B (Fábio):**
```
Antes: Prioridade = Alta
↓ (sem atualizar página)
Depois: Prioridade = Urgente ✅ (automático!)
```

**Indicadores:**
- ✅ Mudança aparece **instantaneamente** ou em **< 2 segundos**
- ✅ Notificação: "Chamado atualizado por Victor"
- ✅ Histórico pode ter entrada nova (com refresh)

---

## 🎯 Checklist Final — Todos os Testes

```
✅ [T1] Reatribuição
├─ Botão "Reatribuir" existe?
├─ Modal abre corretamente?
├─ Novo atendente é atribuído?
├─ Histórico registra mudança?
└─ Toast de sucesso aparece?

✅ [T2] Alterar Prioridade
├─ Badge de prioridade muda?
├─ Data limite encurta para Urgente?
├─ Histórico registra (Alta → Urgente)?
└─ Chamado vai pro topo da lista?

✅ [T3] Histórico
├─ Timeline aparece?
├─ Todas as ações estão lá?
├─ Timestamps estão corretos?
├─ Ordem é descendente (novo → antigo)?
└─ Ícones diferem por ação?

✅ [T4] Comentários Internos
├─ Adicionar comentário público funciona?
├─ Adicionar comentário interno funciona?
├─ Toggle "Interno" existe?
├─ Badge aparece corretamente?
└─ Comentários internos têm 🔒?

✅ [T5] Filtro por Perfil
├─ Cliente vê 2 comentários (públicos)?
├─ Admin vê 3+ comentários (públicos + internos)?
├─ Contagem está correta?
└─ Sem comentários internos visíveis para cliente?

✅ [T6] Tempo Real (SignalR)
├─ 2 browsers mostram dados sincronizados?
├─ Mudança em um aparece no outro?
├─ Sem atualizar página (F5)?
├─ Tempo < 2 segundos?
└─ Notificação de atualização aparece?
```

---

## 📞 Se algo quebrar:

**Erro comum: "Comentário não aparece"**
- ✅ Tente atualizar a página (F5)
- ✅ Verifique se está logado

**Erro: "Prioridade não muda"**
- ✅ Verifique se é Admin/Gerente
- ✅ Tente abrir console (F12) e procure por erro

**Erro: "Histórico vazio"**
- ✅ Crie uma ação (reatribua ou mude prioridade)
- ✅ Aguarde 2 segundos
- ✅ Atualize a página

---

**Pronto! Agora é com você, Victor! 🚀**

Quando acabar todos os 6 testes, me avisa qual passou, qual falhou e o que precisa corrigir! 👍
