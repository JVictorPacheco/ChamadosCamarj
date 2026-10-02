import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { atualizarPreferenciaLeitura as atualizarPreferenciaLeituraApi, autenticarGoogle, login, obterPerfilAtual, type AutenticacaoResponse } from './api'
import { clearToken, getToken, registrarLogoutAutomatico, setToken } from '@/lib/api'
import type { ChatPerfil, TipoPerfil, UsuarioPerfilResponse } from '@/types/api'

export type { TipoPerfil }

export interface Perfil {
  tipo: TipoPerfil
  id: string
  nome: string
  email: string
  chatPerfil?: ChatPerfil
  mostrarConfirmacaoLeitura: boolean
  /** Grupo = área de quem está logado; preenche a Área na abertura (spec area-e-tipo AC-02). */
  grupoId?: string | null
}

const STORAGE_KEY = 'chamados-camarj:perfil'

interface AuthContextValue {
  perfil: Perfil | null
  loginComGoogle: (idToken: string) => Promise<void>
  loginComSenha: (email: string, senha: string) => Promise<void>
  logout: () => void
  atualizarChatPerfil: (novo: ChatPerfil) => void
  atualizarPreferenciaLeitura: (mostrar: boolean) => Promise<void>
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function paraPerfil(resposta: AutenticacaoResponse): Perfil {
  return {
    tipo: resposta.perfil,
    id: resposta.id,
    nome: resposta.nome,
    email: resposta.email,
    chatPerfil: resposta.chatPerfil,
    mostrarConfirmacaoLeitura: resposta.mostrarConfirmacaoLeitura ?? true,
    grupoId: resposta.grupoId ?? null,
  }
}

function paraPerfilAtual(resposta: UsuarioPerfilResponse): Perfil {
  return {
    tipo: resposta.perfil,
    id: resposta.id,
    nome: resposta.nome,
    email: resposta.email,
    chatPerfil: resposta.chatPerfil,
    mostrarConfirmacaoLeitura: resposta.mostrarConfirmacaoLeitura ?? true,
    grupoId: resposta.grupoId ?? null,
  }
}

function lerPerfilSalvo(): Perfil | null {
  const salvo = localStorage.getItem(STORAGE_KEY)
  if (!salvo) return null

  try {
    const perfil = JSON.parse(salvo) as Perfil
    // Perfil salvo antes desta extensão (2026-09-04) não tem o campo — sem isso, ficaria
    // `undefined` em memória mesmo com o tipo dizendo `boolean`.
    return { ...perfil, mostrarConfirmacaoLeitura: perfil.mostrarConfirmacaoLeitura ?? true }
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

  // AC-48 / review-fase9-independente.md #10: perfil vem só de localStorage no boot, então uma
  // mudança de ChatPerfil enquanto a pessoa estava deslogada (ou com a aba fechada, sem conexão
  // SignalR pra receber o evento em tempo real) nunca era refletida até um novo login. Revalida uma
  // vez no boot direto do banco. Falha de rede aqui não desloga ninguém — mantém o snapshot salvo;
  // um 401 de verdade (conta excluída/desativada) já é tratado por registrarLogoutAutomatico acima.
  useEffect(() => {
    if (!getToken()) return
    obterPerfilAtual()
      .then((resposta) => {
        const atualizado = paraPerfilAtual(resposta)
        localStorage.setItem(STORAGE_KEY, JSON.stringify(atualizado))
        setPerfil(atualizado)
      })
      .catch(() => {
        // best-effort — ver comentário acima
      })
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

  // AC-48: reflete uma mudança de ChatPerfil vinda em tempo real (ChatPerfilAtualizado, via
  // ChamadosHub) sem precisar de logout/login — sem isso, o link "Chat" só apareceria/sumiria
  // da barra lateral depois de gerar um token novo.
  const atualizarChatPerfil = (novo: ChatPerfil) => {
    setPerfil((atual) => {
      if (!atual) return atual
      const atualizado = { ...atual, chatPerfil: novo }
      localStorage.setItem(STORAGE_KEY, JSON.stringify(atualizado))
      return atualizado
    })
  }

  // AC-56/AC-58: aplica no backend (persiste no perfil) e só então reflete localmente — evita a UI
  // otimista ficar dessincronizada se a requisição falhar (mesmo padrão de erro inline do projeto,
  // sem toast — quem chama trata o reject).
  const atualizarPreferenciaLeitura = async (mostrar: boolean) => {
    await atualizarPreferenciaLeituraApi(mostrar)
    setPerfil((atual) => {
      if (!atual) return atual
      const atualizado = { ...atual, mostrarConfirmacaoLeitura: mostrar }
      localStorage.setItem(STORAGE_KEY, JSON.stringify(atualizado))
      return atualizado
    })
  }

  const value = useMemo(
    () => ({ perfil, loginComGoogle, loginComSenha, logout, atualizarChatPerfil, atualizarPreferenciaLeitura }),
    [perfil],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider')
  }
  return context
}
