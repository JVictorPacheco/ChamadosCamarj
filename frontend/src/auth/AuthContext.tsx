import { createContext, useContext, useState, type ReactNode } from 'react'
import { obterUsuarioPorEmail } from './api'
import type { TipoPerfil, UsuarioPerfilResponse } from '@/types/api'

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
  login: (email: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function paraPerfil(usuario: UsuarioPerfilResponse): Perfil {
  return { tipo: usuario.perfil, id: usuario.id, nome: usuario.nome, email: usuario.email }
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

  const login = async (email: string) => {
    const usuario = await obterUsuarioPorEmail(email)
    const perfilLogado = paraPerfil(usuario)
    localStorage.setItem(STORAGE_KEY, JSON.stringify(perfilLogado))
    setPerfil(perfilLogado)
  }

  const logout = () => {
    localStorage.removeItem(STORAGE_KEY)
    setPerfil(null)
  }

  return <AuthContext.Provider value={{ perfil, login, logout }}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider')
  }
  return context
}
