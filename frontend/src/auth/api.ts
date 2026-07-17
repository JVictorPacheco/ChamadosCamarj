import { apiFetch } from '@/lib/api'
import type { TipoPerfil, UsuarioPerfilResponse } from '@/types/api'

export function obterUsuarioPorEmail(email: string): Promise<UsuarioPerfilResponse> {
  return apiFetch<UsuarioPerfilResponse>(`/usuarios/por-email?email=${encodeURIComponent(email)}`)
}

export function listarUsuarios(perfilRequisitante: TipoPerfil): Promise<UsuarioPerfilResponse[]> {
  return apiFetch<UsuarioPerfilResponse[]>(
    `/usuarios?perfilRequisitante=${encodeURIComponent(perfilRequisitante)}`,
  )
}

export interface CriarUsuarioRequest {
  email: string
  nome: string
  perfil: TipoPerfil
}

export function criarUsuario(
  dados: CriarUsuarioRequest,
  perfilRequisitante: TipoPerfil,
): Promise<UsuarioPerfilResponse> {
  return apiFetch<UsuarioPerfilResponse>(
    `/usuarios?perfilRequisitante=${encodeURIComponent(perfilRequisitante)}`,
    {
      method: 'POST',
      body: JSON.stringify(dados),
    },
  )
}

export interface AtualizarUsuarioRequest {
  nome: string
  perfil: TipoPerfil
  ativo: boolean
}

export function atualizarUsuario(
  id: string,
  dados: AtualizarUsuarioRequest,
  perfilRequisitante: TipoPerfil,
): Promise<void> {
  return apiFetch<void>(`/usuarios/${id}?perfilRequisitante=${encodeURIComponent(perfilRequisitante)}`, {
    method: 'PUT',
    body: JSON.stringify(dados),
  })
}
