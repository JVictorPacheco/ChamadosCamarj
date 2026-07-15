---
titulo: "Entender Spec-Driven Development (SDD)"
categoria: "Learning"
data: "2026-07-11"
---

# 📚 SPEC-DRIVEN DEVELOPMENT — GUIA DE APRENDIZADO

## 🧠 O QUE É SDD?

**SDD = Escrever a especificação ANTES de programar**

Em vez de:
```
Ideia → Código → Testes → Erros → Correção
```

Você faz:
```
Especificação Clara → Código → Testes ✅ (primeira vez!)
```

---

## 🎯 POR QUE FUNCIONA?

| Sem SDD | Com SDD |
|---------|---------|
| ❌ Você programa "no escuro" | ✅ Você sabe exatamente o que fazer |
| ❌ Testes vêm depois | ✅ Testes já estão na spec |
| ❌ Cliente quer mudança → Tudo muda | ✅ Spec clara → Menos mudanças |
| ❌ "Achei que era assim..." | ✅ "Está exatamente como combinado" |

---

## 📖 ESTRUTURA PADRÃO SDD

### 1️⃣ **TÍTULO + DESCRIÇÃO**
```markdown
# Feature: Autenticar Usuário

## Descrição
Permite que um usuário faça login usando email e senha.
```

### 2️⃣ **OBJETIVO / POR QUE?**
```markdown
## Por quê?
- Proteger dados da aplicação
- Saber quem é cada usuário
- Registrar ações por usuário
```

### 3️⃣ **REGRAS DE NEGÓCIO**
```markdown
## Regras
1. Email deve ser válido (formato correto)
2. Senha deve ter no mínimo 6 caracteres
3. Máximo 3 tentativas erradas → conta bloqueada por 15 min
4. Usuário deve existir no banco de dados
5. Senha deve estar correta (hash comparado)
```

### 4️⃣ **FLUXO PASSO-A-PASSO**
```markdown
## Fluxo de Sucesso
1. Usuário acessa tela de login
2. Digita email e senha
3. Clica "Entrar"
4. Sistema valida formato do email
5. Sistema busca usuário no banco
6. Sistema compara hash da senha
7. Senha está correta
8. Sistema gera token JWT
9. Redireciona para dashboard
10. Exibe mensagem "Bem-vindo!"

## Fluxo de Erro — Email Inválido
1. Usuário digita "joao@"
2. Clica "Entrar"
3. Sistema valida formato
4. Formato está errado
5. Exibe erro: "Email inválido"
6. Mantém na tela de login

## Fluxo de Erro — Senha Errada
1. Usuário digita email correto mas senha errada
2. Clica "Entrar"
3. Sistema busca usuário (encontrou!)
4. Sistema compara senha (errada!)
5. Incrementa contador de tentativas (1/3)
6. Exibe erro: "Email ou senha incorretos"
7. Mostra quantas tentativas restam (2 restam)
8. Se 3 tentativas = bloqueia por 15 min
```

### 5️⃣ **DADOS DE ENTRADA E SAÍDA**
```markdown
## Dados de Entrada (Request)
```json
{
  "email": "joao@camarj.com.br",
  "senha": "senha123"
}
```

## Dados de Saída — SUCESSO (200 OK)
```json
{
  "success": true,
  "mensagem": "Login realizado com sucesso",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "usuario": {
    "id": "uuid-123",
    "nome": "João Silva",
    "email": "joao@camarj.com.br",
    "perfil": "Cliente"
  }
}
```

## Dados de Saída — ERRO 400 (Email inválido)
```json
{
  "success": false,
  "codigo": "INVALID_EMAIL",
  "mensagem": "Formato de email inválido"
}
```

## Dados de Saída — ERRO 401 (Senha errada)
```json
{
  "success": false,
  "codigo": "INVALID_CREDENTIALS",
  "mensagem": "Email ou senha incorretos",
  "tentativas_restantes": 2
}
```

## Dados de Saída — ERRO 429 (Bloqueado)
```json
{
  "success": false,
  "codigo": "ACCOUNT_LOCKED",
  "mensagem": "Conta bloqueada por tentativas erradas",
  "desbloqueio_em": "2026-07-11T15:30:00Z"
}
```
```

