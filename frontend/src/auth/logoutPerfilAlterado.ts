// spec controle-de-acesso AC-15 (review-2 R-03, decisão de 2026-10-05): quando o Admin muda o perfil
// de quem está com o sistema aberto, a tela sai e pede um login novo — o token antigo ainda carrega o
// perfil anterior. Mesmo mecanismo do aviso de inatividade (logoutInatividade.ts).

// sessionStorage: o aviso aparece só na aba que foi desconectada e só uma vez.
const CHAVE_MOTIVO = 'chamados-camarj:logout-por-perfil-alterado'

export const MENSAGEM_PERFIL_ALTERADO = 'Seu perfil foi alterado. Entre novamente.'

export function marcarLogoutPorPerfilAlterado(): void {
  try {
    sessionStorage.setItem(CHAVE_MOTIVO, '1')
  } catch {
    // Sem sessionStorage a tela de login só não mostra o motivo.
  }
}

export function saiuPorPerfilAlterado(): boolean {
  try {
    return sessionStorage.getItem(CHAVE_MOTIVO) === '1'
  } catch {
    return false
  }
}

/** Apaga o aviso no próximo login (mesmo motivo do limparLogoutPorInatividade: não apagar ao exibir). */
export function limparLogoutPorPerfilAlterado(): void {
  try {
    sessionStorage.removeItem(CHAVE_MOTIVO)
  } catch {
    // nada a fazer
  }
}
