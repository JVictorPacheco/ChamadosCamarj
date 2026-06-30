import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { obterMetricas, obterTendencia } from './api'
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

export function useDashboardTendencia(dias = 7) {
  return useQuery({
    queryKey: ['dashboard', 'tendencia', dias],
    queryFn: () => obterTendencia(dias),
    staleTime: 30_000,
  })
}
