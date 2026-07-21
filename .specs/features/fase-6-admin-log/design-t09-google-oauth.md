# Design — T09/F5b: Login real via Google Workspace

> Baseado em `spec.md` (seção F5b) + pesquisa de documentação atual via Context7/Microsoft Learn em 2026-07-18 (ver fontes ao final). Escopo **Complex** (introduz autenticação de verdade pela primeira vez no projeto — ambiguidade de segurança genuína, não só reaproveitamento de padrão existente).
>
> **Status: Design completo, 3 decisões confirmadas pelo usuário em 2026-07-18** (chave simétrica, token 8-12h sem refresh, logout automático por inatividade em 20min). Pronto pra virar `tasks.md` quando o Client ID da TI chegar (bloqueio real de Execute, não do Design/Tasks).

---

## Visão geral do fluxo

```
Frontend (React)                          Backend (.NET)
─────────────────                         ──────────────────
GoogleOAuthProvider (clientId)            
  └─ LoginPage: <GoogleLogin />
       hosted_domain="camarj.com.br"      
       onSuccess(credentialResponse)      
            │                             
            │ credentialResponse.credential
            │ (id_token do Google, um JWT)
            ▼                             
     POST /auth/google { idToken } ──────▶ AutenticarGoogleCommand
                                              │
                                              ▼
                                     GoogleJsonWebSignature.ValidateAsync(
                                       idToken, Audience=[ClientId])
                                              │ (valida assinatura contra
                                              │  chaves públicas do Google,
                                              │  expiração, emissor — tudo
                                              │  dentro da lib, não à mão)
                                              ▼
                                     payload.Email, EmailVerified, Name
                                              │
                                              ▼
                                     ObterPorEmailAsync(payload.Email)
                                     (mesmo repositório do F5a —
                                      IUsuarioPerfilRepository)
                                              │
                              não encontrado / inativo → 403
                                              │ encontrado
                                              ▼
                                     Emitir JWT próprio (claims:
                                     sub=Id, email, nome, perfil)
                                              │
     ◀─────────────────────────────── { token, id, nome, email, perfil }
     AuthContext salva o token
     apiFetch passa a enviar
     Authorization: Bearer <token>
     em toda requisição
```

**Por que trocar o id_token do Google por um JWT próprio, em vez de usar o do Google direto:** o token do Google não tem o campo `perfil` (Admin/Atendente/Solicitante) — isso só existe na nossa tabela `UsuarioPerfil`. Emitir um JWT nosso permite embutir esse claim direto no token, evitando uma consulta ao banco em toda requisição autenticada subsequente (o middleware de auth só decodifica o JWT, não bate no banco).

---

## Backend

### 1. Endpoint `POST /auth/google`

Novo `AuthController`, fora do padrão MediatR só neste caso específico? **Não** — segue o mesmo padrão CQRS do resto do projeto: `AutenticarGoogleCommand` (`Application/Features/Auth/Commands/`) + Handler, `AuthController` só despacha via `IMediator`, igual todos os outros controllers.

```csharp
public record AutenticarGoogleCommand(string IdToken) : IRequest<AutenticacaoResponse>;

public record AutenticacaoResponse(string Token, Guid Id, string Nome, string Email, Perfil Perfil);
```

Handler:
1. Chama `GoogleJsonWebSignature.ValidateAsync(request.IdToken, new ValidationSettings { Audience = [_googleClientId] })` — `_googleClientId` vem de configuração (`appsettings`/env var, não é segredo, é público).
2. Captura `InvalidJwtException` → lança `UnauthorizedException` (nova, mapeada pra 401 — token do Google inválido/expirado/adulterado).
3. **Defesa em profundidade:** mesmo o Google Cloud Console estando configurado como "Internal" (restrito à organização) e o frontend usando `hosted_domain="camarj.com.br"`, o backend confirma de novo — nunca confiar só em restrição client-side. Checar `payload.EmailVerified == true` e `payload.Email` termina em `@camarj.com.br`; se não, `UnauthorizedException`.
4. Busca `payload.Email` via `IUsuarioPerfilRepository.ObterPorEmailAsync` (mesmo repositório do F5a, reaproveitado sem mudança).
5. Se não encontrado ou inativo → `ForbiddenException` (já existe, mapeada pra 403) — mesma semântica do F5a: "e-mail não cadastrado, peça a um Admin".
6. Se encontrado → gera o JWT próprio (ver seção "Emissão do JWT") e retorna `AutenticacaoResponse`.

### 2. Emissão do JWT — biblioteca e claims

