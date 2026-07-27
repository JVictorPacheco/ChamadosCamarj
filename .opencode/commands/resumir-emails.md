---
description: Resume emails do suporte — baixa e resume emails nao lidos
agent: spec
model: opencode-go/deepseek-v4-flash
---
!`powershell -NoProfile -File ler-emails.ps1 -Limite 10`

Leia os arquivos .md na pasta emails/ (ordenados por data, mais recentes primeiro).

Se houver mais de 3 emails, liste todos com um resumo de 1 linha cada (assunto + quem enviou).
Se o usuario pedir detalhes de algum, leia o arquivo completo.

Filtre:
- Spam / notificacoes automaticas do Google: ignore (so mencione se perguntarem)
- Emails de pessoas reais: resuma com atencao (quem, o que quer, qual urgencia)
- Se houver solicitacao de chamado/help, destaque com prioridade

Responda em portugues, tom direto, sem firula.
