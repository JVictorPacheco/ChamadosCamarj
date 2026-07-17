import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/auth/AuthContext'
import {
  atualizarUsuario,
  criarUsuario,
  listarUsuarios,
  type AtualizarUsuarioRequest,
  type CriarUsuarioRequest,
} from '@/auth/api'

export function useUsuarios() {
  const { perfil } = useAuth()
  const perfilRequisitante = perfil?.tipo ?? 'Admin'

  return useQuery({
    queryKey: ['usuarios', perfilRequisitante],
    queryFn: () => listarUsuarios(perfilRequisitante),
    enabled: !!perfil,
  })
}

export function useCriarUsuario() {
  const queryClient = useQueryClient()
  const { perfil } = useAuth()
  const perfilRequisitante = perfil?.tipo ?? 'Admin'

  return useMutation({
    mutationFn: (dados: CriarUsuarioRequest) => criarUsuario(dados, perfilRequisitante),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] })
    },
  })
}

export function useAtualizarUsuario() {
  const queryClient = useQueryClient()
  const { perfil } = useAuth()
  const perfilRequisitante = perfil?.tipo ?? 'Admin'

  return useMutation({
    mutationFn: ({ id, dados }: { id: string; dados: AtualizarUsuarioRequest }) =>
      atualizarUsuario(id, dados, perfilRequisitante),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['usuarios'] })
    },
  })
}
