import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Link } from 'react-router'
import { KanbanBoard } from './kanban/KanbanBoard'
import { useKanbanChamados } from './kanban/useKanbanChamados'

export function KanbanPage() {
  const { data: chamados, isPending, isError } = useKanbanChamados()

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

      {!isPending && chamados && <KanbanBoard chamados={chamados} />}
    </div>
  )
}
