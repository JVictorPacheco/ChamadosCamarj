import { useEffect, useRef } from 'react'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { useMensagens } from '../hooks/useMensagens'
import { MensagemItem } from './MensagemItem'
import { TypingIndicator } from './TypingIndicator'
import type { ChatMensagemResponse } from '@/types/api'

interface MensagemListProps {
  conversaId: string
  digitandoNome: string | null
  onResponder: (mensagem: ChatMensagemResponse) => void
}

function mesmaData(a: string, b: string): boolean {
  const da = new Date(a)
  const db = new Date(b)
  return da.getFullYear() === db.getFullYear() &&
    da.getMonth() === db.getMonth() &&
    da.getDate() === db.getDate()
}

function formatarDataSeparador(dataIso: string): string {
  const data = new Date(dataIso)
  const agora = new Date()
  const hoje = new Date(agora.getFullYear(), agora.getMonth(), agora.getDate())
  const dataMsg = new Date(data.getFullYear(), data.getMonth(), data.getDate())
  const diffDias = Math.round((hoje.getTime() - dataMsg.getTime()) / (1000 * 60 * 60 * 24))

  if (diffDias === 0) return 'Hoje'
  if (diffDias === 1) return 'Ontem'
  return data.toLocaleDateString('pt-BR', { day: '2-digit', month: 'long', year: 'numeric' })
}

export function MensagemList({ conversaId, digitandoNome, onResponder }: MensagemListProps) {
  const { data, isPending, isError, fetchNextPage, hasNextPage, isFetchingNextPage } = useMensagens(conversaId)
  const endRef = useRef<HTMLDivElement | null>(null)
  const prevLengthRef = useRef(0)

  // Todas as mensagens em ordem cronológica (mais antigas primeiro)
  const todasMensagens: ChatMensagemResponse[] = []
  if (data) {
    // InfiniteQuery retorna pages do mais recente ao mais antigo (paginação reversa)
    // Invertemos para exibir cronologicamente
    const pages = [...data.pages].reverse()
    for (const page of pages) {
      const itens = [...page.items].reverse()
      todasMensagens.push(...itens)
    }
  }

  // Auto-scroll para o fim quando chegam novas mensagens
  useEffect(() => {
    if (todasMensagens.length > prevLengthRef.current) {
      endRef.current?.scrollIntoView({ behavior: 'smooth' })
    }
    prevLengthRef.current = todasMensagens.length
  }, [todasMensagens.length])

  const scrollParaMensagem = (mensagemId: string) => {
    const el = document.getElementById(`msg-${mensagemId}`)
    el?.scrollIntoView({ behavior: 'smooth', block: 'center' })
  }

  if (isPending) {
    return (
      <div className="flex flex-1 items-center justify-center text-sm text-muted-foreground">
        Carregando mensagens...
      </div>
    )
  }

  if (isError) {
    return (
      <div className="flex flex-1 items-center justify-center text-sm text-destructive">
        Erro ao carregar mensagens.
      </div>
    )
  }

  return (
    <div className="flex flex-1 flex-col overflow-y-auto">
      {/* Botão "Carregar mais" no topo */}
      {hasNextPage && (
        <div className="flex justify-center p-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => fetchNextPage()}
            disabled={isFetchingNextPage}
          >
            {isFetchingNextPage ? 'Carregando...' : 'Carregar mais'}
          </Button>
        </div>
      )}

      {/* Lista de mensagens */}
      <div className="flex flex-col gap-2 px-4 py-3">
        {todasMensagens.map((mensagem, idx) => {
          const anterior = todasMensagens[idx - 1]
          const mostrarSeparador =
            !anterior || !mesmaData(anterior.dataCriacao, mensagem.dataCriacao)

          return (
            <div key={mensagem.id} id={`msg-${mensagem.id}`}>
              {mostrarSeparador && (
                <div className="flex items-center gap-2 my-2">
                  <Separator className="flex-1" />
                  <span className="flex-shrink-0 text-xs text-muted-foreground">
                    {formatarDataSeparador(mensagem.dataCriacao)}
                  </span>
                  <Separator className="flex-1" />
                </div>
              )}
              <MensagemItem
                mensagem={mensagem}
                conversaId={conversaId}
                onResponder={onResponder}
                onScrollParaMensagem={scrollParaMensagem}
              />
            </div>
          )
        })}
      </div>

      {/* Typing indicator */}
      {digitandoNome && <TypingIndicator usuarioNome={digitandoNome} />}

      {/* Âncora para auto-scroll */}
      <div ref={endRef} />
    </div>
  )
}
