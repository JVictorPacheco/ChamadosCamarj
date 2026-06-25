import { useState } from 'react'
import { Link } from 'react-router'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { useAuth } from '@/auth/AuthContext'
import { ChamadoCard } from './components/ChamadoCard'
import { FiltroChamados, type FiltroChamadosValue } from './components/FiltroChamados'
import { useChamados } from './hooks/useChamados'

export function ChamadosListPage() {
  const { perfil } = useAuth()
  const [filtros, setFiltros] = useState<FiltroChamadosValue>({})
  const [pagina, setPagina] = useState(1)

  const { data, isPending, isError } = useChamados({
    ...filtros,
    pagina,
    solicitanteEmail: perfil?.email,
  })

  const handleFiltrosChange = (novosFiltros: FiltroChamadosValue) => {
    setFiltros(novosFiltros)
    setPagina(1)
  }

  return (
    <div className="flex flex-col gap-4 p-4">
      <h1 className="text-xl font-heading">Meus Chamados</h1>

      <FiltroChamados value={filtros} onChange={handleFiltrosChange} />

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
