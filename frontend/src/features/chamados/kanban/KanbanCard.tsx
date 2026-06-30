import { useDraggable } from '@dnd-kit/core'
import type { ChamadoResponse } from '@/types/api'
import { ChamadoCard } from '@/features/chamados/components/ChamadoCard'

export function KanbanCard({ chamado }: { chamado: ChamadoResponse }) {
  const { attributes, listeners, setNodeRef, transform, isDragging } = useDraggable({
    id: chamado.id,
    data: { chamado },
  })

  const style = transform
    ? {
        transform: `translate(${transform.x}px, ${transform.y}px)`,
        opacity: isDragging ? 0.5 : 1,
        zIndex: isDragging ? 50 : undefined,
      }
    : undefined

  return (
    <div ref={setNodeRef} style={style} {...listeners} {...attributes} className="cursor-grab active:cursor-grabbing">
      <ChamadoCard chamado={chamado} />
    </div>
  )
}
