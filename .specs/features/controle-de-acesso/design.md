# Controle de Acesso por Módulo — Design Técnico

> **Spec:** `spec.md` (aprovada em 2026-10-02, AC-15 conforme a proposta)
> **Autor:** Claude Code (Opus 5.5), sessão única — design, backend e frontend
> **Data:** 2026-10-02

---

## 1. Visão Geral

Reaproveita o padrão que o Chat já usa com sucesso (`ChatPerfil`): o acesso **fica no cadastro do
usuário, não no token**; o servidor confere no banco a cada pedido dos módulos protegidos; um evento
de tempo real atualiza o menu da pessoa na hora (AC-11, AC-13).

| Bloco | ACs | Onde |
|---|---|---|
| A. Modelo e regra | AC-02..AC-08, AC-15, AC-16 | `ModuloSistema` (enum), `UsuarioPerfil` (+2 colunas), `ModulosDeAcesso` (regra), migration |
| B. Auditoria | AC-14 | `AuditoriaAcesso` (tabela nova, mesma migration) |
| C. API de controle de acesso | AC-01..AC-06, AC-09, AC-14 | `AcessosController` (só Admin), commands/queries novos |
| D. Proteção no servidor | AC-07, AC-12 | Dashboard (2 queries), `RelatoriosController`, `ModuloGuard` |
| E. Tempo real + login | AC-11, AC-13 | `AcessosAtualizadosNotification` → SignalR `AcessosAtualizados`; `Modulos` no login e no `/auth/me` |
| F. Tela | AC-01..AC-13 | `ControleAcessoPage` (nova), `AppLayout` (menu), rotas com `RequerModulo`, Usuários sem Chat |

---

## 2. Bloco A — Modelo e regra

- `Domain/Enums/ModuloSistema` (`[Flags]`): `Arquivo = 1, Kanban = 2, Fila = 4, Dashboard = 8,
  RelatorioMensal = 16`. (Abrir/Meus chamados e Administração **não** entram: são fixos.)
- `UsuarioPerfil` ganha `ModulosConcedidos` e `ModulosRetirados` (`ModuloSistema`, gravados como
  inteiro, padrão 0). Métodos: `AjustarModulos(concedidos, retirados)`, `VoltarAoPadrao()`.
  Guardam só **exceções** ao padrão do perfil — por isso todo mundo começa igual a hoje (AC-16).
- `Application/Common/Autorizacao/ModulosDeAcesso` (regra única, pura, testável):
  - `Padrao(perfil)`: Solicitante = `Arquivo`; Atendente = os 5; Admin = os 5.
  - `Ajustaveis(perfil)`: Solicitante = `Arquivo, Dashboard, RelatorioMensal`; Atendente = os 5;
    Admin = nenhum (AC-06, AC-08).
  - `Efetivos(perfil, concedidos, retirados)` = Admin → os 5; senão `(Padrao ∪ concedidos) − retirados`,
    **sempre** limitado a `Padrao ∪ Ajustaveis` (um ajuste inválido gravado nunca dá Kanban/Fila a
    Solicitante).
  - `CalcularAjustes(perfil, desejados)` → `(concedidos, retirados)` mínimos em relação ao padrão;
    rejeita (400) módulo não ajustável para o perfil.
- **Migration** `AddControleDeAcesso`: 2 colunas `int not null default 0` em `UsuariosPerfil` +
  tabela `AuditoriaAcessos`. **Compatível com a versão em produção** (ela não conhece as colunas
  novas e as ignora; nenhuma coluna existente muda). **Aplicar no banco real é parada obrigatória**
  (dev = prod) — só com OK do usuário.

## 3. Bloco B — Auditoria (AC-14)

- Entidade `AuditoriaAcesso`: `Id, UsuarioId` (de quem), `UsuarioNome`, `AlteradoPorId`,
  `AlteradoPorNome`, `Item` (texto: nome do módulo, "Chat" ou "Perfil"), `Anterior`, `Novo`
  (texto, ex.: "ligado"/"desligado", "Participante"), `DataHora`. Repositório com `AdicionarAsync` e
  `ListarPorUsuarioAsync` (mais recentes primeiro).
