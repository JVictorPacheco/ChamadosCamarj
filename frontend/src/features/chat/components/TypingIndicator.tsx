import { cn } from '@/lib/utils'

interface TypingIndicatorProps {
  usuarioNome: string
  className?: string
}

export function TypingIndicator({ usuarioNome, className }: TypingIndicatorProps) {
  return (
    <div className={cn('flex items-center gap-1.5 px-4 py-1 text-xs text-muted-foreground', className)}>
      <span>{usuarioNome} está digitando</span>
      <span className="flex gap-0.5">
        <span className="h-1 w-1 rounded-full bg-muted-foreground animate-bounce [animation-delay:0ms]" />
        <span className="h-1 w-1 rounded-full bg-muted-foreground animate-bounce [animation-delay:150ms]" />
        <span className="h-1 w-1 rounded-full bg-muted-foreground animate-bounce [animation-delay:300ms]" />
      </span>
    </div>
  )
}
