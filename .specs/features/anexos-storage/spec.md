# Spec — Anexos (Supabase Storage)

> Status: IMPLEMENTADA E VERIFICADA DE PONTA A PONTA (2026-07-21) — upload/listagem/download real confirmados contra o Supabase Storage
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
| ~~Remoção de anexo: Nunca remove~~ | **REVERTIDO em 2026-07-24, a pedido explícito do usuário** — ver seção "Decisões novas (2026-07-24)" abaixo. Anexo agora PODE ser removido (exclusão real, Storage + banco) | Decisão assumida aqui em 2026-07-20 nunca tinha sido confirmada; o usuário pediu a remoção ao testar a feature de verdade |
| RBAC de verdade no backend (upload) | **Não** — mesmo padrão "soft" já usado em `Comentar` (nenhum guard de dono hoje). Anexar não é mais sensível que comentar; não introduz uma exceção nova | Consistente com a decisão de não replicar RBAC real em toda ação (só onde já foi feito: Relatório Mensal, Admin>Usuários, Forçar Encerramento). **Nota:** a *remoção* de anexo, diferente do upload, ganhou RBAC real — ver abaixo |

## Decisões novas (2026-07-24 — remoção de anexo)

> A pedido do usuário, testando a feature já em produção contra o Supabase real.

| Decisão | Escolha |
|---|---|
| Tipo de exclusão | **Real** (hard delete) — remove o arquivo do Supabase Storage **e** a linha da tabela `Anexos`. Não é soft-delete/arquivamento — decisão explícita do usuário ("foi um anexo errado, não teria sentido ser soft delete") |
| Confirmação | Pop-up (`Dialog`) perguntando "tem certeza?" antes de remover, mostrando o nome do arquivo |
| RBAC de quem pode remover | **Real, no backend** (diferente do upload, que é soft): Admin remove qualquer anexo; Atendente/Solicitante só removem os anexos que **eles mesmos enviaram** (compara `EnviadoPorId`, vindo do token, com o requisitante) — 403 se não for dono nem Admin |
| `IStorageService.RemoverAsync` | Reintroduzido (tinha sido removido da interface em 2026-07-20 por não ter consumidor — ver `design.md`/`tasks.md` originais). Implementado em `SupabaseStorageService` (`Storage.From(bucket).Remove(...)`) e em `NullStorageService` (mesmo fallback dos outros métodos) |
| Contrato aditivo | `AnexoResponse` ganhou `EnviadoPorId` (Guid?) — usado pelo frontend só pra decidir se mostra o botão "Remover"; não quebra nada que já consumia o DTO |

## Out of Scope

| Item | Motivo |
|---|---|
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

### P1: Remover anexo ⭐ (adicionada em 2026-07-24)

**User Story**: Como Admin (qualquer anexo) ou Atendente/Solicitante (só os que eu mesmo enviei), quero excluir um anexo enviado por engano, pra ele parar de aparecer no chamado.

**Acceptance Criteria**:
1. WHEN o usuário clica em "Remover" e confirma no pop-up THEN o sistema SHALL apagar o arquivo do Supabase Storage e o registro `Anexo` do banco (exclusão real, sem desfazer)
2. WHEN quem tenta remover não é Admin nem o autor original do anexo THEN o sistema SHALL retornar 403, sem apagar nada
3. WHEN o botão "Remover" é renderizado no frontend THEN o sistema SHALL só mostrá-lo pra Admin ou pro autor do anexo (evita um clique que só vai dar 403)

**Independent Test**: Como Atendente, tentar remover um anexo enviado por outro Atendente — botão nem aparece; como Admin, remover qualquer anexo — some da lista, do banco e do bucket.

## Requirement Traceability

| Requirement ID | Story | Status |
|---|---|---|
| ANX-01 | Upload com validação de tipo/tamanho | Done |
| ANX-02 | Registro `Anexo` vinculado ao chamado (ou comentário) | Done |
| ANX-03 | Listagem de anexos por chamado | Done |
| ANX-04 | Download via URL assinada (1h) | Done |
| ANX-05 | Remoção real de anexo, com RBAC (Admin ou autor) | Done (2026-07-24) |

**Coverage:** 5 total, 5 verificados de ponta a ponta contra o Supabase Storage real (upload/listagem/download em 2026-07-21; remoção em 2026-07-24).

## Bloqueio resolvido — verificação de ponta a ponta (2026-07-21)

Service Role Key obtida pelo usuário (aba "Legacy anon, service_role API keys" do dashboard — o Supabase tem um formato de chave novo, `sb_secret_...`, mas o SDK C# usado aqui é da geração anterior, então a legada/JWT é a compatível) e configurada em `user-secrets`. Bucket `chamados-anexos` criado via chamada direta à Storage REST API.

**Bug real encontrado e corrigido nesta verificação:** o SDK `Supabase` v1.3.0 devolve a URL de `CreateSignedUrl` com um `?` sobrando no final, que quebra o parse do JWT no servidor do Storage (`InvalidJWT`). Corrigido com `url.TrimEnd('?')` em `SupabaseStorageService`. Ver `tasks.md` pro detalhe completo.

Upload real de um PDF, listagem, geração de URL assinada e **download real do arquivo (conteúdo conferido byte a byte)** — tudo confirmado contra o Supabase Storage de verdade. 216 testes de backend passando depois do fix. Feature 100% funcional, sem pendências.
