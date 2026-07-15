import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from 'recharts'

export interface DonutChartItem {
  label: string
  value: number
  color: string
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
