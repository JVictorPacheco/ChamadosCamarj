import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  atualizarTipo,
  criarTipo,
  excluirTipo,
  listarTiposAdmin,
} from '../api'

export function useTiposAdmin() {
  return useQuery({
    queryKey: ['tipos'],
    queryFn: listarTiposAdmin,
  })
}

export function useCriarTipo() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: criarTipo,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tipos'] })
    },
  })
}

export function useAtualizarTipo() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: { nome: string; descricao: string; ativo: boolean } }) =>
      atualizarTipo(id, dados),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tipos'] })
    },
  })
}

export function useExcluirTipo() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => excluirTipo(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tipos'] })
    },
  })
}
