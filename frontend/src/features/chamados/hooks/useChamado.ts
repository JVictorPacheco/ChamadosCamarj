import { useQuery } from '@tanstack/react-query'
import { obterChamado } from '../api'

export function useChamado(id: string) {
  return useQuery({
    queryKey: ['chamado', id],
    queryFn: () => obterChamado(id),
  })
}
