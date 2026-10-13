---
tipo: decisão
status: vigente
decisao: aceita
data: 2026-10-09
atualizado: 2026-10-09
tags: [decisão, segurança, permissões]
---

# ADR-011 — Perfil, equipe e conta valem na hora

## Contexto
O sistema decidia o que cada pessoa pode fazer pelo perfil e pela equipe gravados no momento do login, que
vale 10 horas. Se o Admin rebaixava alguém, trocava a equipe ou desativava a conta nesse período, o servidor
continuava obedecendo ao login antigo. A tela já desconectava a pessoa, mas quem usasse o servidor direto,
sem a tela, mantinha os direitos antigos até o login vencer — um risco real em desligamentos e rebaixamentos
por motivo de confiança.

## Decisão
- O login passa a servir **só para identificar a pessoa**. A cada pedido, o servidor confere no cadastro o
  **perfil, a equipe e se a conta está ativa**, e usa esses valores no lugar dos que vieram no login.
- Conta **desativada ou apagada** é recusada na hora (a pessoa volta à tela de entrada).
- Quem está com o **tempo real** aberto também acompanha o cadastro: o rebaixado deixa de receber os avisos
  restritos, o promovido passa a receber, e a conexão de quem foi desativado é derrubada.
- Para não somar uma ida ao banco em cada pedido, o resultado fica guardado por **15 segundos** e é apagado no
  instante em que o sistema grava qualquer mudança no usuário. Mudança feita pelo sistema vale no pedido
  seguinte; só uma edição direta no banco, ou um segundo servidor, demora até 15 segundos.
- A **equipe** também vale na hora (revisão da regra "só no próximo login", de ADR-010).

## Consequências
- ✅ Tirar um direito tem efeito imediato, também para quem usa o servidor direto.
- ✅ Qualquer tela ou rota nova já nasce protegida: não depende de cada uma lembrar de conferir.
- ✅ O custo de desempenho do dia a dia é mínimo por causa da memória de 15 segundos.
- ⚠️ O cadastro passa a ser consultado a cada pedido; se o banco oscilar, o pedido falha em vez de seguir com
  um login que não foi conferido.
- ⚠️ A janela de até 15 segundos vale para edição direta no banco e para o caso de mais de um servidor.
- ⚠️ A conexão em tempo real de quem acabou de abrir o sistema no instante exato de uma mudança só se corrige ao
  reconectar.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Só conferir nas telas de Administração e nas ações de Atendente | Deixaria a visibilidade de chamados e o tempo real com o login antigo |
| Conferir o banco em todo pedido, sem memória | Soma cerca de 160 ms a cada chamada |
| Invalidar os logins antigos quando algo muda | Camada extra de segurança, planejada como feature futura |
| Encurtar a validade do login | Obriga todo mundo a entrar de novo com frequência, sem resolver a janela |
