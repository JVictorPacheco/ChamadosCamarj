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

- **Banco em produção:** PostgreSQL via Supabase
- **Banco em dev:** SQLite (temporário — ver CONCERNS.md)
- **Auth:** Azure AD (corporativo Microsoft)
- **Anexos:** Supabase Storage (S3)
- **Tempo real:** SignalR (planejado Fase 5)
- **Email entrada:** MailKit IMAP (planejado Fase 4)
- **Frontend:** React + TypeScript + Vite + TailwindCSS + Shadcn/ui (não iniciado)
