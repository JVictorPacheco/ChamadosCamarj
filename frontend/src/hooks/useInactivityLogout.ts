import { useEffect } from 'react'

const EVENTOS_DE_ATIVIDADE = ['mousemove', 'keydown', 'click', 'scroll'] as const

/**
 * Desloga automaticamente após `minutos` sem interação (mouse/teclado/clique/scroll),
 * independente da expiração do token — protege contra alguém deixar o computador
 * desbloqueado com a aba aberta e sem vigilância.
 */
export function useInactivityLogout(minutos: number, aoExpirar: () => void): void {
  useEffect(() => {
    let timer: ReturnType<typeof setTimeout>

    const reiniciar = () => {
      clearTimeout(timer)
      timer = setTimeout(aoExpirar, minutos * 60_000)
    }

    EVENTOS_DE_ATIVIDADE.forEach((evento) => window.addEventListener(evento, reiniciar))
    reiniciar()

    return () => {
      clearTimeout(timer)
      EVENTOS_DE_ATIVIDADE.forEach((evento) => window.removeEventListener(evento, reiniciar))
    }
  }, [minutos, aoExpirar])
}
