import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts'
import type { TendenciaItem } from '@/types/dashboard'

export function TendenciaChart({ data }: { data: TendenciaItem[] }) {
  return (
    <ResponsiveContainer width="100%" height={300}>
      <LineChart data={data} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
        <XAxis
          dataKey="data"
          tick={{ fontSize: 12 }}
          tickFormatter={(v: string) => {
            const [_, mes, dia] = v.split('-')
            return `${dia}/${mes}`
          }}
        />
        <YAxis tick={{ fontSize: 12 }} allowDecimals={false} />
        <Tooltip />
        <Legend />
        <Line type="monotone" dataKey="abertos" stroke="#ef4444" name="Abertos" strokeWidth={2} dot={{ r: 4 }} />
        <Line type="monotone" dataKey="resolvidos" stroke="#22c55e" name="Resolvidos" strokeWidth={2} dot={{ r: 4 }} />
      </LineChart>
    </ResponsiveContainer>
  )
}
