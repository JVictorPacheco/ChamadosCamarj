import { Alert, AlertDescription } from '@/components/ui/alert'
import { useDashboardMetrics, useDashboardTendencia } from './hooks'
import { DashboardKpi } from './DashboardKpi'
import { TendenciaChart } from './TendenciaChart'
import { CategoriaChart } from './CategoriaChart'

export function DashboardPage() {
  const { data: metrics, isPending, isError } = useDashboardMetrics()
  const { data: tendencia } = useDashboardTendencia(7)

  return (
    <div className="flex flex-col gap-4 p-4">
      <h1 className="text-xl font-heading">Dashboard</h1>

      {isError && (
        <Alert variant="destructive">
          <AlertDescription>Serviço indisponível. Tente novamente em instantes.</AlertDescription>
        </Alert>
      )}

      {isPending && <p className="text-sm text-muted-foreground">Carregando métricas...</p>}

      {!isPending && metrics && (
        <>
          <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
            <DashboardKpi titulo="Abertos" valor={metrics.totalAbertos} />
            <DashboardKpi titulo="Em Andamento" valor={metrics.totalEmAndamento} />
            <DashboardKpi titulo="Resolvidos Hoje" valor={metrics.totalResolvidosHoje} />
            <DashboardKpi
              titulo="Tempo Médio"
              valor={metrics.tempoMedioResolucaoHoras != null ? `${metrics.tempoMedioResolucaoHoras}h` : '—'}
              subtexto="Resolução"
            />
          </div>

          <div className="rounded-lg border bg-card p-4">
            <h2 className="mb-3 text-sm font-heading">Tendência (7 dias)</h2>
            {tendencia && tendencia.items.length > 0 ? (
              <TendenciaChart data={tendencia.items} />
            ) : (
              <p className="py-8 text-center text-sm text-muted-foreground">Sem dados no período.</p>
            )}
          </div>

          <div className="rounded-lg border bg-card p-4">
            <h2 className="mb-3 text-sm font-heading">Chamados Ativos por Categoria</h2>
            {metrics.porCategoria.length > 0 ? (
              <CategoriaChart data={metrics.porCategoria} />
            ) : (
              <p className="py-8 text-center text-sm text-muted-foreground">Nenhum chamado ativo.</p>
            )}
          </div>
        </>
      )}
    </div>
  )
}