### 6️⃣ **CASOS DE TESTE**
```markdown
## Testes (✅ = deve passar)

### Sucesso
- ✅ T1: Login com dados corretos
  - Email: joao@camarj.com.br
  - Senha: senha123
  - Esperado: Token retornado, redireciona para dashboard

### Erros de Validação
- ✅ T2: Email vazio
  - Email: ""
  - Esperado: Erro "Email obrigatório"

- ✅ T3: Email inválido (sem @)
  - Email: "joaoabc"
  - Esperado: Erro "Formato de email inválido"

- ✅ T4: Senha muito curta
  - Senha: "123"
  - Esperado: Erro "Mínimo 6 caracteres"

### Erros de Negócio
- ✅ T5: Usuário não existe
  - Email: "naoexiste@camarj.com.br"
  - Esperado: Erro "Email ou senha incorretos"

- ✅ T6: Senha incorreta
  - Email: joao@camarj.com.br
  - Senha: senhaErrada123
  - Esperado: Erro "Email ou senha incorretos", contador = 1/3

- ✅ T7: 3 tentativas erradas = bloqueado
  - Tentar 3x com senha errada
  - Esperado: Erro "Conta bloqueada por 15 min"

- ✅ T8: Após 15 min de bloqueio = libera
  - Aguardar 15 minutos
  - Tentar login com senha correta
  - Esperado: Login bem-sucedido
```

### 7️⃣ **EXCEÇÕES / EDGE CASES**
```markdown
## Casos Extremos

### E1: Caracteres especiais na senha
- Entrada: senha com: ç, ñ, @, #, $
- Esperado: Funciona normalmente (qualquer caractere é aceito)

### E2: Espaços em branco
- Entrada: "joao@camarj.com.br " (com espaço)
- Esperado: Sistema remove espaços automaticamente

### E3: Uppercase vs Lowercase em email
- Entrada: "JOAO@CAMARJ.COM.BR"
- Esperado: Sistema converte para minúsculas, encontra usuário

### E4: Tentativas simultâneas
- Cenário: Dois logins ao mesmo tempo
- Esperado: Ambos processam, cada um incrementa contador corretamente

### E5: Banco de dados indisponível
- Cenário: Servidor de BD está offline
- Esperado: Erro 500 "Serviço indisponível, tente mais tarde"
```

---

## 🔗 COMO SDD SE CONECTA COM CODE

### Spec → Tests → Code

**Spec define:**
```
T1: Login com dados corretos → Retorna token
```

**Teste (antes do código):**
```csharp
[Fact]
public async Task Handle_ComCredenciaisCorretas_RetornaToken()
{
    // Arrange
    var usuario = new Usuario("joao@camarj.com.br", "senha123");
    _repositoryMock.Setup(r => r.ObterPorEmailAsync("joao@camarj.com.br"))
        .ReturnsAsync(usuario);

    // Act
    var resultado = await _handler.Handle(
        new LoginCommand("joao@camarj.com.br", "senha123"), 
        CancellationToken.None
    );

    // Assert
    resultado.Should().NotBeNull();
    resultado.Token.Should().NotBeNullOrEmpty();
}
```

**Código (para passar no teste):**
```csharp
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _repository.ObterPorEmailAsync(request.Email);
        
        if (usuario == null)
            throw new NotFoundException("Usuário não encontrado");
        
        if (!usuario.VerificarSenha(request.Senha))
            throw new UnauthorizedException("Senha incorreta");
        
        var token = _jwtService.GerarToken(usuario);
        
        return new LoginResponse 
        { 
            Token = token,
            Usuario = usuario
        };
    }
}
```

---

## ✨ BENEFÍCIOS NA PRÁTICA

### Seu projeto ChamadosCamarj — Fase 6

**Você escreveu a spec PRIMEIRO:**
```
ESPECIFICAÇÃO: Reatribuir Chamado
- Regra 1: Não pode reatribuir para si mesmo
- Regra 2: Deve registrar no histórico
- Teste: Verificar se histórico foi registrado
```

**Resultado:**
✅ Testes passaram de primeira
✅ Código funcionou exatamente como esperado
✅ Cliente viu exatamente o que pediu
✅ Sem retrabalho!

---

## 🎯 EXERCÍCIO PRÁTICO PARA VOCÊ

**Escreva a especificação SDD para:**

"Usuário comum (cliente) NÃO deve ver comentários internos"

**Siga a estrutura:**
1. Título
2. Descrição
3. Regras de negócio
4. Fluxo passo-a-passo
5. Dados entrada/saída
6. Casos de teste
7. Exceções

---

## 📚 RESUMO

| Conceito | O que é | Por quê |
|----------|---------|--------|
| **Spec** | Documento claro antes de codar | Evita erros depois |
| **Testes na Spec** | Casos já definidos | Testes passam de primeira |
| **Linguagem Precisa** | Evitar "talvez", "mais ou menos" | Sem ambiguidades |
| **Fluxo Completo** | Passo-a-passo com sucesso + erros | Código fica robusto |
| **Edge Cases** | Casos extremos já pensados | Sem surpresas depois |

---

**Entendeu o conceito? Qual dúvida? 👍**
