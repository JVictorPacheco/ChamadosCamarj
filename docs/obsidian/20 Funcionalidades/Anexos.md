---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-02
spec: .specs/features/anexos-storage/spec.md, .specs/features/correcoes-pre-deploy/spec.md
tags: [funcionalidade, anexos]
---

# Anexos

## Para que serve
Permitir que documentos, prints e planilhas acompanhem o chamado, sem depender de e-mail.

## Quem usa
Todos os perfis.

## Como funciona
- Anexar **na abertura**, **na tela de detalhe** ou **junto de um comentário**.
- Vários arquivos de uma vez.
- Para baixar, o sistema gera um **link temporário** (válido por 1 hora) — o arquivo nunca fica
  exposto publicamente.

## Regras de negócio
| Regra | Valor |
|---|---|
| Tipos aceitos | PDF, imagens, Word, Excel, ZIP |
| Tamanho máximo | 10 MB por arquivo |
| Quem enviou | Registrado automaticamente com o usuário logado |
| Remover | Admin remove qualquer anexo; os demais só os que eles mesmos enviaram |
| Remoção | Pede confirmação e apaga de verdade (arquivo e registro) |
| Anexo em comentário | Só pode ser ligado a um comentário **do mesmo chamado**; caso contrário é recusado ("O comentário informado não pertence a este chamado.") e nada é guardado |

## Onde ficam
No armazenamento de arquivos do Supabase, separado do banco de dados. Os arquivos do
[[Chat Corporativo]] ficam em um espaço próprio, com as mesmas regras de tipo e tamanho.
