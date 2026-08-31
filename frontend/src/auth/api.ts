import { apiFetch } from '@/lib/api'
import type { ChatPerfil, TipoPerfil, UsuarioPerfilResponse } from '@/types/api'

export interface AutenticacaoResponse {
  token: string
  id: string
  nome: string
  email: string
  perfil: TipoPerfil
  chatPerfil?: ChatPerfil
}

export function autenticarGoogle(idToken: string): Promise<AutenticacaoResponse> {
  return apiFetch<AutenticacaoResponse>('/auth/google', {
    method: 'POST',
    body: JSON.stringify({ idToken }),
  })
}

export function login(email: string, senha: string): Promise<AutenticacaoResponse> {
  return apiFetch<AutenticacaoResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, senha }),
  })
}

export function listarUsuarios(): Promise<UsuarioPerfilResponse[]> {
  return apiFetch<UsuarioPerfilResponse[]>('/usuarios')
}

export interface CriarUsuarioRequest {
  email: string
  nome: string
  perfil: TipoPerfil
  senha: string
  grupoId?: string | null
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
  grupoId?: string | null
}

export function atualizarUsuario(id: string, dados: AtualizarUsuarioRequest): Promise<void> {
  return apiFetch<void>(`/usuarios/${id}`, {
    method: 'PUT',
    body: JSON.stringify(dados),
  })
}

export function redefinirSenha(id: string, novaSenha: string): Promise<void> {
  return apiFetch<void>(`/usuarios/${id}/senha`, {
    method: 'PATCH',
    body: JSON.stringify({ novaSenha }),
  })
}

export function esqueciSenha(email: string): Promise<{ mensagem: string }> {
  return apiFetch<{ mensagem: string }>('/auth/esqueci-senha', {
    method: 'POST',
    body: JSON.stringify({ email }),
  })
}

export function resetarSenha(token: string, novaSenha: string): Promise<{ mensagem: string }> {
  return apiFetch<{ mensagem: string }>('/auth/resetar-senha', {
    method: 'POST',
    body: JSON.stringify({ token, novaSenha }),
  })
}
