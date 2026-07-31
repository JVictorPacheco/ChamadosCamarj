import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { useForcarEncerramentoChamado } from '../hooks/useAcoesChamado'
import type { MotivoEncerramento } from '@/types/api'

const MOTIVO_LABELS: Record<MotivoEncerramento, string> = {
  Resolvido: 'Resolvido',
  CanceladoSolicitante: 'Cancelado pelo solicitante',
  AbertoIndevidamente: 'Aberto indevidamente',
  Duplicata: 'Duplicata',
  SemResposta: 'Sem resposta do solicitante',
  Outro: 'Outro',
}

const MOTIVO_OUTRO_MIN = 5

interface ForcarEncerramentoModalProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  chamadoId: string
}

export function ForcarEncerramentoModal({ open, onOpenChange, chamadoId }: ForcarEncerramentoModalProps) {
  const [motivo, setMotivo] = useState<MotivoEncerramento>('AbertoIndevidamente')
  const [motivoOutro, setMotivoOutro] = useState('')
  const [observacao, setObservacao] = useState('')
  const { mutate, isPending, error, reset } = useForcarEncerramentoChamado(chamadoId)

  const fechar = (proximoEstado: boolean) => {
    if (!proximoEstado) {
      setMotivo('AbertoIndevidamente')
      setMotivoOutro('')
      setObservacao('')
      reset()
    }
    onOpenChange(proximoEstado)
  }

  const confirmar = () => {
    mutate({ motivo, motivoOutro: motivoOutro.trim() || undefined, observacao: observacao.trim() || undefined }, { onSuccess: () => fechar(false) })
  }

  const motivoValido = motivo !== 'Outro' || motivoOutro.trim().length >= MOTIVO_OUTRO_MIN

  return (
    <Dialog open={open} onOpenChange={fechar}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Forçar encerramento</DialogTitle>
          <DialogDescription>
            Fecha o chamado imediatamente, fora do fluxo normal. Essa ação é registrada no histórico e nos comentários do chamado.
          </DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-3">
          <div className="flex flex-col gap-1">
            <Label htmlFor="forcar-motivo" className="text-sm">
              Motivo do encerramento
            </Label>
            <Select value={motivo} onValueChange={(v) => setMotivo(v as MotivoEncerramento)}>
              <SelectTrigger id="forcar-motivo">
                <SelectValue placeholder="Selecione o motivo" />
              </SelectTrigger>
              <SelectContent>
                {(Object.keys(MOTIVO_LABELS) as MotivoEncerramento[]).map((m) => (
                  <SelectItem key={m} value={m}>
                    {MOTIVO_LABELS[m]}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {motivo === 'Outro' && (
            <div className="flex flex-col gap-1">
              <Label htmlFor="forcar-motivo-outro" className="text-sm">
                Descreva o motivo (mínimo {MOTIVO_OUTRO_MIN} caracteres)
              </Label>
              <Input
                id="forcar-motivo-outro"
                value={motivoOutro}
                onChange={(e) => setMotivoOutro(e.target.value)}
                placeholder="Ex: Chamado aberto por engano"
              />
            </div>
          )}
          <div className="flex flex-col gap-1">
            <Label htmlFor="forcar-observacao" className="text-sm">
              Comentário (opcional)
            </Label>
            <Textarea
              id="forcar-observacao"
              value={observacao}
              onChange={(e) => setObservacao(e.target.value)}
              placeholder="Escreva uma observação sobre o encerramento..."
              rows={4}
            />
          </div>
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