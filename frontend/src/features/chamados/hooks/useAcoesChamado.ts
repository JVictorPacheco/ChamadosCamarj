import { useMutation, useQueryClient } from '@tanstack/react-query'
import { atribuirChamado, resolverChamado, fecharChamado, cancelarChamado } from '@/features/chamados/api'
import type { AtribuirRequest } from '@/features/chamados/api'

function invalidarChamado(queryClient: ReturnType<typeof useQueryClient>, id: string) {
  queryClient.invalidateQueries({ queryKey: ['chamado', id] })
  queryClient.invalidateQueries({ queryKey: ['chamados'] })
}

export function useAtribuirChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dados: AtribuirRequest) => atribuirChamado(chamadoId, dados),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useResolverChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => resolverChamado(chamadoId),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useFecharChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => fecharChamado(chamadoId),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useCancelarChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => cancelarChamado(chamadoId),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}
