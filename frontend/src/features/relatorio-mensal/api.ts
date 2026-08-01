import { apiFetch } from '@/lib/api'
import type { RelatorioMensalResponse } from '@/types/relatorio'

export function obterRelatorioMensal(
  ano: number,
  mes: number,
  responsavelId?: string,
): Promise<RelatorioMensalResponse> {
  const params = new URLSearchParams({ ano: String(ano), mes: String(mes) })
  if (responsavelId) {
    params.set('responsavelId', responsavelId)
  }
  return apiFetch<RelatorioMensalResponse>(`/relatorios/mensal?${params.toString()}`)
}
