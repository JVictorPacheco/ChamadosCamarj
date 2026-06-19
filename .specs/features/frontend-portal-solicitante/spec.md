# Fase 3 — Portal do Solicitante (Frontend) Specification

> Decisões confirmadas com o usuário em 2026-06-19. Ver `.specs/project/STATE.md` para o estado geral do projeto.

## Problem Statement

O backend (Fase 2.5) está completo e funcional, mas só pode ser testado via Scalar/curl. A CAMARJ precisa de uma interface web pra que colaboradores (Solicitantes) abram chamados, acompanhem o status e comentem — sem depender de e-mail informal. A autenticação corporativa real (Azure AD) depende de acesso ao tenant da empresa, que ainda não está disponível, então a autenticação é mockada nesta fase para não bloquear o desenvolvimento da UI.

## Goals

- [ ] Colaborador consegue abrir, listar e acompanhar seus próprios chamados via navegador
- [ ] Troca de perfil mockado (Admin/Atendente/Solicitante) funciona sem Azure AD real
- [ ] Troca futura pro Azure AD real exige só reescrever o contexto de autenticação — zero retrabalho nas telas

## Out of Scope

| Feature | Reason |
|---------|--------|
| Ações de Atendente (fila geral, assumir, resolver, fechar, cancelar) | Decisão do usuário: fatia menor primeiro. Fica pra quando o Kanban (Fase 5) for feito |
| Autenticação real via Azure AD / MSAL | Depende de acesso ao tenant da empresa, ainda não disponível |
| Upload de anexos | `IStorageService` não implementado no backend ainda (Fase 4) |
| Notificações em tempo real | SignalR planejado pra Fase 5 |
| Admin (categorias, usuários) | Fora do escopo do Solicitante |

---

## Pré-requisito de backend (descoberto durante o spec)

`ChamadoResponse` hoje só expõe `QuantidadeComentarios` (int), não o conteúdo dos comentários. A tela de Detalhe do Chamado precisa listar os comentários públicos de verdade. Duas opções:

- **Opção A (escolhida):** novo endpoint `GET /api/chamados/{id}/comentarios` retornando `ComentarioResponse[]` (Id, Autor, Conteudo, Tipo, DataCriacao) — filtrando `Tipo == Publico` para o Solicitante.

Essa é uma tarefa de backend pequena (Query + Handler + endpoint, seguindo o padrão CQRS já existente) que entra como requisito API-01 abaixo, antes das tarefas de frontend que dependem dela.

---

## User Stories

### P1: Selecionar perfil mockado ⭐ MVP

**User Story**: Como qualquer usuário do sistema, quero escolher "entrar como" Admin, Atendente ou Solicitante, para testar a interface sem depender do Azure AD real.

**Why P1**: Sem isso, nenhuma tela autenticada pode ser testada.

**Acceptance Criteria**:
1. WHEN o usuário acessa a aplicação sem sessão ativa THEN o sistema SHALL mostrar uma tela de seleção de perfil mockado
2. WHEN o usuário escolhe um perfil THEN o sistema SHALL salvar a escolha (localStorage) e redirecionar para a área correspondente
3. WHEN o usuário já tem um perfil mockado salvo THEN o sistema SHALL pular a tela de seleção e ir direto pra aplicação
4. WHEN o usuário troca de perfil (ex: botão "sair") THEN o sistema SHALL limpar a sessão mockada e voltar pra seleção

**Independent Test**: Selecionar "Solicitante", ver nome mockado no header, clicar "sair", voltar pra tela de seleção.

---

### P1: Abrir chamado (portal) ⭐ MVP

**User Story**: Como Solicitante, quero abrir um chamado preenchendo título, descrição e categoria, para que o time de suporte seja notificado do meu problema.

**Why P1**: É a entrada principal do sistema — sem isso não há chamados pra acompanhar.

**Acceptance Criteria**:
1. WHEN o Solicitante preenche o formulário com dados válidos e envia THEN o sistema SHALL chamar `POST /api/chamados` e redirecionar para o detalhe do chamado criado
2. WHEN a categoria selecionada não existe mais (404 da API) THEN o sistema SHALL mostrar mensagem amigável e recarregar a lista de categorias
3. WHEN algum campo obrigatório está vazio (400 da API) THEN o sistema SHALL mostrar o erro de validação ao lado do campo correspondente
4. WHEN o formulário é enviado com sucesso THEN o sistema SHALL preencher `solicitanteNome`/`solicitanteEmail` automaticamente a partir do perfil mockado ativo (sem o usuário digitar)

**Independent Test**: Abrir um chamado com categoria "Autorização", ver o redirecionamento pro detalhe com status "Aberto".

---

### P1: Listar meus chamados com filtros ⭐ MVP

**User Story**: Como Solicitante, quero ver a lista dos meus chamados com filtros por status/categoria, para acompanhar o andamento sem precisar perguntar ao suporte.

**Why P1**: É o uso recorrente do sistema (não só abrir, mas acompanhar).

**Acceptance Criteria**:
1. WHEN o Solicitante acessa "Meus Chamados" THEN o sistema SHALL chamar `GET /api/chamados` filtrado pelo e-mail do perfil mockado ativo e listar só os chamados dele
2. WHEN o Solicitante aplica um filtro de status ou categoria THEN o sistema SHALL refazer a chamada com os query params correspondentes
3. WHEN a lista está vazia THEN o sistema SHALL mostrar um estado vazio com call-to-action pra abrir um chamado
4. WHEN há mais de uma página de resultados THEN o sistema SHALL mostrar paginação usando `PagedResult` (Total, Pagina, TamanhoPagina, TemProxima, TemAnterior)

