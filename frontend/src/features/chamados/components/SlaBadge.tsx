import { Badge } from '@/components/ui/badge'
import type { SlaStatus, StatusChamado } from '@/types/api'

const STATUS_TERMINAL: StatusChamado[] = ['Resolvido', 'Fechado', 'Cancelado']

interface SlaBadgeProps {
  dataLimite: string | null
  status: StatusChamado
  slaStatus?: SlaStatus
  slaLabel?: string
}

export function SlaBadge({ dataLimite, status, slaStatus, slaLabel }: SlaBadgeProps) {
  if (!dataLimite || STATUS_TERMINAL.includes(status)) return null

  const label = slaLabel ?? ''
  if (slaStatus === 'Atrasado')
    return <Badge variant="destructive">{label || 'Atrasado'}</Badge>
  if (slaStatus === 'Atencao')
    return <Badge variant="secondary">{label || 'Atenção'}</Badge>
  return <Badge variant="outline">{label || 'No prazo'}</Badge>
}