import { useEffect, useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { ApiError } from '@/lib/api'
import { useAtualizarChamado } from '../hooks/useAcoesChamado'
import type { ChamadoResponse } from '@/types/api'

// Mesmos limites da abertura e do servidor (spec editar-chamado AC-13).
const MAX_TITULO = 200
const MAX_DESCRICAO = 5000

interface EditarChamadoModalProps {
  /** Chamado exibido no detalhe — se recarregar com o modal aberto, chega aqui atualizado. */
  chamado: ChamadoResponse
  onClose: () => void
}

/**
 * Modal "Editar" (spec editar-chamado). Montado só enquanto aberto: cada abertura começa com o texto
 * e a versão atuais do chamado (AC-12, AC-14).
 */
export function EditarChamadoModal({ chamado, onClose }: EditarChamadoModalProps) {
  const [original] = useState({ titulo: chamado.titulo, descricao: chamado.descricao })
  const [titulo, setTitulo] = useState(chamado.titulo)
  const [descricao, setDescricao] = useState(chamado.descricao)
  const [versao, setVersao] = useState(chamado.versao)
  const [conflito, setConflito] = useState(false)
  // Depois de um 409, espera a recarga que ele dispara para adotar a versão nova — uma vez só (review R-01).
  const [aguardandoRecarga, setAguardandoRecarga] = useState(false)
  const { mutate, isPending, error } = useAtualizarChamado(chamado.id, versao)

  // Conflito (AC-19/AC-20): o texto digitado fica; quando o chamado termina de recarregar, o modal
  // passa a usar a versão nova — a pessoa já viu o aviso e o "Texto atual no chamado". Só a versão
  // dessa recarga é adotada; uma alteração posterior volta a ser detectada como conflito (review R-01).
  useEffect(() => {
    if (aguardandoRecarga && chamado.versao !== versao) {
      setVersao(chamado.versao)
      setAguardandoRecarga(false)
    }
  }, [aguardandoRecarga, chamado.versao, versao])

  const erroTitulo = !titulo.trim()
    ? 'Título é obrigatório.'
    : titulo.length > MAX_TITULO ? `Título deve ter no máximo ${MAX_TITULO} caracteres.` : null
  const erroDescricao = !descricao.trim()
    ? 'Descrição é obrigatória.'
    : descricao.length > MAX_DESCRICAO ? `Descrição deve ter no máximo ${MAX_DESCRICAO} caracteres.` : null

  const salvar = () => {
    if (erroTitulo || erroDescricao) return
    // Sem mudança: fecha sem gravar (AC-16).
    if (titulo === original.titulo && descricao === original.descricao) {
      onClose()
      return
    }
    // Campo que a pessoa não mudou vai com o valor atual do chamado, não com o texto antigo do modal:
    // depois de um conflito, isso preserva o que a outra pessoa gravou nele (review R-02).
    mutate(
      {
        titulo: titulo !== original.titulo ? titulo : chamado.titulo,
        descricao: descricao !== original.descricao ? descricao : chamado.descricao,
      },
      {
        onSuccess: onClose,
        onError: (e) => {
          if (e instanceof ApiError && e.status === 409) {
            setConflito(true)
            setAguardandoRecarga(true)
          }
        },
      },
    )
  }

  const outraPessoaMudou = conflito && (chamado.titulo !== original.titulo || chamado.descricao !== original.descricao)

  return (
    <Dialog open onOpenChange={(aberto) => { if (!aberto) onClose() }}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>Editar chamado</DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-3">
          <div className="flex flex-col gap-1">
            <Label htmlFor="editar-titulo">Título</Label>
            <Input id="editar-titulo" value={titulo} onChange={(e) => setTitulo(e.target.value)} />
            <div className="flex justify-between text-xs">
              <span className="text-destructive">{erroTitulo}</span>
              <span className="text-muted-foreground">{titulo.length}/{MAX_TITULO}</span>
            </div>
          </div>
          <div className="flex flex-col gap-1">
            <Label htmlFor="editar-descricao">Descrição</Label>
            <Textarea id="editar-descricao" rows={8} value={descricao} onChange={(e) => setDescricao(e.target.value)} />
            <div className="flex justify-between text-xs">
              <span className="text-destructive">{erroDescricao}</span>
              <span className="text-muted-foreground">{descricao.length}/{MAX_DESCRICAO}</span>
            </div>
          </div>
        </div>

        {error && <p className="text-sm text-destructive">{error.message}</p>}

        {outraPessoaMudou && (
          <div className="rounded-lg border border-border p-3 text-sm">
            <p className="mb-1 font-medium">Texto atual no chamado</p>
            <p className="font-medium">{chamado.titulo}</p>
            <p className="max-h-40 overflow-y-auto whitespace-pre-wrap text-muted-foreground">{chamado.descricao}</p>
          </div>
        )}

        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            Cancelar
          </Button>
          <Button onClick={salvar} disabled={isPending || aguardandoRecarga || !!erroTitulo || !!erroDescricao}>
            {isPending ? 'Salvando...' : 'Salvar'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
