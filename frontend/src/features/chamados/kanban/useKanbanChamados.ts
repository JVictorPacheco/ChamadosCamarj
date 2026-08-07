import { useState, useCallback, useEffect } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { listarChamados } from '@/features/chamados/api'
import type { ChamadoResponse } from '@/types/api'
import { useSignalR } from '@/hooks/useSignalR'

export function useKanbanChamados() {
  const queryClient = useQueryClient()
  const { subscribe } = useSignalR()
  const [paginaAtual, setPaginaAtual] = useState(1)
  const [temMais, setTemMais] = useState(false)
  const [carregandoMais, setCarregandoMais] = useState(false)

  const query = useQuery<ChamadoResponse[]>({
    queryKey: ['chamados', 'kanban'],
    queryFn: async () => {
      const result = await listarChamados({ pagina: 1, tamanhoPagina: 100 })
      setTemMais(result.temProxima)
      setPaginaAtual(1)
      return result.items
    },
    staleTime: 10_000,
  })

  const carregarMais = useCallback(async () => {
    if (carregandoMais || !temMais) return
    setCarregandoMais(true)
    try {
      const proximaPagina = paginaAtual + 1
      const result = await listarChamados({ pagina: proximaPagina, tamanhoPagina: 100 })
      queryClient.setQueryData<ChamadoResponse[]>(['chamados', 'kanban'], (old) => {
        if (!old) return result.items
        const existingIds = new Set(old.map(c => c.id))
        const newItems = result.items.filter(c => !existingIds.has(c.id))
        return [...old, ...newItems]
      })
      setTemMais(result.temProxima)
      setPaginaAtual(proximaPagina)
    } finally {
      setCarregandoMais(false)
    }
  }, [carregandoMais, temMais, paginaAtual, queryClient])

  useEffect(() => {
    return subscribe((event) => {
      if (event.type === 'ChamadoCriado' || event.type === 'StatusAlterado') {
        queryClient.invalidateQueries({ queryKey: ['chamados', 'kanban'] })
      }
    })
  }, [subscribe, queryClient])

  return {
    data: query.data,
    isPending: query.isPending,
    isError: query.isError,
    temMais,
    carregarMais,
    carregandoMais,
  }
}
