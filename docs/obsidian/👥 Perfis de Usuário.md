# 👥 Perfis de Usuário

## 👑 Admin (Victor)

- Tudo que o Atendente faz
- Gerenciar categorias
- Gerenciar usuários
- Relatórios e métricas
- Configurações do sistema

## 🛠️ Atendente (Victor + Fábio)

- Ver fila de chamados
- Assumir chamados
- Resolver, fechar, reabrir, cancelar
- Comentários públicos e internos
- Anexar arquivos

## 🙋 Solicitante (Colaboradores)

- Abrir chamado (via email ou portal)
- Ver **apenas seus próprios chamados**
- Comentar (visível apenas publicamente)
- Anexar arquivos

---

## Fluxo de Permissões

```
                    ┌─────────────────┐
                    │   Solicitante   │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │   Atendente     │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │     Admin       │
                    └─────────────────┘
```

## Autenticação

[[🔐 Azure AD]] — Login corporativo Microsoft
