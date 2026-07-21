import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group'
import { Label } from '@/components/ui/label'
import { useAlterarPrioridadeChamado } from '../hooks/useAcoesChamado'
import type { PrioridadeChamado } from '@/types/api'

interface AlterarPrioridadeModalProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  chamadoId: string
  prioridadeAtual: PrioridadeChamado
}

const PRIORIDADES: { value: PrioridadeChamado; label: string }[] = [
  { value: 'Baixa', label: 'Baixa' },
  { value: 'Media', label: 'Média' },
  { value: 'Alta', label: 'Alta' },
  { value: 'Urgente', label: 'Urgente' },
]

export function AlterarPrioridadeModal({
  open,
  onOpenChange,
  chamadoId,
  prioridadeAtual,
}: AlterarPrioridadeModalProps) {
  const [novaPrioridade, setNovaPrioridade] = useState<PrioridadeChamado>(prioridadeAtual)
  const { mutate, isPending, error, reset } = useAlterarPrioridadeChamado(chamadoId)

  const fechar = (proximoEstado: boolean) => {
    if (!proximoEstado) {
      setNovaPrioridade(prioridadeAtual)
      reset()
    }
    onOpenChange(proximoEstado)
  }

  const salvar = () => {
    mutate(novaPrioridade, { onSuccess: () => fechar(false) })
  }

  return (
    <Dialog open={open} onOpenChange={fechar}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Alterar prioridade</DialogTitle>
        </DialogHeader>

        <RadioGroup value={novaPrioridade} onValueChange={(valor) => setNovaPrioridade(valor as PrioridadeChamado)}>
          {PRIORIDADES.map((prioridade) => (
            <div key={prioridade.value} className="flex items-center gap-2">
              <RadioGroupItem value={prioridade.value} id={`prioridade-${prioridade.value}`} />
              <Label htmlFor={`prioridade-${prioridade.value}`}>{prioridade.label}</Label>
            </div>
          ))}
        </RadioGroup>

        {error && <p className="text-sm text-destructive">{error.message}</p>}

        <DialogFooter>
          <Button variant="outline" onClick={() => fechar(false)}>
            Cancelar
          </Button>
          <Button onClick={salvar} disabled={novaPrioridade === prioridadeAtual || isPending}>
            {isPending ? 'Salvando...' : 'Salvar'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
