import { useMutation, useQueryClient } from '@tanstack/react-query'
import { comentar } from '../api'
import type { ComentarChamadoRequest } from '@/types/api'

export function useComentar(chamadoId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dados: ComentarChamadoRequest) => comentar(chamadoId, dados),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['comentarios', chamadoId] })
    },
  })
}
