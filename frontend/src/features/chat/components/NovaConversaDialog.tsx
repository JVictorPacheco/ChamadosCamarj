import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog'
import { useCriarConversa } from '../hooks/useChat'
import { usePresencas } from '../hooks/usePresencas'
import { useAuth } from '@/auth/AuthContext'
import { PresencaBadge } from './PresencaBadge'
import type { ChatPresencaResponse } from '@/types/api'

const ORDEM_STATUS: Record<string, number> = { Online: 0, Ausente: 1, Offline: 2 }

function ordenarPresencas(presencas: ChatPresencaResponse[]): ChatPresencaResponse[] {
  return [...presencas].sort((a, b) => {
    const ordemA = ORDEM_STATUS[a.status] ?? 3
    const ordemB = ORDEM_STATUS[b.status] ?? 3
    if (ordemA !== ordemB) return ordemA - ordemB
    return a.usuarioNome.localeCompare(b.usuarioNome, 'pt-BR')
  })
}

interface NovaConversaDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: (conversaId: string) => void
}

export function NovaConversaDialog({ open, onOpenChange, onSuccess }: NovaConversaDialogProps) {
  const { perfil } = useAuth()
  const [selecionadoId, setSelecionadoId] = useState<string | null>(null)
  const [erro, setErro] = useState<string | null>(null)

  const { data: presencas, isPending } = usePresencas()
  const criarConversa = useCriarConversa()

  const disponiveis = ordenarPresencas((presencas ?? []).filter((u) => u.usuarioId !== perfil?.id))

  const fechar = () => {
    setSelecionadoId(null)
    setErro(null)
    onOpenChange(false)
  }

  const confirmar = () => {
    if (!selecionadoId) {
      setErro('Selecione uma pessoa para conversar.')
      return
    }
    setErro(null)
    criarConversa.mutate(selecionadoId, {
      onSuccess: (conversa) => {
        onSuccess(conversa.id)
        fechar()
      },
      onError: (err) => setErro(err instanceof Error ? err.message : 'Erro ao iniciar conversa.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v) fechar() }}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Nova conversa</DialogTitle>
          <DialogDescription>Escolha uma pessoa para conversar em particular.</DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-4 py-2">
          <div className="max-h-72 overflow-y-auto rounded-md border border-border p-1 flex flex-col gap-0.5">
            {isPending && (
              <p className="py-2 text-center text-xs text-muted-foreground">Carregando...</p>
            )}
            {!isPending && disponiveis.length === 0 && (
              <p className="py-2 text-center text-xs text-muted-foreground">
                Nenhuma pessoa disponível.
              </p>
            )}
            {!isPending &&
              disponiveis.map((u) => (
                <button
                  key={u.usuarioId}
                  type="button"
                  onClick={() => setSelecionadoId(u.usuarioId)}
                  className={`flex items-center gap-2 rounded-md px-2 py-1.5 text-left text-sm ${
                    selecionadoId === u.usuarioId
                      ? 'bg-muted ring-1 ring-primary'
                      : 'hover:bg-muted'
                  }`}
                >
                  <div className="relative flex-shrink-0">
                    <div className="flex h-7 w-7 items-center justify-center rounded-full bg-muted text-xs font-medium uppercase text-muted-foreground">
                      {u.usuarioNome.charAt(0)}
                    </div>
                    <span className="absolute -right-0.5 -bottom-0.5">
                      <PresencaBadge status={u.status} size="sm" />
                    </span>
                  </div>
                  <span className="flex-1 truncate">{u.usuarioNome}</span>
                </button>
              ))}
          </div>

          {erro && (
            <Alert variant="destructive">
              <AlertDescription>{erro}</AlertDescription>
            </Alert>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={fechar}>
            Cancelar
          </Button>
          <Button onClick={confirmar} disabled={!selecionadoId || criarConversa.isPending}>
            {criarConversa.isPending ? 'Iniciando...' : 'Iniciar conversa'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
