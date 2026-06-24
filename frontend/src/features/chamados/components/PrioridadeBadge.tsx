import { Badge } from '@/components/ui/badge'
import type { PrioridadeChamado } from '@/types/api'

const VARIANTS: Record<PrioridadeChamado, 'default' | 'secondary' | 'outline' | 'destructive'> = {
  Baixa: 'outline',
  Media: 'secondary',
  Alta: 'default',
  Urgente: 'destructive',
}

export function PrioridadeBadge({ prioridade }: { prioridade: PrioridadeChamado }) {
  return <Badge variant={VARIANTS[prioridade]}>{prioridade}</Badge>
}
