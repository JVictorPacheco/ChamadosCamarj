# 🔐 Azure AD — Autenticação Corporativa

## Por quê Azure AD?

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
