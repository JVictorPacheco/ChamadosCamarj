import { useCallback } from 'react'
import { Link, useSearchParams } from 'react-router'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { useAuth } from '@/auth/AuthContext'
import { ChamadoCard } from './components/ChamadoCard'
import { FiltroChamados, type FiltroChamadosValue } from './components/FiltroChamados'
import { useChamados } from './hooks/useChamados'
import type { PrioridadeChamado, StatusChamado, SlaStatus } from '@/types/api'

const STATUS_VALUES: StatusChamado[] = ['Aberto', 'EmAndamento', 'Resolvido', 'Fechado', 'Cancelado']
const PRIORIDADE_VALUES: PrioridadeChamado[] = ['Baixa', 'Media', 'Alta', 'Urgente']
const SLA_VALUES: SlaStatus[] = ['DentroPrazo', 'Atencao', 'Atrasado']

function parseFiltrosFromParams(searchParams: URLSearchParams): FiltroChamadosValue {
  const status = searchParams.get('status') as StatusChamado | null
  const prioridade = searchParams.get('prioridade') as PrioridadeChamado | null
  const categoriaId = searchParams.get('categoriaId')
  const busca = searchParams.get('busca')
  const slaStatus = searchParams.get('slaStatus') as SlaStatus | null

  return {
    ...(status && STATUS_VALUES.includes(status) ? { status } : {}),
    ...(prioridade && PRIORIDADE_VALUES.includes(prioridade) ? { prioridade } : {}),
    ...(categoriaId ? { categoriaId } : {}),
    ...(busca ? { busca } : {}),
    ...(slaStatus && SLA_VALUES.includes(slaStatus) ? { slaStatus } : {}),
  }
}

export function ChamadosListPage() {
  const { perfil } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const initialFiltros = parseFiltrosFromParams(searchParams)
  const pageFromUrl = Number(searchParams.get('pagina')) || 1

  const isAdmin = perfil?.tipo === 'Admin'
  const isAtendente = perfil?.tipo === 'Atendente'

  const filtros = initialFiltros
  const pagina = pageFromUrl

  const filtrosQuery = {
    ...filtros,
    pagina,
    ...(isAdmin ? {} : isAtendente
      ? { responsavelId: perfil?.id }
      : { solicitanteEmail: perfil?.email }),
  }

  const { data, isPending, isError } = useChamados(filtrosQuery)

  const handleFiltrosChange = useCallback((novosFiltros: FiltroChamadosValue) => {
    const params = new URLSearchParams()
    if (novosFiltros.status) params.set('status', novosFiltros.status)
    if (novosFiltros.prioridade) params.set('prioridade', novosFiltros.prioridade)
    if (novosFiltros.categoriaId) params.set('categoriaId', novosFiltros.categoriaId)
    if (novosFiltros.busca) params.set('busca', novosFiltros.busca)
    if (novosFiltros.slaStatus) params.set('slaStatus', novosFiltros.slaStatus)
    setSearchParams(params, { replace: true })
  }, [setSearchParams])

  const setPagina = useCallback((novaPagina: number) => {
    const params = new URLSearchParams(searchParams)
    if (novaPagina <= 1) params.delete('pagina')
    else params.set('pagina', String(novaPagina))
    setSearchParams(params, { replace: true })
  }, [searchParams, setSearchParams])

  return (
    <div className="flex flex-col gap-4 p-4">
      <h1 className="text-xl font-heading">
        {isAdmin ? 'Todos os Chamados' : isAtendente ? 'Chamados em Atendimento' : 'Meus Chamados'}
      </h1>

      <FiltroChamados value={filtros} onChange={handleFiltrosChange} mostrarSla />

      {isError && (
        <Alert variant="destructive">
          <AlertDescription>Serviço indisponível. Tente novamente em instantes.</AlertDescription>
        </Alert>
      )}

      {isPending && <p className="text-sm text-muted-foreground">Carregando...</p>}

      {!isPending && !isError && data?.items.length === 0 && (
        <div className="flex flex-col items-center gap-3 rounded-lg border border-dashed border-border p-8 text-center">
          <p className="text-sm text-muted-foreground">Você ainda não tem chamados.</p>
          <Button asChild>
            <Link to="/chamados/novo">Abrir chamado</Link>
          </Button>
        </div>
      )}

      {!isPending && data && data.items.length > 0 && (
        <>
          <div className="flex flex-col gap-3">
            {data.items.map((chamado) => (
              <Link key={chamado.id} to={`/chamados/${chamado.id}`} className="block">
                <ChamadoCard chamado={chamado} />
              </Link>
            ))}
          </div>

          <div className="flex items-center justify-between">
            <Button variant="outline" size="sm" disabled={!data.temAnterior} onClick={() => setPagina(pagina - 1)}>
              Anterior
            </Button>
            <span className="text-sm text-muted-foreground">
              Página {data.pagina} de {data.totalPaginas}
            </span>
            <Button variant="outline" size="sm" disabled={!data.temProxima} onClick={() => setPagina(pagina + 1)}>
              Próxima
            </Button>
          </div>
        </>
      )}
    </div>
  )
}
