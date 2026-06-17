# 📋 SPEC — Sistema de Gestão de Chamados (Ticket System)

> **Versão:** 1.1  
> **Status:** Em desenvolvimento (decisões tomadas)  
> **Metodologia:** Spec-Driven Development (SDD)  
> **Stack sugerida:** .NET 9 + React/TypeScript

---

## 1. VISÃO GERAL

Sistema interno para **gestão de chamados corporativos**. Colaboradores enviam e-mails que **se transformam automaticamente em chamados** na plataforma, permitindo que a equipe de suporte acompanhe, priorize e resolva solicitações de forma organizada.

### Problema Atual
- Chamados chegam por e-mail solto na caixa de entrada
- Sem triagem, sem priorização, sem histórico
- Dificuldade de acompanhar prazos e responsáveis

### Solução
- Sistema web com cadastro de chamados
- Integração com e-mail (IMAP/POP3) para captura automática
- Pipeline completo: Abertura → Triagem → Atendimento → Resolução → Feedback

---

## 2. ARQUITETURA DO SISTEMA

```
┌─────────────────────────────────────────────────────────┐
│                    FRONTEND (React + TS)                │
│  React Router • React Hook Form • TailwindCSS/MUI       │
├─────────────────────────────────────────────────────────┤
│                    API (ASP.NET Core 9)                 │
│  REST + SignalR (notificações em tempo real)            │
├─────────────────────────────────────────────────────────┤
│                    APPLICATION (Clean Architecture)     │
│  MediatR • FluentValidation • AutoMapper • Serilog      │
├─────────────────────────────────────────────────────────┤
│                    INFRASTRUCTURE                       │
│  EF Core + PostgreSQL/MySQL • MailKit (email)           │
│  Redis (cache/fila) • MinIO/S3 (anexos)                 │
└─────────────────────────────────────────────────────────┘
```

### Clean Architecture — Camadas

```
src/
├── Chamados.Domain/          # Entidades, Enums, Interfaces (repository)
│   ├── Entities/
│   │   ├── Chamado.cs
│   │   ├── Comentario.cs
│   │   ├── Anexo.cs
│   │   └── Categoria.cs
│   ├── Enums/
│   │   ├── StatusChamado.cs
│   │   ├── Prioridade.cs
│   │   └── TipoSolicitante.cs
│   └── Interfaces/
│       └── IChamadoRepository.cs
│
├── Chamados.Application/     # Casos de uso (CQRS + MediatR)
│   ├── Features/
│   │   └── Chamados/
│   │       ├── Commands/
│   │       │   ├── AbrirChamadoCommand.cs
│   │       │   ├── AtribuirChamadoCommand.cs
│   │       │   ├── FinalizarChamadoCommand.cs
│   │       │   └── AdicionarComentarioCommand.cs
│   │       ├── Queries/
│   │       │   ├── ListarChamadosQuery.cs
│   │       │   └── ObterChamadoPorIdQuery.cs
│   │       ├── DTOs/
│   │       └── Validators/
│   ├── Common/
│   │   ├── Behaviours/
│   │   └── Exceptions/
│   └── Services/
│       └── EmailReceiverService.cs
│
├── Chamados.Infrastructure/  # EF Core, Repositórios, Email
│   ├── Data/
│   ├── Repositories/
│   └── Email/
│
└── Chamados.WebApi/          # Controllers, Auth, Middleware
    ├── Controllers/
    ├── Hubs/                 # SignalR
    └── Program.cs
```

---

## 3. MODELO DE DADOS

### 3.1 Entidade: Chamado

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | Identificador único |
| Titulo | string (200) | Título/resumo do chamado |
| Descricao | text | Descrição detalhada |
| Status | enum | Aberto, EmAndamento, Resolvido, Fechado, Cancelado |
| Prioridade | enum | Baixa, Média, Alta, Urgente |
| SolicitanteNome | string (150) | Nome de quem abriu |
| SolicitanteEmail | string (200) | Email do solicitante |
| ResponsavelId | Guid? | ID do atendente responsável |
| CategoriaId | Guid | Categoria do chamado |
| DataAbertura | DateTime | Quando foi aberto |
| DataLimite | DateTime? | SLA / prazo |
| DataConclusao | DateTime? | Quando foi resolvido |
| Origem | enum | Email, Portal, Api |
| EmailOrigemId | string? | ID do email original (para rastreio) |

### 3.2 Entidade: Comentario

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | Identificador único |
| ChamadoId | Guid | Chamado vinculado |
| Autor | string (150) | Quem comentou |
| Conteudo | text | Texto do comentário |
| Tipo | enum | Interno, Publico |
| DataCriacao | DateTime | |

