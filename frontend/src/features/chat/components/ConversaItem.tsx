import { cn } from '@/lib/utils'
import { Badge } from '@/components/ui/badge'
import type { ChatConversaResponse } from '@/types/api'
import { Users, MessageSquare } from 'lucide-react'

interface ConversaItemProps {
  conversa: ChatConversaResponse
  ativa: boolean
  onClick: () => void
}

function formatarTimestamp(dataIso?: string): string {
  if (!dataIso) return ''
  const data = new Date(dataIso)
  const agora = new Date()
  const hoje = new Date(agora.getFullYear(), agora.getMonth(), agora.getDate())
  const dataConversa = new Date(data.getFullYear(), data.getMonth(), data.getDate())

  if (dataConversa.getTime() === hoje.getTime()) {
    return data.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
  }
  return data.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })
}

export function ConversaItem({ conversa, ativa, onClick }: ConversaItemProps) {
  const nome = conversa.nome ?? 'Conversa'
  const icone =
    conversa.tipo === 'Grupo' ? (
      <Users className="h-4 w-4 text-muted-foreground" />
    ) : (
      <MessageSquare className="h-4 w-4 text-muted-foreground" />
    )

  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        'flex w-full items-start gap-3 rounded-lg px-3 py-2.5 text-left transition-colors',
        ativa
          ? 'bg-accent text-accent-foreground'
          : 'hover:bg-muted/50 text-foreground'
      )}
    >
      {/* Avatar */}
      <div className="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full bg-muted">
        {icone}
      </div>

      {/* Conteúdo */}
      <div className="flex min-w-0 flex-1 flex-col gap-0.5">
        <div className="flex items-center justify-between gap-1">
          <span className="truncate text-sm font-medium">{nome}</span>
          <span className="flex-shrink-0 text-xs text-muted-foreground">
            {formatarTimestamp(conversa.ultimaMensagemEm)}
          </span>
        </div>
        <div className="flex items-center justify-between gap-1">
          <span className="truncate text-xs text-muted-foreground">
            {conversa.ultimaMensagem ?? 'Nenhuma mensagem'}
          </span>
          {conversa.naoLidas > 0 && (
            <Badge
              variant="destructive"
              className="flex-shrink-0 min-w-[1.25rem] justify-center px-1"
            >
              {conversa.naoLidas > 99 ? '99+' : conversa.naoLidas}
            </Badge>
          )}
        </div>
      </div>
    </button>
  )
}
