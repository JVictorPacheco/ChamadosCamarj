import { useRef, useState } from 'react'
import { useUploadAnexo } from '../hooks/useAnexos'
import { validarArquivo } from '../lib/anexoValidacao'

export function UploadAnexoForm({ chamadoId }: { chamadoId: string }) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [erro, setErro] = useState<string | null>(null)
  const { mutate, isPending } = useUploadAnexo(chamadoId)

  const selecionarArquivo = (e: React.ChangeEvent<HTMLInputElement>) => {
    const arquivo = e.target.files?.[0]
    if (!arquivo) return

    const erroValidacao = validarArquivo(arquivo)
    if (erroValidacao) {
      setErro(erroValidacao)
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