Usa `System.IdentityModel.Tokens.Jwt` (já vem transitivamente com `Microsoft.AspNetCore.Authentication.JwtBearer`, não precisa de pacote novo). Claims: `sub` (UsuarioPerfil.Id), `email`, `name`, e um claim customizado `perfil` (Admin/Atendente/Solicitante) — é esse claim que substitui o `perfilRequisitante` enviado hoje via query string.

### 3. Validação do JWT nas requisições — `AddJwtBearer`

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "ChamadosCamarj",
            ValidateAudience = true,
            ValidAudience = "ChamadosCamarj.Frontend",
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Auth:JwtSigningKey"]!)), // ver Decisão 1 (confirmada)
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
```

`SetFallbackPolicy` com `RequireAuthenticatedUser()` — **todo endpoint exige token válido por padrão**, seguindo a recomendação do próprio ASP.NET Core (em vez de proteger endpoint por endpoint com `[Authorize]`, que é fácil de esquecer um). Os únicos endpoints públicos (`[AllowAnonymous]` explícito) são: `POST /auth/google` (óbvio — é ele que gera o token) e `GET /usuarios/por-email` (usado hoje pelo login mockado do F5a — mas ver Decisão 2 sobre o que acontece com essa rota quando o F5b entrar).

`Program.cs` ganha `app.UseAuthentication()` e `app.UseAuthorization()` no pipeline, entre `app.UseCors(...)` e `app.MapControllers()`.

### 4. Substituir `UsuarioId`/`UsuarioNome`/`perfilRequisitante` client-supplied por claims do token

Hoje, Reatribuir/AlterarPrioridade/Resolver/Fechar/Cancelar/AlterarStatus recebem `UsuarioId`/`UsuarioNome` do cliente (aceitável só porque não havia autenticação real — aprendizado já registrado no `STATE.md`), e os 3 Handlers de `Usuarios` recebem `PerfilRequisitante` também do cliente (`PerfilRequisitanteGuard`, criado no débito técnico D-09).

**Introduzir `ICurrentUserService`** (interface em `Application/Common/`, implementação em `WebApi` que lê `HttpContext.User.Claims`):

```csharp
public interface ICurrentUserService
{
    Guid UsuarioId { get; }
    string Nome { get; }
    string Perfil { get; }
}
```

Os Controllers passam a extrair esses valores de `ICurrentUserService` (injetado) em vez de receber via body/query — os Commands/Queries em si **não mudam de forma** (continuam recebendo `UsuarioId`/`UsuarioNome`/`PerfilRequisitante` como campos), só a **fonte** desses valores muda, do cliente pro token. Isso limita o blast radius da mudança: os Handlers e testes existentes continuam funcionando, só os Controllers mudam.

### 5. Configuração (Client ID, chave de assinatura)

`appsettings.json`/`user-secrets`:
```json
{
  "Auth": {
    "GoogleClientId": "<vem da TI, não é segredo>",
    "JwtSigningKey": "<segredo, só em user-secrets/produção>"
  }
}
```

---

## ✅ Decisão 1 (confirmada em 2026-07-18) — Chave de assinatura: simétrica

A documentação oficial do ASP.NET Core recomenda por padrão chaves assimétricas (RSA), mas como a mesma API emite e valida o próprio token (sem um segundo serviço validando de forma independente), o usuário confirmou a escolha pragmática: **chave simétrica (HMAC SHA-256)**, um segredo só, gerado e guardado em `user-secrets`/produção — nunca commitado, nunca em `appsettings.json`. Ver `IssuerSigningKey` na seção 3 acima.

## ✅ Decisão 2 (confirmada em 2026-07-18) — Duração do token: 8-12h, sem refresh

Confirmado: token de vida curta-média (8-12h, dorme "dura o expediente"), sem mecanismo de refresh token no v1 — expirado, a pessoa loga de novo com um clique no Google.

**`GET /usuarios/por-email` (pendência técnica, não é decisão do usuário):** revisar no Execute se esse endpoint ainda tem algum uso público além do login mockado do F5a — se não tiver, sai da lista de `[AllowAnonymous]` e passa a exigir token, fechando uma porta que hoje fica aberta sem necessidade.

## ✅ Decisão 3 (confirmada em 2026-07-18) — Logout automático por inatividade: 20 minutos

Além da expiração absoluta do token (8-12h, protege contra token vazado/roubado), o usuário pediu um segundo mecanismo, **independente e mais agressivo**: se a pessoa ficar sem interagir com a tela (mouse, teclado, clique, scroll) por **20 minutos**, o sistema desloga sozinho — protege contra alguém deixar o computador desbloqueado com a aba aberta, mesmo que o token em si ainda fosse válido por horas.

**Implementação (só frontend, não precisa de nada novo no backend):** hook `useInactivityLogout(minutos: number)`, registrado uma vez dentro do `AppLayout` (o shell que envolve todas as rotas protegidas). Escuta os eventos `mousemove`, `keydown`, `click`, `scroll` no `window`, reinicia um timer a cada evento; se o timer chegar a zero, chama `logout()` do `AuthContext` e redireciona pra `/login` — igual ao botão "Sair" já existente, só que disparado automaticamente. Sem chamada de rede, sem dependência do estado do token no backend — puramente client-side.

```tsx
// frontend/src/hooks/useInactivityLogout.ts (esboço)
export function useInactivityLogout(minutos: number, aoExpirar: () => void) {
  useEffect(() => {
    let timer: ReturnType<typeof setTimeout>
    const reiniciar = () => {
      clearTimeout(timer)
      timer = setTimeout(aoExpirar, minutos * 60_000)
    }
    const eventos = ['mousemove', 'keydown', 'click', 'scroll'] as const
    eventos.forEach((evento) => window.addEventListener(evento, reiniciar))
    reiniciar()
    return () => {
      clearTimeout(timer)
      eventos.forEach((evento) => window.removeEventListener(evento, reiniciar))
    }
  }, [minutos, aoExpirar])
}
```

Uso em `AppLayout.tsx`:
```tsx
const { logout } = useAuth()
const navigate = useNavigate()
useInactivityLogout(20, () => {
  logout()
  navigate('/login')
})
```

---

## Frontend

### `AuthContext.tsx` — reescrito de novo (2ª vez, primeira foi no F5a)

- `login(email)` (F5a) é substituído por `loginComGoogle(idToken: string)`, que faz `POST /auth/google` e salva `{ token, id, nome, email, perfil }` no `localStorage` (mesma chave).
- **`apiFetch` (`lib/api.ts`) ganha o header `Authorization: Bearer <token>`** automaticamente, lido do `localStorage`, em toda chamada — hoje nenhuma chamada manda esse header (não havia necessidade).
- Tratar 401 globalmente em `apiFetch`: se a API responder 401 (token expirado/inválido), fazer logout automático e redirecionar pro login — evita a pessoa ficar vendo erros genéricos com um token morto.

### `LoginPage.tsx` — `GoogleLogin` no lugar do campo de e-mail

```tsx
<GoogleOAuthProvider clientId={import.meta.env.VITE_GOOGLE_CLIENT_ID}>
  <GoogleLogin
    hosted_domain="camarj.com.br"
    onSuccess={(credentialResponse) => loginComGoogle(credentialResponse.credential!)}
    onError={() => setErro('Não foi possível entrar com Google.')}
  />
