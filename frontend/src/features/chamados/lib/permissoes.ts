import type { Perfil } from '@/auth/AuthContext'
import type { ChamadoResponse } from '@/types/api'

const ENCERRADOS: ChamadoResponse['status'][] = ['Resolvido', 'Fechado', 'Cancelado']

/**
 * Mostra o botão "Editar" (spec editar-chamado). Espelho de ChamadoPermissoes no servidor — quem
 * decide de verdade é o servidor; isto só evita oferecer o que vai ser recusado.
 * Não encerrado E (Admin OU responsável atual OU quem abriu enquanto ninguém assumiu).
 */
export function podeEditarChamado(chamado: ChamadoResponse, perfil: Perfil | null | undefined): boolean {
  if (!perfil || ENCERRADOS.includes(chamado.status)) return false
  if (perfil.tipo === 'Admin') return true
  if (chamado.responsavelId) return chamado.responsavelId === perfil.id
  return !!perfil.email && chamado.solicitanteEmail.toLowerCase() === perfil.email.toLowerCase()
}
