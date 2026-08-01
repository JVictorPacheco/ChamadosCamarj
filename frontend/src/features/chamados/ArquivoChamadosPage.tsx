import { useState } from 'react'
import { Link } from 'react-router'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { useAuth } from '@/auth/AuthContext'
import { ChamadoCard } from './components/ChamadoCard'
import { FiltroChamados, type FiltroChamadosValue } from './components/FiltroChamados'
import { useChamados } from './hooks/useChamados'
import type { StatusChamado } from '@/types/api'

const STATUS_FINALIZADOS: StatusChamado[] = ['Resolvido', 'Fechado', 'Cancelado']

export function ArquivoChamadosPage() {
  const { perfil } = useAuth()
  const [filtros, setFiltros] = useState<FiltroChamadosValue>({})
  const [pagina, setPagina] = useState(1)

  const isAdmin = perfil?.tipo === 'Admin'
  const isAtendente = perfil?.tipo === 'Atendente'

  const filtrosQuery = {
    ...filtros,
    pagina,
    finalizados: true,
    ...(isAdmin ? {} : isAtendente
      ? { responsavelId: perfil?.id }
      : { solicitanteEmail: perfil?.email }),
  }

  const { data, isPending, isError } = useChamados(filtrosQuery)

  const handleFiltrosChange = (novosFiltros: FiltroChamadosValue) => {
    setFiltros(novosFiltros)
    setPagina(1)
  }

  return (
    <div className="flex flex-col gap-4 p-4">
      <h1 className="text-xl font-heading">Arquivo de Chamados</h1>

      <FiltroChamados
        value={filtros}
        onChange={handleFiltrosChange}
        statusOptions={STATUS_FINALIZADOS}
        mostrarPeriodo
        mostrarMotivo
      />

      {isError && (
        <Alert variant="destructive">
          <AlertDescription>Serviço indisponível. Tente novamente em instantes.</AlertDescription>
        </Alert>
      )}

      {isPending && <p className="text-sm text-muted-foreground">Carregando...</p>}

      {!isPending && !isError && data?.items.length === 0 && (
        <div className="flex flex-col items-center gap-3 rounded-lg border border-dashed border-border p-8 text-center">
          <p className="text-sm text-muted-foreground">Nenhum chamado finalizado ainda.</p>
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
            <Button variant="outline" size="sm" disabled={!data.temAnterior} onClick={() => setPagina((p) => p - 1)}>
              Anterior
            </Button>
            <span className="text-sm text-muted-foreground">
              Página {data.pagina} de {data.totalPaginas}
            </span>
            <Button variant="outline" size="sm" disabled={!data.temProxima} onClick={() => setPagina((p) => p + 1)}>
              Próxima
            </Button>
          </div>
        </>
      )}
    </div>
  )
}
