# 📧 Integração com Email

## Contas de Suporte

| Email | Finalidade |
|-------|------------|
| suporte@camarj.com.br | Chamados gerais |
| ti@camarj.com.br | Chamados de TI |

## Fluxo de Captura

```
1. Colaborador envia email para suporte@ ou ti@
2. EmailReceiverService consulta IMAP a cada 60s
3. Parseia: assunto → título, corpo → descrição
4. Extrai anexos
5. Cria Chamado via AbrirChamadoCommand
6. Responde automaticamente com número do chamado
```

## Tecnologia

- **MailKit** — Biblioteca .NET para IMAP
- SSL/TLS obrigatório
- Filtro anti-loop (ignora email enviado pelo próprio sistema)

## Regras

- Rate limit: 10 chamados/minuto por remetente
- Assuntos com palavras-chave alteram prioridade automaticamente
- Arquivos > 10MB são ignorados com aviso
