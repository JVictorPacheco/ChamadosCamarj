import { useEffect, useRef, useCallback } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import {
  HubConnectionBuilder,
  type HubConnection,
  HubConnectionState,
} from '@microsoft/signalr'
import { getToken } from '@/lib/api'
import { heartbeat } from '../api'

const SIGNALR_URL = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') ?? 'http://localhost:5000'

interface UseChatSignalROptions {
  conversaAtiva?: string | null
  onAcessoRevogado?: () => void
  onDigitando?: (conversaId: string, usuarioNome: string) => void
  onPararDigitar?: (conversaId: string) => void
}

export function useChatSignalR({
  onAcessoRevogado,
  onDigitando,
  onPararDigitar,
}: UseChatSignalROptions) {
  const queryClient = useQueryClient()
  const connectionRef = useRef<HubConnection | null>(null)
  const heartbeatTimerRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const invalidarConversas = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['chat', 'conversas'] })
  }, [queryClient])

  const invalidarTodasMensagens = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['chat', 'mensagens'] })
  }, [queryClient])

  const invalidarPresencas = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['chat', 'presencas'] })
  }, [queryClient])

  // Heartbeat com pausa quando aba está em background
  useEffect(() => {
    const enviarHeartbeat = async () => {
      if (document.visibilityState === 'hidden') return
      try {
        await heartbeat()
      } catch {
        // heartbeat falhou silenciosamente — presença cairá naturalmente
      }
    }

    // Envia imediatamente ao montar
    enviarHeartbeat()

    heartbeatTimerRef.current = setInterval(enviarHeartbeat, 30_000)

    return () => {
      if (heartbeatTimerRef.current) {
        clearInterval(heartbeatTimerRef.current)
      }
    }
  }, [])

  // Conexão SignalR com ChatHub
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
      onAcessoRevogado?.()
    })

    // Digitando
    conn.on('DigitandoIniciou', (conversaId: string, usuarioNome: string) => {
      onDigitando?.(conversaId, usuarioNome)
    })

    conn.on('DigitandoParou', (conversaId: string) => {
      onPararDigitar?.(conversaId)
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
    conn.on('ParticipanteAdicionado', () => {
      invalidarConversas()
    })

    conn.on('ParticipanteRemovido', () => {
      invalidarConversas()
    })

    conn.start().catch(() => {
      // Falha silenciosa — reconecta automaticamente
    })

    connectionRef.current = conn

    return () => {
      conn.stop()
    }
  }, [invalidarTodasMensagens, invalidarConversas, invalidarPresencas, onAcessoRevogado, onDigitando, onPararDigitar])

  // Envia evento de digitação para o hub
  const emitirDigitando = useCallback(
    async (conversaId: string) => {
      const conn = connectionRef.current
      if (!conn || conn.state !== HubConnectionState.Connected) return
      try {
        await conn.invoke('Digitando', conversaId)
      } catch {
        // falha silenciosa
      }
    },
    []
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

  return { emitirDigitando, emitirPararDigitar }
}
