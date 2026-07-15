import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { ATENDENTES } from '@/auth/AuthContext'
import { useReatribuirChamado } from '../hooks/useAcoesChamado'

interface ReatribuirModalProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  chamadoId: string
  responsavelAtualId: string | null
}

export function ReatribuirModal({ open, onOpenChange, chamadoId, responsavelAtualId }: ReatribuirModalProps) {
  const [novoResponsavelId, setNovoResponsavelId] = useState('')
  const { mutate, isPending, error, reset } = useReatribuirChamado(chamadoId)

  const opcoes = ATENDENTES.filter((atendente) => atendente.id !== responsavelAtualId)

  const fechar = (proximoEstado: boolean) => {
    if (!proximoEstado) {
      setNovoResponsavelId('')
      reset()
    }
    onOpenChange(proximoEstado)
  }

  const reatribuir = () => {
    const novoResponsavel = opcoes.find((atendente) => atendente.id === novoResponsavelId)
    if (!novoResponsavel) return

    mutate(
      { novoResponsavelId: novoResponsavel.id, novoResponsavelNome: novoResponsavel.nome },
      { onSuccess: () => fechar(false) },
    )
  }

  return (
    <Dialog open={open} onOpenChange={fechar}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Reatribuir chamado</DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-3">
          <Select value={novoResponsavelId} onValueChange={setNovoResponsavelId}>
            <SelectTrigger>
              <SelectValue placeholder="Selecione um atendente" />
            </SelectTrigger>
            <SelectContent>
              {opcoes.map((atendente) => (
                <SelectItem key={atendente.id} value={atendente.id}>
                  {atendente.nome}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          {error && <p className="text-sm text-destructive">{error.message}</p>}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => fechar(false)}>
            Cancelar
          </Button>
          <Button onClick={reatribuir} disabled={!novoResponsavelId || isPending}>
            {isPending ? 'Reatribuindo...' : 'Reatribuir'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
