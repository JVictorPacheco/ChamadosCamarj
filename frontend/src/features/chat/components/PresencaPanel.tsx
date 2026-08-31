import { usePresencas } from '../hooks/usePresencas'
import { useAuth } from '@/auth/AuthContext'
import { PresencaBadge } from './PresencaBadge'
import type { ChatPresencaResponse } from '@/types/api'

const ORDEM_STATUS: Record<string, number> = { Online: 0, Ausente: 1, Offline: 2 }

function ordenarPresencas(presencas: ChatPresencaResponse[]): ChatPresencaResponse[] {
  return [...presencas].sort((a, b) => {
    const ordemA = ORDEM_STATUS[a.status] ?? 3
    const ordemB = ORDEM_STATUS[b.status] ?? 3
    if (ordemA !== ordemB) return ordemA - ordemB
    return a.usuarioNome.localeCompare(b.usuarioNome, 'pt-BR')
  })
}

interface PresencaPanelProps {
  onIniciarConversa: (usuarioId: string) => void
}

export function PresencaPanel({ onIniciarConversa }: PresencaPanelProps) {
  const { perfil } = useAuth()
  const { data: presencas, isPending } = usePresencas()

  if (isPending) {
    return <p className="px-3 py-2 text-xs text-muted-foreground">Carregando presença...</p>
  }

  if (!presencas || presencas.length === 0) {
    return <p className="px-3 py-2 text-xs text-muted-foreground">Nenhum usuário encontrado.</p>
  }

  const ordenados = ordenarPresencas(presencas)

  return (
    <div className="flex flex-col gap-0.5 px-1">
      {ordenados.map((u) => {
        const souEu = u.usuarioId === perfil?.id

        return (
          <button
            key={u.usuarioId}
            type="button"
            onClick={() => !souEu && onIniciarConversa(u.usuarioId)}
            disabled={souEu}
            title={souEu ? undefined : `Iniciar conversa com ${u.usuarioNome}`}
            className="flex items-center gap-2 rounded-md px-2 py-1.5 text-left text-sm text-foreground hover:bg-muted transition-colors disabled:pointer-events-none"
          >
            <div className="relative flex-shrink-0">
              <div className="flex h-7 w-7 items-center justify-center rounded-full bg-muted text-xs font-medium uppercase text-muted-foreground">
                {u.usuarioNome.charAt(0)}
              </div>
              <span className="absolute -right-0.5 -bottom-0.5">
                <PresencaBadge status={u.status} size="sm" />
              </span>
            </div>
            <span className="flex-1 truncate">
              {u.usuarioNome}
              {souEu && <span className="text-xs text-muted-foreground"> (você)</span>}
            </span>
            <span className="text-xs text-muted-foreground">{u.status}</span>
          </button>
        )
      })}
    </div>
  )
}
