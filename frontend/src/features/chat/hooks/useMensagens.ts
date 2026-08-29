import { useInfiniteQuery } from '@tanstack/react-query'
import { listarMensagens } from '../api'

export function useMensagens(conversaId: string | null) {
  return useInfiniteQuery({
    queryKey: ['chat', 'mensagens', conversaId],
    queryFn: ({ pageParam = 1 }) => listarMensagens(conversaId!, pageParam as number),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => {
      if (lastPage.temProxima) {
        return lastPage.pagina + 1
      }
      return undefined
    },
    enabled: !!conversaId,
  })
}
