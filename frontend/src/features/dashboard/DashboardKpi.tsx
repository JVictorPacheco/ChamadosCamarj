import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

interface DashboardKpiProps {
  titulo: string
  valor: string | number
  subtexto?: string
}

export function DashboardKpi({ titulo, valor, subtexto }: DashboardKpiProps) {
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
        {subtexto && <p className="mt-1 text-xs text-muted-foreground">{subtexto}</p>}
      </CardContent>
    </Card>
  )
}
