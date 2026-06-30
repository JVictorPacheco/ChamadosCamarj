import { apiFetch } from '@/lib/api'
import type { DashboardMetrics, TendenciaResponse } from '@/types/dashboard'

export function obterMetricas(): Promise<DashboardMetrics> {
  return apiFetch<DashboardMetrics>('/dashboard/metricas')
}

export function obterTendencia(dias = 7): Promise<TendenciaResponse> {
  return apiFetch<TendenciaResponse>(`/dashboard/tendencia?dias=${dias}`)
}
