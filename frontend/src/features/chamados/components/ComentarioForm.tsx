import { useState } from 'react'
import { Textarea } from '@/components/ui/textarea'
import { Button } from '@/components/ui/button'
import { useComentar } from '../hooks/useComentar'

interface ComentarioFormProps {
  chamadoId: string
  autor: string
}

export function ComentarioForm({ chamadoId, autor }: ComentarioFormProps) {
  const [conteudo, setConteudo] = useState('')
  const [erro, setErro] = useState<string | null>(null)
  const { mutate, isPending } = useComentar(chamadoId)

  const enviar = () => {
    if (!conteudo.trim()) {
      return
    }
    setErro(null)
    mutate(
      { autor, conteudo, interno: false },
      {
        onSuccess: () => setConteudo(''),
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
      {erro && <p className="text-sm text-destructive">{erro}</p>}
      <Button onClick={enviar} disabled={isPending || !conteudo.trim()} className="self-end">
        Comentar
      </Button>
    </div>
  )
}
