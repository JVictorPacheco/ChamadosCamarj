# Correções de Acesso a Chamados — Especificação

> **SDD:** implementando
> **Status:** `Em andamento`
> **Branch:** `feature/correcoes-acesso-chamados`
> **Criada em:** 2026-10-01
> **Atualizada em:** 2026-10-01
> **Origem:** pendências registradas no fechamento de `autorizacao-chamados` (bug do 409, R-03, R-04)
> **Aprovação:** o usuário aprovou em 2026-10-01 tratar as três pendências juntas, numa correção
> rápida, e delegou a execução até o fim.

---

## 1. Problema

**Situação atual:**
1. **Editar título e descrição não funciona.** Toda edição responde "Este chamado foi modificado
   por outro usuário" (409), mesmo sem ninguém mexendo no chamado. A tela não usa essa função
   hoje, mas a API é a forma prevista de fazer isso (Atendente e Admin, pela autorização).
2. **A quantidade de comentários entrega os internos.** A API informa ao Solicitante o total de
   comentários, incluindo os internos, que ele não pode ler. A tela não mostra esse número hoje,
   mas qualquer um pode ver a resposta.
3. **Os alertas de SLA vão para todo mundo.** "CAM-123 — prazo estourado" chega a todos os
   usuários conectados, inclusive Solicitantes, de chamados que eles nem podem ver.

**Impacto:** uma função quebrada e dois vazamentos pequenos (quantidade de comentários e número
de chamado), que contrariam a regra de que o usuário não fica sabendo de chamados que não pode
ver (ADR-007).

**Solução esperada:** corrigir as três situações sem mudar mais nada do comportamento.

---

## 2. Fora de Escopo

- Criar tela para editar título e descrição.
- Mostrar a quantidade de comentários na tela.
- Mudar o conteúdo dos alertas de SLA ou a regra de quando eles disparam.

---

## 3. User Stories

| ID | User Story |
|----|------------|
| US-01 | Como **Atendente**, quero corrigir o título ou a descrição de um chamado, para que ele descreva o problema certo. |
| US-02 | Como **CAMARJ**, quero que nenhuma resposta do sistema revele a existência de comentários internos ou de chamados que a pessoa não pode ver, para que as regras de acesso valham por inteiro. |

---

## 4. Critérios de Aceitação

- **AC-01:** Dado um Atendente ou Admin que pode ver o chamado, quando ele edita título e
  descrição sem ninguém ter alterado o chamado antes, então a edição é salva. O conflito (409)
  continua acontecendo quando outra pessoa alterou o chamado no meio do caminho.
- **AC-02:** Dado um Solicitante, quando ele lista chamados ou abre um chamado, então a quantidade
  de comentários informada conta **só os públicos**. Atendente e Admin continuam recebendo o total.
- **AC-03:** Dado um alerta de SLA (atenção ou atrasado), quando ele dispara, então só **Atendentes
  e Admins** conectados o recebem.
- **AC-04:** Dado qualquer usuário conectado ao tempo real, quando ele tenta entrar por conta
  própria num grupo de avisos, então não consegue: os grupos são definidos só pelo servidor, a
  partir do perfil do usuário logado.
- **AC-05:** `dotnet test` e `npm run build` passam sem falhas.

---

## 5. Rastreabilidade

| Critério | Verificação | Resultado |
|----------|-------------|-----------|
| AC-01 a AC-05 | a definir no `tasks.md` | ⬜ Pendente |

---

## 6. Decisões Técnicas

| Decisão | Alternativa considerada | Motivo da escolha |
|---------|------------------------|-------------------|
| `AtualizarChamadoCommandHandler` passa a usar `ObterPorIdComTrackingAsync`, como os outros handlers desde 2026-07-31 | Remover o controle de concorrência da edição | É exatamente o padrão já usado pelos outros 9 handlers; o 409 legítimo continua existindo |
| `ToResponse(bool incluirInternos = true)`: os handlers de leitura passam `false` para Solicitante | Calcular a contagem no repositório | O mapeamento já tem os comentários carregados; mudança mínima e testável |
| No `OnConnectedAsync` do `ChamadosHub`, o servidor põe Atendente e Admin no grupo `Atendimento`, a partir do perfil do token; o `SlaMonitorService` envia para esse grupo | Filtrar no frontend | Filtrar no cliente não impede o recebimento |
| **Remover** `EntrarGrupo`/`SairGrupo` do hub. ⚠️ Mudança de contrato, mas sem nenhum consumidor (busca em `frontend/`, `src/` e `tests/`) | Validar nomes de grupo permitidos | Métodos sem uso que deixariam qualquer cliente entrar no grupo `Atendimento`; código morto vira brecha |

**Pontos de toque cross-feature:** `ChamadoMappings.ToResponse` (Abertura, Lista, Detalhe; o
valor-padrão mantém o comportamento atual de quem não passa nada); `ChamadosHub` (Kanban, Fila,
Detalhe, Chat e badge de não lidas usam o grupo `Todos`, que **não muda**).

---

## 7. Dependências

- Depende de: `autorizacao-chamados` (mergeada em `develop`, PR #43).

---

## 8. Gate Checks

- [ ] `dotnet build` — 0 erros
- [ ] `dotnet test` — 0 falhas
- [ ] `npm run build` — 0 erros
- [ ] ACs verificados
- [ ] STATE, ROADMAP e Obsidian atualizados
- [ ] PR aberto com base `develop`
