import { cn } from '@/lib/utils'
import type { StatusPresenca } from '@/types/api'

interface PresencaBadgeProps {
  status: StatusPresenca
  size?: 'sm' | 'md'
}

export function PresencaBadge({ status, size = 'sm' }: PresencaBadgeProps) {
  return (
    <span
      className={cn(
        'inline-block rounded-full border-2 border-background',
        size === 'sm' ? 'h-2.5 w-2.5' : 'h-3.5 w-3.5',
        status === 'Online' && 'bg-green-500',
        status === 'Ausente' && 'bg-yellow-400',
        status === 'Offline' && 'bg-muted-foreground/40'
      )}
      title={status}
      aria-label={`Status: ${status}`}
    />
  )
}
