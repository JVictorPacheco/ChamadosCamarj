# Spec — Anexos (Supabase Storage)

> Status: EM DESIGN
> Criado em: 2026-07-20
> Fase 4 (metade 1 de 2 — Storage antes de Email, decidido pelo usuário: sem dependência externa nova, dá pra implementar e verificar de ponta a ponta agora)

## Problem Statement

Colaboradores frequentemente precisam anexar evidência a um chamado (print de erro, nota fiscal pra reembolso, planilha). Hoje não existe upload nenhum — `IStorageService`/`Anexo` já existem como esqueleto no código (Fase 1), mas sem implementação nem endpoint.

## Decisões já documentadas (`docs/obsidian/📦 Supabase Storage.md`, preservadas aqui)

| Decisão | Escolha |
|---|---|
| Onde guardar | Supabase Storage (bucket S3-compatible, mesmo projeto do Postgres) |
| Estrutura do bucket | `chamados-anexos/{chamado-id}/{arquivo-uuid}.{ext}` |
| Tamanho máximo | 10MB por arquivo |
| Tipos permitidos | PDF, imagens (jpg/jpeg/png/gif), `.doc`/`.docx`, `.xls`/`.xlsx`, `.zip` |
| Acesso | URLs assinadas, expiram em 1 hora — nunca público direto |
| Autenticação | Só usuários autenticados (já é global desde T09/F5b) |

## Decisões novas confirmadas nesta sessão (2026-07-20)

| Decisão | Escolha | Nota |
|---|---|---|
| Quem pode anexar | Qualquer perfil envolvido no chamado — Solicitante no próprio (`solicitanteEmail`), Atendente/Admin em qualquer um | Seguido por padrão recomendado; **usuário não confirmou explicitamente** (sem resposta na pergunta) — revisar se discordar |
| Remoção de anexo | **Nunca remove** — consistente com a filosofia append-only já usada em chamados/histórico (Fase 8) | Mesmo caso acima — assumido, não confirmado |
| RBAC de verdade no backend | **Não** — mesmo padrão "soft" já usado em `Comentar` (nenhum guard de dono hoje). Anexar não é mais sensível que comentar; não introduz uma exceção nova | Consistente com a decisão de não replicar RBAC real em toda ação (só onde já foi feito: Relatório Mensal, Admin>Usuários, Forçar Encerramento) |

## Out of Scope

| Item | Motivo |
|---|---|
| `RemoverAsync` (a interface já tem o método, resquício do scaffold da Fase 1) | Anexo nunca é removido — método fica sem implementação real, ou é removido da interface durante o Design |
| Anexo em e-mail recebido (Fase 4, metade 2) | Depende do IMAP, que depende de credencial ainda não disponível — spec separada quando chegar a vez |
| Preview de imagem inline no chat/comentário | Só link de download por ora |
| Vírus/malware scanning | Fora de escopo — mitigação é só tipo/tamanho de arquivo |

## User Stories

### P1: Anexar arquivo a um chamado (ao abrir ou depois) ⭐ MVP

**User Story**: Como Solicitante/Atendente/Admin, quero anexar um arquivo (print, nota fiscal, planilha) a um chamado, pra dar evidência sem precisar descrever tudo em texto.

**Acceptance Criteria**:
1. WHEN um arquivo válido (tipo permitido, ≤10MB) é enviado THEN o sistema SHALL fazer upload pro Supabase Storage e criar um registro `Anexo` vinculado ao chamado
2. WHEN o arquivo excede 10MB OU tem um tipo não permitido THEN o sistema SHALL rejeitar antes de fazer upload, com mensagem clara
3. WHEN o chamado não existe THEN o sistema SHALL retornar 404 sem tentar o upload

**Independent Test**: Anexar um PDF de 2MB a um chamado, confirmar que aparece na lista de anexos do Detalhe; tentar anexar um `.exe` ou um arquivo de 15MB e confirmar rejeição.

### P1: Ver e baixar os anexos de um chamado

**User Story**: Como qualquer perfil com acesso ao chamado, quero ver a lista de anexos e baixar cada um.

**Acceptance Criteria**:
1. WHEN o Detalhe do Chamado carrega THEN o sistema SHALL listar os anexos (nome, tamanho, data)
2. WHEN o usuário clica em baixar THEN o sistema SHALL gerar uma URL assinada (expira em 1h) e redirecionar/abrir

**Independent Test**: Subir 2 arquivos, recarregar a página, conferir que os 2 aparecem; clicar em baixar e confirmar que o arquivo abre.

## Requirement Traceability

| Requirement ID | Story | Status |
|---|---|---|
| ANX-01 | Upload com validação de tipo/tamanho | Pending |
| ANX-02 | Registro `Anexo` vinculado ao chamado (ou comentário) | Pending |
| ANX-03 | Listagem de anexos por chamado | Pending |
| ANX-04 | Download via URL assinada (1h) | Pending |

**Coverage:** 4 total, 0 mapped (ver `design.md`).

## Bloqueio conhecido pra verificação de ponta a ponta

Diferente das features anteriores desta sessão (que rodaram contra o Supabase real via token JWT mintado localmente), o Storage do Supabase precisa de uma **credencial própria** (Service Role Key, em Settings > API no dashboard do Supabase) — ainda não configurada em `user-secrets`. Mesma dinâmica do Client ID do Google: o código é implementado e testado com mock; o teste real de upload/download contra o bucket fica pendente até a chave chegar.

**Confirmado em 2026-07-20:** usuário vai atrás da chave em paralelo, sem previsão. Registrado como pendência — seguir a implementação com testes mockados, e fazer a verificação real assim que a chave chegar (mesmo texto de aviso que já existe pro Client ID do Google em `STATE.md`/`HANDOFF.md`).
