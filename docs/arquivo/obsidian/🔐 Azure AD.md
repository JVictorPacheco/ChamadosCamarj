# 🔐 Azure AD — Autenticação Corporativa

> ⚠️ **DECISÃO OBSOLETA — corrigida em 2026-06-25.** A Camarj usa **Google Workspace** (Gmail corporativo), não Microsoft/Azure AD. Esta nota é mantida como histórico da decisão original; veja [[🔐 Google Workspace]] para a decisão vigente. Ver também [[💬 Decisões]] (tabela "Decisões Corrigidas").

## Por quê Azure AD? *(raciocínio original, hoje inválido)*

- ✅ Os colaboradores já têm email corporativo Microsoft
- ✅ Sem cadastro — usa a conta que já existe
- ✅ SSO (Single Sign-On)
- ✅ Mais seguro que login/senha próprio
- ✅ Gerencia permissões via grupos

## Como vai funcionar

```
Usuário → Login Microsoft → Token JWT → Acesso ao sistema
```

## Fluxo Técnico

1. Frontend redireciona para login Microsoft
2. Usuário autentica no Azure AD
3. Azure devolve um token (id_token + access_token)
4. Backend valida o token com `Microsoft.Identity.Web`
5. Extrai claims: nome, email, grupo (Admin/Atendente)
6. Autoriza endpoints baseado no perfil

## Tecnologias

- **Backend:** `Microsoft.Identity.Web` + JWT Bearer
- **Frontend:** `@azure/msal-react` (MSAL.js v2)
- **Escopos:** `openid`, `profile`, `email`, `User.Read`

## Perfis vs Grupos do Azure

| Grupo Azure | Perfil no Sistema |
|-------------|-------------------|
| Chamados-Admin | 👑 Admin |
| Chamados-Atendente | 🛠️ Atendente |
| (todos os outros) | 🙋 Solicitante |
