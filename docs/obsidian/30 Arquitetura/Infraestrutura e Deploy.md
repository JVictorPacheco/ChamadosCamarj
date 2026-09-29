---
tipo: arquitetura
status: vigente
atualizado: 2026-09-29
spec: docs/DEPLOY-CLOUDFLARE.md
tags: [arquitetura, infraestrutura]
---

# Infraestrutura e Deploy

## Onde cada parte roda

| Peça | Onde | Endereço |
|---|---|---|
| Frontend | Cloudflare Pages | https://chamados.okurumin.com.br |
| Backend (API) | Servidor próprio, exposto pelo **Cloudflare Tunnel** | https://chamados.okurumin.com.br/api |
| Banco de dados | Supabase (PostgreSQL) | — |
| Arquivos | Supabase Storage | — |

**Custo de infraestrutura: R$ 0** — todos os serviços estão em planos gratuitos.

## Como uma atualização chega à produção
1. A mudança é desenvolvida numa branch e integrada em `develop` por *pull request*.
2. `develop` é promovida para `main` por *pull request*.
3. O deploy (frontend e backend) é feito **manualmente** a partir da `main`.

Guia técnico passo a passo: `docs/DEPLOY-CLOUDFLARE.md`.

## Pontos de atenção
- **Um único banco para desenvolvimento e produção.** Testes feitos por desenvolvedores gravam em
  dados reais. Decisão consciente, com riscos — ver [[ADR-003 Dev e produção no mesmo banco]].
- **Backend depende de uma máquina ligada.** Com o Cloudflare Tunnel, a API fica disponível
  enquanto o servidor onde ela roda estiver ligado e com o túnel ativo.
- **Planos gratuitos têm limites** (armazenamento, conexões). Se o uso crescer, rever a
  hospedagem.
- **Não há verificação automática** (build e testes) nos *pull requests* hoje; depende de rodar
  manualmente antes do merge.
