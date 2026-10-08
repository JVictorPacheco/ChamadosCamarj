import type { ModuloSistema, TipoPerfil } from '@/types/api'

/**
 * Padrão de cada perfil (spec controle-de-acesso) — o mesmo do servidor. Só é usado enquanto o perfil salvo
 * no navegador ainda não tem a lista vinda do servidor (sessão aberta antes desta versão).
 */
export function modulosPadrao(tipo: TipoPerfil): ModuloSistema[] {
  return tipo === 'Solicitante' ? ['Arquivo'] : ['Arquivo', 'Kanban', 'Fila', 'Dashboard', 'RelatorioMensal']
}

/** A pessoa tem o módulo? Admin tem todos. O servidor confere de novo onde importa. */
export function temModulo(perfil: { tipo: TipoPerfil; modulos?: ModuloSistema[] } | null | undefined, modulo: ModuloSistema): boolean {
  if (!perfil) return false
  if (perfil.tipo === 'Admin') return true
  return (perfil.modulos ?? modulosPadrao(perfil.tipo)).includes(modulo)
}
