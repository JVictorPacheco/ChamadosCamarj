# 🔐 Google Workspace — Autenticação Corporativa

> ⚠️ **DORMENTE (2026-07-24):** O login Google OAuth está implementado mas DORMENTE — a TI informou que o Client ID está fora do plano da CAMARJ. O login ativo é por email+senha (ver spec em `.specs/features/auth-email-senha/spec.md`).

> ⚠️ Decisão corrigida em 2026-06-25: a CAMARJ usa **Google Workspace** (Gmail corporativo), não Azure AD/Microsoft como assumido anteriormente.

## Por quê Google Workspace?

- ✅ Os colaboradores já têm email corporativo Google (`@camarj.com.br`)
- ✅ Sem cadastro extra — usa a conta que já existe
- ✅ SSO (Single Sign-On)
- ✅ Mais seguro que login/senha próprio
- ✅ Contas organizadas por setor (ex: autorizacao@camarj.com.br)

## Status atual

**IMPLEMENTADO (T09/T15, 2026-07-18).** "Sign in with Google" (`GoogleLogin`/`GoogleOAuthProvider`) substituiu por completo o antigo seletor mockado e o login por e-mail do F5a. Código pronto, testado visualmente e commitado (`8166ff0`) — falta só o **Client ID real da TI** (documento de requisitos já entregue) pra funcionar de ponta a ponta contra contas Google de verdade.

**Passo intermediário (F5a, 2026-07-16):** antes do login Google real, foi implementado um login mockado por e-mail com cadastro de usuários pelo Admin (tabela `UsuarioPerfil` + `UsuariosController`). Não foi descartado — o T09 reaproveita a mesma tabela `UsuarioPerfil` sem mudanças, só trocando a fonte de autenticação de "e-mail digitado" para "token Google validado".

## Como funciona (implementado)

```
Usuário → Botão "Entrar com Google" → Google devolve id_token → Backend valida (Google.Apis.Auth)
   → Confere domínio @camarj.com.br + EmailVerified → Lookup em UsuarioPerfil → JWT próprio (perfil incluso)
```

## Fluxo Técnico (como foi implementado)

1. Frontend mostra o botão oficial do Google (`@react-oauth/google`, tema `filled_black`, `hosted_domain="camarj.com.br"`)
2. Usuário autentica com sua conta `@camarj.com.br`
3. Google devolve o `id_token` (`credentialResponse.credential`)
4. Frontend envia pro backend: `POST /auth/google { idToken }`
5. Backend valida com `GoogleJsonWebSignature.ValidateAsync` (via `IGoogleTokenValidator`, abstração pra testabilidade)
6. Confere `EmailVerified` + domínio `@camarj.com.br` (email normalizado/trim antes da checagem)
7. Faz lookup em `UsuarioPerfil` (mesma tabela do F5a) — se não encontrar, 403 ("peça a um Admin pra te cadastrar")
8. Emite JWT próprio (assinatura simétrica, claims `sub`/`email`/`name`/`perfil`, expiração 8-12h)
9. Frontend guarda o token e manda `Authorization: Bearer` em toda requisição; **logout automático após 20min de inatividade** e também em qualquer resposta 401

## Decisões de segurança (confirmadas com o usuário em 2026-07-18)

- **Assinatura JWT:** simétrica (`SymmetricSecurityKey`), gerada via `openssl rand -base64 48`, guardada em `user-secrets` (`Auth:JwtSigningKey`)
- **Expiração do token:** 8-12h, sem refresh token
- **Logout por inatividade:** 20 minutos sem interação (mouse/teclado/scroll/click) — ideia do próprio usuário, não estava no design original

## Mapeamento de Contas

Contas são gerenciadas **pelo Admin** via tela `Admin > Usuários` (F5a) — não é mais um mapeamento fixo por setor no código:

| Email (exemplo em teste) | Perfil no Sistema |
|-------|-------------------|
| victor@camarj.com.br | 👑 Admin |
| fabio@camarj.com.br | 🛠️ Atendente |
| (cadastro pelo Admin) | 🙋 Solicitante / 🛠️ Atendente / 👑 Admin |

> Um e-mail `@camarj.com.br` que faz login com Google mas não está cadastrado em `UsuarioPerfil` recebe 403 — precisa ser cadastrado antes pelo Admin.

## Tecnologias usadas

- **Backend:** `Google.Apis.Auth` (validação do token) + `Microsoft.AspNetCore.Authentication.JwtBearer` + `System.IdentityModel.Tokens.Jwt` (emissão do JWT próprio)
- **Frontend:** `@react-oauth/google`
- **Escopos:** `openid`, `profile`, `email` (padrão do botão do Google, sem escopo customizado)

## Pendência real

Falta só o **Client ID real da TI** — configurar via `dotnet user-secrets set "Auth:GoogleClientId" "<valor>"` (backend) e `frontend/.env` com `VITE_GOOGLE_CLIENT_ID=<valor>`. Documento de requisitos não-técnico já entregue à TI em `.specs/features/fase-6-admin-log/oauth-requisitos-ti.md`.

## Relação com [[👥 Perfis de Usuário]]

Perfil Admin/Atendente/Solicitante continua sendo o mesmo — só a fonte de autenticação mudou (de `localStorage`/e-mail digitado para token Google real).
