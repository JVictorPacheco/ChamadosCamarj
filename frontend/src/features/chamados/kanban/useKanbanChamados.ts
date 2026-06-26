import { useQuery, useQueryClient } from '@tanstack/react-query'
import { listarChamados } from '@/features/chamados/api'
import type { ChamadoResponse } from '@/types/api'
import { useSignalR } from '@/hooks/useSignalR'
import { useEffect } from 'react'

export function useKanbanChamados() {
  const queryClient = useQueryClient()
  const { subscribe } = useSignalR()

  const query = useQuery<ChamadoResponse[]>({
    queryKey: ['chamados', 'kanban'],
    queryFn: async () => {
      // Busca todos os chamados (até 100 por vez) para o kanban
      const result = await listarChamados({ pagina: 1, tamanhoPagina: 100 })
      return result.items
    },
    staleTime: 10_000,
  })

  // Atualiza a lista quando chega evento SignalR
  useEffect(() => {
    return subscribe(() => {
      queryClient.invalidateQueries({ queryKey: ['chamados', 'kanban'] })
    })
  }, [subscribe, queryClient])

  return query
}
