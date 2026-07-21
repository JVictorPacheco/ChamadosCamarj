# 📋 Histórico de Chamados

> ✅ **Implementado e verificado (Fase 6, T01-T06 backend + T12 frontend, 2026-07-14).** Fornece auditoria completa de cada chamado. Também é a fonte de dados do [[📈 Relatório Mensal]] (Fase 7).

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
| UsuarioId | Guid? | ID do usuário — hoje enviado pelo próprio cliente (`AuthContext` mockado), pois não há auth real; passa a vir de claims do JWT quando o login Google (T09) entrar |
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

## UI Implementada

No **Detalhe do Chamado** (`frontend/src/features/chamados/`), uma seção "Histórico" com timeline vertical, consumindo `GET /api/chamados/{id}/historico` (ordenado por `DataHora` descrescente). Verificada via Playwright em 2026-07-14.

```
📅 2026-07-01 09:15  Victor criou o chamado
📅 2026-07-01 09:20  Fábio assumiu o chamado
📅 2026-07-01 10:45  Victor reatribuiu para Ana Atendente
📅 2026-07-01 14:30  Ana Atendente resolveu o chamado
📅 2026-07-01 15:00  Victor fechou o chamado
```

## Implementação (Backend)

O `HistoricoEntrada` é gerado diretamente nos `CommandHandlers`, logo após cada ação bem-sucedida — não via Domain Events. A geração foi integrada em **todos** os handlers relevantes (Abrir, Atribuir, Resolver, Fechar, Cancelar, Reatribuir, AlterarPrioridade), não só num exemplo isolado:

```csharp
// Padrão usado em cada CommandHandler, ex: ReatribuirChamadoCommandHandler
var entrada = new HistoricoEntrada(
    chamadoId: chamado.Id,
    usuarioNome: request.UsuarioNome,
    usuarioId: request.UsuarioId,
    acao: AcaoHistorico.Reatribuido,
    detalheAnterior: responsavelAnterior,
    detalheNovo: request.NovoResponsavelNome
);
await _historicoRepository.AdicionarAsync(entrada);
```

> `UsuarioId`/`UsuarioNome` vêm do `AuthContext` mockado do frontend (client-supplied), já que não há auth real ainda — sem isso, o handler não teria de onde tirar "quem está fazendo a ação".

## Relação com outros documentos

- [[📊 Modelo de Dados]] — entidade `HistoricoEntrada` detalhada
- [[👥 Perfis de Usuário]] — visibilidade por perfil
- [[📈 Relatório Mensal]] — consome `HistoricoEntrada` para agregação por mês fechado
- [[🗺️ Roadmap]] — implementado na Fase 6 (T01-T06, T12)
