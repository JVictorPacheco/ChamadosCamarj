---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-02
spec: .specs/features/area-e-tipo-do-chamado/spec.md
tags: [funcionalidade, chamado]
---

# Abertura de Chamados

## Para que serve
Transformar uma necessidade do colaborador em um registro formal, com número, prazo e
responsável, que ninguém perde de vista.

## Quem usa
Todos os perfis — normalmente o **Solicitante**. Ver [[Perfis e Permissões]].

## Como funciona
1. Menu **Abrir Chamado**.
2. Preencher **título**, **descrição**, **área**, **tipo** e **prioridade**. A área já vem preenchida
   com a equipe de quem abre; dá para trocar.
3. Opcional: botão **Sugerir área e tipo** — o sistema lê o texto e propõe os dois.
4. Opcional: anexar um ou mais arquivos já na abertura. Ver [[Anexos]].
5. Ao enviar, o chamado nasce **Aberto**, com número **`CAM-N`** e **prazo** calculado.

## Regras de negócio
- Título, descrição, **área**, **tipo**, nome e e-mail do solicitante são **obrigatórios**. Nome e e-mail vêm do
  usuário logado.
- Prioridade padrão: **Média**.
- O número `CAM-N` é **sequencial e único**, gerado pelo banco — nunca se repete, mesmo com duas
  aberturas no mesmo instante. Ver [[ADR-006 Número de chamado CAM-N]].
- O **prazo** é definido pela prioridade no momento da abertura. Ver [[SLA]].
- O envio é **protegido contra clique duplo**: repetir o envio em sequência não cria dois chamados.
- A busca aceita o número do chamado: `42` ou `CAM-42`.

## Triagem automática
A sugestão de área e tipo usa **palavras-chave** do título e da descrição. Exemplos:

| Palavras no texto | Sugestão |
|---|---|
| reembolso, restituição, devolução | Área **Reembolso** |
| fatura, boleto, nota fiscal, cobrança | Área **Financeiro** |
| autorização, auditoria, aprovação | Área **Autorização/Auditoria** |
| erro, não funciona, travou, fora do ar | Tipo **Incidente** |
| como faço, dúvida, como funciona | Tipo **Dúvida** |
| preciso de, acesso, cadastro, segunda via | Tipo **Solicitação** |
| personalizar, customizar | Tipo **Customização** |
| melhorar, sugestão | Tipo **Melhoria** |

É uma **sugestão** — o usuário pode aceitar ou escolher outra.

## Área e tipo
- **Área** é de onde o chamado é aberto. É a mesma lista das equipes: Autorização/Auditoria ·
  Atendimento · Comercial · Contas Médicas · Credenciado · Financeiro · Reembolso · Super e Tendência.
  Ver [[Grupos e Equipes]] e [[ADR-008 Área do chamado é a equipe]].
- **Tipo** é a natureza do pedido: **Incidente**, **Dúvida**, **Solicitação**, **Customização** ou
  **Melhoria**.
- As duas listas são mantidas pelo Admin em [[Administração]].
- Chamados abertos antes desta classificação aparecem com tipo **"Não classificado"**. O Atendente
  pode corrigir o tipo no detalhe do chamado.

## Origem do chamado
O sistema registra por onde o chamado entrou: **Portal** (hoje, o único canal ativo), **E-mail**
(planejado — [[Abertura por E-mail]]) ou **API**.
