import { useState } from 'react'
import { Textarea } from '@/components/ui/textarea'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Label } from '@/components/ui/label'
import { useAuth } from '@/auth/AuthContext'
import { useComentar } from '../hooks/useComentar'

interface ComentarioFormProps {
  chamadoId: string
  autor: string
}

export function ComentarioForm({ chamadoId, autor }: ComentarioFormProps) {
  const { perfil } = useAuth()
  const [conteudo, setConteudo] = useState('')
  const [interno, setInterno] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const { mutate, isPending } = useComentar(chamadoId)

  const podeMarcarInterno = perfil?.tipo === 'Admin' || perfil?.tipo === 'Atendente'

  const enviar = () => {
    if (!conteudo.trim()) {
      return
    }
    setErro(null)
    mutate(
      { autor, conteudo, interno: podeMarcarInterno && interno },
      {
        onSuccess: () => {
          setConteudo('')
          setInterno(false)
        },
        onError: () => setErro('Não foi possível enviar o comentário. Tente novamente.'),
      },
    )
  }

  return (
    <div className="flex flex-col gap-2">
      <Textarea
        value={conteudo}
        onChange={(e) => setConteudo(e.target.value)}
        placeholder="Escreva um comentário..."
        disabled={isPending}
      />
      {podeMarcarInterno && (
        <div className="flex items-center gap-2">
          <Checkbox
            id="comentario-interno"
            checked={interno}
            onCheckedChange={(checked) => setInterno(checked === true)}
            disabled={isPending}
          />
          <Label htmlFor="comentario-interno" className="text-sm font-normal text-muted-foreground">
            Comentário interno (só atendentes veem)
          </Label>
        </div>
      )}
      {erro && <p className="text-sm text-destructive">{erro}</p>}
      <Button onClick={enviar} disabled={isPending || !conteudo.trim()} className="self-end">
        Comentar
      </Button>
    </div>
  )
}
