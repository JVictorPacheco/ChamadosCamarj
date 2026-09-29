---
tipo: decisão
status: vigente
decisao: aceita
data: 2026-07-24
atualizado: 2026-09-29
tags: [decisão, acesso]
---

# ADR-002 — Login por e-mail e senha

## Contexto
O login com a conta **Google corporativa** foi implementado por completo, mas dependia de a TI
liberar uma credencial (Client ID) do Google. A TI informou que esse recurso **está fora do plano
contratado** pela CAMARJ.

## Decisão
Usar **login próprio com e-mail e senha**:
- o Admin cadastra cada usuário com uma senha inicial;
- o usuário pode redefinir a senha por e-mail;
- as senhas são guardadas apenas como hash.

O código do login Google foi **mantido desativado**, caso o plano mude no futuro.

## Consequências
- ✅ Não depende de nenhuma contratação ou liberação externa.
- ✅ Controle total de quem acessa, pelo cadastro do Admin.
- ⚠️ Mais uma senha para o colaborador lembrar (sem login único).
- ⚠️ A redefinição de senha depende do envio de e-mail pelo sistema.

## Alternativas consideradas
| Alternativa | Por que não |
|---|---|
| Login Google (SSO) | Fora do plano contratado |
| Azure AD | A CAMARJ não usa Microsoft — ver [[ADR-001 Google Workspace, não Azure AD]] |

Ver também [[Acesso e Login]].
