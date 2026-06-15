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
| ResponsavelId | Guid? | Atendente responsável |
| CategoriaId | Guid | FK → Categoria |
| DataAbertura | DateTime | |
| DataLimite | DateTime? | SLA |
| DataConclusao | DateTime? | |
| Origem | enum | Email, Portal, API |

### 💬 Comentario

| Campo | Tipo |
|-------|------|
| Id | Guid |
| ChamadoId | Guid (FK) |
| Autor | string(150) |
| Conteudo | text |
| Tipo | enum: Interno, Publico |
| DataCriacao | DateTime |

### 📂 Categoria

| Campo | Tipo |
|-------|------|
| Id | Guid |
| Nome | string(100) |
| Descricao | string(300) |
| Ativa | bool |

### 📎 Anexo

| Campo | Tipo |
|-------|------|
| Id | Guid |
| ChamadoId / ComentarioId | Guid |
| NomeArquivo | string |
| CaminhoStorage | string |
| TipoArquivo | string (MIME) |
| TamanhoBytes | long |

## Categorias da CAMARJ

1. Autorização
2. Atendimento
3. Super e Tendência
4. Reembolso
5. Financeiro

## Status — Ciclo de Vida

```
Aberto → Em Andamento → Resolvido → Fechado
  ↑                        ↓
  └── Reabrir ←───────────┘
  
Cancelado (só de Aberto/Em Andamento)
```
