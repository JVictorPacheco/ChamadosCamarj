import { apiFetch } from '@/lib/api'
import type { TipoPerfil, UsuarioPerfilResponse } from '@/types/api'

export interface AutenticacaoResponse {
  token: string
  id: string
  nome: string
  email: string
  perfil: TipoPerfil
}

export function autenticarGoogle(idToken: string): Promise<AutenticacaoResponse> {
  return apiFetch<AutenticacaoResponse>('/auth/google', {
    method: 'POST',
    body: JSON.stringify({ idToken }),
  })
}

export function listarUsuarios(): Promise<UsuarioPerfilResponse[]> {
  return apiFetch<UsuarioPerfilResponse[]>('/usuarios')
}

export interface CriarUsuarioRequest {
  email: string
  nome: string
  perfil: TipoPerfil
}

export function criarUsuario(dados: CriarUsuarioRequest): Promise<UsuarioPerfilResponse> {
  return apiFetch<UsuarioPerfilResponse>('/usuarios', {
    method: 'POST',
    body: JSON.stringify(dados),
  })
}

export interface AtualizarUsuarioRequest {
  nome: string
  perfil: TipoPerfil
  ativo: boolean
}

export function atualizarUsuario(id: string, dados: AtualizarUsuarioRequest): Promise<void> {
  return apiFetch<void>(`/usuarios/${id}`, {
    method: 'PUT',
    body: JSON.stringify(dados),
  })
}
