import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Link } from 'react-router'
import { listarChamados } from '@/features/chamados/api'
import { ChamadoCard } from '@/features/chamados/components/ChamadoCard'
import { useSignalR } from '@/hooks/useSignalR'
import { useEffect } from 'react'
import { useAuth } from '@/auth/AuthContext'
import { useAtribuirChamado } from '@/features/chamados/hooks/useAcoesChamado'
import type { ChamadoResponse } from '@/types/api'

function BotaoAssumir({ chamado }: { chamado: ChamadoResponse }) {
  const { perfil } = useAuth()
  const atribuir = useAtribuirChamado(chamado.id)

  if (chamado.responsavelId) return null

  return (
    <Button
      size="sm"
      disabled={atribuir.isPending}
      onClick={(e) => {
        e.preventDefault()
        atribuir.mutate({ responsavelId: perfil!.id, responsavelNome: perfil!.nome })
      }}
    >
      {atribuir.isPending ? 'Assumindo...' : 'Assumir'}
    </Button>
  )
}

export function FilaAtendimentoPage() {
  const queryClient = useQueryClient()
  const { subscribe } = useSignalR()

  const { data, isPending, isError } = useQuery<ChamadoResponse[]>({
    queryKey: ['chamados', 'fila'],
    queryFn: async () => {
      const result = await listarChamados({ pagina: 1, tamanhoPagina: 50, status: 'Aberto' })
      const ordem = { Urgente: 0, Alta: 1, Media: 2, Baixa: 3 } as const
      return [...result.items].sort(
        (a, b) =>
          (ordem[a.prioridade as keyof typeof ordem] ?? 99) -
          (ordem[b.prioridade as keyof typeof ordem] ?? 99),
      )
    },
    staleTime: 10_000,
  })

  useEffect(() => {
    return subscribe(() => {
      queryClient.invalidateQueries({ queryKey: ['chamados', 'fila'] })
    })
  }, [subscribe, queryClient])

  return (
    <div className="flex flex-col gap-4 p-4">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-heading">Fila de Atendimento</h1>
        <span className="text-sm text-muted-foreground">
          {data ? `${data.length} chamado(s) pendente(s)` : ''}
        </span>
      </div>

      {isError && (
        <Alert variant="destructive">
          <AlertDescription>Serviço indisponível. Tente novamente em instantes.</AlertDescription>
        </Alert>
      )}

      {isPending && <p className="text-sm text-muted-foreground">Carregando fila...</p>}

      {!isPending && data && data.length === 0 && (
        <div className="flex flex-col items-center gap-3 rounded-lg border border-dashed border-border p-8 text-center">
          <p className="text-sm text-muted-foreground">Nenhum chamado pendente. 🎉</p>
        </div>
      )}

      {!isPending &&
        data?.map((chamado) => (
          <Link key={chamado.id} to={`/chamados/${chamado.id}`} className="block">
            <div className="flex items-center gap-3">
              <div className="flex-1">
                <ChamadoCard chamado={chamado} />
              </div>
              <div className="flex gap-2" onClick={(e) => e.stopPropagation()}>
                <BotaoAssumir chamado={chamado} />
                <Button asChild variant="outline" size="sm">
                  <Link to={`/chamados/${chamado.id}`}>Ver</Link>
                </Button>
              </div>
            </div>
          </Link>
        ))}
    </div>
  )
}
