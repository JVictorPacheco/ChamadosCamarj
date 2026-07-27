# 📧 Leitor e Resumidor de Emails

> Setup: 2026-07-27 | Ferramenta: PowerShell + .NET + MailKit

---

## Objetivo

Baixar emails não lidos da caixa `suporte@camarj.com.br` e gerar resumos rápidos com ajuda do OpenCode, economizando tempo na triagem diária.

---

## Como usar no OpenCode

```
/resumir-emails
```

- Baixa os **10 emails não lidos mais recentes**
- O OpenCode lê tudo e responde com resumo de 1 linha por email
- Spam e notificações do Google são ignorados
- Se houver pedido de ajuda/chamado, destaca com **prioridade**

Depois do resumo inicial, pode pedir detalhes:

| Comando | O que faz |
|---|---|
| `Leia o email sobre [assunto]` | Abre o arquivo completo e mostra detalhes |
| `Resuma meus emails` | Resume o que já foi baixado (sem baixar de novo) |
| `Tem algo urgente?` | Filtra só o que parece importante |

---

## Script PowerShell (terminal, fora do OpenCode)

Local: `C:\Users\jpacheco.CAMARJ.001\Projects\ChamadosCamarj\ler-emails.ps1`

```powershell
# Baixar últimos 10 emails (padrão)
.\ler-emails.ps1

# Só 3 mais recentes
.\ler-emails.ps1 -Limite 3

# Buscar por palavra-chave no assunto/corpo
.\ler-emails.ps1 -Busca "relatorio"

# Filtrar por remetente
.\ler-emails.ps1 -De "chefia@camarj.com.br"

# Combinar filtros
.\ler-emails.ps1 -Limite 5 -Busca "urgente"
```

### O que o script faz

1. Conecta no Gmail via IMAP (`imap.gmail.com:993`)
2. Busca emails **não lidos** com os filtros aplicados
3. Salva cada um como `.md` na pasta `emails/`
4. **Marca como lido** no Gmail (não baixa repetido)
5. **Remove arquivos com mais de 24h** da pasta `emails/`

---

## Onde os emails ficam

```
C:\Users\jpacheco.CAMARJ.001\Projects\ChamadosCamarj\emails\
```

Cada arquivo: `AAAA-MM-DD-HHmmss_Assunto-do-email.md`

---

## Segurança

- A senha de app do Gmail está no script (`ler-emails.ps1`). **NÃO commitar no Git.**
- O script, a pasta `emails/` e `scripts/LeitorEmail/` já estão no `.gitignore`
- A senha de app pode ser revogada a qualquer momento em [myaccount.google.com/security](https://myaccount.google.com/security) → Senhas de app

---

## Dependências

- .NET 9 SDK
- Pacote NuGet MailKit (baixado automaticamente na primeira execução)

---

## Troubleshooting

| Problema | Solução |
|---|---|
| `Authentication failed` | A senha de app expirou ou foi revogada. Gerar nova no Google. |
| `Connection refused` | Sem internet ou firewall bloqueando porta 993 |
| Emails repetidos | O script marca como lido. Se ainda aparecerem, verificar se a flag `Seen` foi aplicada corretamente |
