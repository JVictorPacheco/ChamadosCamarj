import { useRef, useState } from 'react'
import { useUploadAnexo } from '../hooks/useAnexos'

const TAMANHO_MAXIMO_BYTES = 10 * 1024 * 1024
const EXTENSOES_PERMITIDAS = ['.pdf', '.jpg', '.jpeg', '.png', '.gif', '.doc', '.docx', '.xls', '.xlsx', '.zip']

function extensaoPermitida(nomeArquivo: string): boolean {
  const extensao = nomeArquivo.slice(nomeArquivo.lastIndexOf('.')).toLowerCase()
  return EXTENSOES_PERMITIDAS.includes(extensao)
}

export function UploadAnexoForm({ chamadoId }: { chamadoId: string }) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [erro, setErro] = useState<string | null>(null)
  const { mutate, isPending } = useUploadAnexo(chamadoId)

  const selecionarArquivo = (e: React.ChangeEvent<HTMLInputElement>) => {
    const arquivo = e.target.files?.[0]
    if (!arquivo) return

    if (arquivo.size > TAMANHO_MAXIMO_BYTES) {
      setErro('Arquivo excede o tamanho máximo de 10MB.')
      e.target.value = ''
      return
    }

    if (!extensaoPermitida(arquivo.name)) {
      setErro('Tipo de arquivo não permitido. Tipos aceitos: PDF, imagens, Word, Excel, ZIP.')
      e.target.value = ''
      return
    }

    setErro(null)
    mutate(arquivo, {
      onError: () => setErro('Não foi possível enviar o arquivo. Tente novamente.'),
      onSuccess: () => {
        if (inputRef.current) inputRef.current.value = ''
      },
    })
  }

  return (
    <div className="flex flex-col gap-2">
      <input
        ref={inputRef}
        type="file"
        onChange={selecionarArquivo}
        disabled={isPending}
        className="text-sm text-muted-foreground file:mr-3 file:rounded-md file:border-0 file:bg-secondary file:px-3 file:py-1.5 file:text-sm file:text-secondary-foreground"
      />
      {isPending && <p className="text-sm text-muted-foreground">Enviando...</p>}
      {erro && <p className="text-sm text-destructive">{erro}</p>}
    </div>
  )
}
