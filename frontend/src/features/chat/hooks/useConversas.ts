import { useQuery } from '@tanstack/react-query'
import { listarConversas } from '../api'

export function useConversas() {
  return useQuery({
    queryKey: ['chat', 'conversas'],
    queryFn: () => listarConversas(),
  })
}
