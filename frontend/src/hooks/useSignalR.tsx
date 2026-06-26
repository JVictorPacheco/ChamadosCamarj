import { createContext, useContext, useEffect, useState, useCallback, type ReactNode } from 'react'
import {
  HubConnectionBuilder,
  HubConnectionState,
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
  const [subscribers, setSubscribers] = useState<Set<(event: SignalREvent) => void>>(new Set())

  const subscribe = useCallback((handler: (event: SignalREvent) => void) => {
    setSubscribers((prev) => new Set(prev).add(handler))
    return () => {
      setSubscribers((prev) => {
        const next = new Set(prev)
        next.delete(handler)
        return next
      })
    }
  }, [])

  const notify = useCallback(
    (event: SignalREvent) => {
      setLastEvent(event)
      subscribers.forEach((handler) => handler(event))
    },
    [subscribers],
  )

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
