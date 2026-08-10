import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Link } from 'react-router'
import { useAuth } from '@/auth/AuthContext'
import { KanbanBoard } from './kanban/KanbanBoard'
import { useKanbanChamados } from './kanban/useKanbanChamados'

export function KanbanPage() {
  const { perfil } = useAuth()
  const { data: chamados, isPending, isError, temMais, carregarMais, carregandoMais } = useKanbanChamados()

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

  return (
    <div className="flex flex-col">
      <div className="flex items-center justify-between px-4 pt-4">
        <h1 className="text-xl font-heading">Kanban</h1>
        <Button asChild size="sm">
          <Link to="/chamados/novo">+ Novo Chamado</Link>
        </Button>
      </div>

      {isError && (
        <Alert variant="destructive" className="mx-4 mt-4">
          <AlertDescription>Serviço indisponível. Tente novamente em instantes.</AlertDescription>
        </Alert>
      )}

      {isPending && <p className="p-4 text-sm text-muted-foreground">Carregando kanban...</p>}

      {!isPending && chamados && (
        <>
          <KanbanBoard chamados={chamados} />
          {temMais && (
            <div className="flex justify-center px-4 pb-4">
              <Button variant="outline" size="sm" onClick={carregarMais} disabled={carregandoMais}>
                {carregandoMais ? 'Carregando...' : 'Carregar mais'}
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  )
}
