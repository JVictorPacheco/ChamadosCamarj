import { apiFetch } from '@/lib/api'
import type { DashboardMetrics, DistribuicaoResponse } from '@/types/dashboard'

export function obterMetricas(): Promise<DashboardMetrics> {
  return apiFetch<DashboardMetrics>('/dashboard/metricas')
}

export function obterDistribuicao(): Promise<DistribuicaoResponse> {
  return apiFetch<DistribuicaoResponse>('/dashboard/distribuicao')
}
