import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { autenticarGoogle, login, type AutenticacaoResponse } from './api'
import { clearToken, registrarLogoutAutomatico, setToken } from '@/lib/api'
import type { ChatPerfil, TipoPerfil } from '@/types/api'

export type { TipoPerfil }

export interface Perfil {
  tipo: TipoPerfil
  id: string
  nome: string
  email: string
  chatPerfil?: ChatPerfil
}

const STORAGE_KEY = 'chamados-camarj:perfil'

interface AuthContextValue {
  perfil: Perfil | null
  loginComGoogle: (idToken: string) => Promise<void>
  loginComSenha: (email: string, senha: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function paraPerfil(resposta: AutenticacaoResponse): Perfil {
  return { tipo: resposta.perfil, id: resposta.id, nome: resposta.nome, email: resposta.email, chatPerfil: resposta.chatPerfil }
}

function lerPerfilSalvo(): Perfil | null {
  const salvo = localStorage.getItem(STORAGE_KEY)
  if (!salvo) return null

  try {
    return JSON.parse(salvo) as Perfil
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [perfil, setPerfil] = useState<Perfil | null>(() => lerPerfilSalvo())

  const logout = () => {
    clearToken()
    localStorage.removeItem(STORAGE_KEY)
    setPerfil(null)
  }

  // Se a API responder 401 (token expirado/inválido) em qualquer requisição,
  // desloga automaticamente em vez de deixar a pessoa vendo erros genéricos.
  useEffect(() => {
    registrarLogoutAutomatico(logout)
  }, [])

  const loginComGoogle = async (idToken: string) => {
    const resposta = await autenticarGoogle(idToken)
    setToken(resposta.token)
    const perfilLogado = paraPerfil(resposta)
    localStorage.setItem(STORAGE_KEY, JSON.stringify(perfilLogado))
    setPerfil(perfilLogado)
  }

  const loginComSenha = async (email: string, senha: string) => {
    const resposta = await login(email, senha)
    setToken(resposta.token)
    const perfilLogado = paraPerfil(resposta)
    localStorage.setItem(STORAGE_KEY, JSON.stringify(perfilLogado))
    setPerfil(perfilLogado)
  }

  const value = useMemo(() => ({ perfil, loginComGoogle, loginComSenha, logout }), [perfil])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider')
  }
  return context
}
