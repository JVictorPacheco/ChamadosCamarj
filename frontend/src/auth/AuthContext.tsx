import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { autenticarGoogle, type AutenticacaoResponse } from './api'
import { clearToken, registrarLogoutAutomatico, setToken } from '@/lib/api'
import type { TipoPerfil } from '@/types/api'

export type { TipoPerfil }

export interface Perfil {
  tipo: TipoPerfil
  id: string
  nome: string
  email: string
}

const STORAGE_KEY = 'chamados-camarj:perfil'

interface AuthContextValue {
  perfil: Perfil | null
  loginComGoogle: (idToken: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function paraPerfil(resposta: AutenticacaoResponse): Perfil {
  return { tipo: resposta.perfil, id: resposta.id, nome: resposta.nome, email: resposta.email }
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

  return <AuthContext.Provider value={{ perfil, loginComGoogle, logout }}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider')
  }
  return context
}
