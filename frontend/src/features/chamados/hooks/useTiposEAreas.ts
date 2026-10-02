import { useQuery } from '@tanstack/react-query'
import { listarAreas, listarTipos } from '../api'

export function useTipos() {
  return useQuery({
    queryKey: ['tipos'],
    queryFn: listarTipos,
  })
}

export function useAreas() {
  return useQuery({
    queryKey: ['areas'],
    queryFn: listarAreas,
    select: (areas) => areas.filter((a) => a.ativo),
  })
}