- Uma linha por item que mudou. A auditoria do Chat que já existe (`ChatHistoricos`) **continua**
  sendo gravada como hoje (AC-09); a nova só acrescenta.

## 4. Bloco C — API (`/api/acessos`, só Admin — AC-05)

| Rota | Faz |
|---|---|
| `GET /api/acessos` | Lista todos os usuários: nome, e-mail, perfil, ativo, módulos efetivos, chat, se tem ajuste. Admins com `acessoTotal: true` (AC-01, AC-06) |
| `GET /api/acessos/{usuarioId}` | Detalhe: para cada módulo ajustável — `padrao`, `efetivo`, `ajustado`; nível do Chat; auditoria (AC-02, AC-14) |
| `PUT /api/acessos/{usuarioId}` | `{ modulos: ["Arquivo", ...], chatPerfil }` → calcula os ajustes, grava, audita, avisa (AC-03, AC-07, AC-09). Recusa: alvo Admin, o próprio Admin, módulo não ajustável (400) |
| `POST /api/acessos/{usuarioId}/padrao` | Volta ao padrão do perfil (mantém o Chat), audita, avisa (AC-04) |

- Mudança de Chat pela tela nova chama o **mesmo** `DefinirChatPerfilCommand` de hoje (mantém a
  notificação aos participantes e o `ChatHistoricos` — AC-09).
- Verificação de perfil Admin no controller (como `UsuariosController`) **e** no handler.

## 5. Bloco D — Proteção no servidor (AC-07, AC-12)

- `ModuloGuard.ExigirAsync(modulo)` (Application): carrega o `UsuarioPerfil` do usuário logado (como o
  Chat faz) e lança 403 "Você não tem acesso a este módulo." se o módulo não está nos efetivos.
- **Dashboard** (`ObterMetricasQueryHandler`, `ObterDistribuicaoQueryHandler`): a checagem "Solicitante
  → 403" vira `ModuloGuard.ExigirAsync(Dashboard)`. Os números já são calculados com a visibilidade do
  usuário (`AplicarVisibilidade` com o `ContextoAcesso`), então um Solicitante com o módulo vê só os
  chamados que já vê (AC-07).
- **Relatório mensal** (`RelatoriosController`): "Solicitante → 403" vira `ExigirAsync(RelatorioMensal)`.
  Escopo dos números: Admin = todos (como hoje); Atendente = os próprios (como hoje, AC-21 de
  autorizacao); **Solicitante = os chamados que ele vê** — `ObterRelatorioMensalQuery` ganha
  `IdsVisiveis` opcional, preenchido com `IChamadoRepository.ListarIdsVisiveisAsync(acesso)` (novo,
  reaproveita `AplicarVisibilidade`) e aplicado aos eventos em memória.
- **Arquivo, Kanban, Fila:** sem checagem no servidor (spec AC-12) — usam a mesma listagem de "Meus
  chamados"; a regra de visibilidade continua sendo a de sempre.
- **Chat:** já protegido pelo `ChatPerfilGuard` (inalterado).

## 6. Bloco E — Tempo real e login (AC-11, AC-13, AC-16)

- `AutenticacaoResponse` e `UsuarioPerfilResponse` (usado pelo `/auth/me` e pela lista de Usuários)
  ganham `Modulos` (lista de nomes dos módulos efetivos). Campo **aditivo**.
- `AcessosAtualizadosNotification(UsuarioId, Modulos, ChatPerfil)` publicada em toda mudança (tela
  nova, volta ao padrão, mudança de perfil); handler em WebApi envia `AcessosAtualizados` para
  `Clients.User(id)` — mesmo mecanismo do `ChatPerfilAtualizado`.
- **AC-15:** `AtualizarUsuarioPerfilCommandHandler`, quando o **perfil** muda, chama
  `VoltarAoPadrao()`, audita ("Perfil": antigo → novo; ajustes apagados) e publica a notificação.

## 7. Bloco F — Tela