**Independent Test**: Abrir 2 chamados, filtrar por status "Aberto", ver os 2 na lista; filtrar por "Resolvido", ver lista vazia.

---

### P1: Ver detalhe do chamado ⭐ MVP

**User Story**: Como Solicitante, quero ver todos os dados de um chamado específico, incluindo comentários públicos, para entender o andamento do meu pedido.

**Why P1**: Completa o ciclo "abrir → acompanhar".

**Acceptance Criteria**:
1. WHEN o Solicitante acessa o detalhe de um chamado seu THEN o sistema SHALL chamar `GET /api/chamados/{id}` e `GET /api/chamados/{id}/comentarios` e exibir título, descrição, status, prioridade, categoria, datas (criação/limite/conclusão) e comentários públicos em ordem cronológica
2. WHEN o chamado não existe (404) THEN o sistema SHALL mostrar uma página de "não encontrado" com link de volta pra lista
3. WHEN o chamado pertence a outro Solicitante THEN o sistema SHALL bloquear o acesso no frontend (comparar `solicitanteEmail` do chamado com o e-mail do perfil mockado ativo) — nota: hoje a API não impõe essa regra no backend; é só uma trava de UI nesta fase

**Independent Test**: Abrir o detalhe de um chamado próprio (funciona) e de um ID inexistente (mostra "não encontrado").

---

### P1: Comentar publicamente ⭐ MVP

**User Story**: Como Solicitante, quero adicionar um comentário público a um chamado meu, para complementar informações ou perguntar o andamento.

**Why P1**: Parte do ciclo de acompanhamento, já suportado pela API.

**Acceptance Criteria**:
1. WHEN o Solicitante envia um comentário com conteúdo preenchido THEN o sistema SHALL chamar `POST /api/chamados/{id}/comentarios` com `interno: false` e atualizar a lista de comentários na tela sem recarregar a página inteira
2. WHEN o conteúdo está vazio THEN o sistema SHALL bloquear o envio no frontend antes de chamar a API
3. WHEN a API retorna erro (400/404) THEN o sistema SHALL mostrar mensagem de erro e manter o texto digitado no campo (não perder o que o usuário escreveu)

**Independent Test**: Comentar num chamado, ver o comentário aparecer na timeline imediatamente.

---

### P2: Indicador visual de SLA

**User Story**: Como Solicitante, quero ver visualmente se meu chamado está dentro ou fora do prazo (SLA), para saber se devo cobrar o suporte.

**Why P2**: Importante pra transparência, mas não bloqueia o ciclo básico de uso.

**Acceptance Criteria**:
1. WHEN `dataLimite` já passou e o status não é Resolvido/Fechado/Cancelado THEN o sistema SHALL mostrar um indicador visual de atraso (ex: badge vermelho "Atrasado")
2. WHEN `dataLimite` está no futuro THEN o sistema SHALL mostrar o prazo restante (ex: "vence em 6h")

---

### P3: Loading skeletons e empty states refinados

**User Story**: Como Solicitante, quero feedback visual claro enquanto os dados carregam, para não achar que o sistema travou.

**Why P3**: Polimento de UX, não bloqueia funcionalidade.

**Acceptance Criteria**:
1. WHEN uma requisição está em andamento THEN o sistema SHALL mostrar um skeleton/spinner no lugar do conteúdo

---

## Edge Cases

- WHEN a API está fora do ar (erro de rede) THEN o sistema SHALL mostrar mensagem de "serviço indisponível", não uma tela branca
- WHEN o `categoriaId` enviado não existe mais (categoria removida entre o carregamento do form e o envio) THEN o sistema SHALL tratar o 404 da API e recarregar a lista de categorias
- WHEN o usuário mockado é "Atendente" ou "Admin" e acessa o portal do Solicitante THEN o sistema SHALL permitir (sem restrição nesta fase — RBAC de UI fica pra quando as telas de Atendente existirem)
- WHEN o e-mail do perfil mockado não corresponde a nenhum chamado existente THEN a lista SHALL mostrar o estado vazio normalmente

---

## Requirement Traceability

| Requirement ID | Story | Phase | Status |
|----------------|-------|-------|--------|
| API-01 | Pré-requisito backend: endpoint de comentários | Design | Pending |
| FE-01 | P1: Selecionar perfil mockado | Design | Pending |
| FE-02 | P1: Abrir chamado (portal) | Design | Pending |
| FE-03 | P1: Listar meus chamados com filtros | Design | Pending |
| FE-04 | P1: Ver detalhe do chamado | Design | Pending |
| FE-05 | P1: Comentar publicamente | Design | Pending |
| FE-06 | P2: Indicador visual de SLA | - | Pending |
| FE-07 | P3: Loading skeletons e empty states | - | Pending |

**Coverage:** 8 total, 0 mapped to tasks, 8 unmapped ⚠️ (mapeamento acontece na fase de Tasks)

---

## Success Criteria

- [ ] Um Solicitante mockado consegue abrir um chamado, vê-lo na lista, abrir o detalhe e comentar — tudo sem erro
- [ ] Troca de perfil mockado funciona sem reiniciar a aplicação
- [ ] Zero código de UI precisa mudar quando o Azure AD real for plugado (só o contexto de autenticação)
