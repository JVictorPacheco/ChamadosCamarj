import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Alert, AlertDescription } from '@/components/ui/alert'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog'
import { Checkbox } from '@/components/ui/checkbox'
import { useAuth } from '@/auth/AuthContext'
import { useCriarGrupo } from '../hooks/useChat'
import { usePresencas } from '../hooks/usePresencas'
import { PresencaBadge } from './PresencaBadge'

interface CriarGrupoDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSuccess: (conversaId: string) => void
}

export function CriarGrupoDialog({ open, onOpenChange, onSuccess }: CriarGrupoDialogProps) {
  const { perfil } = useAuth()
  const [nome, setNome] = useState('')
  const [participantesSelecionados, setParticipantesSelecionados] = useState<Set<string>>(new Set())
  const [erro, setErro] = useState<string | null>(null)

  // /chat/presencas já retorna só quem tem acesso ao chat — ao contrário de /api/usuarios
  // (era o que este diálogo usava antes), não exige perfil Admin pra ser consultado.
  const { data: presencas } = usePresencas()
  const criarGrupo = useCriarGrupo()

  const participantesDisponiveis = (presencas ?? []).filter((u) => u.usuarioId !== perfil?.id)

  const toggleParticipante = (id: string) => {
    setParticipantesSelecionados((prev) => {
      const novo = new Set(prev)
      if (novo.has(id)) {
        novo.delete(id)
      } else {
        novo.add(id)
      }
      return novo
    })
  }

  const fechar = () => {
    setNome('')
    setParticipantesSelecionados(new Set())
    setErro(null)
    onOpenChange(false)
  }

  const confirmar = () => {
    if (!nome.trim()) {
      setErro('O nome do grupo é obrigatório.')
      return
    }
    if (participantesSelecionados.size < 2) {
      setErro('Selecione ao menos 2 participantes além de você.')
      return
    }
    setErro(null)

    criarGrupo.mutate(
      {
        nome: nome.trim(),
        participanteIds: Array.from(participantesSelecionados),
      },
      {
        onSuccess: (conversa) => {
          onSuccess(conversa.id)
          fechar()
        },
        onError: (err) => setErro(err instanceof Error ? err.message : 'Erro ao criar grupo.'),
      }
    )
  }

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v) fechar() }}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Novo grupo</DialogTitle>
          <DialogDescription>
            Crie um grupo de chat. Selecione ao menos 2 participantes.
          </DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-4 py-2">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="nome-grupo">Nome do grupo</Label>
            <Input
              id="nome-grupo"
              value={nome}
              onChange={(e) => setNome(e.target.value)}
              placeholder="Ex: Equipe de Atendimento"
              maxLength={100}
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <Label>Participantes</Label>
            <div className="max-h-48 overflow-y-auto rounded-md border border-border p-2 flex flex-col gap-1">
              {participantesDisponiveis.length === 0 && (
                <p className="text-xs text-muted-foreground py-2 text-center">
                  Nenhum usuário com acesso ao chat disponível.
                </p>
              )}
              {participantesDisponiveis.map((u) => (
                <label
                  key={u.usuarioId}
                  className="flex items-center gap-2 rounded-md px-2 py-1.5 hover:bg-muted cursor-pointer"
                >
                  <Checkbox
                    checked={participantesSelecionados.has(u.usuarioId)}
                    onCheckedChange={() => toggleParticipante(u.usuarioId)}
                  />
                  <PresencaBadge status={u.status} size="sm" />
                  <span className="flex-1 text-sm">{u.usuarioNome}</span>
                </label>
              ))}
            </div>
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
          <Button
            onClick={confirmar}
            disabled={criarGrupo.isPending}
          >
            {criarGrupo.isPending ? 'Criando...' : 'Criar grupo'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
