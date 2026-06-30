export interface ChamadoCriadoPayload {
  chamadoId: string
  titulo: string
  status: string
}

export interface StatusAlteradoPayload {
  chamadoId: string
  novoStatus: string
  dataAtualizacao: string
}

export interface ComentarioAdicionadoPayload {
  chamadoId: string
  autor: string
  conteudo: string
}

export type SignalREvent =
  | { type: 'ChamadoCriado'; payload: ChamadoCriadoPayload }
  | { type: 'StatusAlterado'; payload: StatusAlteradoPayload }
  | { type: 'ComentarioAdicionado'; payload: ComentarioAdicionadoPayload }
  | { type: 'MetricasAtualizadas' }
