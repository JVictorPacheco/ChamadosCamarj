import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from 'recharts'

export interface DonutChartItem {
  label: string
  value: number
  color: string
}

// Labels diretos nas fatias: sem isso, o valor só aparece no hover do Tooltip,
// e o Relatório Mensal exportado em PDF (via impressão) não tem hover.
function renderValueLabel(props: unknown) {
  const { value, x, y, textAnchor } = props as {
    value: number
    x: number
    y: number
    textAnchor: 'inherit' | 'end' | 'middle' | 'start'
  }
  if (!value) return null
  return (
    <text x={x} y={y} textAnchor={textAnchor} dominantBaseline="central" className="fill-foreground text-xs font-medium">
      {value}
    </text>
  )
}

export function DonutChart({ data }: { data: DonutChartItem[] }) {
  return (
    <ResponsiveContainer width="100%" height={260}>
      <PieChart>
        <Pie
          data={data}
          dataKey="value"
          nameKey="label"
          innerRadius={60}
          outerRadius={90}
          paddingAngle={2}
          label={renderValueLabel}
          labelLine={false}
          isAnimationActive={false}
        >
          {data.map((item) => (
            <Cell key={item.label} fill={item.color} />
          ))}
        </Pie>
        <Tooltip />
        <Legend />
      </PieChart>
    </ResponsiveContainer>
  )
}
