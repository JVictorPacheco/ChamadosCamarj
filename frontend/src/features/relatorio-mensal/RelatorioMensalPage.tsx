import { useState } from 'react'
import { Link } from 'react-router'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/auth/AuthContext'
import { DonutChart } from '@/components/charts/DonutChart'
import { DashboardKpi } from '../dashboard/DashboardKpi'
import { CategoriaChart } from '../dashboard/CategoriaChart'
import { SeletorMes } from './components/SeletorMes'
import { useRelatorioMensal } from './hooks/useRelatorioMensal'
import { exportarCsv, imprimirRelatorio } from './exportar'

function formatarVariacao(percentual: number | null): string | undefined {
  if (percentual === null) return undefined
  const sinal = percentual >= 0 ? '+' : ''
  return `${sinal}${percentual}% vs mês anterior`
}

export function RelatorioMensalPage() {
  const { perfil } = useAuth()
  const agora = new Date()
  const [ano, setAno] = useState(agora.getFullYear())
  const [mes, setMes] = useState(agora.getMonth() + 1)

  const isSolicitante = perfil?.tipo === 'Solicitante'
  const responsavelId = perfil?.tipo === 'Atendente' ? perfil.id : undefined
  const { data: relatorio, isPending, isError } = useRelatorioMensal(ano, mes, responsavelId, !isSolicitante)

  const totalGeral = relatorio
    ? relatorio.totalAbertos + relatorio.totalResolvidos + relatorio.totalCancelados
    : 0

  if (isSolicitante) {
    return (
      <div className="flex flex-col items-center gap-3 p-8 text-center">
        <Alert variant="destructive" className="max-w-md">
          <AlertDescription>Este relatório não está disponível para o seu perfil.</AlertDescription>
        </Alert>
        <Button asChild variant="outline">
          <Link to="/chamados">Voltar para a lista</Link>
        </Button>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-4 p-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-xl font-heading">Relatório Mensal</h1>
        <SeletorMes ano={ano} mes={mes} onChange={(a, m) => { setAno(a); setMes(m) }} />
      </div>

      {isError && (
        <Alert variant="destructive">
          <AlertDescription>Serviço indisponível. Tente novamente em instantes.</AlertDescription>
        </Alert>
      )}

      {isPending && <p className="text-sm text-muted-foreground">Carregando relatório...</p>}

      {!isPending && relatorio && totalGeral === 0 && (
        <p className="py-8 text-center text-sm text-muted-foreground">
          Nenhum chamado neste período.
        </p>
      )}

      {!isPending && relatorio && totalGeral > 0 && (
        <div id="relatorio-mensal-conteudo" className="flex flex-col gap-4">
          {relatorio.mesParcial && (
            <Alert>
              <AlertDescription>
                Mês em andamento — dados até hoje, {agora.toLocaleDateString('pt-BR')}.
              </AlertDescription>
            </Alert>
          )}

          <div className="flex flex-wrap gap-2 print:hidden">
            <Button variant="outline" size="sm" onClick={() => exportarCsv(relatorio)}>
              Exportar CSV
            </Button>
            <Button variant="outline" size="sm" onClick={imprimirRelatorio}>
              Exportar PDF (imprimir)
            </Button>
          </div>

          <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
            <DashboardKpi
              titulo="Abertos"
              valor={relatorio.totalAbertos}
              subtexto={formatarVariacao(relatorio.comparacao?.variacaoAbertosPercentual ?? null)}
            />
            <DashboardKpi
              titulo="Resolvidos"
              valor={relatorio.totalResolvidos}
              subtexto={formatarVariacao(relatorio.comparacao?.variacaoResolvidosPercentual ?? null)}
            />
            <DashboardKpi
              titulo="Cancelados"
              valor={relatorio.totalCancelados}
              subtexto={formatarVariacao(relatorio.comparacao?.variacaoCanceladosPercentual ?? null)}
            />
            <DashboardKpi
              titulo="Tempo Médio"
              valor={relatorio.tempoMedioResolucaoHoras != null ? `${relatorio.tempoMedioResolucaoHoras}h` : '—'}
              subtexto="Resolução"
            />
          </div>

          <div className="rounded-lg border bg-card p-4">
            <h2 className="mb-3 text-sm font-heading">Cumprimento de SLA</h2>
            {relatorio.sla.totalComPrazo > 0 ? (
              <DonutChart
                data={[
                  { label: 'Dentro do prazo', value: relatorio.sla.dentroDoPrazo, color: '#22c55e' },
                  { label: 'Estourado', value: relatorio.sla.estourados, color: '#ef4444' },
                ]}
              />
            ) : (
              <p className="py-8 text-center text-sm text-muted-foreground">
                Nenhum chamado resolvido neste período.
              </p>
            )}
          </div>

          <div className="rounded-lg border bg-card p-4">
            <h2 className="mb-3 text-sm font-heading">Por categoria</h2>
            {relatorio.porCategoria.length > 0 ? (
              <CategoriaChart data={relatorio.porCategoria} />
            ) : (
              <p className="py-8 text-center text-sm text-muted-foreground">Sem dados.</p>
            )}
          </div>

          {relatorio.porAtendente && (
            <div className="rounded-lg border bg-card p-4">
              <h2 className="mb-3 text-sm font-heading">Por atendente</h2>
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border text-left text-muted-foreground">
                    <th className="pb-2 font-normal">Atendente</th>
                    <th className="pb-2 font-normal">Abertos</th>
                    <th className="pb-2 font-normal">Resolvidos</th>
                    <th className="pb-2 font-normal">Cancelados</th>
                  </tr>
                </thead>
                <tbody>
                  {relatorio.porAtendente.map((item) => (
                    <tr key={item.responsavelNome} className="border-b border-border last:border-0">
                      <td className="py-2">{item.responsavelNome}</td>
                      <td className="py-2">{item.abertos}</td>
                      <td className="py-2">{item.resolvidos}</td>
                      <td className="py-2">{item.cancelados}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
