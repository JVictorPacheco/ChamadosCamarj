# SLA Tracking + Alertas

## Problema
Cada chamado tem um prazo de resolução (SLA) calculado na abertura baseado na prioridade:
- Urgente: 8h | Alta: 24h | Média: 16h | Baixa: 48h

Hoje o `DataLimite` existe no banco mas **ninguém monitora**. O atendente não vê se o chamado está perto de estourar ou já estourou o prazo. Não há alerta.

## Requisitos

### SLA-01: Indicador visual no ChamadoCard
O card do chamado (em Lista, Kanban, Fila, Arquivo) deve mostrar o status de SLA:

| Status | Cor | Critério |
|--------|-----|----------|
| **No prazo** | Verde (borda/esquerda) | `DataLimite > Now + 2h` |
| **Atenção** | Amarelo | `Now + 2h >= DataLimite > Now` |
| **Atrasado** | Vermelho | `DataLimite <= Now` |

Exibir o tempo restante ou o tempo de atraso formatado:
- "Faltam 3h 45min" (no prazo)
- "Fecha em 1h 12min" (atenção)
- "Atrasado 2h 30min" (atrasado)

### SLA-02: Filtro por status de SLA
Adicionar filtro no `FiltroChamados`:
- "Todos"
- "No prazo"
- "Atenção"
- "Atrasado"

### SLA-03: SLA Compliance Rate no Dashboard
Card no Dashboard: porcentagem de chamados resolvidos dentro do prazo no mês atual.
- "SLA: 87%" com indicador verde (>90%), amarelo (>70%), vermelho (<70%)
- Tooltip: "15 de 17 chamados resolvidos dentro do prazo este mês"

### SLA-04: SLA Compliance no Relatório Mensal
Adicionar ao Relatório Mensal:
- SLA compliance rate do período
- Gráfico de evolução mensal do SLA (linha, últimos 6 meses)
- Quebra de SLA por atendente

### SLA-05: Alerta SignalR de SLA
Quando um chamado entrar em estado de "Atenção" (2h antes do vencimento), disparar notificação SignalR para os atendentes do grupo responsável.

## Design

### Backend

**ChamadoResponse** ganha:
```csharp
public SlaStatus SlaStatus { get; set; } // DentroPrazo, Atencao, Atrasado
public string SlaLabel { get; set; } // "Faltam 3h 45min"
public double? SlaHorasRestantes { get; set; } // negativo = atrasado
```

**SlaStatus** (enum, Application/Common):
```csharp
public enum SlaStatus { DentroPrazo, Atencao, Atrasado }
```

**ChamadoMappings.cs**: calcular SlaStatus no `ToResponse()` com base em `DataLimite`.

**ChamadoRepository.cs**:
- Adicionar parâmetro `SlaStatus?` no `ListarAsync`
- Criar `ContarSlaComplianceAsync(DateTime inicio, DateTime fim)` para o dashboard/relatório

**Dashboard**:
- Novo endpoint `GET /api/dashboard/sla` ou adicionar ao existente
- Retornar: total resolvidos no período, total dentro do prazo, porcentagem

**Relatório Mensal**:
- Adicionar SLA ao `RelatorioMensalResponse`
- Calcular por atendente

### Frontend

**ChamadoCard.tsx**: 
- Adicionar faixa lateral colorida baseada em `SlaStatus`
- Exibir `SlaLabel` no canto inferior direito

**FiltroChamados.tsx**:
- Adicionar select "SLA" com 4 opções

**DashboardPage.tsx**:
- Adicionar card "SLA Compliance" com gauge/porcentagem

**RelatorioMensalPage.tsx**:
- Adicionar linha de SLA na tabela
- Gráfico de evolução do SLA

## Tasks

1. [ ] Criar enum `SlaStatus` em Application/Common
2. [ ] Adicionar `SlaStatus`, `SlaLabel`, `SlaHorasRestantes` ao `ChamadoResponse`
3. [ ] Atualizar `ChamadoMappings.ToResponse` para calcular SLA
4. [ ] Adicionar filtro `SlaStatus?` ao `ListarAsync` no repositório
5. [ ] Criar `ContarSlaComplianceAsync` no repositório
6. [ ] Atualizar `ObterMetricasQueryHandler` para incluir SLA
7. [ ] Adicionar SLA ao `RelatorioMensalResponse` e handler
8. [ ] Atualizar `ChamadoCard` com indicador visual e label
9. [ ] Adicionar filtro SLA no `FiltroChamados`
10. [ ] Adicionar card SLA no Dashboard
11. [ ] Adicionar SLA no Relatório Mensal
12. [ ] Testes: atualizar `ListarChamadosQueryHandlerTests`
13. [ ] `dotnet test` + `npm run build`
