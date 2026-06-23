# 🏠 Sistema de Chamados CAMARJ

Bem-vindo ao mapa completo do projeto!

## 🎯 Visão Geral

Sistema interno para **gestão de chamados corporativos** da CAMARJ. Colaboradores enviam e-mails que viram chamados automaticamente.

## 📚 Índice

| Nota | Descrição |
|------|-----------|
| [[📋 SPEC]] | Documento completo do Spec-Driven Development |
| [[🏗️ Arquitetura]] | Clean Architecture + Stack |
| [[📊 Modelo de Dados]] | Entidades, Enums, Relacionamentos |
| [[👥 Perfis de Usuário]] | Admin, Atendente, Solicitante |
| [[📧 Integração Email]] | Captura automática via IMAP/Gmail |
| [[🔐 Azure AD]] | Autenticação corporativa |
| [[📦 Supabase Storage]] | Anexos em bucket S3 |
| [[🗺️ Roadmap]] | Fases do desenvolvimento |
| [[💬 Decisões]] | Decisões tomadas com o Victor |
| [[📝 Perguntas Pendentes]] | O que ainda precisa responder |
| [[⚠️ Concerns]] | Débito técnico e riscos identificados |

---

## 👥 Equipe

- **Victor** — Admin / Desenvolvedor
- **Fábio** — Atendente

## 🔗 Links Rápidos

- [[../SPEC.md|SPEC Oficial]]
- [[../README.md|README do Projeto]]
- [[../../README.md|README Raiz]]

---

## 📍 Onde paramos (2026-06-23)

- ✅ **Fase 2.5 concluída** — backend rodando em PostgreSQL real via Supabase, 3 bugs de teste manual corrigidos (categoria inexistente, transições de status sem validação, erro de concorrência ao comentar). PRs #5 e #6 mergeadas em `develop`.
- ✅ **Fase 3 planejada** — Spec-Driven Development completo (spec + design + 23 tasks) em `.specs/features/frontend-portal-solicitante/`. PR #7 mergeada. `main` e `develop` sincronizados.
- ⏭️ **Próximo passo:** Execute da Fase 3, começando pela T1 (endpoint de backend que falta — `GET /chamados/{id}/comentarios`). Nenhuma linha de código do frontend foi escrita ainda.
- ❓ **Pendente:** decidir se adicionamos 1 teste E2E (Playwright) cobrindo o fluxo feliz completo antes de seguir.
- 📄 Detalhes completos de retomada em `.specs/HANDOFF.md` (na raiz do repo, fora do Obsidian).

---

> *Última atualização: 2026-06-23 — Fase 2.5 concluída (Postgres + bugfixes), Fase 3 planejada (spec/design/tasks), aguardando Execute.*
