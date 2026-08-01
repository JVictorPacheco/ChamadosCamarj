import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from '@/components/ui/button'

const NOMES_MES = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

interface SeletorMesProps {
  ano: number
  mes: number
  onChange: (ano: number, mes: number) => void
}

export function SeletorMes({ ano, mes, onChange }: SeletorMesProps) {
  const agora = new Date()
  const ehMesCorrente = ano === agora.getFullYear() && mes === agora.getMonth() + 1

  const anterior = () => {
    if (mes === 1) {
      onChange(ano - 1, 12)
    } else {
      onChange(ano, mes - 1)
    }
  }

  const proximo = () => {
    if (ehMesCorrente) return
    if (mes === 12) {
      onChange(ano + 1, 1)
    } else {
      onChange(ano, mes + 1)
    }
  }

  return (
    <div className="flex items-center gap-2">
      <Button variant="outline" size="icon-sm" onClick={anterior} aria-label="Mês anterior">
        <ChevronLeft className="h-4 w-4" />
      </Button>
      <span className="min-w-36 text-center text-sm font-medium">
        {NOMES_MES[mes - 1]} {ano}
      </span>
      <Button
        variant="outline"
        size="icon-sm"
        onClick={proximo}
        disabled={ehMesCorrente}
        aria-label="Próximo mês"
      >
        <ChevronRight className="h-4 w-4" />
      </Button>
    </div>
  )
}
