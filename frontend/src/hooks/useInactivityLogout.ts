import { useEffect, useRef } from 'react'

const EVENTOS_DE_ATIVIDADE = ['mousemove', 'keydown', 'click', 'scroll'] as const

// Compartilhada entre abas: atividade em qualquer aba mantém todas conectadas (o token é o mesmo
// para todas — uma aba esquecida não pode deslogar quem está trabalhando em outra).
const CHAVE_ULTIMA_ATIVIDADE = 'chamados-camarj:ultima-atividade'
// Gravar a cada mousemove seria escrita demais no localStorage; 15 s de precisão bastam para 20 min.
const INTERVALO_MINIMO_GRAVACAO_MS = 15_000

function lerUltimaAtividade(): number {
  try {
    return Number(localStorage.getItem(CHAVE_ULTIMA_ATIVIDADE)) || 0
  } catch {
    return 0
  }
}

function gravarUltimaAtividade(agora: number): void {
  try {
    localStorage.setItem(CHAVE_ULTIMA_ATIVIDADE, String(agora))
  } catch {
    // Sem localStorage (modo privado restrito), cada aba conta só a própria atividade.
  }
}

/**
 * Desloga automaticamente após `minutos` sem interação (mouse/teclado/clique/scroll) em nenhuma
 * aba do sistema, independente da expiração do token — protege contra alguém deixar o computador
 * desbloqueado com a aba aberta e sem vigilância. Spec: .specs/features/logout-inatividade.
 *
 * `aoExpirar` fica numa ref: quem chama não precisa memoizar, e re-renderizações da tela (tempo
 * real, chat) não reiniciam nem atrasam a contagem.
 */
export function useInactivityLogout(minutos: number, aoExpirar: () => void): void {
  const aoExpirarRef = useRef(aoExpirar)

  useEffect(() => {
    aoExpirarRef.current = aoExpirar
  }, [aoExpirar])

  useEffect(() => {
    const limiteMs = minutos * 60_000
    let timer: ReturnType<typeof setTimeout>
    let ultimaGravacao = 0

    const agendar = (emMs: number) => {
      clearTimeout(timer)
      timer = setTimeout(verificar, emMs)
    }

    // Quando o timer desta aba vence, confere se outra aba teve atividade mais recente.
    function verificar() {
      const parado = Date.now() - lerUltimaAtividade()
      if (parado >= limiteMs) {
        aoExpirarRef.current()
      } else {
        agendar(limiteMs - parado)
      }
    }

    const registrarAtividade = () => {
      const agora = Date.now()
      if (agora - ultimaGravacao >= INTERVALO_MINIMO_GRAVACAO_MS) {
        ultimaGravacao = agora
        gravarUltimaAtividade(agora)
      }
      agendar(limiteMs)
    }

    EVENTOS_DE_ATIVIDADE.forEach((evento) => window.addEventListener(evento, registrarAtividade))
    registrarAtividade()

    return () => {
      clearTimeout(timer)
      EVENTOS_DE_ATIVIDADE.forEach((evento) => window.removeEventListener(evento, registrarAtividade))
    }
  }, [minutos])
}
