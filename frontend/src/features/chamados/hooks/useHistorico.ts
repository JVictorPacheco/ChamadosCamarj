import { useQuery } from '@tanstack/react-query'
import { listarHistorico } from '../api'

export function useHistorico(chamadoId: string) {
  return useQuery({
    queryKey: ['historico', chamadoId],
    queryFn: () => listarHistorico(chamadoId),
  })
}
