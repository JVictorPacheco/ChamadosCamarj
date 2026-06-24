import { Link, useParams } from 'react-router'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { useAuth } from '@/auth/AuthContext'
import { ApiError } from '@/lib/api'
import { StatusBadge } from './components/StatusBadge'
import { PrioridadeBadge } from './components/PrioridadeBadge'
import { SlaBadge } from './components/SlaBadge'
import { ComentarioList } from './components/ComentarioList'
import { ComentarioForm } from './components/ComentarioForm'
import { useChamado } from './hooks/useChamado'

export function ChamadoDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { perfil } = useAuth()
  const { data: chamado, isPending, error } = useChamado(id!)

  if (isPending) {
    return <p className="p-4 text-sm text-muted-foreground">Carregando...</p>
  }

  if (error instanceof ApiError && error.status === 404) {
    return (
      <div className="flex flex-col items-center gap-3 p-8 text-center">
        <p className="text-sm text-muted-foreground">Chamado não encontrado.</p>
        <Button asChild variant="outline">
          <Link to="/chamados">Voltar para a lista</Link>
        </Button>
      </div>
    )
  }

  if (!chamado) {
    return null
  }

  if (chamado.solicitanteEmail !== perfil?.email) {
    return (
      <div className="flex flex-col items-center gap-3 p-8 text-center">
        <Alert variant="destructive" className="max-w-md">
          <AlertDescription>Este chamado não pertence ao seu perfil ativo.</AlertDescription>
        </Alert>
        <Button asChild variant="outline">
          <Link to="/chamados">Voltar para a lista</Link>
        </Button>
      </div>
    )
  }

  return (
    <div className="flex max-w-2xl flex-col gap-4 p-4">
      <Button asChild variant="ghost" size="sm" className="self-start">
        <Link to="/chamados">← Voltar</Link>
      </Button>

      <h1 className="text-xl font-heading">{chamado.titulo}</h1>

      <div className="flex flex-wrap items-center gap-2">
        <StatusBadge status={chamado.status} />
        <PrioridadeBadge prioridade={chamado.prioridade} />
        <SlaBadge dataLimite={chamado.dataLimite} status={chamado.status} />
      </div>

      <p className="text-sm">{chamado.descricao}</p>

      <dl className="grid grid-cols-2 gap-2 text-sm text-muted-foreground">
        <div>
          <dt className="font-medium text-foreground">Categoria</dt>
          <dd>{chamado.categoriaNome ?? 'Sem categoria'}</dd>
        </div>
        <div>
          <dt className="font-medium text-foreground">Aberto em</dt>
          <dd>{new Date(chamado.dataCriacao).toLocaleString('pt-BR')}</dd>
        </div>
        {chamado.dataLimite && (
          <div>
            <dt className="font-medium text-foreground">Prazo (SLA)</dt>
            <dd>{new Date(chamado.dataLimite).toLocaleString('pt-BR')}</dd>
          </div>
        )}
        {chamado.dataConclusao && (
          <div>
            <dt className="font-medium text-foreground">Concluído em</dt>
            <dd>{new Date(chamado.dataConclusao).toLocaleString('pt-BR')}</dd>
          </div>
        )}
      </dl>

      <h2 className="text-base font-heading">Comentários</h2>
      <ComentarioList chamadoId={chamado.id} />
      <ComentarioForm chamadoId={chamado.id} autor={perfil?.nome ?? ''} />
    </div>
  )
}
