export interface DashboardMetrics {
  totalAbertos: number
  totalEmAndamento: number
  totalResolvidos: number
  totalCancelados: number
  totalResolvidosHoje: number
  tempoMedioResolucaoHoras: number | null
  porCategoria: { categoriaNome: string; quantidade: number }[]
  porPrioridade: { prioridade: string; quantidade: number }[]
}

export interface DistribuicaoResponse {
  aguardando: number
  assumido: number
  resolvido: number
  encerrado: number
  cancelado: number
}
