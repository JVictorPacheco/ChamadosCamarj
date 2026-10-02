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
  TipoReclassificado: 'Tipo alterado',
  ChamadoEditado: 'Chamado editado',
}

const LABEL_CAMPO: Record<string, string> = { titulo: 'Título', descricao: 'Descrição' }

/** Detalhe de "Chamado editado": JSON só com os campos que mudaram (spec editar-chamado AC-17/AC-18). */
function lerCampos(json: string | null): Record<string, string> | null {
  if (!json) return null
  try {
    const valor: unknown = JSON.parse(json)
    return valor && typeof valor === 'object' ? (valor as Record<string, string>) : null
  } catch {
    return null
  }
}

function DetalheEdicao({ anterior, novo }: { anterior: string | null; novo: string | null }) {
  const antes = lerCampos(anterior)
  const depois = lerCampos(novo)
  // JSON inválido: mostra o texto cru em vez de quebrar a tela.
  if (!antes || !depois) return <p className="mt-1 text-sm">{anterior} → {novo}</p>

  return (
    <div className="mt-2 flex flex-col gap-2">
      {Object.keys(depois).map((campo) => (
        <div key={campo} className="text-sm">
          <p className="font-medium">{LABEL_CAMPO[campo] ?? campo}</p>
          <p className="whitespace-pre-wrap text-muted-foreground line-through">{antes[campo]}</p>
          <p className="whitespace-pre-wrap">{depois[campo]}</p>
        </div>
      ))}
    </div>
  )
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
          {entrada.acao === 'ChamadoEditado' ? (
            <DetalheEdicao anterior={entrada.detalheAnterior} novo={entrada.detalheNovo} />
          ) : entrada.detalheAnterior && entrada.detalheNovo && (
            <p className="mt-1 text-sm">
              {entrada.detalheAnterior} → {entrada.detalheNovo}
            </p>
          )}
        </li>
      ))}
    </ul>
  )
}
