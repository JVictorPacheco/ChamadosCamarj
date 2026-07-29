import { apiFetch } from '@/lib/api'
import type { CategoriaResponse, GrupoResponse } from '@/types/api'

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

export function listarCategoriasAdmin(): Promise<CategoriaResponse[]> {
  return apiFetch<CategoriaResponse[]>('/categorias?apenasAtivas=false')
}

export function criarCategoria(dados: { nome: string; descricao: string }): Promise<CategoriaResponse> {
  return apiFetch<CategoriaResponse>('/categorias', { method: 'POST', body: JSON.stringify(dados) })
}

export function atualizarCategoria(id: string, dados: { nome: string; descricao: string; ativa: boolean }): Promise<CategoriaResponse> {
  return apiFetch<CategoriaResponse>(`/categorias/${id}`, { method: 'PUT', body: JSON.stringify(dados) })
}
