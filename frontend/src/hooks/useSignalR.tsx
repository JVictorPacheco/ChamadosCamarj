import { createContext, useContext, useEffect, useMemo, useState, useCallback, useRef, type ReactNode } from 'react'
import {
  HubConnectionBuilder,
  type HubConnection,
} from '@microsoft/signalr'
import type { SignalREvent } from '@/lib/signalr-events'
import { getToken } from '@/lib/api'

const SIGNALR_URL = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') ?? 'http://localhost:5000'

// review-fase9-independente.md #5: withAutomaticReconnect só cobre queda de uma conexão que chegou
// a se estabelecer — um start() que falha (API reiniciando, blip de rede no login) nunca era
// retentado, deixando a pessoa a sessão inteira sem ChatPerfilAtualizado/ChatConversaAtualizada em
// silêncio. Mesmo backoff manual já usado em useChatSignalR.ts (Bug #3), por consistência.
const ATRASOS_RETRY_MS = [1000, 2000, 5000, 10000, 15000, 30000]

interface SignalRContextValue {
  connection: HubConnection | null
  isConnected: boolean
  lastEvent: SignalREvent | null
  subscribe: (handler: (event: SignalREvent) => void) => () => void
}

const SignalRContext = createContext<SignalRContextValue | null>(null)

export function SignalRProvider({ children }: { children: ReactNode }) {
  const [connection, setConnection] = useState<HubConnection | null>(null)
  const [isConnected, setIsConnected] = useState(false)
  const [lastEvent, setLastEvent] = useState<SignalREvent | null>(null)
  // useRef em vez de useState: dá identidade estável pro Set, sem forçar recriação
  // de `notify` (e portanto do efeito de conexão abaixo) a cada subscribe/unsubscribe.
  const subscribersRef = useRef<Set<(event: SignalREvent) => void>>(new Set())

  const subscribe = useCallback((handler: (event: SignalREvent) => void) => {
    subscribersRef.current.add(handler)
    return () => {
      subscribersRef.current.delete(handler)
    }
  }, [])

  const notify = useCallback((event: SignalREvent) => {
    setLastEvent(event)
    subscribersRef.current.forEach((handler) => handler(event))
  }, [])

  useEffect(() => {
    // O SignalR não manda o header Authorization em conexões WebSocket — accessTokenFactory
    // é chamado a cada (re)conexão e o token vai via query string, lido pelo backend
    // (Program.cs, OnMessageReceived) especificamente pro caminho /hubs.
    const conn = new HubConnectionBuilder()
      .withUrl(`${SIGNALR_URL}/hubs/chamados`, {
        accessTokenFactory: () => getToken() ?? '',
      })
      .withAutomaticReconnect()
      .build()

    conn.on('ChamadoCriado', (payload) => notify({ type: 'ChamadoCriado', payload }))
    conn.on('StatusAlterado', (payload) => notify({ type: 'StatusAlterado', payload }))
    conn.on('ComentarioAdicionado', (payload) => notify({ type: 'ComentarioAdicionado', payload }))
    conn.on('MetricasAtualizadas', () => notify({ type: 'MetricasAtualizadas' }))
    conn.on('SlaAtencao', (payload) => notify({ type: 'SlaAtencao', payload }))
    conn.on('SlaAtrasado', (payload) => notify({ type: 'SlaAtrasado', payload }))
    // AC-47/48: chega mesmo pra quem não tem acesso ao chat — essa conexão (/hubs/chamados) é
    // global, ao contrário do ChatHub, que só existe na tela /chat.
    conn.on('ChatPerfilAtualizado', (payload) => notify({ type: 'ChatPerfilAtualizado', payload }))
    // Bug #10: mesmo motivo — quem não está na tela /chat precisa saber que chegou mensagem nova
    // pra atualizar o badge de não lidas da sidebar, e só esta conexão global alcança essa pessoa.
    conn.on('ChatConversaAtualizada', () => notify({ type: 'ChatConversaAtualizada' }))

    let cancelado = false
    let tentativa = 0
    let retryTimer: ReturnType<typeof setTimeout> | null = null

    const tentarConectar = async () => {
      if (cancelado) return
      try {
        await conn.start()
        if (cancelado) return
        setIsConnected(true)
        tentativa = 0
      } catch {
        if (cancelado) return
        setIsConnected(false)
        const atraso = ATRASOS_RETRY_MS[Math.min(tentativa, ATRASOS_RETRY_MS.length - 1)]
        tentativa += 1
        retryTimer = setTimeout(tentarConectar, atraso)
      }
    }

    conn.onreconnecting(() => setIsConnected(false))
    conn.onreconnected(() => setIsConnected(true))
    // onclose só dispara depois que withAutomaticReconnect esgota as tentativas dele (ou .stop()
    // foi chamado) — nesse ponto volta a tentar do zero com o mesmo backoff manual.
    conn.onclose(() => {
      if (cancelado) return
      setIsConnected(false)
      tentarConectar()
    })

    tentarConectar()

    setConnection(conn)

    return () => {
      cancelado = true
      if (retryTimer) clearTimeout(retryTimer)
      conn.stop()
    }
  }, [notify])

  const value = useMemo(() => ({ connection, isConnected, lastEvent, subscribe }), [connection, isConnected, lastEvent])

  return (
    <SignalRContext.Provider value={value}>
      {children}
    </SignalRContext.Provider>
  )
}

export function useSignalR(): SignalRContextValue {
  const ctx = useContext(SignalRContext)
  if (!ctx) {
    throw new Error('useSignalR deve ser usado dentro de <SignalRProvider>')
  }
  return ctx
}
