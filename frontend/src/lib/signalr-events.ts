import type { ModuloSistema } from '@/types/api'

export interface ChamadoCriadoPayload {
  chamadoId: string
  status: string
}

export interface StatusAlteradoPayload {
  chamadoId: string
  novoStatus: string
  dataAtualizacao: string
}

export interface ComentarioAdicionadoPayload {
  chamadoId: string
}

export interface SlaAlertaPayload {
  chamadoId: string
  numero: number
  mensagem: string
}

export interface ChatPerfilAtualizadoPayload {
  chatPerfil: 'SemAcesso' | 'Participante' | 'CriadorDeGrupo'
}

/** Acessos da pessoa logada mudaram (spec controle-de-acesso AC-11/AC-13). */
export interface AcessosAtualizadosPayload {
  modulos: ModuloSistema[]
  chatPerfil: 'SemAcesso' | 'Participante' | 'CriadorDeGrupo'
}

export type SignalREvent =
  | { type: 'ChamadoCriado'; payload: ChamadoCriadoPayload }
  | { type: 'StatusAlterado'; payload: StatusAlteradoPayload }
  | { type: 'ComentarioAdicionado'; payload: ComentarioAdicionadoPayload }
  | { type: 'MetricasAtualizadas' }
  | { type: 'SlaAtencao'; payload: SlaAlertaPayload }
  | { type: 'SlaAtrasado'; payload: SlaAlertaPayload }
  | { type: 'ChatPerfilAtualizado'; payload: ChatPerfilAtualizadoPayload }
  | { type: 'AcessosAtualizados'; payload: AcessosAtualizadosPayload }
  | { type: 'ChatConversaAtualizada' }
