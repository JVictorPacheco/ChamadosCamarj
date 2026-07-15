import { Alert, AlertDescription } from '@/components/ui/alert'
import { useDashboardMetrics, useDashboardDistribuicao } from './hooks'
import { DashboardKpi } from './DashboardKpi'
import { CategoriaChart } from './CategoriaChart'
import { DonutChart } from '@/components/charts/DonutChart'

export function DashboardPage() {
  const { data: metrics, isPending, isError } = useDashboardMetrics()
  const { data: distribuicao } = useDashboardDistribuicao()

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
          <div className="grid grid-cols-2 gap-3">
            <DashboardKpi titulo="Resolvidos Hoje" valor={metrics.totalResolvidosHoje} />
            <DashboardKpi
              titulo="Tempo Médio"
              valor={metrics.tempoMedioResolucaoHoras != null ? `${metrics.tempoMedioResolucaoHoras}h` : '—'}
              subtexto="Resolução"
            />
          </div>

          <div className="rounded-lg border bg-card p-4">
            <h2 className="mb-3 text-sm font-heading">Distribuição por situação</h2>
            {distribuicao &&
            (distribuicao.aguardando +
              distribuicao.assumido +
              distribuicao.resolvido +
              distribuicao.encerrado +
              distribuicao.cancelado) >
              0 ? (
              <DonutChart
                data={[
                  { label: 'Aguardando', value: distribuicao.aguardando, color: '#f59e0b' },
                  { label: 'Assumido', value: distribuicao.assumido, color: '#3b82f6' },
                  { label: 'Resolvido', value: distribuicao.resolvido, color: '#22c55e' },
                  { label: 'Encerrado', value: distribuicao.encerrado, color: '#a855f7' },
                  { label: 'Cancelado', value: distribuicao.cancelado, color: '#94a3b8' },
                ]}
              />
            ) : (
              <p className="py-8 text-center text-sm text-muted-foreground">Nenhum chamado no sistema.</p>
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
