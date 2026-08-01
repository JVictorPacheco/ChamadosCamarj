import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { Textarea } from '@/components/ui/textarea'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Label } from '@/components/ui/label'
import { useAuth } from '@/auth/AuthContext'
import { useComentar } from '../hooks/useComentar'
import { uploadAnexo } from '../api'
import { SeletorArquivosMultiplo } from './SeletorArquivosMultiplo'

interface ComentarioFormProps {
  chamadoId: string
  autor: string
  onUploadChange?: (uploading: boolean) => void
}

export function ComentarioForm({ chamadoId, autor, onUploadChange }: ComentarioFormProps) {
  const { perfil } = useAuth()
  const queryClient = useQueryClient()
  const [conteudo, setConteudo] = useState('')
  const [interno, setInterno] = useState(false)
  const [arquivos, setArquivos] = useState<File[]>([])
  const [enviandoAnexos, setEnviandoAnexos] = useState(false)
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
        onSuccess: async (comentario) => {
          setConteudo('')
          setInterno(false)

          if (arquivos.length === 0) return

          setEnviandoAnexos(true)
          onUploadChange?.(true)
          const resultados = await Promise.allSettled(
            arquivos.map((arquivo) => uploadAnexo(chamadoId, arquivo, comentario.id)),
          )
          setEnviandoAnexos(false)
          onUploadChange?.(false)
          setArquivos([])

          const falhas = resultados.filter((r) => r.status === 'rejected').length
          if (falhas > 0) {
            setErro(`Comentário enviado, mas ${falhas} de ${resultados.length} anexo(s) não foram enviados. Tente novamente na seção de Anexos.`)
          }
          queryClient.invalidateQueries({ queryKey: ['anexos', chamadoId] })
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
      <SeletorArquivosMultiplo arquivos={arquivos} onChange={setArquivos} disabled={isPending || enviandoAnexos} />
      {erro && <p className="text-sm text-destructive">{erro}</p>}
      <Button onClick={enviar} disabled={isPending || enviandoAnexos || !conteudo.trim()} className="self-end">
        {enviandoAnexos ? 'Enviando anexos...' : 'Comentar'}
      </Button>
    </div>
  )
}
