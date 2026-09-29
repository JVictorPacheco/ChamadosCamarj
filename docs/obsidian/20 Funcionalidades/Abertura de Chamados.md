---
tipo: funcionalidade
status: vigente
atualizado: 2026-09-29
spec: .specs/features/frontend-portal-solicitante/spec.md
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
2. Preencher **título**, **descrição**, **categoria** e **prioridade**.
3. Opcional: botão **Sugerir categoria** — o sistema lê o texto e propõe categoria e equipe.
4. Opcional: anexar um ou mais arquivos já na abertura. Ver [[Anexos]].
5. Ao enviar, o chamado nasce **Aberto**, com número **`CAM-N`** e **prazo** calculado.

## Regras de negócio
- Título, descrição, nome e e-mail do solicitante são **obrigatórios**. Nome e e-mail vêm do
  usuário logado.
- Prioridade padrão: **Média**.
- O número `CAM-N` é **sequencial e único**, gerado pelo banco — nunca se repete, mesmo com duas
  aberturas no mesmo instante. Ver [[ADR-006 Número de chamado CAM-N]].
- O **prazo** é definido pela prioridade no momento da abertura. Ver [[SLA]].
- O envio é **protegido contra clique duplo**: repetir o envio em sequência não cria dois chamados.
- A busca aceita o número do chamado: `42` ou `CAM-42`.

## Triagem automática
A sugestão de categoria e equipe usa **palavras-chave** do título e da descrição. Exemplos:

| Palavras no texto | Categoria sugerida |
|---|---|
| reembolso, restituição, devolução | Reembolso |
| autorização, auditoria, aprovação | Autorização/Auditoria |
| fatura, boleto, nota fiscal, cobrança | Financeiro |
| credenciado, rede credenciada, prestador | Credenciado |
| contrato, venda, proposta | Comercial |
| conta médica, hospital, procedimento | Contas Médicas |

É uma **sugestão** — o usuário pode aceitar ou escolher outra.

## Categorias disponíveis
Autorização/Auditoria · Atendimento · Super e Tendência · Reembolso · Financeiro · Credenciado ·
Comercial · Contas Médicas. Mantidas pelo Admin em [[Administração]].

## Origem do chamado
O sistema registra por onde o chamado entrou: **Portal** (hoje, o único canal ativo), **E-mail**
(planejado — [[Abertura por E-mail]]) ou **API**.
