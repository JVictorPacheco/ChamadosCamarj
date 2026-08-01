import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card'
import { StatusBadge } from './StatusBadge'
import { PrioridadeBadge } from './PrioridadeBadge'
import { SlaBadge } from './SlaBadge'
import { formatarNumeroChamado } from '@/lib/utils'
import type { ChamadoResponse } from '@/types/api'

export function ChamadoCard({ chamado }: { chamado: ChamadoResponse }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>
          <span className="mr-2 text-muted-foreground">{formatarNumeroChamado(chamado.numero)}</span>
          {chamado.titulo}
        </CardTitle>
        <CardDescription>{chamado.categoriaNome ?? 'Sem categoria'}</CardDescription>
      </CardHeader>
      <CardContent className="flex flex-wrap items-center gap-2">
        <StatusBadge status={chamado.status} />
        <PrioridadeBadge prioridade={chamado.prioridade} />
        <SlaBadge dataLimite={chamado.dataLimite} status={chamado.status} slaStatus={chamado.slaStatus} slaLabel={chamado.slaLabel} />
        <span className="ml-auto text-xs text-muted-foreground">
          {new Date(chamado.dataCriacao).toLocaleDateString('pt-BR')}
        </span>
      </CardContent>
    </Card>
  )
}
