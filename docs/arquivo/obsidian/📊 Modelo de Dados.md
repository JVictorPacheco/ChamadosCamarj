# 📊 Modelo de Dados

## Entidades

### 🎫 Chamado

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | PK |
| Titulo | string(200) | Resumo do chamado |
| Descricao | text | Descrição detalhada |
| Status | enum | Aberto, EmAndamento, Resolvido, Fechado, Cancelado |
| Prioridade | enum | Baixa, Média, Alta, Urgente |
| SolicitanteNome | string(150) | Nome de quem abriu |
| SolicitanteEmail | string(200) | Email do solicitante |
| ResponsavelId | Guid? | ID do atendente responsável |
| ResponsavelNome | string? | Nome do atendente responsável |
| CategoriaId | Guid | FK → Categoria |
| DataCriacao | DateTime | Data de abertura |
| DataLimite | DateTime? | Prazo SLA calculado automaticamente |
| DataConclusao | DateTime? | Data de encerramento |
| Origem | enum | Email, Portal, API |

### 💬 Comentario

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | PK |
| ChamadoId | Guid (FK) | Chamado ao qual pertence |
| Autor | string(150) | Nome de quem comentou |
| Conteudo | text | Texto do comentário |
| Tipo | enum | **Interno** (Admin/Atendente) ou **Publico** (todos) |
| DataCriacao | DateTime | |

> Comentários `Interno` ✅ filtrados na UI por perfil desde a Fase 6 (`ListarComentariosQueryHandler` recebe `perfilUsuario`).

### 📋 HistoricoEntrada — ✅ implementado (Fase 6)

> Auditoria completa do fluxo de cada chamado. Também é a fonte de dados do [[📈 Relatório Mensal]] (Fase 7) — usa a data real de cada evento, não o status atual do chamado.

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | PK |
| ChamadoId | Guid (FK) | Chamado relacionado |
| UsuarioNome | string | Quem realizou a ação |
| UsuarioId | Guid? | ID do usuário — hoje enviado pelo próprio cliente (`AuthContext` mockado do frontend), pois não há auth real; passa a vir de claims do JWT quando o login Google (T09) entrar |
| Acao | enum | Criado, Assumido, Reatribuido, Resolvido, Fechado, Cancelado, ComentarioAdicionado, PrioridadeAlterada |
| DetalheAnterior | string? | Estado anterior (ex: responsável anterior na reatribuição) |
| DetalheNovo | string? | Estado novo |
| DataHora | DateTime | Quando ocorreu |

### 📂 Categoria

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | PK |
| Nome | string(100) | Nome da categoria |
| Descricao | string(300) | Descrição |
| Ativa | bool | Se aparece nas listagens |

### 📎 Anexo

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | PK |
| ChamadoId / ComentarioId | Guid | Vínculo (chamado ou comentário) |
| NomeArquivo | string | Nome original do arquivo |
| CaminhoStorage | string | Path no Supabase Storage |
| TipoArquivo | string (MIME) | Ex: image/png, application/pdf |
| TamanhoBytes | long | Tamanho em bytes |

---

## Categorias da CAMARJ

| # | Nome |
|---|------|
| 1 | Autorização |
| 2 | Atendimento |
| 3 | Super e Tendência |
| 4 | Reembolso |
| 5 | Financeiro |

---

## Enums

### StatusChamado
```
Aberto | EmAndamento | Resolvido | Fechado | Cancelado
```

### PrioridadeChamado + SLA
```
Baixa (48h) | Media (16h) | Alta (24h) | Urgente (8h)
```

### OrigemChamado
```
Email | Portal | API
```

### TipoComentario
```
Publico | Interno
```

### AcaoHistorico — ✅ implementado (Fase 6)
```
Criado | Assumido | Reatribuido | Resolvido | Fechado | Cancelado | ComentarioAdicionado | PrioridadeAlterada
```

---

## Status — Ciclo de Vida

```
                    ┌─────────────────────────────┐
                    │           ABERTO             │
                    └──────────────┬───────────────┘
                                   │ Assumir (Atendente/Admin)
                    ┌──────────────▼───────────────┐
                    │         EM ANDAMENTO          │◄─── Admin pode Reatribuir aqui
                    └──────────────┬───────────────┘
                                   │ Resolver (Atendente/Admin)
                    ┌──────────────▼───────────────┐
                    │           RESOLVIDO           │
                    └──────────────┬───────────────┘
                                   │ Fechar (Atendente/Admin)
                    ┌──────────────▼───────────────┐
                    │            FECHADO            │
                    └─────────────────────────────┘

Cancelado ← de Aberto ou EmAndamento (qualquer perfil com acesso)
```

---

## Perfis Mock (Fase 3+ — login Google real ainda pendente, ver T09/T15 em [[🗺️ Roadmap]])

| Perfil | ID Mock | Nome | Email |
|--------|---------|------|-------|
| Admin | a1000000-0000-0000-0000-000000000001 | Victor | victor@camarj.com.br |
| Atendente | a2000000-0000-0000-0000-000000000002 | Fábio | fabio@camarj.com.br |
| Solicitante | a3000000-0000-0000-0000-000000000003 | Ana Colaboradora | ana.colaboradora@camarj.com.br |
