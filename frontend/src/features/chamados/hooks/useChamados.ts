import { useQuery } from '@tanstack/react-query'
import { listarChamados, type ListarChamadosFiltros } from '../api'

export function useChamados(filtros: ListarChamadosFiltros = {}) {
  return useQuery({
    queryKey: ['chamados', filtros],
    queryFn: () => listarChamados(filtros),
  })
}
