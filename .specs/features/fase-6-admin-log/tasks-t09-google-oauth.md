# Tasks — T09/F5b: Login real via Google Workspace

> Ver `design-t09-google-oauth.md` para arquitetura completa e as 3 decisões de segurança já confirmadas (chave simétrica, token 8-12h sem refresh, logout por 20min de inatividade).
> **Status: TODAS as tasks (T09.1-T09.6) implementadas em 2026-07-18.** 177 testes de backend passando, builds limpos nos dois lados, verificado manualmente (401 sem token, Scalar liberado, token do Google inválido tratado com 401 sem crash). **Único bloqueio real restante:** o teste de ponta a ponta com o Google de verdade (login funcionando na prática) depende do Client ID, ainda com a TI.

---

### T09.1 — Backend: endpoint `POST /auth/google` + validação do id_token ✅ Concluída (2026-07-18)

**O quê:** Pacote `Google.Apis.Auth` (NuGet). `AutenticarGoogleCommand(string IdToken)` + Handler: `GoogleJsonWebSignature.ValidateAsync` (Audience = Client ID de config), captura `InvalidJwtException` → `UnauthorizedException` (nova, mapear pra 401 no `ExceptionHandlingMiddleware`). Checagem extra de `EmailVerified` + domínio `@camarj.com.br` (defesa em profundidade). Busca em `IUsuarioPerfilRepository.ObterPorEmailAsync` (reaproveitado do F5a) — não encontrado/inativo → `ForbiddenException` (já existe). `AuthController` novo, endpoint `[AllowAnonymous]`.

**Onde:** `Application/Features/Auth/Commands/`, `WebApi/Controllers/AuthController.cs`, `Common/Exceptions/UnauthorizedException.cs`.

**Depende de:** nada.

**Gate:** `dotnet build` + `dotnet test` (mock do `IUsuarioPerfilRepository`; a chamada real ao Google não é mockável facilmente — cobrir só a lógica em volta, documentar a limitação).

---

### T09.2 — Backend: emissão do JWT próprio ✅ Concluída (2026-07-18)

**O quê:** No mesmo Handler do T09.1, gerar o JWT (claims: `sub`=Id, `email`, `name`, `perfil`) via `System.IdentityModel.Tokens.Jwt`, assinado com `SymmetricSecurityKey` (config `Auth:JwtSigningKey`). Retornar `AutenticacaoResponse(Token, Id, Nome, Email, Perfil)`.

**Onde:** mesmo Handler do T09.1 (ou um `IJwtTokenService` separado, se ficar limpo demais misturar geração de token na lógica de autenticação do Google — decidir no Execute conforme o tamanho do handler).

**Depende de:** T09.1.

**Gate:** teste unitário validando que o token gerado contém os claims esperados e é validável com a mesma chave.

---

### T09.3 — Backend: middleware de autenticação JWT (global) ✅ Concluída (2026-07-18)

**O quê:** `AddAuthentication().AddJwtBearer(...)` com `TokenValidationParameters` (issuer/audience própria, `IssuerSigningKey` simétrica). `AddAuthorizationBuilder().SetFallbackPolicy(RequireAuthenticatedUser)` — **todo endpoint exige token por padrão**. `app.UseAuthentication()`/`app.UseAuthorization()` no pipeline. Marcar `[AllowAnonymous]` explicitamente em `POST /auth/google` e revisar `GET /usuarios/por-email` (ver nota no design — se não tiver mais uso público, tira do `AllowAnonymous`).

**Onde:** `Program.cs`, `AuthController.cs`, `UsuariosController.cs`.

**Depende de:** T09.2.

**Pronto quando:** subir a API sem token e chamar qualquer endpoint protegido dá 401; com um token válido (gerado manualmente num teste), passa.

**Gate:** `dotnet build` + smoke test manual (gerar um token via um endpoint de teste temporário ou script, chamar um endpoint protegido com/sem `Authorization: Bearer`).

---

### T09.4 — Backend: `ICurrentUserService` substitui identidade client-supplied ✅ Concluída (2026-07-18)

