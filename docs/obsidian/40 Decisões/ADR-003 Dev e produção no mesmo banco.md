---
tipo: decisão
status: vigente
decisao: aceita
data: 2026-06-19
atualizado: 2026-09-29
tags: [decisão, infraestrutura, risco]
---

# ADR-003 — Desenvolvimento e produção no mesmo banco

## Contexto
O projeto começou com um banco local para desenvolvimento. Ao migrar para o **Supabase**, optou-se
por usar a **mesma instância** para desenvolvimento e produção, pela simplicidade e por estar no
plano gratuito. Em julho houve uma tentativa de usar um banco local (SQLite) só para
desenvolvimento; ela **não foi adotada**.

## Decisão
Desenvolvimento e produção apontam para o **mesmo banco Supabase**.

## Consequências
- ✅ Um único ambiente para manter; custo zero.
- ✅ Testes de desenvolvimento usam dados e comportamento reais.
- ⚠️ **Qualquer teste local grava dados reais** — exige cuidado (contas e chamados de teste
  identificados e removidos depois).
- ⚠️ Mudanças de estrutura do banco feitas em desenvolvimento **já valem em produção**.

## Quando rever
Se o sistema ganhar mais usuários ou dados sensíveis em volume, separar um banco de homologação
passa a ser recomendável.
