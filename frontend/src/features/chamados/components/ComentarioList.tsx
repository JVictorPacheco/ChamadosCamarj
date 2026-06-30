import { useComentarios } from '../hooks/useComentarios'

export function ComentarioList({ chamadoId }: { chamadoId: string }) {
  const { data: comentarios, isPending } = useComentarios(chamadoId)

  if (isPending) {
    return <p className="text-sm text-muted-foreground">Carregando comentários...</p>
  }

  const publicos = (comentarios ?? [])
    .filter((c) => c.tipo === 'Publico')
    .sort((a, b) => new Date(a.dataCriacao).getTime() - new Date(b.dataCriacao).getTime())

  if (publicos.length === 0) {
    return <p className="text-sm text-muted-foreground">Nenhum comentário ainda.</p>
  }

  return (
    <ul className="flex flex-col gap-3">
      {publicos.map((comentario) => (
        <li key={comentario.id} className="rounded-lg border border-border p-3">
          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span className="font-medium text-foreground">{comentario.autor}</span>
            <span>{new Date(comentario.dataCriacao).toLocaleString('pt-BR')}</span>
          </div>
          <p className="mt-1 text-sm">{comentario.conteudo}</p>
        </li>
      ))}
    </ul>
  )
}
