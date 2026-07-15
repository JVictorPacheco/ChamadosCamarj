import { useAuth } from '@/auth/AuthContext'
import { Badge } from '@/components/ui/badge'
import { useComentarios } from '../hooks/useComentarios'

export function ComentarioList({ chamadoId }: { chamadoId: string }) {
  const { perfil } = useAuth()
  const { data: comentarios, isPending } = useComentarios(chamadoId, perfil?.tipo)

  if (isPending) {
    return <p className="text-sm text-muted-foreground">Carregando comentários...</p>
  }

  const ordenados = (comentarios ?? []).slice().sort(
    (a, b) => new Date(a.dataCriacao).getTime() - new Date(b.dataCriacao).getTime(),
  )

  if (ordenados.length === 0) {
    return <p className="text-sm text-muted-foreground">Nenhum comentário ainda.</p>
  }

  return (
    <ul className="flex flex-col gap-3">
      {ordenados.map((comentario) => (
        <li key={comentario.id} className="rounded-lg border border-border p-3">
          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <div className="flex items-center gap-2">
              <span className="font-medium text-foreground">{comentario.autor}</span>
              {comentario.tipo === 'Interno' && <Badge variant="secondary">Interno</Badge>}
            </div>
            <span>{new Date(comentario.dataCriacao).toLocaleString('pt-BR')}</span>
          </div>
          <p className="mt-1 text-sm">{comentario.conteudo}</p>
        </li>
      ))}
    </ul>
  )
}
