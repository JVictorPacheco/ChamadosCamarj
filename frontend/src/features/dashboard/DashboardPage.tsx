import { useCallback } from 'react'
import { Link, useNavigate } from 'react-router'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/auth/AuthContext'
import { useDashboardMetrics, useDashboardDistribuicao } from './hooks'
import { DashboardKpi } from './DashboardKpi'
import { CategoriaChart } from './CategoriaChart'
import { DonutChart } from '@/components/charts/DonutChart'

const STATUS_MAP: Record<string, string> = {
  Aguardando: 'Aberto',
  Assumido: 'EmAndamento',
  Resolvido: 'Resolvido',
  Encerrado: 'Fechado',
  Cancelado: 'Cancelado',
}

interface PrioridadeClickData {
  prioridadeNome: string
  quantidade: number
}

export function DashboardPage() {
  const { perfil } = useAuth()
  const navigate = useNavigate()
  const { data: metrics, isPending, isError } = useDashboardMetrics()
  const { data: distribuicao, isPending: distribuicaoPending, isError: distribuicaoError } = useDashboardDistribuicao()

  const handleStatusClick = useCallback((label: string) => {
    const status = STATUS_MAP[label]
    if (status) navigate(`/chamados?status=${status}`)
  }, [navigate])

  const handleCategoriaClick = useCallback((item: { categoriaNome: string; categoriaId?: string | null; quantidade: number }) => {
    if (item.categoriaId) {
      navigate(`/chamados?categoriaId=${item.categoriaId}`)
    }
  }, [navigate])

  const handlePrioridadeClick = useCallback((item: PrioridadeClickData) => {
    navigate(`/chamados?prioridade=${item.prioridadeNome}`)
  }, [navigate])

  if (perfil?.tipo === 'Solicitante') {
    return (
      <div className="flex flex-col items-center gap-3 p-8 text-center">
        <Alert variant="destructive" className="max-w-md">
          <AlertDescription>Esta área não está disponível para o seu perfil.</AlertDescription>
        </Alert>
        <Button asChild variant="outline">
          <Link to="/chamados">Voltar para a lista</Link>
        </Button>
      </div>
    )
  }

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
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
            <DashboardKpi
              titulo="Resolvidos Hoje"
              valor={metrics.totalResolvidosHoje}
              onClick={() => navigate('/chamados?status=Resolvido')}
            />
            <DashboardKpi
              titulo="Tempo Médio"
              valor={metrics.tempoMedioResolucaoHoras != null ? `${metrics.tempoMedioResolucaoHoras}h` : '—'}
              subtexto="Resolução"
            />
            {metrics.slaCompliance && (
              <DashboardKpi
                titulo="SLA (mês)"
                valor={`${metrics.slaCompliance.percentual}%`}
                subtexto={`${metrics.slaCompliance.dentroPrazo} de ${metrics.slaCompliance.totalResolvidos} chamados`}
              />
            )}
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
                  onSliceClick={handleStatusClick}
                />
              ) : (
                <p className="py-8 text-center text-sm text-muted-foreground">Nenhum chamado no sistema.</p>
              )
            )}
          </div>

          <div className="rounded-lg border bg-card p-4">
            <h2 className="mb-3 text-sm font-heading">Chamados Ativos por Categoria</h2>
            {metrics.porCategoria.length > 0 ? (
              <CategoriaChart data={metrics.porCategoria} onBarClick={handleCategoriaClick} />
            ) : (
              <p className="py-8 text-center text-sm text-muted-foreground">Nenhum chamado ativo.</p>
            )}
          </div>

          <div className="rounded-lg border bg-card p-4">
            <h2 className="mb-3 text-sm font-heading">Chamados Ativos por Prioridade</h2>
            {metrics.porPrioridade.length > 0 ? (
              <CategoriaChart
                data={metrics.porPrioridade.map(p => ({ categoriaNome: p.prioridade, quantidade: p.quantidade }))}
                onBarClick={(item) => handlePrioridadeClick({ prioridadeNome: item.categoriaNome, quantidade: item.quantidade })}
              />
            ) : (
              <p className="py-8 text-center text-sm text-muted-foreground">Nenhum chamado ativo.</p>
            )}
          </div>
        </>
      )}
    </div>
  )
}
