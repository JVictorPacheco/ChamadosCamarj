import { useQuery } from '@tanstack/react-query'
import { obterRelatorioMensal } from '../api'

export function useRelatorioMensal(ano: number, mes: number, responsavelId?: string, enabled = true) {
  return useQuery({
    queryKey: ['relatorio-mensal', ano, mes, responsavelId],
    queryFn: () => obterRelatorioMensal(ano, mes, responsavelId),
    staleTime: 30_000,
    enabled,
  })
}
