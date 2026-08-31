import { useMutation, useQueryClient } from '@tanstack/react-query'
import {
  enviarMensagem,
  enviarArquivo,
  editarMensagem,
  deletarMensagem,
  adicionarReacao,
  marcarComoLido,
  criarConversa,
  criarGrupo,
} from '../api'

function invalidarConversa(queryClient: ReturnType<typeof useQueryClient>, conversaId: string) {
  queryClient.invalidateQueries({ queryKey: ['chat', 'mensagens', conversaId] })
  queryClient.invalidateQueries({ queryKey: ['chat', 'conversas'] })
}

export function useEnviarMensagem(conversaId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ conteudo, respostaParaMensagemId }: { conteudo: string; respostaParaMensagemId?: string }) =>
      enviarMensagem(conversaId, conteudo, respostaParaMensagemId),
    onSuccess: () => invalidarConversa(queryClient, conversaId),
  })
}

export function useEnviarArquivo(conversaId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (arquivo: File) => enviarArquivo(conversaId, arquivo),
    onSuccess: () => invalidarConversa(queryClient, conversaId),
  })
}

export function useEditarMensagem(conversaId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ mensagemId, conteudo }: { mensagemId: string; conteudo: string }) =>
      editarMensagem(mensagemId, conteudo),
    onSuccess: () => invalidarConversa(queryClient, conversaId),
  })
}

export function useDeletarMensagem(conversaId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (mensagemId: string) => deletarMensagem(mensagemId),
    onSuccess: () => invalidarConversa(queryClient, conversaId),
  })
}

export function useAdicionarReacao(conversaId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ mensagemId, emoji }: { mensagemId: string; emoji: string }) =>
      adicionarReacao(mensagemId, emoji),
    onSuccess: () => invalidarConversa(queryClient, conversaId),
  })
}

export function useMarcarComoLido() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (conversaId: string) => marcarComoLido(conversaId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversas'] })
    },
  })
}

export function useCriarConversa() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (destinatarioId: string) => criarConversa(destinatarioId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversas'] })
    },
  })
}

export function useCriarGrupo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ nome, participanteIds }: { nome: string; participanteIds: string[] }) =>
      criarGrupo(nome, participanteIds),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversas'] })
    },
  })
}
