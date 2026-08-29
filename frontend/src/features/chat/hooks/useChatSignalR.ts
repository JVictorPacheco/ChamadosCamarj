import { useEffect, useRef, useCallback } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import {
  HubConnectionBuilder,
  type HubConnection,
  HubConnectionState,
} from '@microsoft/signalr'
import { getToken } from '@/lib/api'
import { useAuth } from '@/auth/AuthContext'
import { heartbeat } from '../api'

const SIGNALR_URL = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') ?? 'http://localhost:5000'

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
  const heartbeatTimerRef = useRef<ReturnType<typeof setInterval> | null>(null)

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
    conn.on('ParticipanteAdicionado', () => {
      invalidarConversas()
    })

    conn.on('ParticipanteRemovido', () => {
      invalidarConversas()
    })

    // Ao reconectar, reentra no grupo da conversa ativa (o servidor perde os grupos na reconexão).
    conn.onreconnected(() => {
      const atual = conversaAtivaRef.current
      if (atual) {
        conn.invoke('EntrarConversa', atual).catch(() => {
          // falha silenciosa
        })
      }
    })

    conn.start()
      .then(() => {
        // Se já havia uma conversa ativa quando a conexão subiu, entra no grupo dela.
        const atual = conversaAtivaRef.current
        if (atual) {
          return conn.invoke('EntrarConversa', atual)
        }
      })
      .catch(() => {
        // Falha silenciosa — reconecta automaticamente
      })

    connectionRef.current = conn

    return () => {
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

  return { emitirDigitando, emitirPararDigitar }
}
