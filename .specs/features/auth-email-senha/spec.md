# Spec — Login por E-mail e Senha (substitui Google OAuth)

> Status: **CONCLUÍDO** — backend + frontend implementados, 218 testes passando, build limpo.
> Criado em: 2026-07-24. Concluído em: 2026-07-27.
> Decidido com o usuário na mesma sessão em que foi criado — ver `.specs/project/STATE.md` (sessão 2026-07-24) pro contexto completo da conversa.

## Problem Statement

A TI informou ao usuário (Victor) que o Client ID do Google OAuth **está fora do plano da CAMARJ** — não é mais uma questão de "aguardar a TI configurar", é uma mudança de direção. O login real via Google Workspace (T09/F5b, Fase 6) foi implementado por completo e commitado (`8166ff0`, 2026-07-18/19), mas **não vai poder ser usado em produção**.

Decisão: substituir "Sign in with Google" por login tradicional **e-mail + senha**, com o Admin cadastrando o e-mail e a senha inicial de cada usuário (mesmo modelo já usado pra cadastro de usuários desde a F5a — só ganha um campo de senha).

## Decisões confirmadas com o usuário (2026-07-24)

| Decisão | Escolha | Por quê |
|---|---|---|
| Método de login | E-mail + senha, cadastrados pelo Admin | Substitui o botão do Google — Client ID fora do plano da CAMARJ |
| Hash de senha | `PasswordHasher<UsuarioPerfil>` do ASP.NET Core Identity (pacote `Microsoft.Extensions.Identity.Core`, PBKDF2 com salt) | Já vem do ecossistema .NET, sem pacote de terceiros, padrão de mercado |
| Reset de senha (V1) | **Admin redefine manualmente** pela tela `Admin > Usuários` (sem e-mail) | Autoatendimento ("esqueci minha senha") depende de SMTP, que depende da Fase 4 (ainda sem a senha do IMAP) — fica pra depois |
| Código do login Google | **Mantido no backend, dormant** (`POST /auth/google` continua existindo) | Se a decisão da TI mudar no futuro, não precisa reimplementar do zero — só o frontend deixa de mostrar o botão |
| Mensagem de erro no login | Sempre genérica ("E-mail ou senha inválidos"), mesmo pra conta sem senha configurada ou inativa | Evita enumeração de contas cadastradas |

## Out of Scope (por ora)

| Item | Motivo |
|---|---|
| Autoatendimento "esqueci minha senha" via e-mail | Depende de SMTP (Fase 4, ainda sem a senha do IMAP `suporte@`/`ti@camarj.com.br`) |
| Política de complexidade de senha além do mínimo de 8 caracteres | Não pedido, escopo mínimo por ora |
| Rate limiting / bloqueio por tentativas de login | Sistema interno pequeno (5 usuários conhecidos hoje), fora de escopo por ora |
| Remoção do código do login Google (`AutenticarGoogleCommand`, `GoogleTokenValidator`, etc.) | Decidido manter dormant, não remover — ver decisão acima |
| Tela de login (frontend) | Ainda **não implementada** nesta sessão — ver `tasks.md` |

## O que já existia e foi reaproveitado (não é trabalho novo)

- Tabela `UsuariosPerfil` (email/nome/perfil/ativo) — só ganhou uma coluna nova (`SenhaHash`).
- Emissão de JWT (issuer `ChamadosCamarj`, audience `ChamadosCamarj.Frontend`, claims `sub`/`email`/`name`/`perfil`) — já existia dentro do `AutenticarGoogleCommandHandler`, foi **extraída** pra um serviço `IJwtTokenService` compartilhado entre o login Google (dormant) e o login por senha (novo). Nenhuma mudança na chave de assinatura (`Auth:JwtSigningKey`) ou no formato do token — tokens antigos e novos são intercambiáveis.
- Autenticação global (`SetFallbackPolicy(RequireAuthenticatedUser)`), `ICurrentUserService`, RBAC por claim `perfil` — nada disso muda, só a forma de conseguir o token inicial.
- Tela `Admin > Usuários` (CRUD) já existia — só ganha campo de senha na criação + botão de "Redefinir senha".

## User Stories

### P1: Admin cadastra usuário com senha inicial ⭐ MVP

**User Story**: Como Admin, quero definir uma senha ao cadastrar um novo usuário, para que ele consiga fazer login sem depender de e-mail/token.

**Acceptance Criteria**:
1. WHEN o Admin cadastra um usuário com senha válida (≥8 caracteres) THEN o sistema SHALL fazer o hash da senha e salvar o usuário com login habilitado
2. WHEN a senha tem menos de 8 caracteres THEN o sistema SHALL rejeitar com mensagem de validação clara

### P1: Usuário faz login com e-mail e senha ⭐ MVP

**User Story**: Como qualquer perfil (Admin/Atendente/Solicitante), quero entrar com meu e-mail e senha cadastrados, para acessar o sistema sem depender do Google.

**Acceptance Criteria**:
1. WHEN o e-mail e a senha conferem, e o usuário está ativo THEN o sistema SHALL emitir um JWT idêntico em formato ao emitido pelo login Google (mesmos claims/issuer/audience)
2. WHEN o e-mail não existe, a senha não confere, o usuário está inativo, ou não tem senha configurada THEN o sistema SHALL retornar 401 com a mesma mensagem genérica em todos os casos

### P1: Admin redefine a senha de um usuário ⭐ MVP

**User Story**: Como Admin, quero redefinir a senha de qualquer usuário direto pela tela, para resolver "esqueci minha senha" sem precisar de e-mail.

**Acceptance Criteria**:
1. WHEN o Admin define uma nova senha (≥8 caracteres) para um usuário existente THEN o sistema SHALL substituir o hash salvo, sem precisar da senha antiga
2. WHEN quem chama o endpoint não é Admin THEN o sistema SHALL retornar 403

## Requirement Traceability

| Requirement ID | Story | Status |
|---|---|---|
| AUTH-01 | Coluna `SenhaHash` + migration | ✅ Done |
| AUTH-02 | `IJwtTokenService` extraído e reaproveitado | ✅ Done |
| AUTH-03 | `POST /auth/login` (email+senha) | ✅ Done (backend) |
| AUTH-04 | Cadastro de usuário exige senha inicial | ✅ Done (backend) |
| AUTH-05 | `PATCH /usuarios/{id}/senha` (Admin redefine) | ✅ Done (backend) |
| AUTH-06 | Testes unitários atualizados pros novos construtores | ✅ Done (não verificado com `dotnet build`/`dotnet test` ainda — ver `tasks.md`) |
| AUTH-07 | Frontend: tela de login (email+senha) | ✅ Done |
| AUTH-08 | Frontend: campo de senha no cadastro de usuário | ✅ Done |
| AUTH-09 | Frontend: botão "Redefinir senha" no Admin | ✅ Done |

**Coverage:** 9 total, 9 done. Ver `tasks.md` para o detalhe.
