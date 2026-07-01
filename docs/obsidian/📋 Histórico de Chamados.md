# 📋 Histórico de Chamados

> Planejado para **Fase 6**. Fornece auditoria completa de cada chamado.

## O que é

O **Log de Histórico** registra automaticamente cada evento relevante no ciclo de vida de um chamado: quem criou, quem assumiu, quem reatribuiu, quem resolveu, quem comentou — com data/hora exata.

## Por que precisamos

- **Transparência:** Solicitante pode ver o histórico público do seu chamado
- **Auditoria:** Admin vê o rastro completo de quem fez o quê
- **Rastreabilidade:** Entender por que um chamado demorou ou mudou de responsável
- **Compliance:** Registro imutável para prestação de contas

## Entidade `HistoricoEntrada`

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | PK |
| ChamadoId | Guid (FK) | Chamado relacionado |
| UsuarioNome | string | Nome de quem realizou a ação |
| UsuarioId | Guid? | ID do usuário (preenchido na Fase 6 com auth real) |
| Acao | enum | Ver `AcaoHistorico` abaixo |
| DetalheAnterior | string? | Estado anterior (ex: "Fábio" antes da reatribuição) |
| DetalheNovo | string? | Estado novo (ex: "Victor" após a reatribuição) |
| DataHora | DateTime | Timestamp UTC da ação |

## Enum `AcaoHistorico`

| Valor | Quando é gerado |
|-------|-----------------|
| `Criado` | Chamado aberto (portal ou email) |
| `Assumido` | Atendente/Admin clica "Assumir" |
| `Reatribuido` | Admin muda o responsável (Reatribuir) |
| `Resolvido` | Atendente/Admin clica "Resolver" |
| `Fechado` | Atendente/Admin clica "Fechar" |
| `Cancelado` | Qualquer perfil com permissão cancela |
| `ComentarioAdicionado` | Novo comentário (público ou interno) |
| `PrioridadeAlterada` | Admin altera a prioridade |

## Visibilidade

| Tipo de entrada | Admin | Atendente | Solicitante |
|-----------------|-------|-----------|-------------|
| Criado | ✅ | ✅ | ✅ |
| Assumido | ✅ | ✅ | ✅ |
| Reatribuido | ✅ | ✅ | ❌ (interno) |
| Resolvido / Fechado | ✅ | ✅ | ✅ |
| Cancelado | ✅ | ✅ | ✅ |
| ComentarioAdicionado (público) | ✅ | ✅ | ✅ |
| ComentarioAdicionado (interno) | ✅ | ✅ | ❌ |
| PrioridadeAlterada | ✅ | ✅ | ❌ (interno) |

## UI Planejada

No **Detalhe do Chamado**, uma seção "Histórico" com timeline vertical:

```
📅 2026-07-01 09:15  Victor criou o chamado
📅 2026-07-01 09:20  Fábio assumiu o chamado
📅 2026-07-01 10:45  Victor reatribuiu para Ana Atendente
📅 2026-07-01 14:30  Ana Atendente resolveu o chamado
📅 2026-07-01 15:00  Victor fechou o chamado
```

## Implementação (Backend)

O `HistoricoEntrada` será gerado via **Domain Events** ou diretamente nos `CommandHandlers`, logo após cada ação bem-sucedida:

```csharp
// Exemplo em AtribuirChamadoCommandHandler
var entrada = new HistoricoEntrada(
    chamadoId: chamado.Id,
    usuarioNome: request.ResponsavelNome,
    acao: AcaoHistorico.Assumido,
    detalheNovo: request.ResponsavelNome
);
await _historicoRepository.AdicionarAsync(entrada);
```

## Relação com outros documentos

- [[📊 Modelo de Dados]] — entidade `HistoricoEntrada` detalhada
- [[👥 Perfis de Usuário]] — visibilidade por perfil
- [[🗺️ Roadmap]] — planejado na Fase 6
