import { useQuery } from '@tanstack/react-query'
import { listarPresencas } from '../api'

export function usePresencas() {
  return useQuery({
    queryKey: ['chat', 'presencas'],
    queryFn: () => listarPresencas(),
    refetchInterval: 30_000,
  })
}