### 3.3 Entidade: Categoria

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | |
| Nome | string (100) | Ex: Suporte Técnico, RH, Financeiro |
| Descricao | string (300) | |
| Ativa | bool | |

### 3.4 Entidade: Anexo

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | |
| ChamadoId / ComentarioId | Guid | |
| NomeArquivo | string | |
| CaminhoStorage | string | |
| TipoArquivo | string | MIME type |
| TamanhoBytes | long | |

---

## 4. FLUXO DE UM CHAMADO

```
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│  ENVIO   │ →  │ CAPTURA  │ →  │ TRIAGEM  │ → │ ATEND.   │ → │ RESOLU.  │
│  EMAIL   │    │ AUTOM.   │    │ AUTOM.   │    │          │    │          │
│          │    │          │    │          │    │          │    │          │
│ Colabor. │    │ Serviço  │    │ Cat. +   │    │ Técnico  │    │ + Notif. │
│ enviou   │    │ IMAP     │    │ Priorid. │    │ assume   │    │ email    │
│ para     │    │ lê e     │    │          │    │          │    │          │
│ suporte@ │    │ cria     │    │          │    │          │    │          │
└──────────┘    └──────────┘    └──────────┘    └──────────┘    └──────────┘
```

---

## 5. REGRAS DE NEGÓCIO

1. **Abertura**: Chamado nasce com status `Aberto` e prioridade `Média`
2. **Priorização automática**: Palavras-chave no assunto/body podem alterar prioridade
3. **Triagem**: Primeiro atendente que "assumir" vira responsável
4. **SLA**: Categorias podem ter prazos distintos (ex: Urgente = 4h, Baixa = 48h)
5. **Notificação**: Ao criar/comentar/resolver, email é enviado ao solicitante
6. **Comentários internos**: Não visíveis ao solicitante (apenas atendentes)
7. **Resolução**: Atendente marca como Resolvido → email de feedback pro solicitante
8. **Fechamento automático**: Se solicitante não responder em 72h → Fechado
9. **Anexos**: Máximo de 10MB por arquivo
10. **Cancelamento**: Só pode cancelar se status for Aberto ou Em Andamento

---

## 6. INTEGRAÇÃO COM EMAIL

### Fluxo de captura
```
1. Colaborador envia email para: suporte@empresa.com.br
2. Serviço EmailReceiverService (worker) consulta IMAP a cada 60s
3. Parseia: assunto → título, corpo → descrição, anexos → anexos
4. Cria Chamado via Command (AbrirChamadoCommand)
5. Responde email automaticamente: "Chamado #123 aberto com sucesso!"
```

### Configurações
- Servidor IMAP: configurável (appsettings)
- Criptografia: SSL/TLS
- Filtro: apenas emails NÃO respondidos automaticamente
- Rate limit: max 10 chamados/minuto por remetente

---

## 7. PERFIS DE USUÁRIO

| Perfil | Permissões |
|--------|------------|
| **Solicitante** | Abrir chamado, comentar, ver seus chamados |
| **Atendente** | Tudo do solicitante + assumir, resolver, reabrir, comentários internos |
| **Admin** | Tudo + gerenciar categorias, usuários, relatórios |

---

## 8. API — ENDPOINTS PRINCIPAIS

```
GET    /api/chamados                → Listar (com filtros/paginação)
GET    /api/chamados/{id}           → Detalhe do chamado
POST   /api/chamados                → Abrir chamado
PUT    /api/chamados/{id}/assumir   → Atribuir a um atendente
PUT    /api/chamados/{id}/resolver  → Resolver
PUT    /api/chamados/{id}/fechar    → Fechar
PUT    /api/chamados/{id}/cancelar  → Cancelar
PUT    /api/chamados/{id}/reabrir   → Reabrir

GET    /api/chamados/{id}/comentarios
POST   /api/chamados/{id}/comentarios

GET    /api/categorias
POST   /api/categorias              → (Admin)

GET    /api/chamados/relatorios     → Dashboard/Métricas
```

---

## 9. FRONTEND (React + TypeScript)

### Páginas
- **Login** — Autenticação (pode ser integrado com Google/Azure AD)
- **Dashboard** — Kanban + métricas + últimos chamados
- **Meus Chamados** — Lista do solicitante
- **Painel de Atendimento** — Fila geral (atendentes)
- **Detalhe do Chamado** — Timeline, comentários, anexos
- **Admin** — Categorias, usuários, configurações

