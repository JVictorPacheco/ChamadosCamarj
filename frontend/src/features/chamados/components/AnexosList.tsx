import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { useAnexos } from '../hooks/useAnexos'
import { obterUrlDownloadAnexo } from '../api'

function formatarTamanho(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function LinhaAnexo({ chamadoId, anexoId, nomeArquivo, tamanhoBytes, enviadoPorNome }: {
  chamadoId: string
  anexoId: string
  nomeArquivo: string
  tamanhoBytes: number
  enviadoPorNome: string
}) {
  const [baixando, setBaixando] = useState(false)
  const [erro, setErro] = useState<string | null>(null)

  const baixar = async () => {
    setBaixando(true)
    setErro(null)
    try {
      const { url } = await obterUrlDownloadAnexo(chamadoId, anexoId)
      window.open(url, '_blank')
    } catch {
      setErro('Não foi possível gerar o link de download.')
    } finally {
      setBaixando(false)
    }
  }

  return (
    <li className="flex flex-col gap-1">
      <div className="flex items-center justify-between gap-2 rounded-lg border border-border p-2">
        <div className="flex flex-col text-sm">
          <span className="font-medium text-foreground">{nomeArquivo}</span>
          <span className="text-xs text-muted-foreground">
            {formatarTamanho(tamanhoBytes)} · enviado por {enviadoPorNome}
          </span>
        </div>
        <Button size="sm" variant="outline" onClick={baixar} disabled={baixando}>
          {baixando ? 'Gerando link...' : 'Baixar'}
        </Button>
      </div>
      {erro && <p className="text-sm text-destructive">{erro}</p>}
    </li>
  )
}

export function AnexosList({ chamadoId }: { chamadoId: string }) {
  const { data: anexos, isPending } = useAnexos(chamadoId)

  if (isPending) {
    return <p className="text-sm text-muted-foreground">Carregando anexos...</p>
  }

  if (!anexos || anexos.length === 0) {
    return <p className="text-sm text-muted-foreground">Nenhum anexo.</p>
  }

  return (
    <ul className="flex flex-col gap-2">
      {anexos.map((anexo) => (
        <LinhaAnexo
          key={anexo.id}
          chamadoId={chamadoId}
          anexoId={anexo.id}
          nomeArquivo={anexo.nomeArquivo}
          tamanhoBytes={anexo.tamanhoBytes}
          enviadoPorNome={anexo.enviadoPorNome}
        />
      ))}
    </ul>
  )
}
