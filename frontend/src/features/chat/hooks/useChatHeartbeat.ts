import { useEffect } from 'react'
import { heartbeat } from '../api'

/**
 * Mantém a presença do usuário como "Online" enquanto ele estiver logado,
 * em qualquer tela do app (não só na página de chat). Envia um heartbeat
 * imediato ao montar e depois a cada 30s, pulando quando a aba está em segundo plano.
 */
export function useChatHeartbeat(enabled: boolean) {
  useEffect(() => {
    if (!enabled) return

    const enviarHeartbeat = async () => {
      if (document.visibilityState === 'hidden') return
      try {
        await heartbeat()
      } catch {
        // falha silenciosa — presença cairá naturalmente para Ausente/Offline
      }
    }

    enviarHeartbeat()

    const timer = setInterval(enviarHeartbeat, 30_000)

    return () => clearInterval(timer)
  }, [enabled])
}
