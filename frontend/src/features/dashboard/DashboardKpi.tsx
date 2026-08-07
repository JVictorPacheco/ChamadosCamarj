import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

interface DashboardKpiProps {
  titulo: string
  valor: string | number
  subtexto?: string
  subtextoTom?: 'bom' | 'ruim'
  onClick?: () => void
}

const TOM_CLASS: Record<'bom' | 'ruim', string> = {
  bom: 'text-[var(--status-good)]',
  ruim: 'text-[var(--status-critical)]',
}

export function DashboardKpi({ titulo, valor, subtexto, subtextoTom, onClick }: DashboardKpiProps) {
  const display = typeof valor === 'number' && !Number.isInteger(valor)
    ? (valor as number).toFixed(1)
    : String(valor)

  return (
    <Card className={onClick ? 'cursor-pointer hover:bg-accent/50 transition-colors' : undefined} onClick={onClick}>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-normal text-muted-foreground">{titulo}</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-3xl font-heading">{display}</p>
        {subtexto && (
          <p className={`mt-1 text-xs ${subtextoTom ? TOM_CLASS[subtextoTom] : 'text-muted-foreground'}`}>
            {subtexto}
          </p>
        )}
      </CardContent>
    </Card>
  )
}
