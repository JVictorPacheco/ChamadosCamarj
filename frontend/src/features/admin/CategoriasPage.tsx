import { useState } from 'react'
import { Link } from 'react-router'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAuth } from '@/auth/AuthContext'
import { useCategoriasAdmin } from './hooks/useCategorias'
import { CategoriaFormDialog } from './components/CategoriaFormDialog'
import type { CategoriaResponse } from '@/types/api'

export function CategoriasPage() {
  const { perfil } = useAuth()
  const isAdmin = perfil?.tipo === 'Admin'
  const { data: categorias, isPending, isError } = useCategoriasAdmin()
  const [dialogAberto, setDialogAberto] = useState(false)
  const [categoriaSelecionada, setCategoriaSelecionada] = useState<CategoriaResponse | null>(null)

  const abrirNovo = () => {
    setCategoriaSelecionada(null)
    setDialogAberto(true)
  }

  const abrirEdicao = (categoria: CategoriaResponse) => {
    setCategoriaSelecionada(categoria)
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
        <h1 className="text-xl font-heading">Categorias</h1>
        <Button onClick={abrirNovo}>Nova categoria</Button>
      </div>

      {isError && (
        <Alert variant="destructive">
          <AlertDescription>Serviço indisponível. Tente novamente em instantes.</AlertDescription>
        </Alert>
      )}

      {isPending && <p className="text-sm text-muted-foreground">Carregando categorias...</p>}

      {!isPending && categorias && categorias.length === 0 && (
        <p className="py-8 text-center text-sm text-muted-foreground">Nenhuma categoria cadastrada.</p>
      )}

      {!isPending && categorias && categorias.length > 0 && (
        <div className="rounded-lg border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nome</TableHead>
                <TableHead>Descrição</TableHead>
                <TableHead>Ativa</TableHead>
                <TableHead className="text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {categorias.map((categoria) => (
                <TableRow key={categoria.id}>
                  <TableCell>{categoria.nome}</TableCell>
                  <TableCell>{categoria.descricao}</TableCell>
                  <TableCell>
                    <Badge variant={categoria.ativa ? 'default' : 'secondary'}>
                      {categoria.ativa ? 'Ativa' : 'Inativa'}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right space-x-2">
                    <Button variant="outline" size="sm" onClick={() => abrirEdicao(categoria)}>
                      Editar
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <CategoriaFormDialog open={dialogAberto} onOpenChange={setDialogAberto} categoria={categoriaSelecionada} />
    </div>
  )
}
