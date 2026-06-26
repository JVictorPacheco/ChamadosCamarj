import { useDroppable } from '@dnd-kit/core'
import type { ChamadoResponse, StatusChamado } from '@/types/api'
import { KanbanCard } from './KanbanCard'

interface KanbanColumnProps {
  status: StatusChamado
  titulo: string
  chamados: ChamadoResponse[]
  cor: string
}

export function KanbanColumn({ status, titulo, chamados, cor }: KanbanColumnProps) {
  const { setNodeRef, isOver } = useDroppable({ id: status })

  return (
    <div
      ref={setNodeRef}
      className={`flex min-h-[200px] flex-col gap-2 rounded-lg border-2 p-3 transition-colors ${
        isOver ? 'border-blue-400 bg-blue-50' : 'border-border bg-muted/20'
      }`}
    >
      <div className="flex items-center gap-2">
        <div className={`h-3 w-3 rounded-full ${cor}`} />
        <h2 className="text-sm font-heading">{titulo}</h2>
        <span className="ml-auto text-xs text-muted-foreground">{chamados.length}</span>
      </div>

      <div className="flex flex-col gap-2">
        {chamados.map((chamado) => (
          <KanbanCard key={chamado.id} chamado={chamado} />
        ))}
        {chamados.length === 0 && (
          <p className="py-4 text-center text-xs text-muted-foreground">Nenhum chamado</p>
        )}
      </div>
    </div>
  )
}
