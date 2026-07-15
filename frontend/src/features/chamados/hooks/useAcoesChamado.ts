import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/auth/AuthContext'
import {
  atribuirChamado,
  resolverChamado,
  fecharChamado,
  cancelarChamado,
  reatribuirChamado,
  alterarPrioridade,
} from '@/features/chamados/api'
import type { AtorRequest, AtribuirRequest, ReatribuirRequest } from '@/features/chamados/api'
import type { PrioridadeChamado } from '@/types/api'

function invalidarChamado(queryClient: ReturnType<typeof useQueryClient>, id: string) {
  queryClient.invalidateQueries({ queryKey: ['chamado', id] })
  queryClient.invalidateQueries({ queryKey: ['chamados'] })
  queryClient.invalidateQueries({ queryKey: ['historico', id] })
}

function useAtorAtual(): AtorRequest {
  const { perfil } = useAuth()
  return { usuarioId: perfil?.id ?? '', usuarioNome: perfil?.nome ?? 'Sistema' }
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
  const ator = useAtorAtual()
  return useMutation({
    mutationFn: () => resolverChamado(chamadoId, ator),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useFecharChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  const ator = useAtorAtual()
  return useMutation({
    mutationFn: () => fecharChamado(chamadoId, ator),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useCancelarChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  const ator = useAtorAtual()
  return useMutation({
    mutationFn: () => cancelarChamado(chamadoId, ator),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useReatribuirChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  const ator = useAtorAtual()
  return useMutation({
    mutationFn: (dados: ReatribuirRequest) => reatribuirChamado(chamadoId, dados, ator),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useAlterarPrioridadeChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  const ator = useAtorAtual()
  return useMutation({
    mutationFn: (novaPrioridade: PrioridadeChamado) => alterarPrioridade(chamadoId, novaPrioridade, ator),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}
