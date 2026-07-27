# Tasks — Login por E-mail e Senha

> Ver `spec.md` para o contexto/decisões. Backend + frontend CONCLUÍDOS em 2026-07-27. 218 testes passando, build limpo.

## ✅ Backend — CONFIRMADO — 218 testes passando

1. **`SenhaHash` na entidade + migration**
   - `src/ChamadosCamarj.Domain/Entities/UsuarioPerfil.cs` — propriedade `SenhaHash` (nullable) + método `DefinirSenhaHash(string)`
   - `src/ChamadosCamarj.Infrastructure/Data/Configurations/UsuarioPerfilConfiguration.cs` — `HasMaxLength(500)` pro novo campo
   - Migration `20260724195834_AddSenhaHashUsuarioPerfil` (`.cs` + `.Designer.cs`) em `src/ChamadosCamarj.Infrastructure/Migrations/` — **atenção**: na primeira tentativa a migration foi criada no lugar errado (`Data/Migrations`, pasta nova) por causa de um `--output-dir` errado no comando `dotnet ef migrations add`; foi apagada e recriada no lugar certo (`Migrations/`, sem `--output-dir`). Já checado que `ApplicationDbContextModelSnapshot.cs` reflete a coluna nova.
   - **Migration ainda NÃO foi aplicada no Supabase real** — só existe localmente. Aplica sozinha no próximo `dotnet run` (o `Program.cs` já roda `MigrateAsync()` automático).

2. **`IJwtTokenService` compartilhado** (evita duplicar a geração de JWT entre Google e senha)
   - Novo: `src/ChamadosCamarj.Application/Common/IJwtTokenService.cs`
   - Novo: `src/ChamadosCamarj.Application/Common/JwtTokenService.cs` (implementação — mesma lógica que já existia dentro do `AutenticarGoogleCommandHandler`, só movida)
   - `src/ChamadosCamarj.Application/Features/Auth/Commands/AutenticarGoogleCommandHandler.cs` — refatorado pra injetar `IJwtTokenService` em vez de gerar o token inline

3. **Login por e-mail/senha**
   - Novo: `src/ChamadosCamarj.Application/Features/Auth/Commands/LoginCommand.cs`
   - Novo: `src/ChamadosCamarj.Application/Features/Auth/Commands/LoginCommandHandler.cs` — usa `IPasswordHasher<UsuarioPerfil>` + `IJwtTokenService`, mensagem de erro sempre genérica, rehash automático se o hasher pedir (`PasswordVerificationResult.SuccessRehashNeeded`)
   - Novo: `src/ChamadosCamarj.Application/Features/Auth/Validators/LoginCommandValidator.cs`
   - `src/ChamadosCamarj.WebApi/Controllers/AuthController.cs` — novo endpoint `POST /auth/login` (`[AllowAnonymous]`, ao lado do `/auth/google` que continua existindo)

4. **Cadastro de usuário exige senha inicial**
   - `src/ChamadosCamarj.Application/Features/Usuarios/Commands/CriarUsuarioPerfilCommand.cs` — novo parâmetro obrigatório `Senha` (posição 4, antes de `PerfilRequisitante`)
   - `src/ChamadosCamarj.Application/Features/Usuarios/Commands/CriarUsuarioPerfilCommandHandler.cs` — injeta `IPasswordHasher<UsuarioPerfil>`, faz hash e chama `DefinirSenhaHash` (tanto no caminho de usuário novo quanto no de reativar um usuário desativado)
   - `src/ChamadosCamarj.Application/Features/Usuarios/Validators/CriarUsuarioPerfilCommandValidator.cs` — regra `Senha` mínimo 8 caracteres

