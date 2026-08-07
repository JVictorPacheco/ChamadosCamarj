import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'

interface CategoriaData {
  categoriaNome: string
  quantidade: number
  categoriaId?: string | null
}

interface CategoriaChartProps {
  data: CategoriaData[]
  onBarClick?: (item: CategoriaData) => void
}

export function CategoriaChart({ data, onBarClick }: CategoriaChartProps) {
  return (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart data={data} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
        <XAxis dataKey="categoriaNome" tick={{ fontSize: 11 }} />
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
