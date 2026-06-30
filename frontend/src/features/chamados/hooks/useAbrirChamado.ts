import { useMutation, useQueryClient } from '@tanstack/react-query'
import { abrirChamado } from '../api'

export function useAbrirChamado() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: abrirChamado,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chamados'] })
    },
  })
}
