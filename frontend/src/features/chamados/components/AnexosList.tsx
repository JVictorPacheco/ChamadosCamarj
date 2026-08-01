import { useState } from 'react'
import { Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter, DialogDescription } from '@/components/ui/dialog'
import { useAuth } from '@/auth/AuthContext'
import { ApiError } from '@/lib/api'
import { useAnexos, useRemoverAnexo } from '../hooks/useAnexos'
import { obterUrlDownloadAnexo } from '../api'

function formatarTamanho(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function LinhaAnexo({ chamadoId, anexoId, nomeArquivo, tamanhoBytes, enviadoPorId, enviadoPorNome }: {
  chamadoId: string
  anexoId: string
  nomeArquivo: string
  tamanhoBytes: number
  enviadoPorId: string | null
  enviadoPorNome: string
}) {
  const { perfil } = useAuth()
  const [baixando, setBaixando] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const [confirmarRemocaoAberto, setConfirmarRemocaoAberto] = useState(false)
  const { mutate: remover, isPending: removendo } = useRemoverAnexo(chamadoId)

  const podeRemover = perfil?.tipo === 'Admin' || (!!perfil && perfil.id === enviadoPorId)

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

  const confirmarRemocao = () => {
    setErro(null)
    remover(anexoId, {
      onSuccess: () => setConfirmarRemocaoAberto(false),
      onError: (err) => {
        const proibido = err instanceof ApiError && err.status === 403
        setErro(proibido ? 'Você só pode remover anexos que você mesmo enviou.' : 'Não foi possível remover o anexo. Tente novamente.')
      },
    })
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
        <div className="flex gap-2">
          <Button size="sm" variant="outline" onClick={baixar} disabled={baixando}>
            {baixando ? 'Gerando link...' : 'Baixar'}
          </Button>
          {podeRemover && (
            <Button size="sm" variant="destructive" onClick={() => setConfirmarRemocaoAberto(true)}>
              Remover
            </Button>
          )}
        </div>
      </div>
      {erro && <p className="text-sm text-destructive">{erro}</p>}

      <Dialog open={confirmarRemocaoAberto} onOpenChange={setConfirmarRemocaoAberto}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Remover anexo</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja excluir o anexo <strong>{nomeArquivo}</strong>? Essa ação não pode ser desfeita.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirmarRemocaoAberto(false)} disabled={removendo}>
              Cancelar
            </Button>
            <Button variant="destructive" onClick={confirmarRemocao} disabled={removendo}>
              {removendo ? 'Removendo...' : 'Remover'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </li>
  )
}

export function AnexosList({ chamadoId }: { chamadoId: string }) {
  const { data: anexos, isPending, isFetching } = useAnexos(chamadoId)

  if (isPending) {
    return (
      <section className="space-y-4">
        <h2 className="text-xl font-heading">Anexos</h2>
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <Loader2 className="h-4 w-4 animate-spin" />
          Carregando...
        </div>
      </section>
    )
  }

  if (!anexos || anexos.length === 0) {
    return null
  }

  return (
    <section className="space-y-4">
      <div className="flex items-center gap-2">
        <h2 className="text-xl font-heading">Anexos</h2>
        {isFetching && <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />}
      </div>
      <ul className="flex flex-col gap-2">
        {anexos.map((anexo) => (
          <LinhaAnexo
            key={anexo.id}
            chamadoId={chamadoId}
            anexoId={anexo.id}
            nomeArquivo={anexo.nomeArquivo}
            tamanhoBytes={anexo.tamanhoBytes}
            enviadoPorId={anexo.enviadoPorId}
            enviadoPorNome={anexo.enviadoPorNome}
          />
        ))}
      </ul>
    </section>
  )
}
