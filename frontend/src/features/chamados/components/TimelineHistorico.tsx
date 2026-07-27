import { useHistorico } from '../hooks/useHistorico'
import type { AcaoHistorico } from '@/types/api'

const LABEL_ACAO: Record<AcaoHistorico, string> = {
  Criado: 'Chamado criado',
  Assumido: 'Assumido',
  Reatribuido: 'Reatribuído',
  Resolvido: 'Resolvido',
  Fechado: 'Fechado',
  Cancelado: 'Cancelado',
  ComentarioAdicionado: 'Comentário adicionado',
  PrioridadeAlterada: 'Prioridade alterada',
  StatusAlterado: 'Status alterado',
  EncerramentoForcado: 'Encerramento forçado',
  Reaberto: 'Reaberto',
}

export function TimelineHistorico({ chamadoId }: { chamadoId: string }) {
  const { data: historico, isPending } = useHistorico(chamadoId)

  if (isPending) {
    return <p className="text-sm text-muted-foreground">Carregando histórico...</p>
  }

  if (!historico || historico.length === 0) {
    return <p className="text-sm text-muted-foreground">Sem histórico.</p>
  }

  const ordenado = [...historico].sort(
    (a, b) => new Date(b.dataHora).getTime() - new Date(a.dataHora).getTime(),
  )

  return (
    <ul className="flex flex-col gap-3">
      {ordenado.map((entrada) => (
        <li key={entrada.id} className="rounded-lg border border-border p-3">
          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span className="font-medium text-foreground">{LABEL_ACAO[entrada.acao]}</span>
            <span>{new Date(entrada.dataHora).toLocaleString('pt-BR')}</span>
          </div>
          <p className="mt-1 text-sm text-muted-foreground">por {entrada.usuarioNome}</p>
          {entrada.detalheAnterior && entrada.detalheNovo && (
            <p className="mt-1 text-sm">
              {entrada.detalheAnterior} → {entrada.detalheNovo}
            </p>
          )}
        </li>
      ))}
    </ul>
  )
}
