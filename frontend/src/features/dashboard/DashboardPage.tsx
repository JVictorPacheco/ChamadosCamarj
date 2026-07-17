import { Alert, AlertDescription } from '@/components/ui/alert'
import { useDashboardMetrics, useDashboardDistribuicao } from './hooks'
import { DashboardKpi } from './DashboardKpi'
import { CategoriaChart } from './CategoriaChart'
import { DonutChart } from '@/components/charts/DonutChart'

export function DashboardPage() {
  const { data: metrics, isPending, isError } = useDashboardMetrics()
  const { data: distribuicao, isPending: distribuicaoPending, isError: distribuicaoError } = useDashboardDistribuicao()

  const totalDistribuicao = distribuicao
    ? distribuicao.aguardando +
      distribuicao.assumido +
      distribuicao.resolvido +
      distribuicao.encerrado +
      distribuicao.cancelado
    : 0

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
            {distribuicaoPending && (
              <p className="py-8 text-center text-sm text-muted-foreground">Carregando distribuição...</p>
            )}

            {!distribuicaoPending && distribuicaoError && (
              <Alert variant="destructive">
                <AlertDescription>Serviço indisponível. Tente novamente em instantes.</AlertDescription>
              </Alert>
            )}

            {!distribuicaoPending && !distribuicaoError && distribuicao && (
              totalDistribuicao > 0 ? (
                <DonutChart
                  data={[
                    { label: 'Aguardando', value: distribuicao.aguardando, color: 'var(--chart-3)' },
                    { label: 'Assumido', value: distribuicao.assumido, color: 'var(--chart-1)' },
                    { label: 'Resolvido', value: distribuicao.resolvido, color: 'var(--chart-4)' },
                    { label: 'Encerrado', value: distribuicao.encerrado, color: 'var(--chart-5)' },
                    { label: 'Cancelado', value: distribuicao.cancelado, color: 'var(--chart-2)' },
                  ]}
                />
              ) : (
                <p className="py-8 text-center text-sm text-muted-foreground">Nenhum chamado no sistema.</p>
              )
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
