import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'

export interface BarraData {
  nome: string
  quantidade: number
  id?: string | null
}

interface BarrasChartProps {
  data: BarraData[]
  onBarClick?: (item: BarraData) => void
}

export function BarrasChart({ data, onBarClick }: BarrasChartProps) {
  return (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart data={data} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
        <XAxis dataKey="nome" tick={{ fontSize: 11 }} />
        <YAxis tick={{ fontSize: 12 }} allowDecimals={false} />
        <Tooltip />
        <Bar
          dataKey="quantidade"
          fill="var(--chart-1)"
          radius={[4, 4, 0, 0]}
          onClick={onBarClick ? (_data, index) => onBarClick(data[index]) : undefined}
          className={onBarClick ? 'cursor-pointer' : undefined}
        />
      </BarChart>
    </ResponsiveContainer>
  )
}
