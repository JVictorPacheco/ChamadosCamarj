export interface SlaResponse {
  totalComPrazo: number
  dentroDoPrazo: number
  estourados: number
  percentualCumprido: number | null
}

export interface PorCategoriaItem {
  categoriaNome: string
  quantidade: number
}

export interface PorAtendenteItem {
  responsavelNome: string
  abertos: number
  resolvidos: number
  cancelados: number
}

export interface ComparacaoMesAnteriorResponse {
  variacaoAbertosPercentual: number | null
  variacaoResolvidosPercentual: number | null
  variacaoCanceladosPercentual: number | null
}

export interface RelatorioMensalResponse {
  ano: number
  mes: number
  mesParcial: boolean
  totalAbertos: number
  totalResolvidos: number
  totalCancelados: number
  tempoMedioResolucaoHoras: number | null
  sla: SlaResponse
  porCategoria: PorCategoriaItem[]
  porAtendente: PorAtendenteItem[] | null
  comparacao: ComparacaoMesAnteriorResponse | null
}
