import { apiFetch } from '@/lib/api'
import type {
  AbrirChamadoRequest,
  CategoriaResponse,
  ChamadoResponse,
  ComentarChamadoRequest,
  ComentarioResponse,
  PagedResult,
  PrioridadeChamado,
  StatusChamado,
} from '@/types/api'

export interface ListarChamadosFiltros {
  pagina?: number
  tamanhoPagina?: number
  status?: StatusChamado
  prioridade?: PrioridadeChamado
  responsavelId?: string
  categoriaId?: string
  busca?: string
  solicitanteEmail?: string
}

function buildQueryString<T extends object>(filtros: T): string {
  const params = new URLSearchParams()
  for (const [chave, valor] of Object.entries(filtros)) {
    if (valor !== undefined) {
      params.set(chave, String(valor))
    }
  }
  const query = params.toString()
  return query ? `?${query}` : ''
}

export function listarChamados(filtros: ListarChamadosFiltros = {}): Promise<PagedResult<ChamadoResponse>> {
  return apiFetch<PagedResult<ChamadoResponse>>(`/chamados${buildQueryString(filtros)}`)
}

export function obterChamado(id: string): Promise<ChamadoResponse> {
  return apiFetch<ChamadoResponse>(`/chamados/${id}`)
}

export function abrirChamado(dados: AbrirChamadoRequest): Promise<ChamadoResponse> {
  return apiFetch<ChamadoResponse>('/chamados', {
    method: 'POST',
    body: JSON.stringify(dados),
  })
}

export function listarComentarios(chamadoId: string): Promise<ComentarioResponse[]> {
  return apiFetch<ComentarioResponse[]>(`/chamados/${chamadoId}/comentarios`)
}

export function comentar(chamadoId: string, dados: ComentarChamadoRequest): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/comentarios`, {
    method: 'POST',
    body: JSON.stringify(dados),
  })
}

export function listarCategorias(): Promise<CategoriaResponse[]> {
  return apiFetch<CategoriaResponse[]>('/categorias')
}

export interface AlterarStatusRequest {
  novoStatus: StatusChamado
}

export function alterarStatus(chamadoId: string, novoStatus: StatusChamado): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/status`, {
    method: 'PUT',
    body: JSON.stringify({ novoStatus }),
  })
}
