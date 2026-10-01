---
tipo: funcionalidade
status: vigente
atualizado: 2026-10-01
spec: .specs/features/auth-email-senha/spec.md
tags: [funcionalidade, acesso]
---

# Acesso e Login

## Como entrar
Login com **e-mail corporativo e senha** em https://chamados.okurumin.com.br.

## Quem pode entrar
Só quem foi **cadastrado pelo Admin** e está **ativo**. Não existe autocadastro. Ver
[[Administração]].

## Senhas
| Situação | Como resolver |
|---|---|
| Primeiro acesso | O Admin define uma senha inicial no cadastro (mínimo 8 caracteres) |
| Esqueci a senha | Link **Esqueci minha senha** na tela de login → chega um e-mail com link de redefinição, válido por **1 hora** |
| Usuário bloqueado ou sem acesso ao e-mail | O Admin redefine a senha pela tela de usuários |

As senhas são guardadas de forma criptografada (hash) — nem o Admin consegue vê-las.

## Sessão
- Após o login, a sessão tem **validade limitada**; ao expirar, o sistema pede login de novo.
- **Desconexão por inatividade:** depois de **20 minutos** sem mouse, teclado, clique ou rolagem em
  nenhuma aba do sistema, a pessoa é desconectada e volta para a tela de login com o aviso "Sua
  sessão foi encerrada por inatividade". Protege contra computador desbloqueado com o sistema
  aberto. Trabalhar em qualquer aba mantém todas conectadas; atualizações automáticas da tela não
  contam como atividade.
- **Sair** encerra a sessão e marca o usuário como Offline no chat.

## Tema
Claro (padrão) ou escuro, com a identidade visual da CAMARJ. A escolha fica salva no navegador.

## Histórico da decisão
O plano original era entrar com a conta Google corporativa; foi substituído por e-mail e senha.
Ver [[ADR-002 Login por e-mail e senha]].
