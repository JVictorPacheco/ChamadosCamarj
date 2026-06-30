import { Badge } from '@/components/ui/badge'
import type { StatusChamado } from '@/types/api'

const STATUS_TERMINAL: StatusChamado[] = ['Resolvido', 'Fechado', 'Cancelado']

interface SlaBadgeProps {
  dataLimite: string | null
  status: StatusChamado
}

export function SlaBadge({ dataLimite, status }: SlaBadgeProps) {
  if (!dataLimite || STATUS_TERMINAL.includes(status)) {
    return null
  }

  const limite = new Date(dataLimite)
  const agora = new Date()

  if (limite < agora) {
    return <Badge variant="destructive">Atrasado</Badge>
  }

  const diffHoras = Math.round((limite.getTime() - agora.getTime()) / (1000 * 60 * 60))
  return <Badge variant="outline">vence em {diffHoras}h</Badge>
}
