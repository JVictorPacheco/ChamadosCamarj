import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { listarAnexos, removerAnexo, uploadAnexo } from '../api'

export function useAnexos(chamadoId: string) {
  return useQuery({
    queryKey: ['anexos', chamadoId],
    queryFn: () => listarAnexos(chamadoId),
  })
}

export function useUploadAnexo(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (arquivo: File) => uploadAnexo(chamadoId, arquivo),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['anexos', chamadoId] }),
  })
}

export function useRemoverAnexo(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (anexoId: string) => removerAnexo(chamadoId, anexoId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['anexos', chamadoId] }),
  })
}
