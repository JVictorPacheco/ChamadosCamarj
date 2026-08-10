import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from 'recharts'

export interface DonutChartItem {
  label: string
  value: number
  color: string
}

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

interface DonutChartProps {
  data: DonutChartItem[]
  onSliceClick?: (label: string) => void
}

export function DonutChart({ data, onSliceClick }: DonutChartProps) {
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
          onClick={onSliceClick ? (_data, index) => onSliceClick(data[index].label) : undefined}
          className={onSliceClick ? 'cursor-pointer' : undefined}
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
