import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from '@/components/ui/dialog'
import { useAuth } from '@/auth/AuthContext'
import { useConversaDetalhe } from '../hooks/useConversas'
import { usePresencas } from '../hooks/usePresencas'
import { useAdicionarParticipante, useRemoverParticipante } from '../hooks/useChat'
import { PresencaBadge } from './PresencaBadge'
import { UserPlus, UserMinus, X } from 'lucide-react'
import type { StatusPresenca } from '@/types/api'

interface MembrosGrupoDialogProps {
  conversaId: string
  open: boolean
  onOpenChange: (open: boolean) => void
  onIniciarConversa: (usuarioId: string) => void
}

export function MembrosGrupoDialog({ conversaId, open, onOpenChange, onIniciarConversa }: MembrosGrupoDialogProps) {
  const { perfil } = useAuth()
  const [mostrarAdicionar, setMostrarAdicionar] = useState(false)
  const [erro, setErro] = useState<string | null>(null)

  const { data: detalhe, isPending } = useConversaDetalhe(conversaId)
  const { data: presencas } = usePresencas()
  const adicionar = useAdicionarParticipante(conversaId)
  const remover = useRemoverParticipante(conversaId)

  const souGerente = !!perfil && !!detalhe && (perfil.id === detalhe.criadoPorId || perfil.tipo === 'Admin')

  const statusDe = (usuarioId: string): StatusPresenca =>
    presencas?.find((p) => p.usuarioId === usuarioId)?.status ?? 'Offline'

  const membros = detalhe?.participantes ?? []
  const candidatos = (presencas ?? []).filter((p) => !membros.some((m) => m.usuarioId === p.usuarioId))

  const irParaConversa = (usuarioId: string) => {
    if (usuarioId === perfil?.id) return
    onIniciarConversa(usuarioId)
    onOpenChange(false)
  }

  const handleRemover = (usuarioId: string) => {
    setErro(null)
    remover.mutate(usuarioId, {
      onError: (err) => setErro(err instanceof Error ? err.message : 'Erro ao remover participante.'),
    })
  }

  const handleAdicionar = (usuarioId: string) => {
    setErro(null)
    adicionar.mutate(usuarioId, {
      onSuccess: () => setMostrarAdicionar(false),
      onError: (err) => setErro(err instanceof Error ? err.message : 'Erro ao adicionar participante.'),
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Membros do grupo</DialogTitle>
          <DialogDescription>
            {souGerente
              ? 'Clique numa pessoa pra conversar direto com ela, ou gerencie quem participa do grupo.'
              : 'Clique numa pessoa pra conversar direto com ela.'}
          </DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-3 py-2">
          {isPending && <p className="py-2 text-center text-xs text-muted-foreground">Carregando...</p>}

          {!isPending && (
            <div className="max-h-64 overflow-y-auto rounded-md border border-border p-1 flex flex-col gap-0.5">
              {membros.map((m) => {
                const souEu = m.usuarioId === perfil?.id
                return (
                  <div
                    key={m.usuarioId}
                    className="flex items-center gap-2 rounded-md px-2 py-1.5 text-sm hover:bg-muted transition-colors"
                  >
                    <button
                      type="button"
                      onClick={() => irParaConversa(m.usuarioId)}
                      disabled={souEu}
                      className="flex flex-1 items-center gap-2 text-left disabled:pointer-events-none"
                    >
                      <div className="relative flex-shrink-0">
                        <div className="flex h-7 w-7 items-center justify-center rounded-full bg-muted text-xs font-medium uppercase text-muted-foreground">
                          {m.usuarioNome.charAt(0)}
                        </div>
                        <span className="absolute -right-0.5 -bottom-0.5">
                          <PresencaBadge status={statusDe(m.usuarioId)} size="sm" />
                        </span>
                      </div>
                      <span className="flex-1 truncate">
                        {m.usuarioNome}
                        {souEu && <span className="text-xs text-muted-foreground"> (você)</span>}
                        {m.usuarioId === detalhe?.criadoPorId && (
                          <span className="text-xs text-muted-foreground"> · criador</span>
                        )}
                      </span>
                    </button>
                    {souGerente && !souEu && (
                      <button
                        type="button"
                        onClick={() => handleRemover(m.usuarioId)}
                        disabled={remover.isPending}
                        title="Remover do grupo"
                        className="flex-shrink-0 rounded p-1 text-muted-foreground hover:bg-destructive/10 hover:text-destructive transition-colors"
                      >
                        <UserMinus className="h-4 w-4" />
                      </button>
                    )}
                  </div>
                )
              })}
            </div>
          )}

          {erro && (
            <Alert variant="destructive">
              <AlertDescription>{erro}</AlertDescription>
            </Alert>
          )}

          {souGerente && !mostrarAdicionar && (
            <Button type="button" variant="outline" size="sm" onClick={() => setMostrarAdicionar(true)}>
              <UserPlus className="mr-1.5 h-4 w-4" />
              Adicionar participante
            </Button>
          )}

          {souGerente && mostrarAdicionar && (
            <div className="flex flex-col gap-1.5">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-muted-foreground">Escolha quem adicionar</span>
                <button
                  type="button"
                  onClick={() => setMostrarAdicionar(false)}
                  className="rounded p-0.5 hover:bg-muted-foreground/20"
                >
                  <X className="h-3.5 w-3.5 text-muted-foreground" />
                </button>
              </div>
              <div className="max-h-48 overflow-y-auto rounded-md border border-border p-1 flex flex-col gap-0.5">
                {candidatos.length === 0 && (
                  <p className="py-2 text-center text-xs text-muted-foreground">
                    Ninguém disponível pra adicionar.
                  </p>
                )}
                {candidatos.map((c) => (
                  <button
                    key={c.usuarioId}
                    type="button"
                    onClick={() => handleAdicionar(c.usuarioId)}
                    disabled={adicionar.isPending}
                    className="flex items-center gap-2 rounded-md px-2 py-1.5 text-left text-sm hover:bg-muted transition-colors"
                  >
                    <div className="relative flex-shrink-0">
                      <div className="flex h-7 w-7 items-center justify-center rounded-full bg-muted text-xs font-medium uppercase text-muted-foreground">
                        {c.usuarioNome.charAt(0)}
                      </div>
                      <span className="absolute -right-0.5 -bottom-0.5">
                        <PresencaBadge status={c.status} size="sm" />
                      </span>
                    </div>
                    <span className="flex-1 truncate">{c.usuarioNome}</span>
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  )
}
