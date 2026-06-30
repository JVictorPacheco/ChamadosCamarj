export interface DashboardMetrics {
  totalAbertos: number
  totalEmAndamento: number
  totalResolvidosHoje: number
  tempoMedioResolucaoHoras: number | null
  porCategoria: { categoriaNome: string; quantidade: number }[]
  porPrioridade: { prioridade: string; quantidade: number }[]
}

export interface TendenciaItem {
  data: string
  abertos: number
  resolvidos: number
}

export interface TendenciaResponse {
  items: TendenciaItem[]
}
