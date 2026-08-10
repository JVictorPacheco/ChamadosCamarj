export interface DashboardMetrics {
  totalResolvidosHoje: number
  tempoMedioResolucaoHoras: number | null
  porCategoria: { categoriaNome: string; categoriaId: string | null; quantidade: number }[]
  porPrioridade: { prioridade: string; quantidade: number }[]
  slaCompliance: { totalResolvidos: number; dentroPrazo: number; percentual: number } | null
}

export interface DistribuicaoResponse {
  aguardando: number
  assumido: number
  resolvido: number
  encerrado: number
  cancelado: number
}
