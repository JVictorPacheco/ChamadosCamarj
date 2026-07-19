import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { useForcarEncerramentoChamado } from '../hooks/useAcoesChamado'

const MOTIVO_MIN = 10
const MOTIVO_MAX = 500

interface ForcarEncerramentoModalProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  chamadoId: string
}

export function ForcarEncerramentoModal({ open, onOpenChange, chamadoId }: ForcarEncerramentoModalProps) {
  const [motivo, setMotivo] = useState('')
  const { mutate, isPending, error, reset } = useForcarEncerramentoChamado(chamadoId)

  const fechar = (proximoEstado: boolean) => {
    if (!proximoEstado) {
      setMotivo('')
      reset()
    }
    onOpenChange(proximoEstado)
  }

  const confirmar = () => {
    mutate(motivo.trim(), { onSuccess: () => fechar(false) })
  }

  const motivoValido = motivo.trim().length >= MOTIVO_MIN && motivo.length <= MOTIVO_MAX

  return (
    <Dialog open={open} onOpenChange={fechar}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Forçar encerramento</DialogTitle>
          <DialogDescription>
            Fecha o chamado imediatamente, fora do fluxo normal. Essa ação é registrada no histórico do chamado.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-1">
          <Textarea
            value={motivo}
            onChange={(e) => setMotivo(e.target.value)}
            placeholder="Explique por que este chamado está sendo encerrado fora do fluxo normal..."
            maxLength={MOTIVO_MAX}
            rows={4}
          />
          <p className="text-xs text-muted-foreground text-right">
            {motivo.length}/{MOTIVO_MAX} (mínimo {MOTIVO_MIN})
          </p>
        </div>

        {error && <p className="text-sm text-destructive">{error.message}</p>}

        <DialogFooter>
          <Button variant="outline" onClick={() => fechar(false)}>
            Cancelar
          </Button>
          <Button variant="destructive" onClick={confirmar} disabled={!motivoValido || isPending}>
            {isPending ? 'Encerrando...' : 'Forçar Encerramento'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
