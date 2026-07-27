# 👥 Perfis de Usuário

## 👑 Admin (Victor)

- **Tudo que o Atendente faz**
- Ver **todos** os chamados do sistema (não filtrado por email/responsável)
- **Reatribuir chamado** entre atendentes, mesmo em `EmAndamento` — ✅ implementado (Fase 6)
- **Forçar encerramento** de qualquer chamado direto de qualquer status não-final, com motivo obrigatório auditado — ✅ implementado (2026-07-19)
- **Alterar prioridade** de qualquer chamado — ✅ implementado (Fase 6)
- **Gerenciar usuários** (cadastrar, editar, desativar/reativar) — ✅ implementado (F5a, tela `Admin > Usuários`)
- Gerenciar categorias e configurações do sistema — ⏳ ainda não implementado (Fase 6, pendente)
- **Relatório Mensal** — vê todos os números, quebra por categoria e por atendente *(Fase 7 ✅)*, ver [[📈 Relatório Mensal]]

## 🛠️ Atendente (Fábio)

- Ver fila de chamados (`Aberto`, sem responsável)
- **Assumir** chamados da fila
- **Resolver, Fechar, Cancelar** chamados que está atendendo
- Comentários públicos e internos — filtro por perfil ✅ implementado (Fase 6)
- Anexar arquivos — ✅ implementado, ver [[📦 Supabase Storage]]
- "Chamados em Atendimento" — lista filtrada por `responsavelId` (só os seus)
- **Relatório Mensal** — vê só os próprios números, sem quebra por atendente *(Fase 7 ✅)*

## 🙋 Solicitante (Colaboradores / Ana)

- Abrir chamado (via email ou portal)
- Ver **apenas seus próprios chamados** (filtrado por `solicitanteEmail`)
- Comentar publicamente
- Anexar arquivos — ✅ implementado, ver [[📦 Supabase Storage]]
- Cancelar seus próprios chamados enquanto em `Aberto` ou `EmAndamento`
- **Relatório Mensal** — bloqueado de verdade (não só link escondido; a única tela do sistema com esse RBAC "hard block")

---

## Fluxo de Permissões por Ação

| Ação | Admin | Atendente | Solicitante |
|------|-------|-----------|-------------|
| Abrir chamado | ✅ | ✅ | ✅ |
| Ver seus chamados | ✅ (todos) | ✅ (os seus) | ✅ (os seus) |
| Assumir da fila | ✅ | ✅ | ❌ |
| Resolver | ✅ | ✅ (só os seus) | ❌ |
| Fechar | ✅ | ✅ (só os seus) | ❌ |
| Cancelar | ✅ | ✅ | ✅ (só os seus) |
| Reatribuir | ✅ | ❌ | ❌ |
| Alterar prioridade | ✅ | ❌ | ❌ |
| Ver histórico | ✅ | ✅ | ✅ (público) |
| Comentário interno | ✅ | ✅ | ❌ |
| Ver Relatório Mensal | ✅ (tudo) | ✅ (só os seus) | ❌ (bloqueio real) |
| Forçar encerramento | ✅ | ❌ | ❌ |
| Anexar arquivo | ✅ | ✅ | ✅ (no próprio chamado) |

---

## O que "Meus Chamados" mostra por perfil

| Perfil | Título | Filtro aplicado |
|--------|--------|-----------------|
| Admin | **Todos os Chamados** | Nenhum (vê tudo) |
| Atendente | **Chamados em Atendimento** | `responsavelId = perfil.id` |
| Solicitante | **Meus Chamados** | `solicitanteEmail = perfil.email` |

---

## Ciclo de Vida do Chamado por Perfil

```
Solicitante abre → Aberto
                       ↓
             Atendente/Admin assume → EmAndamento
                       ↓
             Atendente/Admin resolve → Resolvido
                       ↓
             Atendente/Admin fecha → Fechado

Cancelado (de Aberto/EmAndamento) — por qualquer perfil com acesso
Admin pode reatribuir em qualquer etapa não-final
```

---

## Autenticação

**Login ativo: email+senha** via ASP.NET Core Identity (`PasswordHasher`). Senhas são definidas pelo Admin ao cadastrar/editar o usuário na tela `Admin > Usuários`. Login Google OAuth está implementado mas **dormante** (TI informou que o Client ID está fora do plano CAMARJ). Spec em `.specs/features/auth-email-senha/spec.md`.

> Perfil (Admin/Atendente/Solicitante) vem do JWT emitido no login, buscado na tabela `UsuarioPerfil` (cadastrada/gerenciada pelo Admin em `Admin > Usuários`, ver F5a). Usuário sem cadastro não consegue entrar.