</GoogleOAuthProvider>
```

`VITE_GOOGLE_CLIENT_ID` — variável de ambiente nova, vem do Client ID que a TI devolver (documento já entregue).

### O que NÃO muda

- A tela `Admin > Usuários` (F5a) continua igual — é o Admin quem cadastra e-mail→perfil, isso não depende de como o login acontece.
- `UsuarioPerfil` (tabela) não muda de schema.

---

## Requirement Traceability (novo, dentro do escopo F5b)

| ID | O quê | Componente |
|---|---|---|
| T09.1 | Endpoint `POST /auth/google` + validação do id_token do Google | `AutenticarGoogleCommand`/Handler, `Google.Apis.Auth` |
| T09.2 | Emissão de JWT próprio com claim de perfil | Handler + `System.IdentityModel.Tokens.Jwt` |
| T09.3 | Middleware de autenticação JWT (`AddJwtBearer` + fallback policy) | `Program.cs` |
| T09.4 | `ICurrentUserService` + Controllers atualizados (Reatribuir/AlterarPrioridade/Resolver/Fechar/Cancelar/AlterarStatus/Usuarios) | WebApi |
| T09.5 (T15 no spec original) | Frontend: `GoogleOAuthProvider`/`GoogleLogin`, `AuthContext.loginComGoogle`, `apiFetch` com Bearer + logout automático em 401 | Frontend |
| T09.6 | Logout automático por inatividade (20min) — `useInactivityLogout` | Frontend, `AppLayout.tsx` |

---

## Fontes consultadas (2026-07-18)

- Microsoft Learn: "Configure JWT bearer authentication in ASP.NET Core" (recomendações de chave assimétrica, nunca emitir token de usuário/senha, uso de `SetFallbackPolicy`)
- Microsoft Learn: "Authentication and authorization in Minimal APIs" (`AddJwtBearer` básico)
- Context7 `/googleapis/google-api-dotnet-client`: API real de `GoogleJsonWebSignature.ValidateAsync` (`Google.Apis.Auth`)
- Context7 `/momensherif/react-oauth`: API real de `GoogleOAuthProvider`/`GoogleLogin`, incluindo o prop `hosted_domain`
