import { useEffect, useRef, useCallback, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import {
  HubConnectionBuilder,
  type HubConnection,
  HubConnectionState,
} from '@microsoft/signalr'
import { getToken } from '@/lib/api'
import { useAuth } from '@/auth/AuthContext'

const SIGNALR_URL = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') ?? 'http://localhost:5000'

// Backoff pra tentativa inicial de conexão (withAutomaticReconnect só cobre reconexão de uma
// conexão que já foi estabelecida — se o primeiro .start() falhar, ninguém tenta de novo sozinho).
const ATRASOS_RETRY_MS = [1000, 2000, 5000, 10000, 15000, 30000]

export type StatusConexaoChat = 'conectando' | 'conectado' | 'reconectando' | 'offline'

interface UseChatSignalROptions {
  conversaAtiva?: string | null
  onAcessoRevogado?: () => void
  onDigitando?: (conversaId: string, usuarioNome: string) => void
  onPararDigitar?: (conversaId: string) => void
}

export function useChatSignalR({
  conversaAtiva,
  onAcessoRevogado,
  onDigitando,
  onPararDigitar,
}: UseChatSignalROptions) {
  const queryClient = useQueryClient()
  const { perfil } = useAuth()
  const connectionRef = useRef<HubConnection | null>(null)
  const [status, setStatus] = useState<StatusConexaoChat>('conectando')

  // Guarda os callbacks/estado em refs para que o effect de conexão possa ter deps estáveis
  // (evita recriar o WebSocket a cada troca de conversa — issue de reconexão desnecessária).
  const onAcessoRevogadoRef = useRef(onAcessoRevogado)
  const onDigitandoRef = useRef(onDigitando)
  const onPararDigitarRef = useRef(onPararDigitar)
  const conversaAtivaRef = useRef<string | null | undefined>(conversaAtiva)

  useEffect(() => {
    onAcessoRevogadoRef.current = onAcessoRevogado
    onDigitandoRef.current = onDigitando
    onPararDigitarRef.current = onPararDigitar
  }, [onAcessoRevogado, onDigitando, onPararDigitar])

  const invalidarConversas = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['chat', 'conversas'] })
  }, [queryClient])

  const invalidarTodasMensagens = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['chat', 'mensagens'] })
  }, [queryClient])

  const invalidarPresencas = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['chat', 'presencas'] })
  }, [queryClient])

  // Conexão SignalR com ChatHub — criada uma única vez (deps estáveis).
  useEffect(() => {
    const conn = new HubConnectionBuilder()
      .withUrl(`${SIGNALR_URL}/hubs/chat`, {
        accessTokenFactory: () => getToken() ?? '',
      })
      .withAutomaticReconnect()
      .build()

    // Nova mensagem recebida
    conn.on('NovaMensagem', () => {
      invalidarTodasMensagens()
      invalidarConversas()
    })

    // Mensagem editada
    conn.on('MensagemEditada', () => {
      invalidarTodasMensagens()
      invalidarConversas()
    })

    // Mensagem deletada
    conn.on('MensagemDeletada', () => {
      invalidarTodasMensagens()
      invalidarConversas()
    })

    // Reação atualizada
    conn.on('ReacaoAtualizada', () => {
      invalidarTodasMensagens()
    })

    // Status de presença
    conn.on('PresencaAtualizada', () => {
      invalidarPresencas()
    })

    // Acesso revogado
    conn.on('AcessoRevogado', () => {
      onAcessoRevogadoRef.current?.()
    })

    // Digitando
    conn.on('DigitandoIniciou', (conversaId: string, usuarioNome: string) => {
      onDigitandoRef.current?.(conversaId, usuarioNome)
    })

    conn.on('DigitandoParou', (conversaId: string) => {
      onPararDigitarRef.current?.(conversaId)
    })

    // Leitura confirmada
    conn.on('MensagemLida', () => {
      invalidarTodasMensagens()
    })

    // Nova conversa criada
    conn.on('NovaConversa', () => {
      invalidarConversas()
    })

    // Participante adicionado/removido
    conn.on('ParticipanteAdicionado', (conversaId: string) => {
      invalidarConversas()
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversa-detalhe', conversaId] })
    })

    conn.on('ParticipanteRemovido', (conversaId: string) => {
      invalidarConversas()
      queryClient.invalidateQueries({ queryKey: ['chat', 'conversa-detalhe', conversaId] })
    })

    // withAutomaticReconnect cobre quedas de uma conexão já estabelecida. onreconnecting/
    // onreconnected/onclose cobrem esse ciclo — o retry manual abaixo cobre só a tentativa
    // INICIAL, que withAutomaticReconnect não reenvia sozinho se falhar.
    conn.onreconnecting(() => setStatus('reconectando'))

    conn.onreconnected(() => {
      setStatus('conectado')
      const atual = conversaAtivaRef.current
      if (atual) {
        conn.invoke('EntrarConversa', atual).catch(() => {
          // falha silenciosa
        })
      }
    })

    let cancelado = false
    let tentativa = 0
    let retryTimer: ReturnType<typeof setTimeout> | null = null

    const tentarConectar = async () => {
      if (cancelado) return
      setStatus(tentativa === 0 ? 'conectando' : 'reconectando')
      try {
        await conn.start()
        if (cancelado) return
        setStatus('conectado')
        tentativa = 0
        const atual = conversaAtivaRef.current
        if (atual) {
          await conn.invoke('EntrarConversa', atual).catch(() => {
            // falha silenciosa
          })
        }
      } catch {
        if (cancelado) return
        setStatus('offline')
        const atraso = ATRASOS_RETRY_MS[Math.min(tentativa, ATRASOS_RETRY_MS.length - 1)]
        tentativa += 1
        retryTimer = setTimeout(tentarConectar, atraso)
      }
    }

    // onclose só dispara depois que withAutomaticReconnect já esgotou as tentativas dele (ou
    // .stop() foi chamado) — nesse ponto voltamos a tentar do zero com o mesmo backoff manual.
    conn.onclose(() => {
      if (cancelado) return
      setStatus('offline')
      tentarConectar()
    })

    tentarConectar()

    connectionRef.current = conn

    return () => {
      cancelado = true
      if (retryTimer) clearTimeout(retryTimer)
      conn.stop()
      connectionRef.current = null
    }
  }, [invalidarTodasMensagens, invalidarConversas, invalidarPresencas])

  // Entra/sai dos grupos SignalR conforme a conversa ativa muda.
  useEffect(() => {
    const conn = connectionRef.current
    const anterior = conversaAtivaRef.current
    conversaAtivaRef.current = conversaAtiva

    if (!conn) return

    const sincronizarGrupos = async () => {
      if (conn.state !== HubConnectionState.Connected) return
      try {
        if (anterior && anterior !== conversaAtiva) {
          await conn.invoke('SairConversa', anterior)
        }
        if (conversaAtiva && anterior !== conversaAtiva) {
          await conn.invoke('EntrarConversa', conversaAtiva)
        }
      } catch {
        // falha silenciosa
      }
    }

    sincronizarGrupos()
  }, [conversaAtiva])

  // Envia evento de digitação para o hub
  const emitirDigitando = useCallback(
    async (conversaId: string) => {
      const conn = connectionRef.current
      if (!conn || conn.state !== HubConnectionState.Connected) return
      try {
        await conn.invoke('Digitando', conversaId, perfil?.nome ?? '')
      } catch {
        // falha silenciosa
      }
    },
    [perfil?.nome]
  )

  const emitirPararDigitar = useCallback(
    async (conversaId: string) => {
      const conn = connectionRef.current
      if (!conn || conn.state !== HubConnectionState.Connected) return
      try {
        await conn.invoke('PararDigitar', conversaId)
      } catch {
        // falha silenciosa
      }
    },
    []
  )

  return { emitirDigitando, emitirPararDigitar, status }
}
