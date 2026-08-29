import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { useAuth } from '@/auth/AuthContext'
import { useConversas } from '../hooks/useConversas'
import { ConversaItem } from './ConversaItem'
import { CriarGrupoDialog } from './CriarGrupoDialog'
import { Plus, Users } from 'lucide-react'

interface ConversaListProps {
  conversaAtivaId: string | null
  onSelectConversa: (id: string) => void
}

export function ConversaList({ conversaAtivaId, onSelectConversa }: ConversaListProps) {
  const { perfil } = useAuth()
  const { data: conversas, isPending, isError } = useConversas()
  const [criarGrupoAberto, setCriarGrupoAberto] = useState(false)

  const podeCriarGrupo = perfil?.chatPerfil === 'CriadorDeGrupo'

  return (
    <div className="flex flex-col h-full">
      {/* Cabeçalho */}
      <div className="flex items-center justify-between px-3 py-2 border-b border-border">
        <h2 className="text-sm font-semibold text-foreground">Conversas</h2>
        <div className="flex items-center gap-1">
          {podeCriarGrupo && (
            <Button
              variant="ghost"
              size="sm"
              className="h-7 w-7 p-0"
              onClick={() => setCriarGrupoAberto(true)}
              title="Novo grupo"
            >
              <Users className="h-4 w-4" />
            </Button>
          )}
          <Button
            variant="ghost"
            size="sm"
            className="h-7 w-7 p-0"
            title="Nova conversa"
            disabled
          >
            <Plus className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Conteúdo */}
      <div className="flex-1 overflow-y-auto">
        {isError && (
          <div className="p-2">
            <Alert variant="destructive">
              <AlertDescription>Erro ao carregar conversas.</AlertDescription>
            </Alert>
          </div>
        )}

        {isPending && (
          <p className="px-3 py-2 text-xs text-muted-foreground">Carregando...</p>
        )}

        {!isPending && conversas && conversas.length === 0 && (
          <p className="px-3 py-6 text-center text-xs text-muted-foreground">
            Nenhuma conversa ainda.
          </p>
        )}

        {!isPending && conversas && conversas.length > 0 && (
          <div className="flex flex-col gap-0.5 p-1">
            {conversas.map((conversa) => (
              <ConversaItem
                key={conversa.id}
                conversa={conversa}
                ativa={conversa.id === conversaAtivaId}
                onClick={() => onSelectConversa(conversa.id)}
              />
            ))}
          </div>
        )}
      </div>

      {podeCriarGrupo && (
        <CriarGrupoDialog
          open={criarGrupoAberto}
          onOpenChange={setCriarGrupoAberto}
          onSuccess={(id) => {
            setCriarGrupoAberto(false)
            onSelectConversa(id)
          }}
        />
      )}
    </div>
  )
}
