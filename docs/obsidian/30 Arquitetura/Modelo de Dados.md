---
tipo: arquitetura
status: vigente
atualizado: 2026-10-06
tags: [arquitetura, dados]
---

# Modelo de Dados

As principais informações que o sistema guarda e como se relacionam. Nomes em português, como no
código.

## Chamados

```mermaid
erDiagram
    GRUPO |o--o{ CHAMADO : "área"
    TIPO_CHAMADO |o--o{ CHAMADO : classifica
    CHAMADO ||--o{ COMENTARIO : tem
    CHAMADO ||--o{ ANEXO : tem
    COMENTARIO |o--o{ ANEXO : "pode ter"
    CHAMADO ||--o{ HISTORICO_ENTRADA : registra
    GRUPO |o--o{ USUARIO_PERFIL : agrupa
    USUARIO_PERFIL ||--o{ AUDITORIA_ACESSO : "histórico de acessos"
    USUARIO_PERFIL |o--o{ CHAMADO : "é responsável"

    CHAMADO {
        int Numero "CAM-N"
        string Titulo
        string Descricao
        enum Status
        enum Prioridade
        string SolicitanteEmail
        datetime DataLimite "prazo SLA"
        datetime DataConclusao
        enum Origem "Portal, E-mail, API"
        enum MotivoEncerramento
    }
    COMENTARIO {
        string Autor
        string Conteudo
        enum Tipo "Público ou Interno"
    }
    ANEXO {
        string NomeArquivo
        string TipoArquivo
        long TamanhoBytes
        string EnviadoPor
    }
    HISTORICO_ENTRADA {
        enum Acao
        string UsuarioNome
        string DetalheAnterior
        string DetalheNovo
        datetime DataHora
        enum Origem "Humano ou Automático"
    }
    USUARIO_PERFIL {
        string Email
        string Nome
        enum Perfil "Admin, Atendente, Solicitante"
        bool Ativo
        enum ChatPerfil
        bool MostrarConfirmacaoLeitura
        flags ModulosConcedidos "telas dadas além do padrão do perfil"
        flags ModulosRetirados "telas tiradas do padrão do perfil"
    }
    AUDITORIA_ACESSO {
        string UsuarioNome
        string AlteradoPorNome
        string Item "tela, Chat, Perfil ou Conta"
        string Anterior
        string Novo
    }
    GRUPO {
        string Nome
        bool Ativo
    }
    TIPO_CHAMADO {
        string Nome
        bool Ativo
    }
```

## Chat

```mermaid
erDiagram
    CHAT_CONVERSA ||--o{ CHAT_PARTICIPANTE : tem
    CHAT_CONVERSA ||--o{ CHAT_MENSAGEM : contém
    CHAT_MENSAGEM ||--o{ CHAT_REACAO : recebe
    CHAT_MENSAGEM |o--o| CHAT_MENSAGEM : "responde a"

    CHAT_CONVERSA {
        enum Tipo "Privada ou Grupo"
        string Nome
        bool Ativa
    }
    CHAT_PARTICIPANTE {
        string UsuarioNome
        datetime UltimaLeituraEm
        bool Ativo
    }
    CHAT_MENSAGEM {
        string Conteudo
        enum Tipo "Texto, Arquivo, Sistema"
        bool Deletada
        datetime EditadaEm
    }
    CHAT_REACAO {
        string Emoji
        string UsuarioNome
    }
```

Tabelas de apoio do chat: **ChatPresenca** (status Online/Ausente/Offline de cada usuário) e
**ChatHistorico** (auditoria de todas as ações do chat).

## Observações para o negócio
- **Nada de chamado é apagado** — o histórico e o relatório dependem disso.
  Ver [[ADR-005 Chamados nunca são apagados]].
- O **solicitante é identificado pelo e-mail**, o que prepara a futura
  [[Abertura por E-mail]].
- O **conteúdo original** de mensagens editadas ou excluídas do chat é preservado para auditoria.
- Senhas são guardadas apenas como **hash**, nunca em texto.
- O cadastro do usuário guarda só as **exceções** de telas em relação ao padrão do perfil; toda mudança
  de acesso vai para o **histórico de acessos**. Ver [[ADR-010 Acesso por módulo ajustável por pessoa]].
