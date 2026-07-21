# Design — F5a: Login mockado por e-mail + Cadastro de Usuários (Admin)

> Escopo: sub-feature de `spec.md` (F5a). Criado em 2026-07-15.
> Precede F5b (login real Google Workspace) — ver spec.md para a relação entre os dois.

---

## Visão geral

```
Backend (novo)                          Frontend (novo/alterado)
─────────────────                       ──────────────────────────
Domain
  Enum Perfil (Admin|Atendente|         AuthContext.tsx (reescrito)
   Solicitante)                          - login(email) busca perfil via API
  UsuarioPerfil : BaseEntity              - mantém persistência em localStorage
  IUsuarioPerfilRepository                  (agora guarda o UsuarioPerfil retornado,
                                             não mais um dos 3 fixos)
Infrastructure
  UsuarioPerfilConfiguration (EF)        LoginPage.tsx (novo, substitui ProfileSelector.tsx)
  UsuarioPerfilRepository                  - campo de e-mail + botão "Entrar"
  Migration AddUsuarioPerfil               - trata 404 com mensagem de erro
  DatabaseSeeder: seed Victor/Fábio

Application/Features/Usuarios (novo)    UsuariosPage.tsx (novo, rota Admin-only)
  Commands: CriarUsuarioPerfil,           - tabela de usuários
    AtualizarUsuarioPerfil                - formulário criar/editar (email, nome, perfil, ativo)
  Queries: ListarUsuariosPerfil,
    ObterUsuarioPerfilPorEmail            api.ts: criarUsuario(), atualizarUsuario(),
  Validators (FluentValidation)            listarUsuarios(), obterUsuarioPorEmail()

WebApi
  UsuariosController
    GET  /api/usuarios
    POST /api/usuarios
    PUT  /api/usuarios/{id}
    GET  /api/usuarios/por-email
```

---

## Domain

### Enum `Perfil`

Segue o padrão de `AcaoHistorico.cs` (`[JsonConverter(typeof(JsonStringEnumConverter))]`):

```csharp
namespace ChamadosCamarj.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Perfil
{
    Admin = 1,
    Atendente = 2,
    Solicitante = 3
}
```

Motivo de criar o enum agora (e não manter perfil como `string` solto, como em `ListarComentariosQueryHandler.PerfilUsuario`): `UsuarioPerfil` é a primeira entidade persistida que representa perfil como dado estruturado — vale tipar. Os usos legados de `string perfilUsuario` (query params, ex: `ListarComentariosQuery`) **não são migrados** neste escopo — fora de escopo do F5a, risco desnecessário de regressão em código já funcionando.

### Entidade `UsuarioPerfil`

Segue o padrão de `Categoria.cs` (construtor validando invariantes, setters privados, métodos de mutação em vez de setters públicos):

```csharp
namespace ChamadosCamarj.Domain.Entities;

public class UsuarioPerfil : BaseEntity
{
    private UsuarioPerfil() { }

    public UsuarioPerfil(string email, string nome, Perfil perfil)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("E-mail é obrigatório.", nameof(email));
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));

        Email = email.Trim().ToLowerInvariant();
        Nome = nome;
        Perfil = perfil;
        Ativo = true;
    }

    public string Email { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public Perfil Perfil { get; private set; }
    public bool Ativo { get; private set; }

    public void Atualizar(string nome, Perfil perfil)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));

        Nome = nome;
        Perfil = perfil;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        Ativo = false;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Ativar()
    {
        Ativo = true;
        DataAtualizacao = DateTime.UtcNow;
    }
}
```

**Decisão:** e-mail normalizado (`Trim().ToLowerInvariant()`) no construtor — evita duplicidade tipo `Fabio@camarj.com.br` vs `fabio@camarj.com.br`. Unicidade garantida por índice único no EF Configuration (case já normalizado, então `UNIQUE` simples resolve).

### `IUsuarioPerfilRepository`

```csharp
public interface IUsuarioPerfilRepository
{
    Task<UsuarioPerfil?> ObterPorEmailAsync(string email, CancellationToken ct);
    Task<UsuarioPerfil?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<UsuarioPerfil>> ListarAsync(CancellationToken ct);
    Task AdicionarAsync(UsuarioPerfil usuario, CancellationToken ct);
    Task AtualizarAsync(UsuarioPerfil usuario, CancellationToken ct);
}
```

---

## Infrastructure

