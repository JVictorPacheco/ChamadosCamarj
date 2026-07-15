import { useQuery } from '@tanstack/react-query'
import { listarComentarios } from '../api'

export function useComentarios(chamadoId: string, perfilUsuario?: string) {
  return useQuery({
    queryKey: ['comentarios', chamadoId, perfilUsuario],
    queryFn: () => listarComentarios(chamadoId, perfilUsuario),
  })
}
