import { useQuery } from '@tanstack/react-query'
import { listarComentarios } from '../api'

export function useComentarios(chamadoId: string) {
  return useQuery({
    queryKey: ['comentarios', chamadoId],
    queryFn: () => listarComentarios(chamadoId),
  })
}
