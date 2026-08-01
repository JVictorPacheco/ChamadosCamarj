import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/auth/AuthContext'
import {
  atualizarUsuario,
  criarUsuario,
  listarUsuarios,
  redefinirSenha,
  type AtualizarUsuarioRequest,
  type CriarUsuarioRequest,
} from '@/auth/api'

export function useUsuarios() {
  const { perfil } = useAuth()

  return useQuery({
    queryKey: ['usuarios'],
    queryFn: () => listarUsuarios(),
    enabled: !!perfil,
  })
}

export function useCriarUsuario() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (dados: CriarUsuarioRequest) => criarUsuario(dados),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] })
    },
  })
}

export function useRedefinirSenha() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, novaSenha }: { id: string; novaSenha: string }) => redefinirSenha(id, novaSenha),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] })
    },
  })
}

export function useAtualizarUsuario() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: AtualizarUsuarioRequest }) =>
      atualizarUsuario(id, dados),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] })
    },
  })
}
