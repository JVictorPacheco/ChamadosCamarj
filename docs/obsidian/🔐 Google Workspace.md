# 🔐 Google Workspace — Autenticação Corporativa

> ⚠️ Decisão corrigida em 2026-06-25: a CAMARJ usa **Google Workspace** (Gmail corporativo), não Azure AD/Microsoft como assumido anteriormente.

## Por quê Google Workspace?

- ✅ Os colaboradores já têm email corporativo Google (`@camarj.com.br`)
- ✅ Sem cadastro extra — usa a conta que já existe
- ✅ SSO (Single Sign-On)
- ✅ Mais seguro que login/senha próprio
- ✅ Contas organizadas por setor (ex: autorizacao@camarj.com.br)

## Status atual

**Mockada (Fases 3–5):** seletor de perfil em `localStorage` (Admin/Atendente/Solicitante).
**Real (Fase 6):** "Sign in with Google" substitui o seletor mockado.

## Como vai funcionar (Fase 6)

```
Usuário → Botão "Entrar com Google" → OAuth2 Google → Token JWT → Acesso ao sistema
```

## Fluxo Técnico Planejado

1. Frontend redireciona para login Google
2. Usuário autentica com sua conta `@camarj.com.br`
3. Google devolve token (id_token + access_token)
4. Backend valida o token com Google APIs
5. Extrai claims: nome, email
6. Faz lookup no mapeamento conta→perfil (tabela no banco)
7. Retorna JWT próprio com perfil (Admin/Atendente/Solicitante)
8. Frontend usa o JWT para todas as requisições

## Mapeamento de Contas

Contas são **por setor**, não por analista individual:

| Email | Perfil no Sistema |
|-------|-------------------|
| victor@camarj.com.br | 👑 Admin |
| fabio@camarj.com.br | 🛠️ Atendente |
| autorizacao@camarj.com.br | 🛠️ Atendente |
| ti@camarj.com.br | 🛠️ Atendente |
| (demais @camarj.com.br) | 🙋 Solicitante |

> O mapeamento exato precisa ser definido com Victor antes de implementar.

## Tecnologias Planejadas

- **Backend:** Google OAuth2 + JWT Bearer
- **Frontend:** `@react-oauth/google` ou similar
- **Escopos:** `openid`, `profile`, `email`

## Relação com [[👥 Perfis de Usuário]]

Perfil Admin/Atendente/Solicitante continuará sendo o mesmo — só a fonte de autenticação muda (de localStorage para token Google real).