- `AuthContext`: `Perfil.modulos: string[]`; preenchido no login e no `/auth/me`; função
  `atualizarModulos(modulos)`.
- `AppLayout`: itens Arquivo, Kanban, Fila, Dashboard e Relatório mensal aparecem **por módulo
  efetivo** (não mais por perfil). A seção "Atendimento" aparece se houver ao menos um item; um
  Solicitante com Dashboard vê o item. Novo item **"Controle de acesso"** em Administração. Escuta
  `AcessosAtualizados` → `atualizarModulos` (+ `chatPerfil`).
- `RequerModulo` (componente de rota): se a pessoa está numa rota de módulo que perdeu, navega para
  `/chamados` com o aviso "Seu acesso a este módulo foi retirado." (exibido no topo do layout)
  (AC-11). Rotas `/admin/*` passam a usar um `RequerAdmin` equivalente (hoje a proteção é só o menu;
  o servidor já recusa).
- `features/admin/ControleAcessoPage.tsx` (`/admin/acessos`): lista com busca (AC-01); ao abrir a
  pessoa, um painel com os módulos ajustáveis (marcado/desmarcado, etiqueta "padrão do perfil" ou
  "ajustado"), o nível do Chat, "Salvar", "Voltar ao padrão do perfil" e a auditoria (AC-02..AC-04,
  AC-09, AC-14). Admins aparecem como "Admin — acesso total", sem edição (AC-06).
- **Usuários (AC-10):** saem o seletor de Chat da lista e o campo de Chat do formulário.
  `AtualizarUsuarioPerfilCommand.ChatPerfil` vira **opcional** (`null` = não mexe), para a tela de
  Usuários não precisar mais mandá-lo; criar usuário continua com Chat "sem acesso" por padrão.

---

## 8. Constitution Check

| Regra | Cumpre? | Justificativa |
|---|---|---|
| 1. Sem suposição silenciosa | Sim | Decisões da spec §6; nada novo de produto em aberto |
| 2. Spec antes do código | Sim | Spec aprovada |
| 3. Contrato avisado antes | Sim | C1–C9 abaixo, apresentados antes de implementar |
| 4. `/sdd` | Sim | — |
| 5. Cross-feature / `/analise-cod` | Sim | Lista abaixo; `/analise-cod` no fim |
| 6. Obsidian | Planejado | Perfis e Permissões, Administração, Chat Corporativo, Fila/Kanban/Dashboard, Relatório; ADR-010 |

## 9. Mudanças de Contrato

| # | Mudança | Quem consome | Compatibilidade |
|---|---|---|---|
| C1 | **Migration** `AddControleDeAcesso` (2 colunas em `UsuariosPerfil` + tabela `AuditoriaAcessos`) | Banco real (dev = prod) | Aditiva; versão em produção ignora. **Aplicar só com OK do usuário** |
| C2 | `AutenticacaoResponse.Modulos` e `UsuarioPerfilResponse.Modulos` | Login, `/auth/me`, lista de Usuários | Aditivo |
| C3 | Rotas novas `/api/acessos*` (só Admin) | Tela nova | Novas |
| C4 | Dashboard e Relatório: de "Solicitante → 403" para "sem o módulo → 403" | Tela do Dashboard/Relatório | Para todos os usuários atuais o resultado é **idêntico** (ninguém tem ajuste) |
| C5 | `ObterRelatorioMensalQuery.IdsVisiveis` (opcional) + `IChamadoRepository.ListarIdsVisiveisAsync` | Relatório | Aditivo |
| C6 | Evento SignalR novo `AcessosAtualizados` | `AppLayout` | Novo |
| C7 | `AtualizarUsuarioPerfilCommand.ChatPerfil` vira opcional; mudança de perfil zera ajustes (AC-15) | Tela de Usuários, testes | Quem ainda mandar o campo continua funcionando |
| C8 | Menu por **módulo efetivo** em vez de por perfil; rotas com `RequerModulo`/`RequerAdmin` | Todas as telas | Sem ajustes, o menu de cada perfil fica idêntico ao de hoje (AC-16) |
| C9 | Usuários sem os controles de Chat | Admin | Movidos para a tela nova (AC-10) |

