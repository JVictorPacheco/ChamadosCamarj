import { useMutation, useQueryClient } from '@tanstack/react-query'
import {
  atribuirChamado,
  resolverChamado,
  fecharChamado,
  cancelarChamado,
  reabrirChamado,
  reatribuirChamado,
  alterarPrioridade,
  forcarEncerramento,
} from '@/features/chamados/api'
import type { ReatribuirRequest } from '@/features/chamados/api'
import type { MotivoEncerramento, PrioridadeChamado } from '@/types/api'

function invalidarChamado(queryClient: ReturnType<typeof useQueryClient>, id: string) {
  queryClient.invalidateQueries({ queryKey: ['chamado', id] })
  queryClient.invalidateQueries({ queryKey: ['chamados'] })
  queryClient.invalidateQueries({ queryKey: ['historico', id] })
  queryClient.invalidateQueries({ queryKey: ['comentarios', id] })
  queryClient.invalidateQueries({ queryKey: ['anexos', id] })
}

export function useAtribuirChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => atribuirChamado(chamadoId),
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
    mutationFn: (dados: { motivo: MotivoEncerramento; motivoOutro?: string; observacao?: string }) =>
      fecharChamado(chamadoId, dados.motivo, dados.motivoOutro, dados.observacao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useCancelarChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dados: { motivo: MotivoEncerramento; motivoOutro?: string; observacao?: string }) =>
      cancelarChamado(chamadoId, dados.motivo, dados.motivoOutro, dados.observacao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useReabrirChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => reabrirChamado(chamadoId),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useReatribuirChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dados: ReatribuirRequest) => reatribuirChamado(chamadoId, dados),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useAlterarPrioridadeChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (novaPrioridade: PrioridadeChamado) => alterarPrioridade(chamadoId, novaPrioridade),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}

export function useForcarEncerramentoChamado(chamadoId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dados: { motivo: MotivoEncerramento; motivoOutro?: string; observacao?: string }) =>
      forcarEncerramento(chamadoId, dados.motivo, dados.motivoOutro, dados.observacao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
  })
}
