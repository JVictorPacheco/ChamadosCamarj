import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  atualizarGrupo,
  criarGrupo,
  listarGrupos,
} from '../api'

export function useGrupos() {
  return useQuery({
    queryKey: ['grupos'],
    queryFn: listarGrupos,
  })
}

export function useCriarGrupo() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: criarGrupo,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['grupos'] })
    },
  })
}

export function useAtualizarGrupo() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: { nome: string; descricao: string } }) =>
      atualizarGrupo(id, dados),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['grupos'] })
    },
  })
}