## 10. Pontos de Toque Cross-Feature

| Arquivo | Dono / outros usuários | Estratégia de não-regressão |
|---|---|---|
| `UsuarioPerfil` / `UsuarioPerfilConfiguration` | auth, usuários, chat | Testes de domínio; colunas novas com padrão 0 |
| `AtualizarUsuarioPerfilCommandHandler` | Usuários, chat (troca de ChatPerfil) | Testes existentes do handler (ChatPerfil continua funcionando quando enviado) + teste novo do AC-15 |
| `LoginCommandHandler`, `ObterPerfilAtualQuery`, mappings | autenticação | Testes de login existentes + campo novo |
| Dashboard (2 handlers), `RelatoriosController`/handler | dashboard, relatório (autorizacao AC-13, AC-21) | Testes existentes (Solicitante sem módulo → 403; Atendente só os próprios) + novos (Solicitante com módulo) |
| `AppLayout` (menu global, eventos SignalR) | todas as telas, chat | Teste por perfil de que o menu sem ajustes é igual ao de hoje (E2E) |
| `App.tsx` (rotas) | todas | E2E existentes (17) passando |
| `UsuariosPage` / `UsuarioFormDialog` | Usuários | E2E de admin; verificação na tela |

## 11. Riscos e Mitigações

| Risco | Mitigação |
|---|---|
| Migration no banco real | Aditiva; parada para OK do usuário antes de aplicar; versão em produção segue funcionando |
| Alguém perder acesso no deploy | Ninguém tem ajuste → efetivos = padrão = hoje (AC-16); teste que compara os padrões com a matriz atual |
| Menu mudar para quem não teve ajuste | Teste por perfil; E2E e verificação na tela com Solicitante, Atendente e Admin |
| Ajuste inválido gravado (ex.: Kanban para Solicitante) | `Efetivos` sempre limita a `Padrao ∪ Ajustaveis` |
| Admin se trancar | Admin não é ajustável e não ajusta a si mesmo |

## 12. Perguntas em Aberto

Nenhuma de produto. Parada obrigatória para: contratos C1–C9 e, depois, aplicar a migration (C1).

---

## 13. Ajustes após o review (2026-10-05)

| Achado | Ajuste no design |
|---|---|
| R-01 🔴 | `DashboardPage` e `RelatorioMensalPage` deixam de barrar por perfil e passam a usar `temModulo` (a mesma regra do menu e do servidor). |
| R-02 🔴 (decisão do usuário) | Chat de Admin ajustável na tela nova: `PUT /api/acessos/{id}/chat` (`DefinirChatDeAcessoCommand`), aceito para **qualquer** usuário (inclusive Admin e o próprio); a linha do Admin mostra "Ajustar Chat". Módulos continuam não ajustáveis para Admin. |
| R-03 | Recriar conta desativada pelo "Novo usuário" volta ao padrão de módulos. |
| R-04 | `RequerModulo`: "Seu acesso a este módulo foi retirado." só para quem tinha o módulo com a tela aberta; quem nunca teve recebe "Você não tem permissão para acessar este módulo."; `RequerAdmin` mostra "Você não tem permissão para acessar esta área.". |
| R-05 | `SalvarAcessosRequest.ChatPerfil` obrigatório (sem ele → 400) e validador do comando (Chat válido) — nada é gravado se o pedido for inválido. |
| R-06 | A auditoria "Chat" passa para dentro do `DefinirChatPerfilCommandHandler`: **todo** caminho que muda o Chat (tela nova, edição de usuário, `PATCH /usuarios/{id}/chat-perfil`) entra na auditoria de acessos. |
| R-07 | `ModuloGuard` recusa quando o perfil do cadastro difere do perfil do token ("Seu perfil mudou. Entre novamente para continuar."): os números nunca saem no escopo de um perfil antigo. A parte pré-existente (Admin rebaixado mantém direitos de Admin até o token vencer) vira pendência no STATE. |
| R-08 | Escopo do relatório extraído do controller para `RelatorioEscopo.ResolverAsync` (Application), com testes. |