**O quê:** Interface `ICurrentUserService` (`UsuarioId`, `Nome`, `Perfil`) lendo `HttpContext.User.Claims`. Registrar como `AddHttpContextAccessor()` + `AddScoped<ICurrentUserService, CurrentUserService>()`. Atualizar os Controllers de Reatribuir/AlterarPrioridade/Resolver/Fechar/Cancelar/AlterarStatus (`ChamadosController`) e os 3 endpoints de `Usuarios` (`UsuariosController`) pra extrair `UsuarioId`/`UsuarioNome`/`PerfilRequisitante` do `ICurrentUserService` em vez de query string/body — **os Commands/Handlers não mudam de forma**, só a fonte no Controller.

**Onde:** `Application/Common/ICurrentUserService.cs`, `WebApi/Services/CurrentUserService.cs`, `ChamadosController.cs`, `UsuariosController.cs`.

**Depende de:** T09.3 (precisa do token já validado e populando `HttpContext.User`).

**Gate:** `dotnet test` (handlers não mudam, testes existentes continuam passando) + smoke test manual (Reatribuir com token, confirmar que o histórico grava o usuário certo, vindo do claim).

---

### T09.5 — Frontend: `GoogleLogin` + `AuthContext.loginComGoogle` + Bearer ✅ Concluída (2026-07-18)

**O quê:** `GoogleOAuthProvider`/`GoogleLogin` (`@react-oauth/google`, instalar) na `LoginPage`, com `hosted_domain="camarj.com.br"`. `AuthContext.loginComGoogle(idToken)` chama `POST /auth/google`, salva `{ token, id, nome, email, perfil }`. `apiFetch` (`lib/api.ts`) passa a enviar `Authorization: Bearer <token>` lido do `localStorage`; em 401, logout automático + redirect pra `/login`.

**Onde:** `frontend/src/auth/LoginPage.tsx`, `AuthContext.tsx`, `frontend/src/lib/api.ts`, `frontend/package.json` (nova dependência).

**Depende de:** T09.1, T09.2 (precisa do endpoint funcionando).

**Gate:** `npm run build` limpo. Teste real de ponta a ponta só é possível com o Client ID real (bloqueio já sinalizado).

---

### T09.6 — Frontend: logout automático por inatividade (20min) ✅ Concluída (2026-07-18)

**O quê:** Hook `useInactivityLogout(minutos, aoExpirar)` (ver esboço completo no `design.md`) — escuta `mousemove`/`keydown`/`click`/`scroll`, reinicia timer a cada evento, chama `logout()` + redirect se expirar. Usado dentro de `AppLayout.tsx` com 20 minutos.

**Onde:** `frontend/src/hooks/useInactivityLogout.ts` (novo), `frontend/src/layouts/AppLayout.tsx`.

**Depende de:** nada (independente do resto — só precisa do `AuthContext.logout()` já existente).

**Gate:** `npm run build` limpo + verificação manual (reduzir o tempo pra ex. 10 segundos temporariamente, confirmar que desloga sozinho).

---

## Ordem de execução

T09.1 → T09.2 → T09.3 → T09.4 (backend, sequencial) → T09.5 (frontend, depende do backend) → T09.6 (frontend, independente, pode ser feito em paralelo com qualquer outro).

## O que fica pendente de configuração real (não é código)

- ~~`Auth:JwtSigningKey`~~ ✅ Gerado e configurado via `dotnet user-secrets` nesta sessão (2026-07-18) — não depende da TI.
- `Auth:GoogleClientId` — vem da TI, ainda não temos. Configurado como placeholder (`"PENDENTE-AGUARDANDO-TI"`) via `dotnet user-secrets` — trocar pelo valor real assim que a TI devolver.
- `VITE_GOOGLE_CLIENT_ID` (frontend) — mesmo Client ID da TI, ainda não configurado (não há `.env` no frontend ainda — criar com esse valor quando a TI responder).

## Critério de aceite final

- Sem token, qualquer endpoint protegido responde 401.
- Login mockado do F5a (`LoginPage` por e-mail) é totalmente substituído pelo botão do Google — não convivem os dois.
- Reatribuir/AlterarPrioridade/etc. gravam no histórico o usuário real do token, não mais o que o cliente mandou.
- 20 minutos sem interação desloga sozinho, mesmo com o token ainda válido.
- E-mail fora de `@camarj.com.br` (mesmo que de alguma forma passasse pelo Google) é rejeitado no backend, não só no picker do Google.
