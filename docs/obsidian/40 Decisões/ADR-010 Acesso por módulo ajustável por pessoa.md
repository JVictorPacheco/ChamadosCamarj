---
tipo: decisão
status: vigente
decisao: aceita
data: 2026-10-02
atualizado: 2026-10-09
tags: [decisão, permissões]
---

# ADR-010 — Acesso por módulo ajustável por pessoa

## Contexto
O acesso às telas era decidido só pelo perfil (Solicitante, Atendente, Admin). Na prática, algumas
pessoas precisavam de uma tela que o perfil não dava (ex.: um Solicitante gestor acompanhar o Dashboard)
ou não deviam ter uma que o perfil dava (ex.: um Atendente sem o Relatório Mensal). A única saída era
trocar o perfil inteiro, o que mudava também o que a pessoa podia fazer nos chamados.

## Decisão
- O perfil continua definindo o **padrão** de telas. O Admin ajusta **pessoa a pessoa**, ligando ou
  desligando telas inteiras, numa tela própria (*Controle de acesso*), que também concentra o acesso ao chat.
- O cadastro guarda só as **exceções** ao padrão. Sem ajuste, todos ficam exatamente como antes.
- **Admin tem sempre todas as telas**; só o chat dele é ajustável. Admin não restringe Admin.
- **Fila e Kanban** nunca vão para Solicitante (são de atendimento). Ganhar Dashboard ou Relatório não
  amplia quais chamados a pessoa vê.
- O acesso é conferido **no cadastro a cada pedido**, não no login: a mudança vale na hora, e o menu da
  pessoa se atualiza sozinho.
- Mudar o perfil zera os ajustes e **desconecta** a pessoa (o login carrega o perfil antigo).
- Toda mudança fica num **histórico de acessos** (quem, quando, antes e depois).

## Consequências
- ✅ Exceções sem mexer no perfil nem nas regras de chamados.
- ✅ Tirar um acesso tem efeito imediato, sem esperar o login da pessoa vencer.
- ✅ Auditoria de quem deu ou tirou cada acesso.
- ⚠️ O controle é de tela inteira; ações dentro das telas continuam pelo perfil.
- ✅ O perfil, a equipe e a situação da conta também são conferidos no cadastro a cada pedido (ver [[ADR-011 Perfil, equipe e conta valem na hora]]): um Admin rebaixado perde os direitos de Admin no pedido seguinte.
- ✅ Mudança de equipe vale na hora (revisto em ADR-011).

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Criar mais perfis (ex.: "Solicitante com Dashboard") | Multiplica perfis a cada exceção |
| Controle por ação (comentário interno, cancelar etc.) | Complexo demais para a necessidade atual |
| Mudar o padrão do perfil inteiro | Afeta todo mundo para resolver o caso de uma pessoa |
| Acesso gravado no login | A mudança só valeria depois de horas, quando o login vencesse |
