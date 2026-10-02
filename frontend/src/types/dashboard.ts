export interface PorNomeItem {
  nome: string
  id: string | null
  quantidade: number
}

export interface DashboardMetrics {
  totalResolvidosHoje: number
  tempoMedioResolucaoHoras: number | null
  porArea: PorNomeItem[]
  porTipo: PorNomeItem[]
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
