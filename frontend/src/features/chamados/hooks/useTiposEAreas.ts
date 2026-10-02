import { useQuery } from '@tanstack/react-query'
import { listarAreas, listarTipos } from '../api'

export function useTipos() {
  return useQuery({
    // Chave própria: o Admin usa ['tipos'] com a lista completa (inclui inativos) — review R-03.
    // O Admin invalida ['tipos'], que também invalida esta (prefixo).
    queryKey: ['tipos', 'ativos'],
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
