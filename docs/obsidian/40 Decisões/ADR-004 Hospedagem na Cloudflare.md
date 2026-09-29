---
tipo: decisão
status: vigente
decisao: aceita
data: 2026-08-10
atualizado: 2026-09-29
tags: [decisão, infraestrutura]
---

# ADR-004 — Hospedagem na Cloudflare

## Contexto
O plano era hospedar a API no **Azure App Service** (plano gratuito), com publicação automática
pelo GitHub. Esse caminho **nunca chegou a funcionar**: a publicação automática falhava em todas
as tentativas. Na prática, o sistema foi colocado no ar pela Cloudflare.

## Decisão
- **Frontend** no **Cloudflare Pages**.
- **Backend** exposto pelo **Cloudflare Tunnel**, com domínio próprio
  (`chamados.okurumin.com.br`).
- Configuração do Azure **removida** do projeto em 2026-09-28.

## Consequências
- ✅ Custo zero, domínio próprio com HTTPS.
- ✅ Um único fornecedor para frontend, domínio e acesso ao backend.
- ⚠️ O backend depende de uma máquina ligada com o túnel ativo.
- ⚠️ Deploy manual — não há publicação automática.

Ver [[Infraestrutura e Deploy]].
