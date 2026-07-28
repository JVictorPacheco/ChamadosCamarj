import { useState } from 'react'
import { Link } from 'react-router'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAuth } from '@/auth/AuthContext'
import { useGrupos } from './hooks/useGrupos'
import { GrupoFormDialog } from './components/GrupoFormDialog'
import type { GrupoResponse } from '@/types/api'

export function GruposPage() {
  const { perfil } = useAuth()
  const isAdmin = perfil?.tipo === 'Admin'
  const { data: grupos, isPending, isError } = useGrupos()
  const [dialogAberto, setDialogAberto] = useState(false)
  const [grupoSelecionado, setGrupoSelecionado] = useState<GrupoResponse | null>(null)

  const abrirNovo = () => {
    setGrupoSelecionado(null)
    setDialogAberto(true)
  }

  const abrirEdicao = (grupo: GrupoResponse) => {
    setGrupoSelecionado(grupo)
    setDialogAberto(true)
  }

  if (!isAdmin) {
    return (
      <div className="flex flex-col items-center gap-3 p-8 text-center">
        <Alert variant="destructive" className="max-w-md">
          <AlertDescription>Esta área não está disponível para o seu perfil.</AlertDescription>
        </Alert>
        <Button asChild variant="outline">
          <Link to="/chamados">Voltar para a lista</Link>
        </Button>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-4 p-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-xl font-heading">Grupos</h1>
        <Button onClick={abrirNovo}>Novo grupo</Button>
      </div>

      {isError && (
        <Alert variant="destructive">
          <AlertDescription>Serviço indisponível. Tente novamente em instantes.</AlertDescription>
        </Alert>
      )}

      {isPending && <p className="text-sm text-muted-foreground">Carregando grupos...</p>}

      {!isPending && grupos && grupos.length === 0 && (
        <p className="py-8 text-center text-sm text-muted-foreground">Nenhum grupo cadastrado.</p>
      )}

      {!isPending && grupos && grupos.length > 0 && (
        <div className="rounded-lg border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead>Descrição</TableHead>
                <TableHead className="text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {grupos.map((grupo) => (
                <TableRow key={grupo.id}>
                  <TableCell>{grupo.nome}</TableCell>
                  <TableCell>{grupo.descricao}</TableCell>
                  <TableCell className="text-right space-x-2">
                    <Button variant="outline" size="sm" onClick={() => abrirEdicao(grupo)}>
                      Editar
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <GrupoFormDialog open={dialogAberto} onOpenChange={setDialogAberto} grupo={grupoSelecionado} />
    </div>
  )
}
