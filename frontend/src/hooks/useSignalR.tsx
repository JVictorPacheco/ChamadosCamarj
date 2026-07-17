import { createContext, useContext, useEffect, useState, useCallback, useRef, type ReactNode } from 'react'
import {
  HubConnectionBuilder,
  type HubConnection,
} from '@microsoft/signalr'
import type { SignalREvent } from '@/lib/signalr-events'

const SIGNALR_URL = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') ?? 'http://localhost:5000'

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
    const conn = new HubConnectionBuilder()
      .withUrl(`${SIGNALR_URL}/hubs/chamados`)
      .withAutomaticReconnect()
      .build()

    conn.on('ChamadoCriado', (payload) => notify({ type: 'ChamadoCriado', payload }))
    conn.on('StatusAlterado', (payload) => notify({ type: 'StatusAlterado', payload }))
    conn.on('ComentarioAdicionado', (payload) => notify({ type: 'ComentarioAdicionado', payload }))
    conn.on('MetricasAtualizadas', () => notify({ type: 'MetricasAtualizadas' }))

    conn
      .start()
      .then(() => setIsConnected(true))
      .catch(() => setIsConnected(false))

    conn.onreconnecting(() => setIsConnected(false))
    conn.onreconnected(() => setIsConnected(true))
    conn.onclose(() => setIsConnected(false))

    setConnection(conn)

    return () => {
      conn.stop()
    }
  }, [notify])

  return (
    <SignalRContext.Provider value={{ connection, isConnected, lastEvent, subscribe }}>
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
