import { useState } from 'react'
import { Label } from '@/components/ui/label'
import { validarArquivo } from '../lib/anexoValidacao'

interface SeletorArquivosMultiploProps {
  arquivos: File[]
  onChange: (arquivos: File[]) => void
  disabled?: boolean
}

export function SeletorArquivosMultiplo({ arquivos, onChange, disabled }: SeletorArquivosMultiploProps) {
  const [erro, setErro] = useState<string | null>(null)

  const adicionarArquivos = (e: React.ChangeEvent<HTMLInputElement>) => {
    const novos = Array.from(e.target.files ?? [])
    e.target.value = ''
    if (novos.length === 0) return

    for (const arquivo of novos) {
      const erroValidacao = validarArquivo(arquivo)
      if (erroValidacao) {
        setErro(`${arquivo.name}: ${erroValidacao}`)
        return
      }
    }

    setErro(null)
    onChange([...arquivos, ...novos])
  }

  const remover = (indice: number) => {
    onChange(arquivos.filter((_, i) => i !== indice))
  }

  return (
    <div className="flex flex-col gap-2">
      <Label>Anexos (opcional)</Label>
      <input
        type="file"
        multiple
        onChange={adicionarArquivos}
        disabled={disabled}
        className="text-sm text-muted-foreground file:mr-3 file:rounded-md file:border-0 file:bg-secondary file:px-3 file:py-1.5 file:text-sm file:text-secondary-foreground"
      />
      {erro && <p className="text-sm text-destructive">{erro}</p>}
      {arquivos.length > 0 && (
        <ul className="flex flex-col gap-1">
          {arquivos.map((arquivo, indice) => (
            <li
              key={`${arquivo.name}-${indice}`}
              className="flex items-center justify-between gap-2 rounded-md border border-border px-2 py-1 text-sm"
            >
              <span className="truncate">{arquivo.name}</span>
              <button
                type="button"
                onClick={() => remover(indice)}
                disabled={disabled}
                className="text-muted-foreground hover:text-destructive"
                aria-label={`Remover ${arquivo.name}`}
              >
                ✕
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
