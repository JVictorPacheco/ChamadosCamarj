import { Badge } from '@/components/ui/badge'
import type { StatusChamado } from '@/types/api'

const VARIANTS: Record<StatusChamado, 'default' | 'secondary' | 'outline' | 'destructive'> = {
  Aberto: 'default',
  EmAndamento: 'secondary',
  Resolvido: 'outline',
  Fechado: 'outline',
  Cancelado: 'destructive',
}

const LABELS: Record<StatusChamado, string> = {
  Aberto: 'Aberto',
  EmAndamento: 'Em andamento',
  Resolvido: 'Resolvido',
  Fechado: 'Fechado',
  Cancelado: 'Cancelado',
}

export function StatusBadge({ status }: { status: StatusChamado }) {
  return <Badge variant={VARIANTS[status]}>{LABELS[status]}</Badge>
}