### Componentes principais
- `ChamadoCard` — Card do kanban
- `ChamadoTimeline` — Linha do tempo
- `ComentarioBox` — Área de comentários com toggle público/interno
- `FiltroChamados` — Filtros por status, prioridade, data, categoria
- `DashboardCharts` — Gráficos (Recharts ou Chart.js)

---

## 10. TECNOLOGIAS SUGERIDAS

| Camada | Tecnologia | Motivo |
|--------|------------|--------|
| Backend | .NET 9 + Clean Architecture | Você já usa, perfeito |
| ORM | EF Core 9 | Já conhece |
| BD | SQLite (dev) / PostgreSQL (prod) | Dev: zero setup • Prod: robusto |
| CQRS | MediatR | Você tá aprendendo |
| Validação | FluentValidation | Já tem no ContosoPizza |
| Frontend | React + TypeScript + Vite | Moderno, rápido |
| CSS | TailwindCSS + Shadcn/ui | Produtivo, bonito |
| Email | MailKit | Padrão .NET p/ IMAP |
| Tempo real | SignalR | Notificações ao vivo + Push browser |
| Cache | Redis | SLA, filas |
| Storage | Supabase Storage (bucket S3) | Anexos |
| Auth | Azure AD + Microsoft.Identity.Web | Login corporativo |
| Logs | Serilog + Seq / Arquivo | Rastreabilidade |

---

## 11. PRÓXIMOS PASSOS (Fases)

| Fase | O que inclui |
|------|-------------|
| **Fase 1** | Spec aprovado + Domain layer + Migration inicial |
| **Fase 2** | CQRS (Commands/Queries) + Validators + Controller |
| **Fase 3** | Frontend básico (lista + detalhe + abertura) |
| **Fase 4** | Integração com Email (IMAP receiver) |
| **Fase 5** | Kanban + Dashboard + Notificações SignalR |
| **Fase 6** | Autenticação + Perfis + Admin |
| **Fase 7** | Relatórios + SLA + Anexos |

---

## 12. DECISÕES TOMADAS ✅

| Decisão | Resposta |
|---------|----------|
| 🏢 **Empresa** | CAMARJ |
| 📛 **Nome do sistema** | ChamadosCamarj |
| 🔐 **Autenticação** | Azure AD (login corporativo Microsoft) |
| 📧 **Email suporte** | `suporte@camarj.com.br` e `ti@camarj.com.br` (Gmail) |
| 👥 **Atendentes** | Victor + Fábio (2 pessoas) |
| 📂 **Categorias** | Autorização, Atendimento, Super e Tendência, Reembolso, Financeiro |
| ⏱️ **SLA** | Urgente: 8h | Alta: 24h | Média: 12-16h | Baixa: 48h |
| 🗄️ **Anexos** | Supabase Storage (bucket S3) — Solicitantes e Atendentes anexam |
| 🛢️ **BD** | SQLite (dev) → PostgreSQL (produção via Supabase) |
| 🧠 **Metodologia** | Spec-Driven Development (SDD) |
| 📝 **Docs** | Obsidian (mapeamento completo) |
| 🎨 **Frontend** | React + TypeScript + Vite + TailwindCSS + Shadcn/ui |

## 13. PERGUNTAS RESPONDIDAS ✅

- ✅ **Nome do sistema:** ChamadosCamarj
| - ✅ **Notificações internas:** Sim, notificações push (navegador) + desktop (Electron/Tauri no futuro)
| - ✅ **Dashboard:** Sim, gráficos na página inicial (básico)
| - ✅ **Mobile:** Futuramente, por enquanto só web
| - ✅ **SLA Baixa:** 48h | SLA Média: 12-16h | SLA Alta: 24h | SLA Urgente: 8h
| - ✅ **Anexos:** Atendentes também anexam

---

## 14. ROADMAP (Fases)

| Fase | O que inclui | Status |
|------|-------------|--------|
| **Fase 0** | Spec finalizado + setup do projeto + Obsidian | 🔧 Fazendo agora |
| **Fase 1** | Domain layer + Migration inicial | ⏳ |
| **Fase 2** | CQRS (Commands/Queries) + Validators + Controller | ⏳ |
| **Fase 3** | Frontend básico (login, lista, detalhe, abertura) | ⏳ |
| **Fase 4** | Integração com Email (IMAP receiver) | ⏳ |
| **Fase 5** | Kanban + Dashboard + Notificações SignalR | ⏳ |
| **Fase 6** | Azure AD + Perfis + Admin | ⏳ |
| **Fase 7** | Relatórios + SLA + Anexos (Supabase) | ⏳ |

---

> **Status:** ✅ Spec finalizado com decisões tomadas. Próximo passo: **Fase 0 — Setup do projeto no Obsidian e criação da solution .NET**.