5. **Admin redefine senha de qualquer usuário**
   - Novo: `src/ChamadosCamarj.Application/Features/Usuarios/Commands/RedefinirSenhaCommand.cs`
   - Novo: `src/ChamadosCamarj.Application/Features/Usuarios/Commands/RedefinirSenhaCommandHandler.cs` — guard `PerfilRequisitanteGuard.ExigirAdmin`, 404 se usuário não existe
   - Novo: `src/ChamadosCamarj.Application/Features/Usuarios/Validators/RedefinirSenhaCommandValidator.cs`
   - `src/ChamadosCamarj.WebApi/Controllers/UsuariosController.cs` — novo endpoint `PATCH /usuarios/{id}/senha`

6. **DI (`Program.cs`)**
   - Registrado `IPasswordHasher<UsuarioPerfil>` → `PasswordHasher<UsuarioPerfil>` (Scoped)
   - Registrado `IJwtTokenService` → `JwtTokenService` (Scoped)
   - Pacote NuGet novo: `Microsoft.Extensions.Identity.Core` (adicionado em `ChamadosCamarj.Application.csproj`)

7. **Testes unitários corrigidos** ✅ — confirmado: `dotnet build`/`dotnet test` limpos (218 testes passando)
    - `tests/ChamadosCamarj.UnitTests/Application/Handlers/CriarUsuarioPerfilHandlerTests.cs` — mock de `IPasswordHasher<UsuarioPerfil>` adicionado, todas as chamadas a `new CriarUsuarioPerfilCommand(...)` ganharam o argumento `"SenhaForte123"`
    - `tests/ChamadosCamarj.UnitTests/Application/Handlers/AutenticarGoogleHandlerTests.cs` — construtor do handler agora recebe um `JwtTokenService` real (não mockado, pra preservar a asserção que decodifica o JWT de verdade e confere os claims)
    - `tests/ChamadosCamarj.UnitTests/Application/Validators/CriarUsuarioPerfilValidatorTests.cs` — `ComandoValido()` ganhou a senha; novo teste `Validar_ComSenhaCurtaOuVazia_DeveFalhar`

## ✅ Frontend — CONCLUÍDO

1. ✅ **Tela de login (`frontend/src/auth/LoginPage.tsx`)** — trocado `GoogleLogin`/`GoogleOAuthProvider` por formulário e-mail + senha
2. ✅ **`frontend/src/auth/api.ts`** — nova função `login(email, senha)` → `POST /auth/login`; `autenticarGoogle` mantida dormant
3. ✅ **`frontend/src/auth/AuthContext.tsx`** — novo método `loginComSenha(email, senha)`
4. ✅ **Cadastro de usuário — `frontend/src/features/admin/components/UsuarioFormDialog.tsx`** — campo de senha obrigatório (mín 8 caracteres)
5. ✅ **Redefinir senha — `frontend/src/features/admin/UsuariosPage.tsx`** — botão "Redefinir senha" por linha com modal
6. ✅ **Verificação manual** — build frontend (`npm run build`) verificado: sem erros

## Comandos pra subir o ambiente local (referência rápida)

```powershell
# Backend
cd C:\Users\jpacheco.CAMARJ.001\Projects\ChamadosCamarj
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/ChamadosCamarj.WebApi

# Frontend (outro terminal)
cd C:\Users\jpacheco.CAMARJ.001\Projects\ChamadosCamarj\frontend
npm run dev
```

user-secrets (`ConnectionStrings:DefaultConnection`, `Auth:JwtSigningKey`, `Supabase:Url`/`ServiceRoleKey`) já estão configurados nesta máquina — não precisa reconfigurar.

## Depois de tudo pronto (não esquecer)

- ✅ Atualizar `.specs/project/STATE.md` e `.specs/project/ROADMAP.md` marcando esta feature como concluída — feito em 2026-07-27
- ⏳ Atualizar `docs/obsidian/🔐 Google Workspace.md` (ou criar uma nota equivalente pra login por senha) — a nota atual descreve só o fluxo Google
- ✅ Botão do Google: mantido sem flag — `@react-oauth/google` continua como dependência, código dormant no backend/frontend, não aparece na UI
