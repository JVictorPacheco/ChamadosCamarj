import { usePresencas } from '../hooks/usePresencas'
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

export function PresencaPanel() {
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
      {ordenados.map((u) => (
        <div
          key={u.usuarioId}
          className="flex items-center gap-2 rounded-md px-2 py-1.5 text-sm text-foreground"
        >
          <div className="relative flex-shrink-0">
            <div className="flex h-7 w-7 items-center justify-center rounded-full bg-muted text-xs font-medium uppercase text-muted-foreground">
              {u.usuarioNome.charAt(0)}
            </div>
            <span className="absolute -right-0.5 -bottom-0.5">
              <PresencaBadge status={u.status} size="sm" />
            </span>
          </div>
          <span className="flex-1 truncate">{u.usuarioNome}</span>
          <span className="text-xs text-muted-foreground">{u.status}</span>
        </div>
      ))}
    </div>
  )
}
