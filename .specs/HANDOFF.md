# Handoff

**Date:** 2026-07-14
**Feature:** Fase 6 — Admin Completo + Log + Google Workspace
**Task:** T01-T14 concluídos e verificados via Playwright. Falta decidir o próximo passo (T09/T15 ou Fase 4).

## Completed ✓

- Reescrita completa do frontend da Fase 6 (T10-T14): `ReatribuirModal`, `AlterarPrioridadeModal`, `TimelineHistorico` criados em `frontend/src/features/chamados/components/`; `ComentarioForm`/`ComentarioList` estendidos pra comentário interno. Arquivos órfãos em `src/ChamadosCamarj.Web/` apagados.
- Componentes shadcn instalados via CLI: `dialog`, `checkbox`, `radio-group`.
- Seção "Frontend" adicionada em `.specs/codebase/CONVENTIONS.md`.
- Bug corrigido: endpoint `GET /comentarios` não repassava `perfilUsuario` pra query (filtro de interno nunca disparava).
- Bug corrigido: migration `AddHistoricoEntrada` estava incompleta (sem `.Designer.cs`/snapshot) — travava o startup da API. Regenerada via `dotnet ef migrations add` e sincronizada com o Postgres local (Docker).
- Bug corrigido: histórico gravava `"Sistema"` fixo em Reatribuir/AlterarPrioridade/Resolver/Fechar/Cancelar. Adicionado `UsuarioId`/`UsuarioNome` nesses 5 commands + endpoints; frontend (`useAcoesChamado.ts`) envia `perfil.id`/`perfil.nome` do `AuthContext` automaticamente.
- Verificado ponta a ponta via Playwright ad-hoc (script temporário, removido): reatribuir, alterar prioridade, histórico com usuário real, comentário interno oculto do Solicitante. Testes unitários (96) continuam passando.

## In Progress

Nada em execução.

## Pending

1. Criar PR de `feature/fase-6-admin-log` → `develop`.
2. Decidir: T09/T15 (login Google Workspace real) ou Fase 4 (Email/Storage) em seguida.
3. Quando T09 entrar: trocar `UsuarioId`/`UsuarioNome` (hoje client-supplied, aceitável só por não haver auth real) por extração via claims do JWT no backend.
4. "Forçar encerramento" (Admin fechar/cancelar fora do fluxo normal) — item da spec da Fase 6 ainda não abordado.

## Blockers

Nenhum.

## Context

- Branch local atual: `feature/fase-6-admin-log` (checkout feito nesta sessão a partir do remoto).
- API e frontend rodando em background durante a sessão (`dotnet run` na porta 5000, `npm run dev` na porta 5173) — confirmar se ainda estão de pé antes de continuar, ou reiniciar.
- Banco: Postgres local via Docker (`chamados-postgres`, docker-compose.yml na raiz), não o Supabase compartilhado dev/prod mencionado nas decisões antigas — checar `dotnet user-secrets list` no `ChamadosCamarj.WebApi` se precisar confirmar a conexão ativa.
- Decisões-chave em `.specs/project/STATE.md` (seção Aprendizados, entradas de 2026-07-13/14) e `.specs/codebase/CONVENTIONS.md` (seção Frontend, nova).
