import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  atualizarCategoria,
  criarCategoria,
  excluirCategoria,
  listarCategoriasAdmin,
} from '../api'

export function useCategoriasAdmin() {
  return useQuery({
    queryKey: ['categorias'],
    queryFn: listarCategoriasAdmin,
  })
}

export function useCriarCategoria() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: criarCategoria,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categorias'] })
    },
  })
}

export function useAtualizarCategoria() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: { nome: string; descricao: string; ativa: boolean } }) =>
      atualizarCategoria(id, dados),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categorias'] })
    },
  })
}

export function useExcluirCategoria() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => excluirCategoria(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categorias'] })
    },
  })
}
