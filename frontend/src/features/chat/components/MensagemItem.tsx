import { useState } from 'react'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'
import { useAuth } from '@/auth/AuthContext'
import { useEditarMensagem, useDeletarMensagem, useAdicionarReacao } from '../hooks/useChat'
import { obterUrlArquivo } from '../api'
import type { ChatMensagemResponse } from '@/types/api'
import { Reply, Pencil, Trash2, Download, FileText, Smile } from 'lucide-react'

interface MensagemItemProps {
  mensagem: ChatMensagemResponse
  conversaId: string
  onResponder: (mensagem: ChatMensagemResponse) => void
  onScrollParaMensagem: (mensagemId: string) => void
}

const EMOJIS_RAPIDOS = ['👍', '❤️', '😂', '😮', '😢', '🙏']

function formatarTamanho(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function formatarHora(dataIso: string): string {
  return new Date(dataIso).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}

function formatarDataHora(dataIso: string): string {
  return new Date(dataIso).toLocaleString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function MensagemItem({ mensagem, conversaId, onResponder, onScrollParaMensagem }: MensagemItemProps) {
  const { perfil } = useAuth()
  const [editando, setEditando] = useState(false)
  const [novoConteudo, setNovoConteudo] = useState(mensagem.conteudo ?? '')
  const [mostrarEmojis, setMostrarEmojis] = useState(false)
  const [erro, setErro] = useState<string | null>(null)
  const [baixando, setBaixando] = useState(false)

  const editarMutation = useEditarMensagem(conversaId)
  const deletarMutation = useDeletarMensagem(conversaId)
  const reagirMutation = useAdicionarReacao(conversaId)

  const eAutor = perfil?.id === mensagem.autorId
  const eAdmin = perfil?.tipo === 'Admin'
  const podeDeletar = eAutor || eAdmin
  const podeEditar = eAutor && !mensagem.deletada

  // Tipo Sistema — centralizado, itálico, sem fundo
  if (mensagem.tipo === 'Sistema') {
    return (
      <div className="flex justify-center py-1">
        <p className="text-xs italic text-muted-foreground">{mensagem.conteudo}</p>
      </div>
    )
  }

  const linhaDireita = eAutor

  const salvarEdicao = () => {
    if (!novoConteudo.trim()) return
    setErro(null)
    editarMutation.mutate(
      { mensagemId: mensagem.id, conteudo: novoConteudo.trim() },
      {
        onSuccess: () => setEditando(false),
        onError: (err) => setErro(err instanceof Error ? err.message : 'Erro ao editar mensagem.'),
      }
    )
  }

  const deletar = () => {
    setErro(null)
    deletarMutation.mutate(mensagem.id, {
      onError: (err) => setErro(err instanceof Error ? err.message : 'Erro ao deletar mensagem.'),
    })
  }

  const reagir = (emoji: string) => {
    setMostrarEmojis(false)
    reagirMutation.mutate({ mensagemId: mensagem.id, emoji })
  }

  const baixarArquivo = async () => {
    setErro(null)
    setBaixando(true)
    try {
      const { urlAssinada } = await obterUrlArquivo(mensagem.id)
      window.open(urlAssinada, '_blank', 'noopener,noreferrer')
    } catch (err) {
      setErro(err instanceof Error ? err.message : 'Erro ao baixar arquivo.')
    } finally {
      setBaixando(false)
    }
  }

  return (
    <div
      className={cn(
        'group flex flex-col gap-0.5',
        linhaDireita ? 'items-end' : 'items-start'
      )}
    >
      {/* Nome do autor (apenas para mensagens de outros) */}
      {!eAutor && (
        <span className="px-2 text-xs font-medium text-muted-foreground">
          {mensagem.autorNome}
        </span>
      )}

      {/* Citação/Reply */}
      {mensagem.respostaParaMensagemId && mensagem.respostaConteudo && (
        <button
          type="button"
          onClick={() => onScrollParaMensagem(mensagem.respostaParaMensagemId!)}
          className={cn(
            'max-w-xs rounded-md border-l-4 border-primary bg-muted px-3 py-1.5 text-left',
            linhaDireita ? 'mr-2' : 'ml-2'
          )}
        >
          <p className="truncate text-xs text-muted-foreground">{mensagem.respostaConteudo}</p>
        </button>
      )}

      {/* Bolha da mensagem */}
      <div className="relative flex items-end gap-1 group">
        {/* Botões de ação (hover) */}
        {!linhaDireita && (
          <div className="hidden group-hover:flex items-center gap-0.5 order-last">
            <AcoesHover
              podeEditar={podeEditar}
              podeDeletar={podeDeletar}
              deletada={mensagem.deletada}
              onResponder={() => onResponder(mensagem)}
              onEditar={() => { setEditando(true); setNovoConteudo(mensagem.conteudo ?? '') }}
              onDeletar={deletar}
              onEmoji={() => setMostrarEmojis((v) => !v)}
            />
          </div>
        )}

        <div
          className={cn(
            'relative max-w-xs rounded-2xl px-3 py-2 text-sm',
            linhaDireita
              ? 'rounded-br-sm bg-primary text-primary-foreground'
              : 'rounded-bl-sm bg-muted text-foreground',
            'lg:max-w-md'
          )}
        >
          {mensagem.deletada ? (
            <p className="italic text-muted-foreground">[mensagem removida]</p>
          ) : editando ? (
            <div className="flex flex-col gap-2">
              <textarea
                className="w-full resize-none rounded bg-background/20 p-1 text-sm outline-none"
                value={novoConteudo}
                onChange={(e) => setNovoConteudo(e.target.value)}
                rows={2}
              />
              <div className="flex gap-1 justify-end">
                <Button
                  size="sm"
                  variant="ghost"
                  className="h-6 px-2 text-xs"
                  onClick={() => setEditando(false)}
                >
                  Cancelar
                </Button>
                <Button
                  size="sm"
                  className="h-6 px-2 text-xs"
                  onClick={salvarEdicao}
                  disabled={editarMutation.isPending}
                >
                  {editarMutation.isPending ? '...' : 'Salvar'}
                </Button>
              </div>
            </div>
          ) : mensagem.tipo === 'Arquivo' ? (
            <div className="flex items-center gap-2">
              <FileText className="h-5 w-5 flex-shrink-0" />
              <div className="flex flex-col min-w-0">
                <span className="truncate text-sm font-medium">{mensagem.nomeArquivo}</span>
                {mensagem.tamanhoBytes && (
                  <span className="text-xs opacity-70">{formatarTamanho(mensagem.tamanhoBytes)}</span>
                )}
              </div>
              <button
                type="button"
                onClick={baixarArquivo}
                disabled={baixando}
                className="flex-shrink-0 disabled:opacity-50"
                aria-label="Baixar arquivo"
              >
                <Download className="h-4 w-4" />
              </button>
            </div>
          ) : (
            <p className="whitespace-pre-wrap break-words">{mensagem.conteudo}</p>
          )}

          {/* Timestamp e editado */}
          {!mensagem.deletada && (
            <div className="mt-0.5 flex items-center justify-end gap-1">
              {mensagem.editadaEm && (
                <Tooltip>
                  <TooltipTrigger asChild>
                    <span className="text-xs opacity-60 cursor-default">(editado)</span>
                  </TooltipTrigger>
                  <TooltipContent>
                    <p>Editado em {formatarDataHora(mensagem.editadaEm)}</p>
                  </TooltipContent>
                </Tooltip>
              )}
              <span className="text-xs opacity-60">{formatarHora(mensagem.dataCriacao)}</span>
            </div>
          )}
        </div>

        {linhaDireita && (
          <div className="hidden group-hover:flex items-center gap-0.5 order-first">
            <AcoesHover
              podeEditar={podeEditar}
              podeDeletar={podeDeletar}
              deletada={mensagem.deletada}
              onResponder={() => onResponder(mensagem)}
              onEditar={() => { setEditando(true); setNovoConteudo(mensagem.conteudo ?? '') }}
              onDeletar={deletar}
              onEmoji={() => setMostrarEmojis((v) => !v)}
            />
          </div>
        )}
      </div>

      {/* Picker de emojis rápidos */}
      {mostrarEmojis && (
        <div
          className={cn(
            'flex gap-1 rounded-full border border-border bg-popover px-2 py-1 shadow-md',
            linhaDireita ? 'mr-2' : 'ml-2'
          )}
        >
          {EMOJIS_RAPIDOS.map((emoji) => (
            <button
              key={emoji}
              type="button"
              onClick={() => reagir(emoji)}
              className="text-base hover:scale-110 transition-transform"
            >
              {emoji}
            </button>
          ))}
        </div>
      )}

      {/* Reações */}
      {mensagem.reacoes && mensagem.reacoes.length > 0 && (
        <div className={cn('flex flex-wrap gap-1', linhaDireita ? 'mr-2' : 'ml-2')}>
          {mensagem.reacoes.map((reacao) => (
            <button
              key={reacao.emoji}
              type="button"
              onClick={() => reagirMutation.mutate({ mensagemId: mensagem.id, emoji: reacao.emoji })}
              className={cn(
                'flex items-center gap-0.5 rounded-full border px-1.5 py-0.5 text-xs transition-colors',
                reacao.reagiuEu
                  ? 'border-primary bg-primary/10 text-primary'
                  : 'border-border bg-muted text-foreground hover:bg-muted/80'
              )}
            >
              <span>{reacao.emoji}</span>
              <span>{reacao.quantidade}</span>
            </button>
          ))}
        </div>
      )}

      {/* Erro */}
      {erro && (
        <Alert variant="destructive" className="max-w-xs">
          <AlertDescription className="text-xs">{erro}</AlertDescription>
        </Alert>
      )}
    </div>
  )
}

interface AcoesHoverProps {
  podeEditar: boolean
  podeDeletar: boolean
  deletada: boolean
  onResponder: () => void
  onEditar: () => void
  onDeletar: () => void
  onEmoji: () => void
}

function AcoesHover({ podeEditar, podeDeletar, deletada, onResponder, onEditar, onDeletar, onEmoji }: AcoesHoverProps) {
  if (deletada) return null
  return (
    <div className="flex items-center gap-0.5 rounded-full border border-border bg-popover px-1 py-0.5 shadow-sm">
      <Tooltip>
        <TooltipTrigger asChild>
          <button type="button" onClick={onResponder} className="rounded p-1 hover:bg-muted transition-colors">
            <Reply className="h-3.5 w-3.5 text-muted-foreground" />
          </button>
        </TooltipTrigger>
        <TooltipContent><p>Responder</p></TooltipContent>
      </Tooltip>
      <Tooltip>
        <TooltipTrigger asChild>
          <button type="button" onClick={onEmoji} className="rounded p-1 hover:bg-muted transition-colors">
            <Smile className="h-3.5 w-3.5 text-muted-foreground" />
          </button>
        </TooltipTrigger>
        <TooltipContent><p>Reagir</p></TooltipContent>
      </Tooltip>
      {podeEditar && (
        <Tooltip>
          <TooltipTrigger asChild>
            <button type="button" onClick={onEditar} className="rounded p-1 hover:bg-muted transition-colors">
              <Pencil className="h-3.5 w-3.5 text-muted-foreground" />
            </button>
          </TooltipTrigger>
          <TooltipContent><p>Editar</p></TooltipContent>
        </Tooltip>
      )}
      {podeDeletar && (
        <Tooltip>
          <TooltipTrigger asChild>
            <button type="button" onClick={onDeletar} className="rounded p-1 hover:bg-muted transition-colors">
              <Trash2 className="h-3.5 w-3.5 text-destructive" />
            </button>
          </TooltipTrigger>
          <TooltipContent><p>Deletar</p></TooltipContent>
        </Tooltip>
      )}
    </div>
  )
}
