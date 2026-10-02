import { useEffect, useRef } from 'react'

// Só gestos reais da pessoa. `scroll` ficou de fora de propósito: o navegador dispara scroll sozinho
// quando o tempo real insere conteúdo acima da área visível (scroll anchoring), o que manteria a
// sessão aberta para sempre; e scroll de áreas internas (chat) nem chega à window. Rolar com a roda,
// o toque ou o teclado já é coberto por wheel/touchstart/keydown (review R-01, R-03).
const EVENTOS_DE_ATIVIDADE = ['mousemove', 'pointerdown', 'keydown', 'wheel', 'touchstart'] as const

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

/** Chamado no login: uma marca antiga de outra sessão não pode derrubar o login novo. */
export function registrarInicioDeSessao(): void {
  gravarUltimaAtividade(Date.now())
}

/**
 * True se a última atividade registrada (em qualquer aba) passou do limite — ex.: navegador
 * reaberto, aba descartada. Usado pela rota protegida ANTES de montar a área logada, para que
 * nenhuma tela nem o heartbeat do chat chegue a rodar com a sessão vencida (review R2-02).
 */
export function sessaoVencidaPorInatividade(minutos: number): boolean {
  const ultima = lerUltimaAtividade()
  // Sem marca nenhuma = sessão aberta antes desta proteção existir (login antigo, anterior ao
  // deploy): sem como saber há quanto tempo está parada, então vale como vencida e pede um novo
  // login uma única vez (review R3-02). Todo login novo grava a marca antes de entrar.
  return ultima === 0 || Date.now() - ultima >= minutos * 60_000
}

/**
 * Desloga automaticamente após `minutos` sem interação em nenhuma aba do sistema, independente da
 * expiração do token — protege contra alguém deixar o computador desbloqueado com a aba aberta e
 * sem vigilância. Spec: .specs/features/logout-inatividade.
 *
 * O tempo parado é sempre medido pelo relógio de parede (`Date.now`), nunca pela contagem do timer:
 * o timer congela com o computador suspenso, e o primeiro gesto depois da suspensão não pode
 * renovar uma sessão que já venceu (review R2-01).
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
    let ultimaLocal = Date.now()
    let expirado = false

    // Mais recente entre esta aba e as outras (a gravada pode estar até 15 s atrasada).
    const tempoParado = () => Date.now() - Math.max(ultimaLocal, lerUltimaAtividade())

    const expirar = () => {
      if (expirado) return
      expirado = true
      clearTimeout(timer)
      aoExpirarRef.current()
    }

    const agendar = (emMs: number) => {
      clearTimeout(timer)
      timer = setTimeout(verificar, emMs)
    }

    // Timer venceu, ou a aba voltou a ficar visível: decide pelo relógio de parede.
    function verificar() {
      const parado = tempoParado()
      if (parado >= limiteMs) expirar()
      else agendar(limiteMs - parado)
    }

    const registrarAtividade = () => {
      if (expirado) return
      if (tempoParado() >= limiteMs) {
        expirar()
        return
      }
      const agora = Date.now()
      ultimaLocal = agora
      if (agora - ultimaGravacao >= INTERVALO_MINIMO_GRAVACAO_MS) {
        ultimaGravacao = agora
        gravarUltimaAtividade(agora)
      }
      agendar(limiteMs)
    }

    const aoVoltarParaAba = () => {
      if (document.visibilityState === 'visible') verificar()
    }

    // Fase de captura: o gesto é avaliado ANTES dos handlers da tela. Depois de uma suspensão, o
    // primeiro Enter desconecta em vez de enviar o rascunho do chat com a sessão vencida (R3-01).
    EVENTOS_DE_ATIVIDADE.forEach((evento) =>
      window.addEventListener(evento, registrarAtividade, { capture: true, passive: true }),
    )
    window.addEventListener('focus', verificar)
    document.addEventListener('visibilitychange', aoVoltarParaAba)

    // Sessão já vencida ao montar: a rota protegida normalmente pega antes; aqui fica a garantia.
    if (sessaoVencidaPorInatividade(minutos)) {
      expirar()
    } else {
      gravarUltimaAtividade(ultimaLocal)
      ultimaGravacao = ultimaLocal
      agendar(limiteMs)
    }

    return () => {
      clearTimeout(timer)
      EVENTOS_DE_ATIVIDADE.forEach((evento) =>
        window.removeEventListener(evento, registrarAtividade, { capture: true }),
      )
      window.removeEventListener('focus', verificar)
      document.removeEventListener('visibilitychange', aoVoltarParaAba)
    }
  }, [minutos])
}
