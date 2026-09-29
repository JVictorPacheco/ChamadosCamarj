---
tipo: funcionalidade
status: planejada
atualizado: 2026-09-29
tags: [funcionalidade, email, planejada]
---

# Abertura por E-mail *(planejada)*

> **Ainda não existe no sistema.** Esta nota descreve o comportamento pretendido. Andamento em
> `.specs/project/ROADMAP.md` (Fase 4 — Integração E-mail).

## Para que serve
Permitir que o colaborador abra um chamado simplesmente mandando um e-mail, sem entrar no portal.

## Caixas previstas
| E-mail | Finalidade |
|---|---|
| suporte@camarj.com.br | Chamados gerais |
| ti@camarj.com.br | Chamados de TI |

## Comportamento pretendido
1. O sistema verifica as caixas periodicamente.
2. **Assunto** vira o título; **corpo** vira a descrição; anexos acompanham.
3. O chamado é criado com origem **E-mail**.
4. O remetente recebe uma resposta automática com o número `CAM-N`.

## Regras previstas
- Ignorar e-mails enviados pelo próprio sistema (evita loop de respostas).
- Limitar a quantidade de chamados por remetente em curto intervalo.
- Anexos acima de 10 MB são ignorados, com aviso.

## Dependência para começar
Senha de aplicativo das caixas de e-mail, a ser fornecida pela TI.
