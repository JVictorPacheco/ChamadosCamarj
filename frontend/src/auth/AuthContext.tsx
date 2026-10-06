import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { atualizarPreferenciaLeitura as atualizarPreferenciaLeituraApi, autenticarGoogle, login, obterPerfilAtual, type AutenticacaoResponse } from './api'
import { clearToken, getToken, perfilDoToken, registrarLogoutAutomatico, setToken } from '@/lib/api'
import { marcarLogoutPorPerfilAlterado } from './logoutPerfilAlterado'
import type { ChatPerfil, ModuloSistema, TipoPerfil, UsuarioPerfilResponse } from '@/types/api'
import { modulosPadrao } from '@/lib/modulos'

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
  /** Módulos que a pessoa usa (spec controle-de-acesso) — monta o menu. */
  modulos: ModuloSistema[]
}

const STORAGE_KEY = 'chamados-camarj:perfil'

interface AuthContextValue {
  perfil: Perfil | null
  loginComGoogle: (idToken: string) => Promise<void>
  loginComSenha: (email: string, senha: string) => Promise<void>
  logout: () => void
  atualizarChatPerfil: (novo: ChatPerfil) => void
  atualizarAcessos: (modulos: ModuloSistema[], chatPerfil: ChatPerfil) => void
  atualizarPreferenciaLeitura: (mostrar: boolean) => Promise<void>
  /** Relê o cadastro e confere o perfil com o do token (boot e reconexão do tempo real). */
  revalidarSessao: () => void
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
    modulos: resposta.modulos ?? modulosPadrao(resposta.perfil),
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
    modulos: resposta.modulos ?? modulosPadrao(resposta.perfil),
  }
}

function lerPerfilSalvo(): Perfil | null {
  const salvo = localStorage.getItem(STORAGE_KEY)
  if (!salvo) return null

  try {
    const perfil = JSON.parse(salvo) as Perfil
    // Perfil salvo antes desta extensão (2026-09-04) não tem o campo — sem isso, ficaria
    // `undefined` em memória mesmo com o tipo dizendo `boolean`.
    // Idem para os módulos (spec controle-de-acesso): até o /auth/me do boot, vale o padrão do perfil.
    return {
      ...perfil,
      mostrarConfirmacaoLeitura: perfil.mostrarConfirmacaoLeitura ?? true,
      modulos: perfil.modulos ?? modulosPadrao(perfil.tipo),
    }
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
  // Também chamada quando o tempo real volta depois de uma queda (review-4 R-01): o aviso de perfil mudado
  // pode ter sido perdido enquanto a conexão estava fora.
  const revalidarSessao = () => {
    const tokenDoBoot = getToken()
    if (!tokenDoBoot) return
    obterPerfilAtual()
      .then((resposta) => {
        // Resposta de uma sessão que já acabou (logout, ou a 2ª chamada do StrictMode depois que a 1ª
        // desconectou): não pode ressuscitar a sessão nem apagar o aviso da tela de login.
        if (getToken() !== tokenDoBoot) return
        // spec controle-de-acesso AC-15 (review-3 R-02): o perfil mudou enquanto a pessoa estava fora.
        // O token ainda tem o perfil antigo — sai e pede login novo, igual a quem estava com a tela aberta.
        const perfilLogado = perfilDoToken()
        if (perfilLogado && perfilLogado !== resposta.perfil) {
          marcarLogoutPorPerfilAlterado()
          logout()
          return
        }
        const atualizado = paraPerfilAtual(resposta)
        localStorage.setItem(STORAGE_KEY, JSON.stringify(atualizado))
        setPerfil(atualizado)
      })
      .catch(() => {
        // best-effort — ver comentário acima
      })
  }
  useEffect(revalidarSessao, [])
  // review-4 R-01: a internet voltou (Wi-Fi caiu, notebook acordou) — eventos de tempo real podem ter se
  // perdido nesse meio-tempo, mesmo sem a conexão do SignalR ter percebido a queda.
  useEffect(() => {
    window.addEventListener('online', revalidarSessao)
    return () => window.removeEventListener('online', revalidarSessao)
  })

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

  // spec controle-de-acesso AC-11/AC-13: o Admin mudou os acessos desta pessoa (AcessosAtualizados).
  const atualizarAcessos = (modulos: ModuloSistema[], chatPerfil: ChatPerfil) => {
    setPerfil((atual) => {
      if (!atual) return atual
      const atualizado = { ...atual, modulos, chatPerfil }
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
    () => ({ perfil, loginComGoogle, loginComSenha, logout, atualizarChatPerfil, atualizarAcessos, atualizarPreferenciaLeitura, revalidarSessao }),
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
