---
tipo: processo
status: vigente
atualizado: 2026-09-29
spec: .specs/features/forcar-encerramento/spec.md
tags: [processo, encerramento]
---

# Encerramento e Cancelamento

Existem três formas de um chamado terminar. Em todas, o **motivo** fica registrado para auditoria
e aparece no histórico.

| Forma | Quem | A partir de | Resultado | Motivo |
|---|---|---|---|---|
| **Encerrar** (caminho normal) | Atendente, Admin | *Resolvido* | Fechado | Resolvido (automático) |
| **Cancelar** | Solicitante, Atendente, Admin | *Aberto* ou *Em andamento* | Cancelado | Obrigatório |
| **Forçar encerramento** | Só Admin | Qualquer status não final | Fechado | Obrigatório |

## Motivos disponíveis

| Motivo | Quando usar |
|---|---|
| Resolvido | O problema foi solucionado |
| Cancelado pelo solicitante | Quem abriu desistiu |
| Aberto indevidamente | O chamado não deveria existir (engano, canal errado) |
| Duplicata | Já existe outro chamado para o mesmo pedido |
| Sem resposta | O solicitante parou de responder |
| Outro | Nenhum dos anteriores — **texto explicativo obrigatório** |

## Encerramento forçado

Serve para o Admin resolver situações que travariam o fluxo normal — por exemplo, um chamado
parado há semanas esperando o solicitante. Regras:

- disponível só para o **Admin**;
- funciona a partir de *Aberto*, *Em andamento* ou *Resolvido*;
- o motivo é obrigatório e, se for *Outro*, o texto também;
- fica marcado no histórico como **encerramento forçado**, separado de um encerramento normal;
- se o chamado ainda não tinha data de conclusão, ela é registrada nesse momento.

## Depois de terminado

- Chamados *Resolvidos*, *Fechados* e *Cancelados* saem das telas do dia a dia e ficam
  consultáveis no [[Arquivo de Chamados]].
- **Nunca são apagados** — ver [[ADR-005 Chamados nunca são apagados]].
- Podem ser **reabertos** por um Atendente ou Admin se necessário; voltam para a fila sem
  responsável.
