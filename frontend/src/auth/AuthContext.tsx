import { createContext, useContext, useState, type ReactNode } from 'react'

export type TipoPerfil = 'Admin' | 'Atendente' | 'Solicitante'

export interface Perfil {
  tipo: TipoPerfil
  id: string
  nome: string
  email: string
}

const PERFIS: Record<TipoPerfil, Perfil> = {
  Admin:       { tipo: 'Admin',       id: 'a1000000-0000-0000-0000-000000000001', nome: 'Victor',          email: 'victor@camarj.com.br' },
  Atendente:   { tipo: 'Atendente',   id: 'a2000000-0000-0000-0000-000000000002', nome: 'Fábio',           email: 'fabio@camarj.com.br' },
  Solicitante: { tipo: 'Solicitante', id: 'a3000000-0000-0000-0000-000000000003', nome: 'Ana Colaboradora', email: 'ana.colaboradora@camarj.com.br' },
}

const STORAGE_KEY = 'chamados-camarj:perfil'

interface AuthContextValue {
  perfil: Perfil | null
  login: (tipo: TipoPerfil) => void
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function lerPerfilSalvo(): Perfil | null {
  const tipo = localStorage.getItem(STORAGE_KEY) as TipoPerfil | null
  return tipo && tipo in PERFIS ? PERFIS[tipo] : null
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [perfil, setPerfil] = useState<Perfil | null>(() => lerPerfilSalvo())

  const login = (tipo: TipoPerfil) => {
    localStorage.setItem(STORAGE_KEY, tipo)
    setPerfil(PERFIS[tipo])
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
