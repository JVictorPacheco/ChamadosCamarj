import { apiFetch } from '@/lib/api'
import type {
  AbrirChamadoRequest,
  AnexoResponse,
  GrupoResponse,
  TipoChamadoResponse,
  ChamadoResponse,
  ComentarChamadoRequest,
  ComentarioResponse,
  HistoricoResponse,
  MotivoEncerramento,
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
  areaId?: string
  tipoId?: string
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
    if (valor != null) {
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

/** Tipos ativos — os que podem ser escolhidos na abertura. */
export function listarTipos(): Promise<TipoChamadoResponse[]> {
  return apiFetch<TipoChamadoResponse[]>('/tipos')
}

/** Áreas = grupos (spec area-e-tipo-do-chamado). */
export function listarAreas(): Promise<GrupoResponse[]> {
  return apiFetch<GrupoResponse[]>('/grupos')
}

/** Edita título e descrição (spec editar-chamado). Quem pode é decidido no servidor. */
export function atualizarChamado(chamadoId: string, dados: { titulo: string; descricao: string }, versao?: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}`, {
    method: 'PUT',
    headers: cabecalhoVersao(versao),
    body: JSON.stringify(dados),
  })
}

export function reclassificarTipo(chamadoId: string, novoTipoId: string, versao?: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/tipo`, {
    method: 'PATCH',
    headers: cabecalhoVersao(versao),
    body: JSON.stringify({ novoTipoId }),
  })
}

/**
 * Versão do chamado que a tela mostrou, enviada no If-Match: se outra pessoa alterou o chamado
 * depois disso, o servidor recusa com 409 (spec correcoes-pre-deploy AC-09). Sem versão, sem checagem.
 */
function cabecalhoVersao(versao?: string): Record<string, string> | undefined {
  return versao ? { 'If-Match': `"${versao}"` } : undefined
}

// Quem fez a ação (usuarioId/usuarioNome) vem do token no backend agora — nenhuma
// das funções abaixo precisa mais receber/mandar essa informação pelo cliente.

export function alterarStatus(chamadoId: string, novoStatus: StatusChamado, versao?: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/status`, {
    method: 'PUT',
    headers: cabecalhoVersao(versao),
    body: JSON.stringify({ novoStatus }),
  })
}

export function atribuirChamado(chamadoId: string, versao?: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/atribuir`, {
    method: 'PATCH',
    headers: cabecalhoVersao(versao),
  })
}

export function resolverChamado(chamadoId: string, versao?: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/resolver`, { method: 'PATCH', headers: cabecalhoVersao(versao) })
}

export function fecharChamado(chamadoId: string, motivo: MotivoEncerramento, motivoOutro?: string, observacao?: string, versao?: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/fechar`, {
    method: 'PATCH',
    headers: cabecalhoVersao(versao),
    body: JSON.stringify({ motivo, motivoOutro, observacao }),
  })
}

export function cancelarChamado(chamadoId: string, motivo: MotivoEncerramento, motivoOutro?: string, observacao?: string, versao?: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/cancelar`, {
    method: 'PATCH',
    headers: cabecalhoVersao(versao),
    body: JSON.stringify({ motivo, motivoOutro, observacao }),
  })
}

export function reabrirChamado(chamadoId: string, versao?: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/reabrir`, { method: 'PATCH', headers: cabecalhoVersao(versao) })
}

export interface ReatribuirRequest {
  novoResponsavelId: string
  novoResponsavelNome: string
}

export function reatribuirChamado(chamadoId: string, dados: ReatribuirRequest, versao?: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/reatribuir`, {
    method: 'PATCH',
    headers: cabecalhoVersao(versao),
    body: JSON.stringify(dados),
  })
}

export function alterarPrioridade(chamadoId: string, novaPrioridade: PrioridadeChamado, versao?: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/prioridade`, {
    method: 'PATCH',
    headers: cabecalhoVersao(versao),
    body: JSON.stringify({ novaPrioridade }),
  })
}

export function forcarEncerramento(chamadoId: string, motivo: MotivoEncerramento, motivoOutro?: string, observacao?: string, versao?: string): Promise<void> {
  return apiFetch<void>(`/chamados/${chamadoId}/forcar-encerramento`, {
    method: 'PATCH',
    headers: cabecalhoVersao(versao),
    body: JSON.stringify({ motivo, motivoOutro, observacao }),
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

export interface TriagemSugestao {
  areaId: string | null
  areaNome: string | null
  tipoId: string | null
  tipoNome: string | null
  confianca: number
  temSugestao: boolean
}

export function sugerirTriagem(titulo: string, descricao: string): Promise<TriagemSugestao> {
  return apiFetch<TriagemSugestao>('/chamados/sugerir-triagem', {
    method: 'POST',
    body: JSON.stringify({ titulo, descricao }),
  })
}
