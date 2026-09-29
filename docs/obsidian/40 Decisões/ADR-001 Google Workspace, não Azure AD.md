---
tipo: decisão
status: vigente
decisao: aceita
data: 2026-06-25
atualizado: 2026-09-29
tags: [decisão, acesso]
---

# ADR-001 — Google Workspace, não Azure AD

## Contexto
A especificação inicial previa login corporativo via **Azure AD (Microsoft)**, partindo da
premissa de que a CAMARJ usava o ecossistema Microsoft.

## Decisão
A premissa estava errada: a CAMARJ usa **Google Workspace** (Gmail corporativo). Todo o plano de
autenticação corporativa passou a considerar o Google, e não a Microsoft.

## Consequências
- O desenho de login com Azure AD foi descartado (nota histórica em `99 Arquivo/Azure AD`).
- O login com Google chegou a ser implementado, mas depois foi substituído — ver
  [[ADR-002 Login por e-mail e senha]].

## Lição
Validar premissas de infraestrutura com a TI **antes** de especificar.
