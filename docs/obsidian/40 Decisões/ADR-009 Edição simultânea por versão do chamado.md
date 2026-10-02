---
tipo: decisão
status: vigente
decisao: aceita
data: 2026-10-02
atualizado: 2026-10-02
tags: [decisão, chamado]
---

# ADR-009 — Edição simultânea por versão do chamado

## Contexto
Duas pessoas com o mesmo chamado aberto podiam alterá-lo, e quem salvava por último apagava a
alteração da outra sem aviso. O controle de concorrência que existia só pegava duas gravações no
mesmo instante (milissegundos), não o caso real de minutos entre abrir a tela e agir.

## Decisão
Cada chamado tem uma **versão** (o momento da última alteração). A tela envia a versão que mostrou
junto com a ação; se o chamado mudou desde então, a ação é recusada com o aviso "Outra pessoa
alterou este chamado…" e a tela recarrega. Comentários e anexos não mudam a versão e nunca são
recusados. Uma tela antiga, que não envia a versão, continua funcionando sem a proteção.

## Consequências
- ✅ Ninguém perde a alteração de outra pessoa sem saber.
- ✅ Compatível com quem estiver com o sistema aberto durante o deploy.
- ⚠️ Depois de um aviso, a pessoa precisa conferir e refazer a ação.
- ⚠️ Logo após a própria ação, os botões ficam bloqueados por um instante, até a tela recarregar.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Quem salva por último vence (como antes) | Perda silenciosa de trabalho |
| Proteger só a edição de título/descrição | Duas pessoas resolvendo/encerrando ao mesmo tempo também é conflito real |
| Tratar comentário como alteração | Comentário só acrescenta; recusá-lo atrapalharia sem proteger nada |
