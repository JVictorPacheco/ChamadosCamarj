import { useMutation, useQueryClient } from '@tanstack/react-query'
import {
  atualizarChamado,
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
import { ApiError } from '@/lib/api'
import type { MotivoEncerramento, PrioridadeChamado } from '@/types/api'

function invalidarChamado(queryClient: ReturnType<typeof useQueryClient>, id: string) {
  queryClient.invalidateQueries({ queryKey: ['chamado', id] })
  queryClient.invalidateQueries({ queryKey: ['chamados'] })
  queryClient.invalidateQueries({ queryKey: ['historico', id] })
  queryClient.invalidateQueries({ queryKey: ['comentarios', id] })
  queryClient.invalidateQueries({ queryKey: ['anexos', id] })
}

/**
 * 409 = outra pessoa alterou o chamado depois que a tela o mostrou (spec correcoes-pre-deploy
 * AC-10): recarrega os dados; a mensagem do servidor aparece no bloco de erro de quem chamou.
 */
export function recarregarSeConflito(queryClient: ReturnType<typeof useQueryClient>, id: string, erro: unknown) {
  if (erro instanceof ApiError && erro.status === 409) invalidarChamado(queryClient, id)
}

// `versao` = versão do chamado que a tela mostrou (ChamadoResponse.versao). Vai no If-Match para o
// servidor recusar a ação se o chamado mudou desde então (AC-09).

export function useAtribuirChamado(chamadoId: string, versao?: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => atribuirChamado(chamadoId, versao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
    onError: (erro) => recarregarSeConflito(queryClient, chamadoId, erro),
  })
}

export function useResolverChamado(chamadoId: string, versao?: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => resolverChamado(chamadoId, versao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
    onError: (erro) => recarregarSeConflito(queryClient, chamadoId, erro),
  })
}

export function useFecharChamado(chamadoId: string, versao?: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dados: { motivo: MotivoEncerramento; motivoOutro?: string; observacao?: string }) =>
      fecharChamado(chamadoId, dados.motivo, dados.motivoOutro, dados.observacao, versao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
    onError: (erro) => recarregarSeConflito(queryClient, chamadoId, erro),
  })
}

export function useCancelarChamado(chamadoId: string, versao?: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dados: { motivo: MotivoEncerramento; motivoOutro?: string; observacao?: string }) =>
      cancelarChamado(chamadoId, dados.motivo, dados.motivoOutro, dados.observacao, versao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
    onError: (erro) => recarregarSeConflito(queryClient, chamadoId, erro),
  })
}

export function useReabrirChamado(chamadoId: string, versao?: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => reabrirChamado(chamadoId, versao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
    onError: (erro) => recarregarSeConflito(queryClient, chamadoId, erro),
  })
}

export function useReatribuirChamado(chamadoId: string, versao?: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dados: ReatribuirRequest) => reatribuirChamado(chamadoId, dados, versao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
    onError: (erro) => recarregarSeConflito(queryClient, chamadoId, erro),
  })
}

export function useAlterarPrioridadeChamado(chamadoId: string, versao?: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (novaPrioridade: PrioridadeChamado) => alterarPrioridade(chamadoId, novaPrioridade, versao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
    onError: (erro) => recarregarSeConflito(queryClient, chamadoId, erro),
  })
}

export function useForcarEncerramentoChamado(chamadoId: string, versao?: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dados: { motivo: MotivoEncerramento; motivoOutro?: string; observacao?: string }) =>
      forcarEncerramento(chamadoId, dados.motivo, dados.motivoOutro, dados.observacao, versao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
    onError: (erro) => recarregarSeConflito(queryClient, chamadoId, erro),
  })
}

/** Editar título e descrição (spec editar-chamado). Em 409 recarrega o chamado; o modal decide o resto. */
export function useAtualizarChamado(chamadoId: string, versao?: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (dados: { titulo: string; descricao: string }) => atualizarChamado(chamadoId, dados, versao),
    onSuccess: () => invalidarChamado(queryClient, chamadoId),
    onError: (erro) => recarregarSeConflito(queryClient, chamadoId, erro),
  })
}
