# ChamadosCamarj — Visão do Projeto

## Objetivo

Sistema interno de **gestão de chamados corporativos** da CAMARJ. Colaboradores enviam e-mails ou acessam um portal web para abrir chamados, que são gerenciados pelos atendentes Victor e Fábio.

## Problema que resolve

- Chamados chegavam informalmente por e-mail sem rastreamento
- Sem visibilidade de status, prioridade ou SLA
- Sem histórico de atendimento por categoria

## Usuários

| Perfil | Quem | Acesso |
|--------|------|--------|
| Admin | Victor | Tudo — categorias, usuários, relatórios |
| Atendente | Victor + Fábio | Fila, assumir, resolver, comentar |
| Solicitante | Colaboradores CAMARJ | Abrir via email/portal, ver seus próprios chamados |

## SLAs definidos

| Prioridade | Prazo |
|-----------|-------|
| Urgente | 8h |
| Alta | 24h |
| Média | 12-16h |
| Baixa | 48h |

## Categorias

1. Autorização
2. Atendimento
3. Super e Tendência
4. Reembolso
5. Financeiro

## Stack Decisões

- **Banco (dev e produção):** PostgreSQL via Supabase — mesma instância, conexão via Session pooler
- **Auth:** Google Workspace (Sign in with Google) — corrigido em 2026-06-25, nunca foi Azure AD. Implementado (T09/F5b); falta só o Client ID real da TI
- **Anexos:** Supabase Storage (S3) — **implementado e verificado de ponta a ponta** (2026-07-21), ver `.specs/features/anexos-storage/`
- **Tempo real:** SignalR — implementado desde a Fase 5
- **Email entrada:** MailKit IMAP — planejado, ainda não implementado (Fase 4, metade 2); depende de senha de app das caixas suporte@/ti@camarj.com.br
- **Frontend:** React 19 + TypeScript + Vite + TailwindCSS v4 + Shadcn/ui — implementado
