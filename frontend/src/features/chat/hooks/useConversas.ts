import { useQuery } from '@tanstack/react-query'
import { listarConversas, obterConversa } from '../api'

export function useConversas(enabled = true) {
  return useQuery({
    queryKey: ['chat', 'conversas'],
    queryFn: () => listarConversas(),
    enabled,
  })
}

export function useConversaDetalhe(conversaId: string | null) {
  return useQuery({
    queryKey: ['chat', 'conversa-detalhe', conversaId],
    queryFn: () => obterConversa(conversaId!),
    enabled: !!conversaId,
  })
}
