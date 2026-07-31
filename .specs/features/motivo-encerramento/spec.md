# Motivo de Encerramento + Tags + Filtro

## Problema
Hoje não sabemos **por que** um chamado foi encerrado/cancelado. Um chamado "Fechado" pode ter sido resolvido, cancelado pelo solicitante, ou aberto indevidamente. Não há auditoria sobre a **causa raiz** do encerramento.

## Requisitos

### TAG-01: Campo MotivoEncerramento no Chamado
Adicionar `MotivoEncerramento` ao `Chamado` (enum, obrigatório ao fechar/cancelar/forçar).

Opções:
- `Resolvido` — fluxo normal, atendente resolveu
- `CanceladoSolicitante` — solicitante pediu cancelamento
- `AbertoIndevidamente` — chamado não deveria ter sido criado
- `Duplicata` — já existe outro chamado sobre o mesmo assunto
- `SemResposta` — solicitante não respondeu ao contato
- `Outro` — motivo não listado (com campo livre `MotivoOutro`)

### TAG-02: Auditoria no Histórico
- `HistoricoEntrada` deve registrar o motivo: "Chamado fechado. Motivo: Resolvido"
- Forçar encerramento já tem motivo livre — unificar com o enum

### TAG-03: Filtro por Motivo
No `FiltroChamados`, adicionar filtro "Motivo de encerramento" que aparece só quando "Finalizados" está selecionado.

### TAG-04: Dashboard — Chamados por Motivo
Adicionar gráfico de pizza "Chamados finalizados por motivo" no Dashboard (últimos 30 dias).

## Design

### Backend

**Domain/Enums:**
```csharp
public enum MotivoEncerramento
{
    Resolvido,
    CanceladoSolicitante,
    AbertoIndevidamente,
    Duplicata,
    SemResposta,
    Outro
}
```

**Domain/Entities/Chamado.cs** — nova propriedade:
```csharp
public MotivoEncerramento? MotivoEncerramento { get; private set; }
public string? MotivoOutro { get; private set; }
```

Métodos que definem motivo:
- `Resolver()` — define `MotivoEncerramento = Resolvido`
- `Fechar()` — mantém o motivo existente
- `Cancelar(MotivoEncerramento motivo, string? motivoOutro = null)`
- `ForcarEncerramento(MotivoEncerramento motivo, string? motivoOutro = null)`

**DTOs:**
- `ChamadoResponse` ganha `motivoEncerramento` e `motivoOutro`
- `FecharChamadoCommand` ganha `MotivoEncerramento`
- `CancelarChamadoCommand` ganha `MotivoEncerramento`
- `ForcarEncerramentoChamadoCommand` ganha `MotivoEncerramento`

**Repository:**
- Adicionar filtro `MotivoEncerramento?` ao `ListarAsync`

**Dashboard/Relatório:**
- `ContarPorMotivoAsync(DateTime inicio, DateTime fim)`

**Migration:**
- `AddMotivoEncerramentoChamado` — colunas `MotivoEncerramento` e `MotivoOutro`

### Frontend

**api.ts:**
- Adicionar `motivoEncerramento` e `motivoOutro` ao `ChamadoResponse`
- Adicionar `MotivoEncerramento` type

**Fechar/Cancelar/ForcarEncerramento modais:**
- Adicionar select obrigatório de motivo
- Se "Outro", mostrar campo de texto

**FiltroChamados:**
- Adicionar select "Motivo" visível só em finalizados

**Dashboard:**
- Adicionar gráfico de pizza "Por motivo"

## Tasks

1. [ ] Domain: enum `MotivoEncerramento`
2. [ ] Domain: propriedades + métodos no `Chamado`
3. [ ] Migration `AddMotivoEncerramentoChamado`
4. [ ] Application: atualizar Commands (Fechar, Cancelar, ForcarEncerrar)
5. [ ] Application: atualizar Handlers para usar motivo
6. [ ] Application: atualizar `ChamadoResponse` e `ChamadoMappings`
7. [ ] Application: adicionar filtro motivo no `ListarChamadosQuery`
8. [ ] Application: `ContarPorMotivoAsync` no repositório
9. [ ] Frontend: tipos atualizados
10. [ ] Frontend: modais com select de motivo
11. [ ] Frontend: filtro motivo no `FiltroChamados`
12. [ ] Frontend: gráfico no Dashboard
13. [ ] `dotnet test` + `npm run build`
