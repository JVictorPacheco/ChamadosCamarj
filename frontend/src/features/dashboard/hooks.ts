import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { obterMetricas, obterDistribuicao } from './api'
import { useSignalR } from '@/hooks/useSignalR'

export function useDashboardMetrics() {
  const queryClient = useQueryClient()
  const { subscribe } = useSignalR()

  const query = useQuery({
    queryKey: ['dashboard', 'metricas'],
    queryFn: obterMetricas,
    staleTime: 15_000,
  })

  useEffect(() => {
    return subscribe((event) => {
      if (event.type === 'MetricasAtualizadas') {
        queryClient.invalidateQueries({ queryKey: ['dashboard', 'metricas'] })
      }
    })
  }, [subscribe, queryClient])

  return query
}

export function useDashboardDistribuicao() {
  const queryClient = useQueryClient()
  const { subscribe } = useSignalR()

  const query = useQuery({
    queryKey: ['dashboard', 'distribuicao'],
    queryFn: obterDistribuicao,
    staleTime: 15_000,
  })

  useEffect(() => {
    return subscribe((event) => {
      if (event.type === 'MetricasAtualizadas') {
        queryClient.invalidateQueries({ queryKey: ['dashboard', 'distribuicao'] })
      }
    })
  }, [subscribe, queryClient])

  return query
}
