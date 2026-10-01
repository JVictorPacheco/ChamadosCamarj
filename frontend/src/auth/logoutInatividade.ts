// Minutos sem interação até desconectar — decisão de 2026-07-18 (spec logout-inatividade).
export const MINUTOS_INATIVIDADE = 20

// sessionStorage: o aviso aparece só na aba que foi desconectada e só uma vez.
const CHAVE_MOTIVO = 'chamados-camarj:logout-por-inatividade'

export function marcarLogoutPorInatividade(): void {
  try {
    sessionStorage.setItem(CHAVE_MOTIVO, '1')
  } catch {
    // Sem sessionStorage a tela de login só não mostra o motivo.
  }
}

export function saiuPorInatividade(): boolean {
  try {
    return sessionStorage.getItem(CHAVE_MOTIVO) === '1'
  } catch {
    return false
  }
}

/** Apaga o aviso no próximo login. Não é apagado ao exibir: a tela de login pode ser montada mais
 * de uma vez no redirecionamento, e a mensagem sumiria antes de aparecer. */
export function limparLogoutPorInatividade(): void {
  try {
    sessionStorage.removeItem(CHAVE_MOTIVO)
  } catch {
    // nada a fazer
  }
}
