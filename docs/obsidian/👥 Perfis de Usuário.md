# 👥 Perfis de Usuário

## 👑 Admin (Victor)

- **Tudo que o Atendente faz**
- Ver **todos** os chamados do sistema (não filtrado por email/responsável)
- **Reatribuir chamado** entre atendentes, mesmo em `EmAndamento` *(Fase 6)*
- **Forçar encerramento** de qualquer chamado *(Fase 6)*
- **Alterar prioridade** de qualquer chamado *(Fase 6)*
- Gerenciar categorias, usuários e configurações do sistema *(Fase 6)*
- Relatórios e métricas *(Fase 7)*

## 🛠️ Atendente (Fábio)

- Ver fila de chamados (`Aberto`, sem responsável)
- **Assumir** chamados da fila
- **Resolver, Fechar, Cancelar** chamados que está atendendo
- Comentários públicos e internos *(internos: Fase 6)*
- Anexar arquivos *(Fase 4)*
- "Chamados em Atendimento" — lista filtrada por `responsavelId` (só os seus)

## 🙋 Solicitante (Colaboradores / Ana)

- Abrir chamado (via email ou portal)
- Ver **apenas seus próprios chamados** (filtrado por `solicitanteEmail`)
- Comentar publicamente
- Anexar arquivos *(Fase 4)*
- Cancelar seus próprios chamados enquanto em `Aberto` ou `EmAndamento`

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

[[🔐 Google Workspace]] — Login corporativo Gmail (Fase 6)

> ⚠️ Atualmente mockada: seletor de perfil salvo em `localStorage`. Trocar o perfil exige clicar em **Sair** na sidebar e selecionar novamente.
