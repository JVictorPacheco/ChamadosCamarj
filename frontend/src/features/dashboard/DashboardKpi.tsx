import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

interface DashboardKpiProps {
  titulo: string
  valor: string | number
  subtexto?: string
  /** Colore o subtexto quando a variação tem um sinal claro de bom/ruim (ex: mais Cancelados = ruim). Omitir quando a direção não é óbvia (ex: mais Abertos não é bom nem ruim por si só). */
  subtextoTom?: 'bom' | 'ruim'
}

const TOM_CLASS: Record<'bom' | 'ruim', string> = {
  bom: 'text-[var(--status-good)]',
  ruim: 'text-[var(--status-critical)]',
}

export function DashboardKpi({ titulo, valor, subtexto, subtextoTom }: DashboardKpiProps) {
  const display = typeof valor === 'number' && !Number.isInteger(valor)
    ? (valor as number).toFixed(1)
    : String(valor)

  return (
    <Card>
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