- `UsuarioPerfilConfiguration : IEntityTypeConfiguration<UsuarioPerfil>` — índice único em `Email`, `Perfil` armazenado como string (`HasConversion<string>()`, mesmo padrão usado pelos outros enums no projeto — confirmar em `ChamadoConfiguration` antes de implementar).
- Migration `AddUsuarioPerfil` — **atenção ao aprendizado registrado no `STATE.md`**: uma migration só é válida com os 3 artefatos em sincronia (`.cs`, `.Designer.cs`, `ModelSnapshot.cs`). Gerar via `dotnet ef migrations add`, nunca escrever à mão.
- `DatabaseSeeder`: adicionar seed idempotente (checar `if (!await context.UsuariosPerfil.AnyAsync())`) com Victor (`victor@camarj.com.br`, Admin) e Fábio (`fabio@camarj.com.br`, Atendente) — os mesmos dois usuários hoje hardcoded em `AuthContext.tsx`.

---

## Application (CQRS)

Pasta `Features/Usuarios/`, mesma estrutura de `Features/Categorias/`:

- `Commands/CriarUsuarioPerfilCommand` (+ Handler + Validator): valida e-mail único, perfil válido
- `Commands/AtualizarUsuarioPerfilCommand` (+ Handler + Validator): atualiza nome/perfil/ativo por Id
- `Queries/ListarUsuariosPerfilQuery` (+ Handler): retorna todos, ordenado por nome
- `Queries/ObterUsuarioPerfilPorEmailQuery` (+ Handler): retorna 1 ou `null` (controller traduz `null` → 404)
- `DTOs/UsuarioPerfilResponse`: mapeamento manual, mesmo padrão de `CategoriaResponse`

**Autorização (só Admin cria/edita):** sem JWT ainda, a checagem segue o mesmo padrão soft já usado no resto do app pré-auth real — o cliente informa o perfil de quem está fazendo a ação (ex: query param ou header `X-Perfil-Requisitante`), e o Handler rejeita com 403 se não for Admin. **Isso é uma limitação aceita e documentada**, igual ao restante do sistema hoje (ver `STATE.md` → Aprendizados: "Sem autenticação real, comandos que alteram estado não têm de onde tirar quem está fazendo isso"). Não introduzir JWT parcial aqui — isso é exatamente o escopo do F5b.

---

## WebApi

`UsuariosController`:
```
GET  /api/usuarios?perfilRequisitante=Admin        → 200 [UsuarioPerfilResponse], 403 se não-Admin
POST /api/usuarios?perfilRequisitante=Admin        → 201, 403 se não-Admin, 409 se e-mail duplicado
PUT  /api/usuarios/{id}?perfilRequisitante=Admin   → 204, 403 se não-Admin, 404 se não existe
GET  /api/usuarios/por-email?email=...             → 200 UsuarioPerfilResponse, 404 se não encontrado/inativo
```

O último endpoint (`por-email`) é público (sem checagem de perfil) — é o que a `LoginPage` usa antes de a pessoa "estar logada".

---

## Frontend

### `AuthContext.tsx` (reescrito)

- Remove o dicionário fixo `PERFIS` e `ATENDENTES` hardcoded.
- `login` passa a ser assíncrono: `login(email: string) => Promise<void>` — chama `GET /usuarios/por-email`, em sucesso salva `{ id, tipo, nome, email }` no `localStorage` (mesma chave `STORAGE_KEY`), em 404 lança erro tratado pela `LoginPage`.
- `ATENDENTES` (usado hoje na UI de Reatribuição) passa a vir de `GET /usuarios` filtrado por perfil `Atendente`/`Admin`, via query própria (`useAtendentes()` hook) — não mais array estático.

### `LoginPage.tsx` (novo, substitui `ProfileSelector.tsx`)

- Campo de e-mail + botão "Entrar".
- Chama `login(email)`; em erro, mostra mensagem inline ("e-mail não cadastrado — peça a um Admin para te cadastrar").
- Mantém o visual atual (logo Camarj, mesmo layout) — só troca os 3 cards de perfil por um form de e-mail.

### `UsuariosPage.tsx` (novo)

- Rota `/admin/usuarios`, protegida (Admin only — mesmo padrão de RBAC de UI soft já usado em outras telas, ex: Relatório Mensal antes do bloqueio real; **decidir caso a caso se este precisa de bloqueio real** já que expõe cadastro de acesso, não só dado sensível — sinalizar para o usuário no Execute).
- Tabela de usuários (email, nome, perfil, ativo) + botão "Novo usuário" → formulário (Dialog/Sheet do shadcn) com os 3 campos + toggle ativo.
- Reaproveita padrões existentes de formulário do projeto (ex: `AbrirChamadoPage`) — não inventar um novo padrão de form.

---

## Fora de escopo (F5a)

- Senha / hash / verificação de identidade — não existe neste escopo, é mock
- JWT / middleware de autenticação — escopo do F5b
- Migração dos usos legados de `string perfilUsuario` espalhados pelo código para o novo enum `Perfil`
