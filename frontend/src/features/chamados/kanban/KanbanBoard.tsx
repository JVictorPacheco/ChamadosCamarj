import { useState, useCallback } from 'react'
import { DndContext, type DragEndEvent, type DragStartEvent, PointerSensor, useSensor, useSensors } from '@dnd-kit/core'
import { useQueryClient } from '@tanstack/react-query'
import type { ChamadoResponse, StatusChamado } from '@/types/api'
import { alterarStatus } from '@/features/chamados/api'
import { KanbanColumn } from './KanbanColumn'

const COLUNAS: { status: StatusChamado; titulo: string; cor: string }[] = [
  { status: 'Aberto', titulo: 'Aberto', cor: 'bg-red-500' },
  { status: 'EmAndamento', titulo: 'Em Andamento', cor: 'bg-yellow-500' },
  { status: 'Resolvido', titulo: 'Resolvido', cor: 'bg-green-500' },
  { status: 'Fechado', titulo: 'Fechado', cor: 'bg-gray-500' },
  { status: 'Cancelado', titulo: 'Cancelado', cor: 'bg-purple-500' },
]

interface KanbanBoardProps {
  chamados: ChamadoResponse[]
}

export function KanbanBoard({ chamados }: KanbanBoardProps) {
  const queryClient = useQueryClient()
  const [draggingId, setDraggingId] = useState<string | null>(null)

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: { distance: 8 },
    }),
  )

  const grouped = COLUNAS.map((coluna) => ({
    ...coluna,
    chamados: chamados.filter((c) => c.status === coluna.status),
  }))

  const handleDragStart = useCallback((event: DragStartEvent) => {
    setDraggingId(String(event.active.id))
  }, [])

  const handleDragEnd = useCallback(
    async (event: DragEndEvent) => {
      setDraggingId(null)
      const { active, over } = event
      if (!over) return

      const chamadoId = String(active.id)
      const novoStatus = over.id as StatusChamado

      const chamado = chamados.find((c) => c.id === chamadoId)
      if (!chamado || chamado.status === novoStatus) return

      // Otimista: atualiza o cache imediatamente
      queryClient.setQueryData<ChamadoResponse[]>(['chamados', 'kanban'], (old) =>
        old?.map((c) => (c.id === chamadoId ? { ...c, status: novoStatus } : c)),
      )

      try {
        await alterarStatus(chamadoId, novoStatus)
      } catch {
        // Reverte em caso de erro
        queryClient.invalidateQueries({ queryKey: ['chamados', 'kanban'] })
      }
    },
    [chamados, queryClient],
  )

  return (
    <DndContext sensors={sensors} onDragStart={handleDragStart} onDragEnd={handleDragEnd}>
      <div className="grid grid-cols-5 gap-3 p-4 overflow-x-auto min-w-[900px]">
        {grouped.map((coluna) => (
          <KanbanColumn
            key={coluna.status}
            status={coluna.status}
            titulo={coluna.titulo}
            chamados={coluna.chamados}
            cor={coluna.cor}
          />
        ))}
      </div>
      {draggingId && (
        <p className="px-4 pb-2 text-xs text-muted-foreground">Arrastando chamado #{draggingId.slice(0, 8)}...</p>
      )}
    </DndContext>
  )
}
