import { apiFetch } from '@/lib/api'
import type {
  AcessoUsuarioDetalheResponse,
  AcessoUsuarioResumoResponse,
  ChatPerfil,
  GrupoResponse,
  ModuloSistema,
  TipoChamadoResponse,
} from '@/types/api'

export function listarGrupos(): Promise<GrupoResponse[]> {
  return apiFetch<GrupoResponse[]>('/grupos')
}

export function obterGrupo(id: string): Promise<GrupoResponse> {
  return apiFetch<GrupoResponse>(`/grupos/${id}`)
}

export function criarGrupo(dados: { nome: string; descricao: string }): Promise<GrupoResponse> {
  return apiFetch<GrupoResponse>('/grupos', { method: 'POST', body: JSON.stringify(dados) })
}

export function atualizarGrupo(id: string, dados: { nome: string; descricao: string }): Promise<GrupoResponse> {
  return apiFetch<GrupoResponse>(`/grupos/${id}`, { method: 'PUT', body: JSON.stringify(dados) })
}

export function listarTiposAdmin(): Promise<TipoChamadoResponse[]> {
  return apiFetch<TipoChamadoResponse[]>('/tipos?apenasAtivos=false')
}

export function criarTipo(dados: { nome: string; descricao: string }): Promise<TipoChamadoResponse> {
  return apiFetch<TipoChamadoResponse>('/tipos', { method: 'POST', body: JSON.stringify(dados) })
}

export function atualizarTipo(id: string, dados: { nome: string; descricao: string; ativo: boolean }): Promise<TipoChamadoResponse> {
  return apiFetch<TipoChamadoResponse>(`/tipos/${id}`, { method: 'PUT', body: JSON.stringify(dados) })
}

export function excluirTipo(id: string): Promise<void> {
  return apiFetch<void>(`/tipos/${id}`, { method: 'DELETE' })
}

// ── Controle de acesso (spec controle-de-acesso) ─────────────────────────────

export function listarAcessos(): Promise<AcessoUsuarioResumoResponse[]> {
  return apiFetch<AcessoUsuarioResumoResponse[]>('/acessos')
}

export function obterAcessoUsuario(usuarioId: string): Promise<AcessoUsuarioDetalheResponse> {
  return apiFetch<AcessoUsuarioDetalheResponse>(`/acessos/${usuarioId}`)
}

export function salvarAcessos(usuarioId: string, dados: { modulos: ModuloSistema[]; chatPerfil: ChatPerfil }): Promise<void> {
  return apiFetch<void>(`/acessos/${usuarioId}`, { method: 'PUT', body: JSON.stringify(dados) })
}

export function voltarAoPadrao(usuarioId: string): Promise<void> {
  return apiFetch<void>(`/acessos/${usuarioId}/padrao`, { method: 'POST' })
}

/** Só o Chat — vale também para Admins (spec controle-de-acesso, review R-02). */
export function definirChatDeAcesso(usuarioId: string, chatPerfil: ChatPerfil): Promise<void> {
  return apiFetch<void>(`/acessos/${usuarioId}/chat`, { method: 'PUT', body: JSON.stringify({ chatPerfil }) })
}
