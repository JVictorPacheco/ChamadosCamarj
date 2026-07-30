import { apiFetch } from '@/lib/api'
import type {
  AbrirChamadoRequest,
  AnexoResponse,
  CategoriaResponse,
  ChamadoResponse,
  ComentarChamadoRequest,
  ComentarioResponse,
  HistoricoResponse,
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
  finalizados?: boolean
  dataInicio?: string
  dataFim?: string
  slaStatus?: string
  motivoEncerramento?: string
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

// perfilUsuario (pra filtrar comentário interno) agora vem do token no backend, não
// precisa mais ser mandado pelo cliente.
export function listarComentarios(chamadoId: string): Promise<ComentarioResponse[]> {
  return apiFetch<ComentarioResponse[]>(`/chamados/${chamadoId}/comentarios`)
}

export function comentar(chamadoId: string, dados: ComentarChamadoRequest): Promise<ComentarioResponse> {
  return apiFetch<ComentarioResponse>(`/chamados/${chamadoId}/comentarios`, {
    method: 'POST',
    body: JSON.stringify(dados),
  })
}

export function listarCategorias(): Promise<CategoriaResponse[]> {
  return apiFetch<CategoriaResponse[]>('/categorias')
}

// Quem fez a ação (usuarioId/usuarioNome) vem do token no backend agora — nenhuma
// das funções abaixo precisa mais receber/mandar essa informação pelo cliente.

export function alterarStatus(chamadoId: string, novoStatus: StatusChamado): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/status`, {
    method: 'PUT',
    body: JSON.stringify({ novoStatus }),
  })
}

export function atribuirChamado(chamadoId: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/atribuir`, {
    method: 'PATCH',
  })
}

export function resolverChamado(chamadoId: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/resolver`, { method: 'PATCH' })
}

export function fecharChamado(chamadoId: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/fechar`, { method: 'PATCH' })
}

export function cancelarChamado(chamadoId: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/cancelar`, { method: 'PATCH' })
}

export function reabrirChamado(chamadoId: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/reabrir`, { method: 'PATCH' })
}

export interface ReatribuirRequest {
  novoResponsavelId: string
  novoResponsavelNome: string
}

export function reatribuirChamado(chamadoId: string, dados: ReatribuirRequest): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/reatribuir`, {
    method: 'PATCH',
    body: JSON.stringify(dados),
  })
}

export function alterarPrioridade(chamadoId: string, novaPrioridade: PrioridadeChamado): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/prioridade`, {
    method: 'PATCH',
    body: JSON.stringify({ novaPrioridade }),
  })
}

export function forcarEncerramento(chamadoId: string, motivo: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/forcar-encerramento`, {
    method: 'PATCH',
    body: JSON.stringify({ motivo }),
  })
}

export function uploadAnexo(chamadoId: string, arquivo: File, comentarioId?: string): Promise<AnexoResponse> {
  const formData = new FormData()
  formData.append('arquivo', arquivo)
  if (comentarioId) {
    formData.append('comentarioId', comentarioId)
  }

  return apiFetch<AnexoResponse>(`/chamados/${chamadoId}/anexos`, {
    method: 'POST',
    body: formData,
  })
}

export function listarAnexos(chamadoId: string): Promise<AnexoResponse[]> {
  return apiFetch<AnexoResponse[]>(`/chamados/${chamadoId}/anexos`)
}

export function obterUrlDownloadAnexo(chamadoId: string, anexoId: string): Promise<{ url: string }> {
  return apiFetch<{ url: string }>(`/chamados/${chamadoId}/anexos/${anexoId}/download-url`)
}

export function removerAnexo(chamadoId: string, anexoId: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/anexos/${anexoId}`, { method: 'DELETE' })
}

export function listarHistorico(chamadoId: string): Promise<HistoricoResponse[]> {
  return apiFetch<HistoricoResponse[]>(`/chamados/${chamadoId}/historico`)
}
