# 👥 Perfis de Usuário

## 👑 Admin (Victor)

- **Tudo que o Atendente faz**
- Ver **todos** os chamados do sistema (não filtrado por email/responsável)
- **Reatribuir chamado** entre atendentes, mesmo em `EmAndamento` — ✅ implementado (Fase 6)
- **Forçar encerramento** de qualquer chamado — ⏳ ainda não implementado (Fase 6, pendente)
- **Alterar prioridade** de qualquer chamado — ✅ implementado (Fase 6)
- **Gerenciar usuários** (cadastrar, editar, desativar/reativar) — ✅ implementado (F5a, tela `Admin > Usuários`)
- Gerenciar categorias e configurações do sistema — ⏳ ainda não implementado (Fase 6, pendente)
- **Relatório Mensal** — vê todos os números, quebra por categoria e por atendente *(Fase 7 ✅)*, ver [[📈 Relatório Mensal]]

## 🛠️ Atendente (Fábio)

- Ver fila de chamados (`Aberto`, sem responsável)
- **Assumir** chamados da fila
- **Resolver, Fechar, Cancelar** chamados que está atendendo
- Comentários públicos e internos — filtro por perfil ✅ implementado (Fase 6)
- Anexar arquivos *(Fase 4, não implementado)*
- "Chamados em Atendimento" — lista filtrada por `responsavelId` (só os seus)
- **Relatório Mensal** — vê só os próprios números, sem quebra por atendente *(Fase 7 ✅)*

## 🙋 Solicitante (Colaboradores / Ana)

- Abrir chamado (via email ou portal)
- Ver **apenas seus próprios chamados** (filtrado por `solicitanteEmail`)
- Comentar publicamente
- Anexar arquivos *(Fase 4, não implementado)*
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

[[🔐 Google Workspace]] — Login corporativo Gmail, **implementado (T09/T15, 2026-07-18)**. Falta só o Client ID real da TI pra funcionar de ponta a ponta.

> Perfil (Admin/Atendente/Solicitante) vem do JWT emitido no login com Google, buscado na tabela `UsuarioPerfil` (cadastrada/gerenciada pelo Admin em `Admin > Usuários`, ver F5a). Um e-mail `@camarj.com.br` sem cadastro recebe 403 ao tentar entrar.
